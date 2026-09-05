using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Pooling;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Runtime;
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

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未启动，无法初始化 PlayerSpawnController ");

            if (!GameSessionContext.IsConfigured)
                throw new InvalidOperationException("GameSessionContext 尚未配置 ");

            if (PlayerManager.Instance == null || !PlayerManager.Instance.IsInitialized)
                throw new InvalidOperationException("PlayerManager 尚未初始化 ");

            if (Pooling.SyncObjectPool.Instance == null || !Pooling.SyncObjectPool.Instance.IsInitialized)
                throw new InvalidOperationException("SyncObjectPool 尚未初始化 ");

            if (!Pooling.SyncObjectPool.Instance.ContainsPool(_playerPoolId))
                throw new InvalidOperationException($"不存在玩家网络对象池：{_playerPoolId}");

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
        public async UniTask SpawnInitialPlayersAsync(IReadOnlyList<Transform> spawnPoints, CancellationToken cancellationToken)
        {
            EnsureInitialized();
            EnsureServer();

            if (spawnPoints == null)
                throw new ArgumentNullException(nameof(spawnPoints));

            IReadOnlyList<PlayerSessionData> sessionPlayers = GameSessionContext.Players;

            if (spawnPoints.Count < sessionPlayers.Count)
                throw new InvalidOperationException($"出生点不足：SpawnPoints={spawnPoints.Count}，Players={sessionPlayers.Count}");

            if (PlayerManager.Instance.SpawnedPlayerCount > 0)
                throw new InvalidOperationException("当前已经存在玩家，不能重复执行首次 Spawn ");

            for (int i = 0; i < sessionPlayers.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PlayerSessionData sessionData = sessionPlayers[i];

                if (!IsClientConnected(sessionData.ClientId))
                    throw new InvalidOperationException($"玩家已经不在连接列表中：ClientId={sessionData.ClientId}");

                Transform spawnPoint = spawnPoints[i];

                if (spawnPoint == null)
                    throw new InvalidOperationException($"玩家出生点为空：Index={i}");

                NetworkObject playerObject = Pooling.SyncObjectPool.Instance.SpawnWithOwnership(
                    _playerPoolId,
                    sessionData.ClientId,
                    spawnPoint.position,
                    spawnPoint.rotation);

                if (playerObject == null)
                    throw new InvalidOperationException($"玩家 NetworkObject 生成失败：ClientId={sessionData.ClientId}");
            }

            // Server 本机确认所有 PlayerRuntime 已完成 OnNetworkSpawn 注册 
            await PlayerManager.Instance.WaitUntilAllPlayersRegisteredAsync(cancellationToken);

            Debug.Log($"[{nameof(PlayerSpawnController)}] 玩家生成完成：Count={PlayerManager.Instance.SpawnedPlayerCount}");
        }

        /// <summary>
        /// 普通换层时把已经存在的 PlayerRuntime 移动到新起点 
        /// 不重新 Spawn，也不重新加载角色和武器 
        /// </summary>
        public void RepositionPlayers(IReadOnlyList<Transform> spawnPoints)
        {
            EnsureInitialized();
            EnsureServer();

            if (spawnPoints == null)
                throw new ArgumentNullException(nameof(spawnPoints));

            IReadOnlyList<PlayerSessionData> sessionPlayers = GameSessionContext.Players;

            if (spawnPoints.Count < sessionPlayers.Count)
                throw new InvalidOperationException($"出生点不足：SpawnPoints={spawnPoints.Count}，Players={sessionPlayers.Count}");

            for (int i = 0; i < sessionPlayers.Count; i++)
            {
                PlayerSessionData sessionData = sessionPlayers[i];

                if (!PlayerManager.Instance.TryGetRuntimePlayer(sessionData.ClientId, out PlayerRuntime player))
                    throw new InvalidOperationException($"找不到玩家运行时对象：ClientId={sessionData.ClientId}");

                Transform spawnPoint = spawnPoints[i];

                if (spawnPoint == null)
                    throw new InvalidOperationException($"玩家出生点为空：Index={i}");

                if (player.TryGetComponent(out PlayerSyncController syncController))
                    syncController.ResetAfterWarp(spawnPoint.position, spawnPoint.rotation);
                else
                    player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            Debug.Log($"[{nameof(PlayerSpawnController)}] 已重新布置玩家：Count={sessionPlayers.Count}");
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

                if (player != null && player.IsSpawned)
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

        private bool IsClientConnected(ulong clientId)
        {
            IReadOnlyList<ulong> connectedClientIds = _networkManager.ConnectedClientsIds;

            for (int i = 0; i < connectedClientIds.Count; i++)
            {
                if (connectedClientIds[i] == clientId)
                    return true;
            }

            return false;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(PlayerSpawnController)} 尚未初始化 ");
        }

        private void EnsureServer()
        {
            if (_networkManager == null || !_networkManager.IsServer)
                throw new InvalidOperationException("只有 Server 可以生成玩家 ");
        }
    }
}
