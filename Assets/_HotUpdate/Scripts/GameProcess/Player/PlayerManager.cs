using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// GameRuntimeScene 内的玩家运行时注册表。
    ///
    /// 负责：
    /// 1. 保存本机已经生成的玩家实例。
    /// 2. 提供 ClientId 到 PlayerRuntime 的查询。
    /// 3. 维护本地玩家引用。
    /// 4. 提供常用玩家集合查询。
    /// </summary>
    public sealed class PlayerManager : MonoBehaviour, IGameRuntimeService
    {
        public static PlayerManager Instance { get; private set; }

        private readonly Dictionary<ulong, PlayerRuntime> _playersByClientId = new();
        private readonly List<PlayerRuntime> _orderedPlayers = new();

        private NetworkManager _networkManager;

        public bool IsInitialized { get; private set; }

        public bool IsSinglePlayer => GameSessionContext.IsSinglePlayer;

        public bool IsMultiplayer => GameSessionContext.IsMultiplayer;

        /// <summary>
        /// Lobby 阶段确定的预期玩家数量。
        /// </summary>
        public int ExpectedPlayerCount => GameSessionContext.PlayerCount;

        /// <summary>
        /// 当前本机已经生成并注册的玩家实例数量。
        /// </summary>
        public int SpawnedPlayerCount => _orderedPlayers.Count;

        public IReadOnlyList<PlayerSessionData> SessionPlayers => GameSessionContext.Players;

        public IReadOnlyList<PlayerRuntime> RuntimePlayers => _orderedPlayers;

        public PlayerRuntime LocalPlayer { get; private set; }

        public event Action<PlayerRuntime> PlayerRegistered;
        public event Action<PlayerRuntime> PlayerUnregistered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[{nameof(PlayerManager)}] GameRuntimeScene 中存在重复实例。");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            _networkManager = NetworkManager.Singleton;

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未启动，无法初始化 PlayerManager。");

            if (!GameSessionContext.IsConfigured)
                throw new InvalidOperationException("GameSessionContext 尚未配置，无法建立 Gameplay 玩家数据。");

            _playersByClientId.Clear();
            _orderedPlayers.Clear();
            LocalPlayer = null;

            IsInitialized = true;

            Debug.Log($"[{nameof(PlayerManager)}] 初始化完成，模式={GameSessionContext.Mode}，预期玩家数={ExpectedPlayerCount}");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 由 PlayerRuntime.OnNetworkSpawn 调用。
        /// </summary>
        public void RegisterPlayer(PlayerRuntime player)
        {
            if (player == null)
                return;

            EnsureInitialized();

            ulong clientId = player.ClientId;

            if (_playersByClientId.TryGetValue(clientId, out PlayerRuntime existingPlayer))
            {
                if (existingPlayer == player)
                    return;
                
                //断线重连时，新 PlayerRuntime 可能使用相同 ClientId 替换旧引用。
                //先移除旧对象，避免同一个 ClientId 在列表中出现两次。
                _orderedPlayers.Remove(existingPlayer);
                Debug.LogWarning($"[{nameof(PlayerManager)}] Client {clientId} 的玩家实例已被替换。");
            }

            _playersByClientId[clientId] = player;
            _orderedPlayers.Add(player);
            _orderedPlayers.Sort((left, right) => left.ClientId.CompareTo(right.ClientId));

            if (clientId == _networkManager.LocalClientId)
                LocalPlayer = player;

            PlayerRegistered?.Invoke(player);

            Debug.Log($"[{nameof(PlayerManager)}] 玩家已注册：ClientId={clientId}，当前数量={SpawnedPlayerCount}");
        }

        /// <summary>
        /// 由 PlayerRuntime.OnNetworkDespawn 调用。
        /// </summary>
        public void UnregisterPlayer(PlayerRuntime player)
        {
            if (player == null || !IsInitialized)
                return;

            ulong clientId = player.ClientId;

            
             //旧重连对象晚于新对象 Despawn 时，不能错误删除新玩家引用。
            if (!_playersByClientId.TryGetValue(clientId, out PlayerRuntime registeredPlayer) || registeredPlayer != player)
                return;

            _playersByClientId.Remove(clientId);
            _orderedPlayers.Remove(player);

            if (LocalPlayer == player)
                LocalPlayer = null;

            PlayerUnregistered?.Invoke(player);

            Debug.Log($"[{nameof(PlayerManager)}] 玩家已移除：ClientId={clientId}，当前数量={SpawnedPlayerCount}");
        }

        public bool TryGetRuntimePlayer(ulong clientId, out PlayerRuntime player)
        {
            return _playersByClientId.TryGetValue(clientId, out player);
        }

        public bool TryGetSessionPlayer(ulong clientId, out PlayerSessionData playerData)
        {
            return GameSessionContext.TryGetPlayer(clientId, out playerData);
        }

        /// <summary>
        /// 等待所有会话玩家的 NetworkObject 在本机完成 Spawn。
        /// PlayerSpawnController 后续会使用这个方法作为生成完成条件。
        /// </summary>
        public async UniTask WaitUntilAllPlayersRegisteredAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => SpawnedPlayerCount >= ExpectedPlayerCount, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 获取距离指定位置最近的有效玩家。
        /// 更复杂的仇恨、隐身和阵营过滤应由 AI 目标选择系统处理。
        /// </summary>
        public PlayerRuntime GetNearestPlayer(Vector3 searchPosition)
        {
            PlayerRuntime nearestPlayer = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < _orderedPlayers.Count; i++)
            {
                PlayerRuntime player = _orderedPlayers[i];

                if (player == null || !player.isActiveAndEnabled || !player.IsSpawned)
                    continue;

                float sqrDistance = (player.transform.position - searchPosition).sqrMagnitude;

                if (sqrDistance >= nearestSqrDistance)
                    continue;

                nearestSqrDistance = sqrDistance;
                nearestPlayer = player;
            }

            return nearestPlayer;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            if (_orderedPlayers.Count > 0)
                Debug.LogWarning($"[{nameof(PlayerManager)}] Shutdown 时仍有 {_orderedPlayers.Count} 个玩家实例尚未注销。");

            _playersByClientId.Clear();
            _orderedPlayers.Clear();

            LocalPlayer = null;
            _networkManager = null;
            IsInitialized = false;

            PlayerRegistered = null;
            PlayerUnregistered = null;

            Debug.Log($"[{nameof(PlayerManager)}] 已关闭并清理。");
            return UniTask.CompletedTask;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(PlayerManager)} 尚未初始化，请确认它已加入 GameRuntimeBootstrap。");
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            _playersByClientId.Clear();
            _orderedPlayers.Clear();

            LocalPlayer = null;
            Instance = null;
        }
    }
}