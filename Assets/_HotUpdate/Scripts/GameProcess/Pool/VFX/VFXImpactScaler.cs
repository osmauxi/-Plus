using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// 根据特效权重调整粒子尺寸、喷射速度、数量与存活时间。
    ///
    /// LocalVFXPool 在播放时调用 ApplyWeight，
    /// 回收时调用 ResetToOriginal，防止状态污染下一次播放 
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class VFXImpactScaler : MonoBehaviour
    {
        [Header("Impact Scale")]
        [FormerlySerializedAs("visualMultiplier")]
        [Tooltip("表现夸张系数 最终表现权重 = 外部权重 × 该系数 ")]
        [SerializeField, Min(0f)] private float _visualMultiplier = 1f;

        [Tooltip("权重为 1 时，粒子基础尺寸相对预制体原值的倍率。")]
        [SerializeField, Min(0f)] private float _baseSizeMultiplier = 1.35f;

        [Tooltip("权重为 1 时，粒子基础喷射速度相对预制体原值的倍率。")]
        [SerializeField, Min(0f)] private float _baseSpeedMultiplier = 1.5f;

        [Tooltip("高权重缩短粒子寿命时允许的最低存活时间（秒）。")]
        [SerializeField, Min(0f)] private float _minimumLifetime = 0.5f;

        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _main;
        private ParticleSystem.EmissionModule _emission;

        private ParticleSystem.Burst[] _originalBursts = Array.Empty<ParticleSystem.Burst>();
        private ParticleSystem.Burst[] _workingBursts = Array.Empty<ParticleSystem.Burst>();

        private float _originalStartSizeMultiplier;
        private float _originalStartSpeedMultiplier;
        private float _originalStartLifetimeMultiplier;
        private int _originalMaxParticles;
        private float _originalRateOverTimeMultiplier;
        private float _originalRateOverDistanceMultiplier;
        private bool _isCached;

        private void Awake()
        {
            CacheOriginalState();
        }

        /// <summary>
        /// 根据外部权重调整粒子尺寸、速度、寿命、连续发射率与 Burst 数量。
        /// 尺寸、速度和数量不设上限；寿命不会超过预制体原值，并随高权重缩短到配置下限。
        /// 每次都基于预制体原始值计算，不会叠乘上一次结果 
        /// </summary>
        public void ApplyWeight(float baseWeight)
        {
            EnsureCached();

            float finalWeight = Mathf.Max(0f, baseWeight) * Mathf.Max(0f, _visualMultiplier);

            _main.startSizeMultiplier =
                _originalStartSizeMultiplier * _baseSizeMultiplier * finalWeight;
            _main.startSpeedMultiplier =
                _originalStartSpeedMultiplier * _baseSpeedMultiplier * finalWeight;
            _main.startLifetimeMultiplier = Mathf.Max(
                _minimumLifetime,
                _originalStartLifetimeMultiplier / Mathf.Max(1f, finalWeight));
            _main.maxParticles = Mathf.Max(1, Mathf.CeilToInt(_originalMaxParticles * finalWeight));
            _emission.rateOverTimeMultiplier = _originalRateOverTimeMultiplier * finalWeight;
            _emission.rateOverDistanceMultiplier = _originalRateOverDistanceMultiplier * finalWeight;

            for (int i = 0; i < _originalBursts.Length; i++)
            {
                ParticleSystem.Burst burst = _originalBursts[i];
                burst.count = ScaleCurve(burst.count, finalWeight);
                _workingBursts[i] = burst;
            }

            if (_workingBursts.Length > 0)
                _emission.SetBursts(_workingBursts);
        }

        /// <summary>
        /// 恢复预制体原始缩放和 Burst 配置 
        /// </summary>
        public void ResetToOriginal()
        {
            EnsureCached();

            _main.startSizeMultiplier = _originalStartSizeMultiplier;
            _main.startSpeedMultiplier = _originalStartSpeedMultiplier;
            _main.startLifetimeMultiplier = _originalStartLifetimeMultiplier;
            _main.maxParticles = _originalMaxParticles;
            _emission.rateOverTimeMultiplier = _originalRateOverTimeMultiplier;
            _emission.rateOverDistanceMultiplier = _originalRateOverDistanceMultiplier;

            if (_originalBursts.Length > 0)
                _emission.SetBursts(_originalBursts);
        }

        private void CacheOriginalState()
        {
            if (_isCached)
                return;

            _particleSystem = GetComponent<ParticleSystem>();
            _main = _particleSystem.main;
            _emission = _particleSystem.emission;
            _originalStartSizeMultiplier = _main.startSizeMultiplier;
            _originalStartSpeedMultiplier = _main.startSpeedMultiplier;
            _originalStartLifetimeMultiplier = _main.startLifetimeMultiplier;
            _originalMaxParticles = _main.maxParticles;
            _originalRateOverTimeMultiplier = _emission.rateOverTimeMultiplier;
            _originalRateOverDistanceMultiplier = _emission.rateOverDistanceMultiplier;

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

        private static ParticleSystem.MinMaxCurve ScaleCurve(
            ParticleSystem.MinMaxCurve original, float weight)
        {
            return original.mode switch
            {
                ParticleSystemCurveMode.Constant =>
                    new ParticleSystem.MinMaxCurve(original.constant * weight),
                ParticleSystemCurveMode.TwoConstants =>
                    new ParticleSystem.MinMaxCurve(
                        original.constantMin * weight,
                        original.constantMax * weight),
                ParticleSystemCurveMode.Curve =>
                    new ParticleSystem.MinMaxCurve(
                        original.curveMultiplier * weight,
                        original.curve),
                ParticleSystemCurveMode.TwoCurves =>
                    new ParticleSystem.MinMaxCurve(
                        original.curveMultiplier * weight,
                        original.curveMin,
                        original.curveMax),
                _ => original,
            };
        }

        private void EnsureCached()
        {
            if (!_isCached)
                CacheOriginalState();
        }
    }
}
