using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 玩家网络实例的运行时身份组件，告诉玩家“我是谁”
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerRuntime : NetworkBehaviour
    {
        public ulong ClientId => OwnerClientId;

        public bool IsLocalRuntimePlayer => NetworkManager != null && OwnerClientId == NetworkManager.LocalClientId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (PlayerManager.Instance == null || !PlayerManager.Instance.IsInitialized)
            {
                Debug.LogError($"[{nameof(PlayerRuntime)}] PlayerManager 尚未初始化，无法注册玩家：{OwnerClientId}");
                return;
            }

            PlayerManager.Instance.RegisterPlayer(this);
        }

        public override void OnNetworkDespawn()
        {
            if (PlayerManager.Instance != null)
                PlayerManager.Instance.UnregisterPlayer(this);

            base.OnNetworkDespawn();
        }
    }
}