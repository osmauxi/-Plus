using UnityEngine;
using UnityEngine.VFX;

namespace ProjectGame.HotFix.Character
{
    public class WeaponView : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;
        [Tooltip("右手主握点 为空时使用武器根节点，便于旧武器平滑迁移 ")]
        [SerializeField] private Transform _mainHandGrip;
        [SerializeField] private Transform _offHandGrip;
        [Header("Weapon VFX Anchors")]
        [SerializeField] private Transform _shellEjectionPoint;
        [SerializeField] private Transform _reloadVfxPoint;
        [Header("Embedded Weapon VFX")]
        [SerializeField] private VisualEffect _muzzleFlashVfx;
        [SerializeField] private ParticleSystem _shellEjectionVfx;
        [SerializeField] private ParticleSystem _reloadVfx;

        private static readonly int OnFireEventId = Shader.PropertyToID("VFX_OnFIre");

        public Transform Muzzle => _muzzle;
        public Transform MainHandGrip => _mainHandGrip != null ? _mainHandGrip : transform;
        public Transform OffHandGrip => _offHandGrip;
        public Transform ShellEjectionPoint => _shellEjectionPoint;
        public Transform ReloadVfxPoint => _reloadVfxPoint;

        private void Awake()
        {
            CacheEmbeddedVfx();
            StopAllWeaponVFX();
        }

        public void PlayFireVFX()
        {
            CacheEmbeddedVfx();
            if (_muzzleFlashVfx == null)
                return;

            _muzzleFlashVfx.Reinit();
            _muzzleFlashVfx.SendEvent(OnFireEventId);
        }

        public void PlayShellEjectionVFX()
        {
            CacheEmbeddedVfx();
            EmitParticle(_shellEjectionVfx);
        }

        public void PlayReloadVFX()
        {
            CacheEmbeddedVfx();
            RestartParticle(_reloadVfx);
        }

        public void StopReloadVFX()
        {
            StopParticle(_reloadVfx);
        }

        public void StopAllWeaponVFX()
        {
            if (_muzzleFlashVfx != null)
                _muzzleFlashVfx.Stop();
            StopParticle(_shellEjectionVfx);
            StopParticle(_reloadVfx);
        }

        private void CacheEmbeddedVfx()
        {
            if (_muzzleFlashVfx == null && _muzzle != null)
                _muzzleFlashVfx = _muzzle.GetComponentInChildren<VisualEffect>(true);
            if (_shellEjectionVfx == null && _shellEjectionPoint != null)
                _shellEjectionVfx = _shellEjectionPoint.GetComponentInChildren<ParticleSystem>(true);
            if (_reloadVfx == null && _reloadVfxPoint != null)
                _reloadVfx = _reloadVfxPoint.GetComponentInChildren<ParticleSystem>(true);
        }

        private static void RestartParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }

        /// <summary>
        /// 每枪只追加一枚弹壳，不重启粒子系统，避免清掉仍在飞行或弹跳的旧弹壳。
        /// 关闭时间轴自动发射并保持系统生命周期，由枪击事件显式追加粒子。
        /// </summary>
        private static void EmitParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;

            ParticleSystem.MainModule main = particle.main;
            main.loop = true;
            ParticleSystem.EmissionModule emission = particle.emission;
            emission.enabled = false;
            if (!particle.isPlaying)
                particle.Play(true);
            particle.Emit(1);
        }

        private static void StopParticle(ParticleSystem particle)
        {
            if (particle != null)
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
