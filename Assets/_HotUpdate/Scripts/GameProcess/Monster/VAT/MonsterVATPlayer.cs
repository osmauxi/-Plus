using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>用 VAT 纹理驱动普通 MeshRenderer，支持循环播放与双动画交叉淡入淡出。</summary>
    public sealed class MonsterVATPlayer
    {
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

        private MonsterVATAsset _asset;
        private Material _material;
        private SkinnedMeshRenderer _sourceRenderer;
        private MeshRenderer _vatRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private VATClipData _idleClip;
        private VATClipData _walkClip;
        private VATClipData _attackClip;
        private VATClipData _sourceClip;
        private VATClipData _targetClip;
        private VATClipData _desiredLocomotionClip;
        private float _sourceTime;
        private float _targetTime;
        private float _transitionElapsed;
        private float _transitionDuration;
        private float _crossFadeDuration;
        private bool _sourceLoop;
        private bool _targetLoop;
        private bool _attackPlaying;
        private bool _initialized;

        public Renderer Renderer => _vatRenderer;
        public bool IsInitialized => _initialized;

        public bool Initialize(MonsterVATAsset asset, Material material, Transform searchRoot, float crossFadeDuration)
        {
            if (_initialized)
                return true;

            _asset = asset;
            _material = material;
            _sourceRenderer = searchRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);

            if (!TryFindClip(asset, "idle", out _idleClip)
                || !TryFindClip(asset, "walk", out _walkClip)
                || !TryFindClip(asset, "attack", out _attackClip))
            {
                Debug.LogError("VAT Asset 缺少 Idle、Walk 或 Attack 动画片段。");
                return false;
            }

            _crossFadeDuration = Mathf.Max(0f, crossFadeDuration);
            _propertyBlock = new MaterialPropertyBlock();
            ConfigureMaterial();
            MeshFilter meshFilter = _sourceRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = _sourceRenderer.gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = _asset.Mesh;
            _vatRenderer = _sourceRenderer.GetComponent<MeshRenderer>();
            if (_vatRenderer == null)
                _vatRenderer = _sourceRenderer.gameObject.AddComponent<MeshRenderer>();
            _vatRenderer.sharedMaterial = _material;
            _vatRenderer.shadowCastingMode = _sourceRenderer.shadowCastingMode;
            _vatRenderer.receiveShadows = _sourceRenderer.receiveShadows;
            _vatRenderer.lightProbeUsage = _sourceRenderer.lightProbeUsage;
            _vatRenderer.reflectionProbeUsage = _sourceRenderer.reflectionProbeUsage;
            _vatRenderer.allowOcclusionWhenDynamic = _sourceRenderer.allowOcclusionWhenDynamic;
            _vatRenderer.motionVectorGenerationMode = _sourceRenderer.motionVectorGenerationMode;
            _vatRenderer.enabled = true;
            _sourceRenderer.enabled = false;

            _desiredLocomotionClip = _idleClip;
            _sourceClip = _targetClip = _idleClip;
            _sourceTime = _targetTime = 0f;
            _sourceLoop = _targetLoop = true;
            _transitionElapsed = _transitionDuration = 0f;
            _attackPlaying = false;
            _initialized = true;
            ApplyFrameProperties();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized)
                return;
            deltaTime = Mathf.Max(0f, deltaTime);
            _sourceTime += deltaTime;
            _targetTime += deltaTime;
            if (_attackPlaying && _targetClip.Name == _attackClip.Name && _targetTime >= GetClipDuration(_attackClip))
            {
                _attackPlaying = false;
                BeginTransition(_desiredLocomotionClip, true, _crossFadeDuration);
            }
            if (_transitionDuration > 0f)
            {
                _transitionElapsed += deltaTime;
                if (_transitionElapsed >= _transitionDuration)
                    CompleteTransition();
            }
            ApplyFrameProperties();
        }

        public void SetLocomotion(bool moving)
        {
            if (!_initialized)
                return;
            _desiredLocomotionClip = moving ? _walkClip : _idleClip;
            if (_attackPlaying)
                return;
            if (_targetClip.Name == _desiredLocomotionClip.Name && _targetLoop)
                return;
            BeginTransition(_desiredLocomotionClip, true, _crossFadeDuration);
        }

        public void PlayAttack()
        {
            if (!_initialized)
                return;
            _attackPlaying = true;
            if (_targetClip.Name == _attackClip.Name)
            {
                _sourceClip = _targetClip = _attackClip;
                _sourceTime = _targetTime = 0f;
                _sourceLoop = _targetLoop = false;
                _transitionElapsed = _transitionDuration = 0f;
                ApplyFrameProperties();
                return;
            }
            BeginTransition(_attackClip, false, _crossFadeDuration);
        }

        public void ResetToIdle()
        {
            if (!_initialized)
                return;
            _desiredLocomotionClip = _idleClip;
            _sourceClip = _targetClip = _idleClip;
            _sourceTime = _targetTime = 0f;
            _sourceLoop = _targetLoop = true;
            _transitionElapsed = _transitionDuration = 0f;
            _attackPlaying = false;
            ApplyFrameProperties();
        }

        private void ConfigureMaterial()
        {
            Material sourceMaterial = _sourceRenderer.sharedMaterial;
            if (_material.GetTexture(MainTextureHash) == null && sourceMaterial != null && sourceMaterial.mainTexture != null)
                _material.SetTexture(MainTextureHash, sourceMaterial.mainTexture);
            if (sourceMaterial != null && _material.HasProperty(BaseColorHash))
            {
                if (sourceMaterial.HasProperty(BaseColorHash))
                    _material.SetColor(BaseColorHash, sourceMaterial.GetColor(BaseColorHash));
                else if (sourceMaterial.HasProperty(ColorHash))
                    _material.SetColor(BaseColorHash, sourceMaterial.GetColor(ColorHash));
            }
            if (sourceMaterial != null)
            {
                _material.SetTextureScale("_MainTex", sourceMaterial.mainTextureScale);
                _material.SetTextureOffset("_MainTex", sourceMaterial.mainTextureOffset);
            }
            _asset.PositionTexture.filterMode = FilterMode.Point;
            _asset.PositionTexture.wrapMode = TextureWrapMode.Clamp;
            _asset.NormalTexture.filterMode = FilterMode.Point;
            _asset.NormalTexture.wrapMode = TextureWrapMode.Clamp;
            _material.SetTexture(PositionTextureHash, _asset.PositionTexture);
            _material.SetTexture(NormalTextureHash, _asset.NormalTexture);
            _material.SetFloat(VertexCountHash, _asset.VertexCount);
            _material.SetFloat(FrameCountHash, _asset.FrameCount);
            _material.enableInstancing = true;
        }

        private void BeginTransition(VATClipData clip, bool loop, float duration)
        {
            _sourceClip = _targetClip;
            _sourceTime = _targetTime;
            _sourceLoop = _targetLoop;
            _targetClip = clip;
            _targetTime = 0f;
            _targetLoop = loop;
            _transitionElapsed = 0f;
            _transitionDuration = Mathf.Max(0f, duration);
            if (_transitionDuration <= 0f)
                CompleteTransition();
        }

        private void CompleteTransition()
        {
            _sourceClip = _targetClip;
            _sourceTime = _targetTime;
            _sourceLoop = _targetLoop;
            _transitionElapsed = _transitionDuration = 0f;
        }

        private void ApplyFrameProperties()
        {
            if (_vatRenderer == null || _propertyBlock == null)
                return;
            float currentFrame = GetFrame(_sourceClip, _sourceTime, _sourceLoop);
            float nextFrame = GetFrame(_targetClip, _targetTime, _targetLoop);
            float blend = _transitionDuration > 0f ? Mathf.Clamp01(_transitionElapsed / _transitionDuration) : 0f;
            _propertyBlock.Clear();
            _vatRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(CurrentFrameHash, currentFrame);
            _propertyBlock.SetFloat(NextFrameHash, nextFrame);
            _propertyBlock.SetFloat(BlendHash, blend);
            _vatRenderer.SetPropertyBlock(_propertyBlock);
        }

        private static float GetFrame(VATClipData clip, float time, bool loop)
        {
            float frame = time * clip.FrameRate;
            frame = loop ? Mathf.Repeat(frame, clip.FrameCount) : Mathf.Clamp(frame, 0f, clip.FrameCount - 1f);
            return clip.StartFrame + Mathf.Floor(frame);
        }

        private static float GetClipDuration(VATClipData clip) => clip.FrameCount / Mathf.Max(clip.FrameRate, 0.01f);

        private static bool TryFindClip(MonsterVATAsset asset, string token, out VATClipData result)
        {
            VATClipData[] clips = asset.Clips;
            if (clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (!string.IsNullOrEmpty(clips[i].Name) && clips[i].Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result = clips[i];
                        return true;
                    }
                }
            }
            result = default;
            return false;
        }
    }
}
