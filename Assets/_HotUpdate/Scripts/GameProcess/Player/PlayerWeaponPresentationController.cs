using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Gameplay.Pooling;
using ProjectGame.HotFix.Gameplay.Weapon;
using ProjectGame.HotFix.Gameplay.Weapon.Presentation;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>把玩家的预测/同步武器状态与当前 WeaponView 的本地特效连接起来。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerWeaponController))]
    public sealed class PlayerWeaponPresentationController : MonoBehaviour, IPoolable
    {
        private PlayerWeaponController _weaponController;
        private WeaponPresentationService _service;
        private WeaponView _weaponView;
        private ulong _entityId;
        private uint _lastObservedShotSequence;
        private uint _lastPlayedShotSequence;
        private bool _wasReloading;
        private bool _bound;

        private void Awake()
        {
            _weaponController = GetComponent<PlayerWeaponController>();
        }

        public void Bind(ulong entityId, WeaponView weaponView, WeaponPresentationService service)
        {
            Unbind();
            if (weaponView == null || service == null || !service.IsInitialized)
                throw new System.InvalidOperationException(
                    "WeaponView 或 WeaponPresentationService 尚未就绪。");

            _entityId = entityId;
            _weaponView = weaponView;
            _service = service;
            WeaponRuntimeState state = _weaponController.CurrentWeaponState;
            _lastObservedShotSequence = state.ShotSequence;
            _lastPlayedShotSequence = state.ShotSequence;
            _wasReloading = state.IsReloading;
            _weaponController.WeaponStateChanged += HandleWeaponStateChanged;
            _service.RegisterWeaponView(_entityId, this);
            _bound = true;
        }

        /// <summary>由权威 Shot 广播调用；与预测/快照里的 ShotSequence 使用同一去重键。</summary>
        public void PlayAuthoritativeShot(uint shotSequence)
        {
            PlayShotOnce(shotSequence);
        }

        private void HandleWeaponStateChanged(WeaponRuntimeState state)
        {
            if (!_bound)
                return;

            if (state.ShotSequence != _lastObservedShotSequence)
            {
                _lastObservedShotSequence = state.ShotSequence;
                PlayShotOnce(state.ShotSequence);
            }

            if (state.IsReloading != _wasReloading)
            {
                _wasReloading = state.IsReloading;
                if (_wasReloading) _weaponView.PlayReloadVFX();
                else _weaponView.StopReloadVFX();
            }
        }

        private void PlayShotOnce(uint shotSequence)
        {
            if (!_bound || shotSequence == _lastPlayedShotSequence)
                return;

            _lastPlayedShotSequence = shotSequence;
            _weaponView.PlayFireVFX();
            _weaponView.PlayShellEjectionVFX();
        }

        public void Unbind()
        {
            if (_weaponController != null)
                _weaponController.WeaponStateChanged -= HandleWeaponStateChanged;
            if (_service != null)
                _service.UnregisterWeaponView(_entityId, this);
            _weaponView?.StopAllWeaponVFX();

            _service = null;
            _weaponView = null;
            _entityId = 0;
            _lastObservedShotSequence = 0;
            _lastPlayedShotSequence = 0;
            _wasReloading = false;
            _bound = false;
        }

        public void OnRentFromPool() => Unbind();
        public void OnReturnToPool() => Unbind();
        private void OnDestroy() => Unbind();
    }
}
