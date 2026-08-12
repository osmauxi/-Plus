using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace ProjectGame.HotFix.Gameplay.Pooling
{
    /// <summary>
    /// 挂在特效预制体根节点，根据生命周期或播放状态自动归还 LocalVFXPool。
    ///
    /// 本脚本只负责判断“什么时候归还”；
    /// 粒子停止、Trail 清理和状态重置由 LocalVFXPool 统一完成。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VFXAutoReturn : MonoBehaviour, IPoolable
    {
        [FormerlySerializedAs("forceLifeTime")]
        [Tooltip("大于 0 时，播放指定秒数后强制回收；等于 0 时等待所有粒子自然结束。")]
        [SerializeField, Min(0f)] private float _forceLifetime;

        [Tooltip("自然结束检测的启动缓冲时间，避免粒子刚激活时尚未生成就被立即回收。")]
        [SerializeField, Min(0f)] private float _startupGraceTime = 0.1f;

        private ParticleSystem[] _particleSystems = Array.Empty<ParticleSystem>();
        private VisualEffect[] _visualEffects = Array.Empty<VisualEffect>();

        private float _elapsedTime;
        private bool _isRented;
        private bool _returnRequested;

        private void Awake()
        {
            //只在实例首次创建时扫描一次，池化复用时不再重复查找组件。
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            _visualEffects = GetComponentsInChildren<VisualEffect>(true);

            if (_particleSystems.Length == 0 && _visualEffects.Length == 0)
                Debug.LogWarning($"[{nameof(VFXAutoReturn)}] {name} 没有找到 ParticleSystem 或 VisualEffect。");

            //闲置对象不需要执行 Update，Rent 时再开启。
            enabled = false;
        }

        public void OnRentFromPool()
        {
            _elapsedTime = 0f;
            _returnRequested = false;
            _isRented = true;
            enabled = true;
        }

        public void OnReturnToPool()
        {
            _isRented = false;
            _returnRequested = false;
            _elapsedTime = 0f;
            enabled = false;
        }

        private void Update()
        {
            if (!_isRented || _returnRequested)
                return;

            _elapsedTime += Time.deltaTime;

            // 强制生命周期主要用于循环粒子，例如火焰、烟雾和持续光环。
            if (_forceLifetime > 0f)
            {
                if (_elapsedTime >= _forceLifetime)
                    RequestReturn();

                return;
            }

            // 给粒子和 VFX Graph 一段启动时间，防止 aliveCount 尚未更新。
            if (_elapsedTime < _startupGraceTime)
                return;

            if (!IsAnyEffectAlive())
                RequestReturn();
        }

        private bool IsAnyEffectAlive()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] != null && _particleSystems[i].IsAlive(true))
                    return true;
            }

            for (int i = 0; i < _visualEffects.Length; i++)
            {
                if (_visualEffects[i] != null && _visualEffects[i].aliveParticleCount > 0)
                    return true;
            }

            return false;
        }

        private void RequestReturn()
        {
            if (_returnRequested)
                return;

            _returnRequested = true;
            enabled = false;

            LocalVFXPool pool = LocalVFXPool.Instance;

            if (pool != null && pool.IsInitialized)
            {
                pool.Return(gameObject);
                return;
            }

            //正常运行中不应该进入这里，只作为对象池已销毁时的最终兜底。
            Debug.LogWarning($"[{nameof(VFXAutoReturn)}] LocalVFXPool 不可用，直接关闭特效：{name}");
            gameObject.SetActive(false);
        }
    }
}