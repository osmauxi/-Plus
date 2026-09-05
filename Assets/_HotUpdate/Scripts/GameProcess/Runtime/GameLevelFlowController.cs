using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Runtime
{
    /// <summary>
    /// 一局 Gameplay 内的层级流程控制器 
    ///
    /// 负责：
    /// 初始地图生成 → 首次玩家生成；
    /// 下一层事件 → 新地图生成 → 玩家重新定位 
    ///
    /// 不负责地图算法、玩家实例化细节和房间逻辑 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameLevelFlowController : MonoBehaviour, IGameRuntimeService
    {
        [SerializeField] private MapGenerationController _mapGenerationController;
        [SerializeField] private MapVisualBuilder _mapVisualBuilder;
        [SerializeField] private PlayerSpawnController _playerSpawnController;

        private NetworkManager _networkManager;
        private GameStateController _gameStateController;

        private IDisposable _nextLevelSubscription;
        private CancellationTokenSource _flowCts;

        private bool _runStarted;
        private bool _isTransitioning;

        public bool IsInitialized { get; private set; }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            _networkManager = NetworkManager.Singleton;
            _gameStateController = GameStateController.Instance;

            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("NGO 尚未启动 ");

            if (_gameStateController == null || !_gameStateController.IsSpawned)
                throw new InvalidOperationException("GameStateController 尚未准备完成 ");

            if (_mapGenerationController == null || !_mapGenerationController.IsInitialized)
                throw new InvalidOperationException("MapGenerationController 尚未初始化 ");

            if (_mapVisualBuilder == null || !_mapVisualBuilder.IsInitialized)
                throw new InvalidOperationException("MapVisualBuilder 尚未初始化 ");

            if (_playerSpawnController == null || !_playerSpawnController.IsInitialized)
                throw new InvalidOperationException("PlayerSpawnController 尚未初始化 ");

            // LocalEvent 是本机事件 
            // 只有 Server 才拥有 Gameplay Flow 决策权，所以只有 Server 订阅 
            if (_networkManager.IsServer)
                _nextLevelSubscription = LocalEvents.Subscribe<NextLevelRequestedEvent>(HandleNextLevelRequested);

            _flowCts = new CancellationTokenSource();

            _runStarted = false;
            _isTransitioning = false;
            IsInitialized = true;

            Debug.Log($"[{nameof(GameLevelFlowController)}] 初始化完成 ");

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// RuntimeReady 屏障结束后，由 GameRuntimeBootstrap 在 Server 上调用一次 
        /// </summary>
        public async UniTask StartInitialLevelAsync(CancellationToken cancellationToken)
        {
            EnsureInitialized();
            EnsureServer();

            if (_runStarted)
                throw new InvalidOperationException("Gameplay Run 已经启动 ");

            _runStarted = true;

            try
            {
                await BuildLevelAsync(true, cancellationToken);
            }
            catch
            {
                _runStarted = false;
                throw;
            }
        }

        private void HandleNextLevelRequested(NextLevelRequestedEvent _)
        {
            if (!IsInitialized || !_networkManager.IsServer)
                return;

            if (!_runStarted || _isTransitioning)
                return;

            if (_gameStateController.CurrentState != GameState.GamePlaying)
                return;

            TransitionToNextLevelAsync(_flowCts.Token).Forget();
        }

        private async UniTaskVoid TransitionToNextLevelAsync(CancellationToken cancellationToken)
        {
            _isTransitioning = true;

            try
            {
                _gameStateController.ChangeStateServer(GameState.MapGenerating);
                _gameStateController.IncreaseLevelServer();

                int level = _gameStateController.CurrentLevel;

                MapBuildResult result = await _mapGenerationController.GenerateAndBuildAsync(
                    level,
                    GameSessionContext.PlayerCount,
                    cancellationToken);

                IReadOnlyList<Transform> spawnPoints = ResolveStartRoomSpawnPoints(result);

                _playerSpawnController.RepositionPlayers(spawnPoints);

                _gameStateController.ChangeStateServer(GameState.GamePlaying);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(GameLevelFlowController)}] 转层失败：\n{exception}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private async UniTask BuildLevelAsync(bool initialLevel, CancellationToken cancellationToken)
        {
            _gameStateController.ChangeStateServer(GameState.MapGenerating);

            int level = _gameStateController.CurrentLevel;

            MapBuildResult result = await _mapGenerationController.GenerateAndBuildAsync(
                level,
                GameSessionContext.PlayerCount,
                cancellationToken);

            IReadOnlyList<Transform> spawnPoints = ResolveStartRoomSpawnPoints(result);

            if (initialLevel)
            {
                await _playerSpawnController.SpawnInitialPlayersAsync(spawnPoints, cancellationToken);
                // Network Spawn 完成后还要等待每个 Peer 的角色、武器和 Animator 异步装载 
                // 在屏障结束前保持 MapGenerating，InputManager 不会开放 Gameplay 输入 
                if (GameRuntimeBootstrap.Instance == null)
                    throw new InvalidOperationException("GameRuntimeBootstrap 不存在，无法等待 PlayerRuntime Ready ");

                await GameRuntimeBootstrap.Instance.WaitUntilAllPlayerRuntimesReadyAsync(cancellationToken);
            }
            else
                _playerSpawnController.RepositionPlayers(spawnPoints);

            _gameStateController.ChangeStateServer(GameState.GamePlaying);

            Debug.Log(
                $"[{nameof(GameLevelFlowController)}] Level {level} 准备完成，" +
                $"Initial={initialLevel}，Generation={result.GenerationId}");
        }

        private IReadOnlyList<Transform> ResolveStartRoomSpawnPoints(MapBuildResult result)
        {
            int startRoomId = result.BuildPlan.StartRoomId;

            if (!_mapVisualBuilder.TryGetRoom(startRoomId, out RoomViewRuntime roomRuntime))
                throw new InvalidOperationException($"找不到起始房间运行时对象：RoomId={startRoomId}");

            IReadOnlyList<Transform> spawnPoints = roomRuntime.View.PlayerSpawnPoints;

            if (spawnPoints == null || spawnPoints.Count < GameSessionContext.PlayerCount)
            {
                throw new InvalidOperationException(
                    $"起始房间玩家出生点不足：" +
                    $"RoomId={startRoomId}，SpawnPoints={spawnPoints?.Count ?? 0}，Players={GameSessionContext.PlayerCount}");
            }

            return spawnPoints;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                return UniTask.CompletedTask;

            _nextLevelSubscription?.Dispose();
            _nextLevelSubscription = null;

            if (_flowCts != null)
            {
                _flowCts.Cancel();
                _flowCts.Dispose();
                _flowCts = null;
            }

            _runStarted = false;
            _isTransitioning = false;

            _networkManager = null;
            _gameStateController = null;

            IsInitialized = false;

            Debug.Log($"[{nameof(GameLevelFlowController)}] 已关闭 ");

            return UniTask.CompletedTask;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(GameLevelFlowController)} 尚未初始化 ");
        }

        private void EnsureServer()
        {
            if (_networkManager == null || !_networkManager.IsServer)
                throw new InvalidOperationException("只有 Server 可以驱动关卡流程 ");
        }
    }
}
