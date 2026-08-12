using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// 根据特效权重调整粒子范围和 Burst 数量。
    ///
    /// LocalVFXPool 在播放时调用 ApplyWeight，
    /// 回收时调用 ResetToOriginal，防止状态污染下一次播放。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class VFXImpactScaler : MonoBehaviour
    {
        [Header("Impact Scale")]
        [FormerlySerializedAs("visualMultiplier")]
        [Tooltip("表现夸张系数。最终表现权重 = 外部权重 × 该系数。")]
        [SerializeField, Min(0f)] private float _visualMultiplier = 1f;

        [Tooltip("单个 Burst 允许的最大粒子数量，防止权重过大造成瞬时卡顿。")]
        [SerializeField, Min(1)] private int _maxBurstCount = 500;

        private ParticleSystem _particleSystem;
        private ParticleSystem.EmissionModule _emission;

        private ParticleSystem.Burst[] _originalBursts = Array.Empty<ParticleSystem.Burst>();
        private ParticleSystem.Burst[] _workingBursts = Array.Empty<ParticleSystem.Burst>();

        private Vector3 _originalScale;
        private bool _isCached;

        private void Awake()
        {
            CacheOriginalState();
        }

        /// <summary>
        /// 根据外部权重调整整体缩放和 Burst 数量。
        /// 每次都基于预制体原始值计算，不会叠乘上一次结果。
        /// </summary>
        public void ApplyWeight(float baseWeight)
        {
            EnsureCached();

            float finalWeight = Mathf.Max(0f, baseWeight) * Mathf.Max(0f, _visualMultiplier);

            transform.localScale = _originalScale * finalWeight;

            for (int i = 0; i < _originalBursts.Length; i++)
            {
                ParticleSystem.Burst burst = _originalBursts[i];
                burst.count = ScaleBurstCount(burst.count, finalWeight);
                _workingBursts[i] = burst;
            }

            if (_workingBursts.Length > 0)
                _emission.SetBursts(_workingBursts);
        }

        /// <summary>
        /// 恢复预制体原始缩放和 Burst 配置。
        /// </summary>
        public void ResetToOriginal()
        {
            EnsureCached();

            transform.localScale = _originalScale;

            if (_originalBursts.Length > 0)
                _emission.SetBursts(_originalBursts);
        }

        private void CacheOriginalState()
        {
            if (_isCached)
                return;

            _particleSystem = GetComponent<ParticleSystem>();
            _emission = _particleSystem.emission;
            _originalScale = transform.localScale;

            int burstCount = _emission.burstCount;

            if (burstCount > 0)
            {
                _originalBursts = new ParticleSystem.Burst[burstCount];
                _workingBursts = new ParticleSystem.Burst[burstCount];
                _emission.GetBursts(_originalBursts);

                Array.Copy(_originalBursts, _workingBursts, burstCount);
            }

            _isCached = true;
        }

        private ParticleSystem.MinMaxCurve ScaleBurstCount(ParticleSystem.MinMaxCurve originalCount, float weight)
        {
            float minimum = Mathf.Clamp(originalCount.constantMin * weight, 1f, _maxBurstCount);
            float maximum = Mathf.Clamp(originalCount.constantMax * weight, 1f, _maxBurstCount);

            // Burst 通常使用 Constant 或 TwoConstants，其他模式统一按最大值处理。
            if (originalCount.mode == ParticleSystemCurveMode.TwoConstants)
                return new ParticleSystem.MinMaxCurve(minimum, maximum);

            return new ParticleSystem.MinMaxCurve(maximum);
        }

        private void EnsureCached()
        {
            if (!_isCached)
                CacheOriginalState();
        }
    }
}