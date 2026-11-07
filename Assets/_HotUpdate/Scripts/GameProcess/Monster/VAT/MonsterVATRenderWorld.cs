using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间级 VAT 状态与批量绘制；所有实例按稳定 Monster Slot 索引。
    /// </summary>
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
        //怪物统一绘制的地面高度
        private readonly float _groundY;
        //所有不同的渲染组
        private readonly List<RenderGroup> _groups = new List<RenderGroup>();
        //每个怪物 Slot 对应的动画状态
        private InstanceState[] _states = Array.Empty<InstanceState>();
        private int _slotCount;

        public int SlotCount => _slotCount;
        public int ActiveCount { get; private set; }

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
            ActiveCount++;
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
            //当前本身就在攻击，则重新触发攻击动画，从第一帧重新开始
            if (state.TargetClip == group.AttackClip)
            {
                state.SourceClip = state.TargetClip = group.AttackClip;
                state.SourceTime = state.TargetTime = 0f;
                state.SourceLoop = state.TargetLoop = false;
                state.TransitionElapsed = state.TransitionDuration = 0f;
                return;
            }
            //否则开始过渡到攻击动画
            BeginTransition(ref state, group.AttackClip, false, group.CrossFadeDuration);
        }

        public void PlayHit(int slot)
        {
            ref InstanceState state = ref _states[slot];
            state.HitFlashRemaining = _groups[state.GroupIndex].HitFlashDuration;
        }

        public void Despawn(int slot)
        {
            if (!_states[slot].Active)
                return;
            _states[slot].Active = false;
            ActiveCount--;
        }

        public void Clear()
        {
            _slotCount = 0;
            ActiveCount = 0;
            for (int i = 0; i < _groups.Count; i++)
                _groups[i].Dispose();
            _groups.Clear();
            Array.Clear(_states, 0, _states.Length);
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
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
        }

        public void Render()
        {
            using (DrawMarker.Auto())
            {
                for (int i = 0; i < _groups.Count; i++)
                    Draw(_groups[i]);
            }
        }

        public void Dispose()
        {
            Clear();
        }
        /// <summary>
        /// 尝试获取或创建一个渲染组，如果已经存在相同的VAT资源和材质，则返回其索引
        /// 否则创建一个新的渲染组并返回其索引
        /// </summary>
        private int GetOrCreateGroup(in MonsterVATRenderProfile profile)
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                RenderGroup group = _groups[i];
                if (group.Matches(profile))
                    return i;
            }

            _groups.Add(new RenderGroup(profile));
            return _groups.Count - 1;
        }
        /// <summary>
        /// 实际的绘制逻辑，将一批怪物实例的矩阵和动画状态上传到GPU，并使用Graphics.DrawMeshInstanced进行批量绘制
        /// Tick，SetLocomotion，BeginTransition，CompleteTransition，EvaluateFrameState，GetFrame，GetClipDuration等方法用于更新每个实例的动画状态和过渡状态
        /// 他们实际上只是修改了InstanceState结构体中的字段，并没有直接影响渲染
        /// </summary>
        private void Draw(RenderGroup group)
        {
            int drawCount = 0;
            for (int i = 0; i < group.Slots.Count; i++)
            {
                ref InstanceState state = ref _states[group.Slots[i]];
                if (!state.Active)
                    continue;
                //获得当前实例应该绘制的动画帧
                VATFrameState frameState = EvaluateFrameState(state, group);
                //计算实例的世界矩阵，这里使用Matrix4x4.TRS来构建矩阵，位置在地面高度上，旋转根据Yaw，缩放使用RenderGroup的RootScale
                group.Matrices[drawCount] = Matrix4x4.TRS(
                    new Vector3(state.Position.x, _groundY, state.Position.y),
                    Quaternion.Euler(0f, state.Yaw, 0f),group.RootScale) * group.RendererLocalToRoot;
                group.CurrentFrames[drawCount] = frameState.CurrentFrame;
                group.NextFrames[drawCount] = frameState.NextFrame;
                group.Blends[drawCount] = frameState.Blend;
                group.HitFlashAmounts[drawCount] = state.HitFlashRemaining > 0f ? 1f : 0f;
                drawCount++;
                //达到设定最大合批数量就先行提交一次绘制，然后归零
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
            //使用MaterialPropertyBlock上传每个实例的动画状态和闪烁状态到GPU，
            //这样每个实例可以使用不同的动画帧和混合值
            MaterialPropertyBlock properties = group.Properties;
            using (UploadMarker.Auto())
            {
                properties.Clear();
                properties.SetFloatArray(CurrentFrameHash, group.CurrentFrames);
                properties.SetFloatArray(NextFrameHash, group.NextFrames);
                properties.SetFloatArray(BlendHash, group.Blends);
                properties.SetFloatArray(HitFlashAmountHash, group.HitFlashAmounts);
            }
            //执行批量绘制，将同一组的怪物实例一次性绘制到屏幕上
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
            //判定攻击动画结束
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
        /// <summary>
        /// 根据移动状态设置怪物实例的期望动画片段，如果当前动画片段不是期望的动画片段，则开始过渡到期望的动画片段
        /// </summary>
        private static void SetLocomotion(ref InstanceState state, RenderGroup group, bool moving)
        {
            state.DesiredLocomotionClip = moving ? group.WalkClip : group.IdleClip;
            //正在攻击，或者当前动画片段已经是期望的动画片段且循环播放，则不需要过渡
            if (state.AttackPlaying || state.TargetClip == state.DesiredLocomotionClip && state.TargetLoop)
                return;
            BeginTransition(ref state, state.DesiredLocomotionClip, true, group.CrossFadeDuration);
        }
        /// <summary>
        /// 动画过渡开始，设置源动画片段和目标动画片段，并初始化过渡时间
        /// </summary>
        private static void BeginTransition(ref InstanceState state, int clip, bool loop, float duration)
        {
            //先将可能的源动画片段设置为当前目标动画片段，然后将目标动画片段设置为新的动画片段
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
        /// <summary>
        /// 过度完成，将源动画片段设置为目标动画片段，并重置过渡时间
        /// </summary>
        private static void CompleteTransition(ref InstanceState state)
        {
            state.SourceClip = state.TargetClip;
            state.SourceTime = state.TargetTime;
            state.SourceLoop = state.TargetLoop;
            state.TransitionDuration = 0f;
            state.TransitionElapsed = 0f;
        }

        private static VATFrameState EvaluateFrameState(in InstanceState state, RenderGroup group)
        {
            //SourceClip的当前帧
            float currentFrame = GetFrame(group.Clips[state.SourceClip], state.SourceTime, state.SourceLoop);
            //TargetClip的当前帧
            float nextFrame = GetFrame(group.Clips[state.TargetClip], state.TargetTime, state.TargetLoop);
            //计算过渡混合值，范围在0到1之间
            float blend = state.TransitionDuration > 0f
                ? Mathf.Clamp01(state.TransitionElapsed / state.TransitionDuration)
                : 0f;
            return new VATFrameState(currentFrame, nextFrame, blend);
        }
        /// <summary>
        /// 根据动画时间找到对应的帧索引，如果动画循环播放，则使用Mathf.Repeat，否则使用Mathf.Clamp限制在有效帧范围内
        /// </summary>
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
        /// <summary>
        /// 描述一个slot怪物实例的状态，包括位置、朝向、动画状态等信息
        /// </summary>
        private struct InstanceState
        {
            public bool Active;
            //判断是否有上一次的Pose数据，用于计算移动状态
            public bool HasPreviousPose;
            //当前动画片段是否循环播放
            public bool SourceLoop;
            //目标动画片段是否循环播放，用于在动画过渡时判断是否需要循环播放目标动画片段
            public bool TargetLoop;
            public bool AttackPlaying;
            //表示Slot使用哪一个RenderGroup，RenderGroup中包含了怪物的VAT资源和材质等信息
            public int GroupIndex;
            //过渡开始时的源动画
            public int SourceClip;
            //正在过渡到的目标动画
            public int TargetClip;
            //表示当前怪物实例希望播放的动画片段索引，通常是Idle或Walk动画片段
            public int DesiredLocomotionClip;

            public Vector2 Position;
            public Vector2 PreviousPosition;
            public float Yaw;
            //SourceLoop已经播放的时间
            public float SourceTime;
            public float TargetTime;
            //动画过渡已经过的时间
            public float TransitionElapsed;
            //动画过渡的总时间
            public float TransitionDuration;
            public float HitFlashRemaining;
        }
        /// <summary>
        /// 一批使用同一套材质和VAT资源的怪物实例的渲染组
        /// </summary>
        private sealed class RenderGroup
        {
            // Asset 是项目资源；Material 是当前 RenderGroup 独占的运行时材质实例。
            public readonly MonsterVATAsset Asset;
            public readonly Material Material;
            private readonly Material _materialAsset;
            public readonly VATClipData[] Clips;
            //动画信息
            public readonly int IdleClip;
            public readonly int WalkClip;
            public readonly int AttackClip;

            public readonly float CrossFadeDuration;
            public readonly float MovementThreshold;
            public readonly float HitFlashDuration;
            public readonly Vector3 RootScale;
            public readonly Matrix4x4 RendererLocalToRoot;
            public readonly Material SourceMaterial;
            public readonly Color HitFlashColor;

            public readonly List<int> Slots = new List<int>();
            public readonly Matrix4x4[] Matrices = new Matrix4x4[DrawBatchCapacity];
            public readonly float[] CurrentFrames = new float[DrawBatchCapacity];
            public readonly float[] NextFrames = new float[DrawBatchCapacity];
            public readonly float[] Blends = new float[DrawBatchCapacity];
            public readonly float[] HitFlashAmounts = new float[DrawBatchCapacity];
            //材质属性块，用于在批量绘制时传递每个实例的动画状态和闪烁状态
            //Material是一组怪物的共享资源，而MaterialPropertyBlock代表每个怪物的独立参数状态
            public readonly MaterialPropertyBlock Properties = new MaterialPropertyBlock();
            public readonly ShadowCastingMode ShadowCastingMode;
            public readonly bool ReceiveShadows;
            public readonly int Layer;

            public RenderGroup(in MonsterVATRenderProfile profile)
            {
                Asset = profile.Asset;
                //保留对原始材质的引用
                _materialAsset = profile.Material;
                //复制材质实例，并在实例上修改纹理和颜色属性，避免修改原始材质
                Material = new Material(profile.Material)
                {
                    name = $"{profile.Material.name} (Monster VAT Runtime)"
                };
                Clips = Asset.Clips;
                IdleClip = FindClip(Clips, "idle");
                WalkClip = FindClip(Clips, "walk");
                AttackClip = FindClip(Clips, "attack");
                CrossFadeDuration = Mathf.Max(0f, profile.CrossFadeDuration);
                MovementThreshold = profile.MovementThreshold;
                HitFlashDuration = profile.HitFlashDuration;
                RootScale = profile.RootScale;
                SourceMaterial = profile.SourceMaterial;
                HitFlashColor = profile.HitFlashColor;
                RendererLocalToRoot = profile.RendererLocalToRoot;
                ConfigureMaterial(profile);
                ShadowCastingMode = profile.ShadowCastingMode;
                ReceiveShadows = profile.ReceiveShadows;
                Layer = profile.Layer;
            }

            private void ConfigureMaterial(in MonsterVATRenderProfile profile)
            {
                Material sourceMaterial = profile.SourceMaterial;
                //尝试把怪物材质的主纹理赋值给运行时材质实例，如果运行时材质实例没有主纹理，则使用源材质的主纹理
                Material.SetTexture(MainTextureHash, sourceMaterial.mainTexture);
                //复制颜色
                if (Material.HasProperty(BaseColorHash))
                {
                    if (sourceMaterial.HasProperty(BaseColorHash))
                        Material.SetColor(BaseColorHash, sourceMaterial.GetColor(BaseColorHash));
                    else if (sourceMaterial.HasProperty(ColorHash))
                        Material.SetColor(BaseColorHash, sourceMaterial.GetColor(ColorHash));
                }
                //复制纹理缩放和偏移
                Material.SetTextureScale("_MainTex", sourceMaterial.mainTextureScale);
                Material.SetTextureOffset("_MainTex", sourceMaterial.mainTextureOffset);
                //使用点采样，不对附近像素进行普通双线性混合
                Asset.PositionTexture.filterMode = FilterMode.Point;
                Asset.NormalTexture.filterMode = FilterMode.Point;
                //超出纹理边界时，采样边缘数据
                Asset.PositionTexture.wrapMode = TextureWrapMode.Clamp;
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

            public void Dispose()
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(Material);
                else
                    UnityEngine.Object.DestroyImmediate(Material);
            }

            public bool Matches(in MonsterVATRenderProfile profile)
            {
                return Asset == profile.Asset
                    && _materialAsset == profile.Material
                    && SourceMaterial == profile.SourceMaterial
                    && CrossFadeDuration == Mathf.Max(0f, profile.CrossFadeDuration)
                    && MovementThreshold == profile.MovementThreshold
                    && HitFlashDuration == profile.HitFlashDuration
                    && HitFlashColor == profile.HitFlashColor
                    && RootScale == profile.RootScale
                    && RendererLocalToRoot.Equals(profile.RendererLocalToRoot)
                    && ShadowCastingMode == profile.ShadowCastingMode
                    && ReceiveShadows == profile.ReceiveShadows
                    && Layer == profile.Layer;
            }
        }
    }
}
