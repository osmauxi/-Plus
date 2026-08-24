using ProjectGame.HotFix.Gameplay.Player.State;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectGame.HotFix.Gameplay.Player.Animation
{
    /// <summary>
    /// Development 环境下的玩家动画链路测试入口。
    ///
    /// 只允许 Host 的本地 Owner 执行，调用正式 PlayerSyncController 状态接口，
    /// 因此测试仍会经过 SimulationState、Snapshot 和 PlayerAnimationDriver，
    /// 不直接篡改 Animator 参数。Client 权威测试留给未来正式伤害请求链路。
    ///
    /// H：受击。K：死亡/恢复。Fire、Aim、Reload 继续使用正式 Input Actions。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    public sealed class PlayerAnimationDebugInput : MonoBehaviour
    {
        [Tooltip("关闭后即使在 Editor/Development Build 中也不响应测试按键。")]
        [SerializeField] private bool _enableDebugInput = true;

        private PlayerSyncController _syncController;

        private void Awake()
        {
            _syncController = GetComponent<PlayerSyncController>();
        }

        private void Update()
        {
            if (!_enableDebugInput || (!Application.isEditor && !Debug.isDebugBuild))
                return;

            if (_syncController == null || !_syncController.IsSpawned ||
                !_syncController.IsServer || !_syncController.IsOwner)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.hKey.wasPressedThisFrame)
                _syncController.ApplyHit();

            if (keyboard.kKey.wasPressedThisFrame)
            {
                PlayerLifeState targetState = _syncController.ControlState.IsDead
                    ? PlayerLifeState.Alive
                    : PlayerLifeState.Dead;
                _syncController.SetLifeState(targetState);
            }
        }
    }
}
