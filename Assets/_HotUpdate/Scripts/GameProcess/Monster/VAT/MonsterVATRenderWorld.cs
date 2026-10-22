using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterVATRenderProfile
    {
        public readonly MonsterVATAsset Asset;
        public readonly Material Material;
        public readonly Material SourceMaterial;
        public readonly Vector3 RootScale;
        public readonly Matrix4x4 RendererLocalToRoot;
        public readonly float CrossFadeDuration;
        public readonly float MovementThreshold;
        public readonly float HitFlashDuration;
        public readonly Color HitFlashColor;
        public readonly ShadowCastingMode ShadowCastingMode;
        public readonly bool ReceiveShadows;
        public readonly int Layer;

        public MonsterVATRenderProfile(MonsterVATAsset asset, Material material, Material sourceMaterial,
            Vector3 rootScale, Matrix4x4 rendererLocalToRoot,
            float crossFadeDuration, float movementThreshold, float hitFlashDuration, Color hitFlashColor,
            ShadowCastingMode shadowCastingMode, bool receiveShadows, int layer)
        {
            Asset = asset;
            Material = material;
            SourceMaterial = sourceMaterial;
            RootScale = rootScale;
            RendererLocalToRoot = rendererLocalToRoot;
            CrossFadeDuration = crossFadeDuration;
            MovementThreshold = movementThreshold;
            HitFlashDuration = hitFlashDuration;
            HitFlashColor = hitFlashColor;
            ShadowCastingMode = shadowCastingMode;
            ReceiveShadows = receiveShadows;
            Layer = layer;
        }
    }

    /// <summary>房间级 VAT 状态与批量绘制；所有实例按稳定 Monster Slot 索引。</summary>
    public sealed class MonsterVATRenderWorld : IDisposable
    {
        private const int DrawBatchCapacity = 256;

        private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("Monster.VAT.Update");
        private static readonly ProfilerMarker UploadMarker = new ProfilerMarker("Monster.VAT.Upload");
        private static readonly ProfilerMarker DrawMarker = new ProfilerMarker("Monster.VAT.Draw");
        private static readonly int MainTextureHash = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int PositionTextureHash = Shader.PropertyToID("_PositionTexture");
        private static readonly int NormalTextureHash = Shader.PropertyToID("_NormalTexture");
        private static readonly int VertexCountHash = Shader.PropertyToID("_VertexCount");
        private static readonly int FrameCountHash = Shader.PropertyToID("_FrameCount");
        private static readonly int CurrentFrameHash = Shader.PropertyToID("_CurrentFrame");
        private static readonly int NextFrameHash = Shader.PropertyToID("_NextFrame");
        private static readonly int BlendHash = Shader.PropertyToID("_Blend");
        private static readonly int HitFlashColorHash = Shader.PropertyToID("_HitFlashColor");
        private static readonly int HitFlashAmountHash = Shader.PropertyToID("_HitFlashAmount");

        private readonly float _groundY;
        private readonly List<RenderGroup> _groups = new List<RenderGroup>();
        private InstanceState[] _states = Array.Empty<InstanceState>();
        private int _slotCount;

        public MonsterVATRenderWorld(float groundY)
        {
            _groundY = groundY;
        }

        public void Spawn(int slot, in MonsterVATRenderProfile profile, Vector2 position, float yaw)
        {
            if (slot != _slotCount)
                throw new InvalidOperationException($"Monster VAT Slot 必须连续追加：Expected={_slotCount}, Actual={slot}。");

            EnsureCapacity(slot + 1);
            int groupIndex = GetOrCreateGroup(profile);
            RenderGroup group = _groups[groupIndex];
            _states[slot] = new InstanceState
            {
                Active = true,
                GroupIndex = groupIndex,
                Position = position,
                PreviousPosition = position,
                Yaw = yaw,
                SourceClip = group.IdleClip,
                TargetClip = group.IdleClip,
                DesiredLocomotionClip = group.IdleClip,
                SourceLoop = true,
                TargetLoop = true,
            };
            group.Slots.Add(slot);
            _slotCount++;
        }

        public void SetPose(int slot, Vector2 position, float yaw, float deltaTime)
        {
            ref InstanceState state = ref _states[slot];
            RenderGroup group = _groups[state.GroupIndex];
            bool moving = state.HasPreviousPose
                && Vector2.Distance(state.PreviousPosition, position) / Mathf.Max(deltaTime, 0.0001f)
                > group.MovementThreshold;

            state.Position = position;
            state.Yaw = yaw;
            state.PreviousPosition = position;
            state.HasPreviousPose = true;
            SetLocomotion(ref state, group, moving);
        }

        public void PlayAttack(int slot)
        {
            ref InstanceState state = ref _states[slot];
            RenderGroup group = _groups[state.GroupIndex];
            state.AttackPlaying = true;
            if (state.TargetClip == group.AttackClip)
            {
                state.SourceClip = state.TargetClip = group.AttackClip;
                state.SourceTime = state.TargetTime = 0f;
                state.SourceLoop = state.TargetLoop = false;
                state.TransitionElapsed = state.TransitionDuration = 0f;
                return;
            }
            BeginTransition(ref state, group.AttackClip, false, group.CrossFadeDuration);
        }

        public void PlayHit(int slot)
        {
            ref InstanceState state = ref _states[slot];
            state.HitFlashRemaining = _groups[state.GroupIndex].HitFlashDuration;
        }

        public void Despawn(int slot)
        {
            _states[slot].Active = false;
        }

        public void UpdateAndRender(float deltaTime, float unscaledDeltaTime)
        {
            using (UpdateMarker.Auto())
            {
                for (int slot = 0; slot < _slotCount; slot++)
                {
                    ref InstanceState state = ref _states[slot];
                    if (!state.Active)
                        continue;
                    Tick(ref state, _groups[state.GroupIndex], deltaTime, unscaledDeltaTime);
                }
            }

            using (DrawMarker.Auto())
            {
                for (int i = 0; i < _groups.Count; i++)
                    Draw(_groups[i]);
            }
        }

        public void Dispose()
        {
            _slotCount = 0;
            _groups.Clear();
            Array.Clear(_states, 0, _states.Length);
        }

        private int GetOrCreateGroup(in MonsterVATRenderProfile profile)
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                RenderGroup group = _groups[i];
                if (group.Asset == profile.Asset && group.Material == profile.Material)
                    return i;
            }

            _groups.Add(new RenderGroup(profile));
            return _groups.Count - 1;
        }

        private void Draw(RenderGroup group)
        {
            int drawCount = 0;
            for (int i = 0; i < group.Slots.Count; i++)
            {
                ref InstanceState state = ref _states[group.Slots[i]];
                if (!state.Active)
                    continue;

                VATFrameState frameState = EvaluateFrameState(state, group);
                group.Matrices[drawCount] = Matrix4x4.TRS(
                    new Vector3(state.Position.x, _groundY, state.Position.y),
                    Quaternion.Euler(0f, state.Yaw, 0f),
                    group.RootScale) * group.RendererLocalToRoot;
                group.CurrentFrames[drawCount] = frameState.CurrentFrame;
                group.NextFrames[drawCount] = frameState.NextFrame;
                group.Blends[drawCount] = frameState.Blend;
                group.HitFlashAmounts[drawCount] = state.HitFlashRemaining > 0f ? 1f : 0f;
                drawCount++;

                if (drawCount == DrawBatchCapacity)
                {
                    Submit(group, drawCount);
                    drawCount = 0;
                }
            }

            if (drawCount > 0)
                Submit(group, drawCount);
        }

        private static void Submit(RenderGroup group, int count)
        {
            MaterialPropertyBlock properties = group.Properties;
            using (UploadMarker.Auto())
            {
                properties.Clear();
                properties.SetFloatArray(CurrentFrameHash, group.CurrentFrames);
                properties.SetFloatArray(NextFrameHash, group.NextFrames);
                properties.SetFloatArray(BlendHash, group.Blends);
                properties.SetFloatArray(HitFlashAmountHash, group.HitFlashAmounts);
            }

            Graphics.DrawMeshInstanced(
                group.Asset.Mesh,
                0,
                group.Material,
                group.Matrices,
                count,
                properties,
                group.ShadowCastingMode,
                group.ReceiveShadows,
                group.Layer,
                null,
                LightProbeUsage.Off,
                null);
        }

        private static void Tick(ref InstanceState state, RenderGroup group,
            float deltaTime, float unscaledDeltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            state.SourceTime += deltaTime;
            state.TargetTime += deltaTime;
            state.HitFlashRemaining = Mathf.Max(0f, state.HitFlashRemaining - unscaledDeltaTime);

            if (state.AttackPlaying && state.TargetClip == group.AttackClip
                && state.TargetTime >= GetClipDuration(group.Clips[group.AttackClip]))
            {
                state.AttackPlaying = false;
                BeginTransition(ref state, state.DesiredLocomotionClip, true, group.CrossFadeDuration);
            }

            if (state.TransitionDuration <= 0f)
                return;
            state.TransitionElapsed += deltaTime;
            if (state.TransitionElapsed >= state.TransitionDuration)
                CompleteTransition(ref state);
        }

        private static void SetLocomotion(ref InstanceState state, RenderGroup group, bool moving)
        {
            state.DesiredLocomotionClip = moving ? group.WalkClip : group.IdleClip;
            if (state.AttackPlaying
                || state.TargetClip == state.DesiredLocomotionClip && state.TargetLoop)
                return;
            BeginTransition(ref state, state.DesiredLocomotionClip, true, group.CrossFadeDuration);
        }

        private static void BeginTransition(ref InstanceState state, int clip, bool loop, float duration)
        {
            state.SourceClip = state.TargetClip;
            state.SourceTime = state.TargetTime;
            state.SourceLoop = state.TargetLoop;
            state.TargetClip = clip;
            state.TargetTime = 0f;
            state.TargetLoop = loop;
            state.TransitionElapsed = 0f;
            state.TransitionDuration = Mathf.Max(0f, duration);
            if (state.TransitionDuration <= 0f)
                CompleteTransition(ref state);
        }

        private static void CompleteTransition(ref InstanceState state)
        {
            state.SourceClip = state.TargetClip;
            state.SourceTime = state.TargetTime;
            state.SourceLoop = state.TargetLoop;
            state.TransitionElapsed = state.TransitionDuration = 0f;
        }

        private static VATFrameState EvaluateFrameState(in InstanceState state, RenderGroup group)
        {
            float currentFrame = GetFrame(group.Clips[state.SourceClip], state.SourceTime, state.SourceLoop);
            float nextFrame = GetFrame(group.Clips[state.TargetClip], state.TargetTime, state.TargetLoop);
            float blend = state.TransitionDuration > 0f
                ? Mathf.Clamp01(state.TransitionElapsed / state.TransitionDuration)
                : 0f;
            return new VATFrameState(currentFrame, nextFrame, blend);
        }

        private static float GetFrame(VATClipData clip, float time, bool loop)
        {
            float frame = time * clip.FrameRate;
            frame = loop ? Mathf.Repeat(frame, clip.FrameCount) : Mathf.Clamp(frame, 0f, clip.FrameCount - 1f);
            return clip.StartFrame + Mathf.Floor(frame);
        }

        private static float GetClipDuration(VATClipData clip) =>
            clip.FrameCount / Mathf.Max(clip.FrameRate, 0.01f);

        private void EnsureCapacity(int count)
        {
            if (count <= _states.Length)
                return;
            int capacity = Math.Max(64, _states.Length);
            while (capacity < count)
                capacity = checked(capacity * 2);
            Array.Resize(ref _states, capacity);
        }

        private struct InstanceState
        {
            public bool Active;
            public bool HasPreviousPose;
            public bool SourceLoop;
            public bool TargetLoop;
            public bool AttackPlaying;
            public int GroupIndex;
            public int SourceClip;
            public int TargetClip;
            public int DesiredLocomotionClip;
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public float Yaw;
            public float SourceTime;
            public float TargetTime;
            public float TransitionElapsed;
            public float TransitionDuration;
            public float HitFlashRemaining;
        }

        private sealed class RenderGroup
        {
            public readonly MonsterVATAsset Asset;
            public readonly Material Material;
            public readonly VATClipData[] Clips;
            public readonly int IdleClip;
            public readonly int WalkClip;
            public readonly int AttackClip;
            public readonly float CrossFadeDuration;
            public readonly float MovementThreshold;
            public readonly float HitFlashDuration;
            public readonly Vector3 RootScale;
            public readonly Matrix4x4 RendererLocalToRoot;
            public readonly List<int> Slots = new List<int>();
            public readonly Matrix4x4[] Matrices = new Matrix4x4[DrawBatchCapacity];
            public readonly float[] CurrentFrames = new float[DrawBatchCapacity];
            public readonly float[] NextFrames = new float[DrawBatchCapacity];
            public readonly float[] Blends = new float[DrawBatchCapacity];
            public readonly float[] HitFlashAmounts = new float[DrawBatchCapacity];
            public readonly MaterialPropertyBlock Properties = new MaterialPropertyBlock();
            public readonly ShadowCastingMode ShadowCastingMode;
            public readonly bool ReceiveShadows;
            public readonly int Layer;

            public RenderGroup(in MonsterVATRenderProfile profile)
            {
                Asset = profile.Asset;
                Material = profile.Material;
                Clips = Asset.Clips;
                IdleClip = FindClip(Clips, "idle");
                WalkClip = FindClip(Clips, "walk");
                AttackClip = FindClip(Clips, "attack");
                CrossFadeDuration = Mathf.Max(0f, profile.CrossFadeDuration);
                MovementThreshold = profile.MovementThreshold;
                HitFlashDuration = profile.HitFlashDuration;
                RootScale = profile.RootScale;
                RendererLocalToRoot = profile.RendererLocalToRoot;
                ConfigureMaterial(profile);
                ShadowCastingMode = profile.ShadowCastingMode;
                ReceiveShadows = profile.ReceiveShadows;
                Layer = profile.Layer;
            }

            private void ConfigureMaterial(in MonsterVATRenderProfile profile)
            {
                Material sourceMaterial = profile.SourceMaterial;
                if (Material.GetTexture(MainTextureHash) == null && sourceMaterial.mainTexture != null)
                    Material.SetTexture(MainTextureHash, sourceMaterial.mainTexture);
                if (Material.HasProperty(BaseColorHash))
                {
                    if (sourceMaterial.HasProperty(BaseColorHash))
                        Material.SetColor(BaseColorHash, sourceMaterial.GetColor(BaseColorHash));
                    else if (sourceMaterial.HasProperty(ColorHash))
                        Material.SetColor(BaseColorHash, sourceMaterial.GetColor(ColorHash));
                }
                Material.SetTextureScale("_MainTex", sourceMaterial.mainTextureScale);
                Material.SetTextureOffset("_MainTex", sourceMaterial.mainTextureOffset);
                Asset.PositionTexture.filterMode = FilterMode.Point;
                Asset.PositionTexture.wrapMode = TextureWrapMode.Clamp;
                Asset.NormalTexture.filterMode = FilterMode.Point;
                Asset.NormalTexture.wrapMode = TextureWrapMode.Clamp;
                Material.SetTexture(PositionTextureHash, Asset.PositionTexture);
                Material.SetTexture(NormalTextureHash, Asset.NormalTexture);
                Material.SetFloat(VertexCountHash, Asset.VertexCount);
                Material.SetFloat(FrameCountHash, Asset.FrameCount);
                Material.SetColor(HitFlashColorHash, profile.HitFlashColor);
                Material.enableInstancing = true;
            }

            private static int FindClip(VATClipData[] clips, string token)
            {
                for (int i = 0; i < clips.Length; i++)
                    if (!string.IsNullOrEmpty(clips[i].Name)
                        && clips[i].Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                        return i;
                throw new InvalidOperationException($"VAT Asset 缺少 {token} 动画片段。");
            }
        }
    }
}
