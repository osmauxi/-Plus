using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Spawning;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// Gameplay 玩家 NetworkObject 的生成控制器 
    ///
    /// 所有 Peer：
    /// 提前 Prepare Player NetworkPrefab 
    ///
    /// Server：
    /// 根据 GameSessionContext 生成玩家并分配 Ownership 
    /// </summary>
    public sealed class PlayerSpawnController : MonoBehaviour, IGameRuntimeService
    {
        [SerializeField] private string _playerPoolId = "Player_Runtime";

        private readonly List<NetworkObject> _despawnBuffer = new();

        private NetworkManager _networkManager;

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            cancellationToken.ThrowIfCancellationRequested();

            _networkManager = NetworkManager.Singleton;

            // 不是只有 Server Prepare 
            // 每个 Peer 都必须认识这个动态 NetworkPrefab，
            // 否则 Server Spawn 后 Client 无法通过 PrefabHandler 创建对应实例 
            await Pooling.SyncObjectPool.Instance.PreparePoolAsync(_playerPoolId, cancellationToken);

            IsInitialized = true;

            Debug.Log($"[{nameof(PlayerSpawnController)}] 初始化完成：Pool={_playerPoolId}");
        }

        /// <summary>
        /// 本局首次生成玩家 
        /// 只允许 Server 调用 
        /// </summary>
        public async UniTask SpawnInitialPlayersAsync(IReadOnlyList<SpawnPose> spawnPoints, CancellationToken cancellationToken)
        {
            IReadOnlyList<PlayerSessionData> sessionPlayers = GameSessionContext.Players;

            for (int i = 0; i < sessionPlayers.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PlayerSessionData sessionData = sessionPlayers[i];
                SpawnPose spawnPoint = spawnPoints[i];

                Pooling.SyncObjectPool.Instance.SpawnWithOwnership(
                    _playerPoolId,
                    sessionData.ClientId,
                    spawnPoint.Position,
                    spawnPoint.Rotation);
            }

            // Server 本机确认所有 PlayerRuntime 已完成 OnNetworkSpawn 注册 
            await PlayerManager.Instance.WaitUntilAllPlayersRegisteredAsync(cancellationToken);

            Debug.Log($"[{nameof(PlayerSpawnController)}] 玩家生成完成：Count={PlayerManager.Instance.SpawnedPlayerCount}");
        }

        /// <summary>
        /// 普通换层时把已经存在的 PlayerRuntime 移动到新起点 
        /// 不重新 Spawn，也不重新加载角色和武器 
        /// </summary>
        public void RepositionPlayers(IReadOnlyList<SpawnPose> spawnPoints)
        {
            IReadOnlyList<PlayerSessionData> sessionPlayers = GameSessionContext.Players;

            for (int i = 0; i < sessionPlayers.Count; i++)
            {
                PlayerSessionData sessionData = sessionPlayers[i];
                if (!PlayerManager.Instance.TryGetRuntimePlayer(sessionData.ClientId, out PlayerRuntime player))
                    continue;

                SpawnPose spawnPoint = spawnPoints[i];
                player.GetComponent<PlayerSyncController>().ResetAfterWarp(spawnPoint.Position, spawnPoint.Rotation);
            }

            Debug.Log($"[{nameof(PlayerSpawnController)}] 已重新布置玩家：Count={sessionPlayers.Count}");
        }

        /// <summary>服务端移动单个现有玩家，并重置该玩家的预测、插值和输入基线。</summary>
        public void WarpPlayer(PlayerRuntime player, in SpawnPose destination)
        {
            player.GetComponent<PlayerSyncController>().ResetAfterWarp(destination.Position, destination.Rotation);
        }

        /// <summary>
        /// Gameplay 最终退出时统一回收玩家 
        /// 普通楼层切换不要调用 
        /// </summary>
        public void DespawnAllPlayers()
        {
            if (!IsInitialized || _networkManager == null || !_networkManager.IsServer)
                return;

            _despawnBuffer.Clear();

            IReadOnlyList<PlayerRuntime> players = PlayerManager.Instance.RuntimePlayers;

            // Despawn 会触发 UnregisterPlayer，
            // 所以必须先复制 NetworkObject，不能直接边遍历 RuntimePlayers 边删除 
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];

                if (player.IsSpawned)
                    _despawnBuffer.Add(player.NetworkObject);
            }

            for (int i = 0; i < _despawnBuffer.Count; i++)
                Pooling.SyncObjectPool.Instance.DespawnAndReturn(_despawnBuffer[i]);

            _despawnBuffer.Clear();
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            if (_networkManager != null && _networkManager.IsServer)
                DespawnAllPlayers();

            _despawnBuffer.Clear();
            _networkManager = null;
            IsInitialized = false;

            Debug.Log($"[{nameof(PlayerSpawnController)}] 已关闭 ");

            return UniTask.CompletedTask;
        }
    }
}
