using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Spawning;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterRoomBattleRequest
    {
        public readonly int RoomId;
        public readonly RoomView RoomView;
        public readonly int Budget;
        public readonly int Difficulty;
        public readonly int MaxActiveMonsters;
        public readonly int Seed;

        public MonsterRoomBattleRequest(int roomId, RoomView roomView, int budget,
            int difficulty, int maxActiveMonsters, int seed)
        {
            RoomId = roomId;
            RoomView = roomView;
            Budget = budget;
            Difficulty = difficulty;
            MaxActiveMonsters = maxActiveMonsters;
            Seed = seed;
        }
    }

    public interface IRoomBattleController
    {
        bool IsBattleActive { get; }
        int CurrentRoomId { get; }
        int AliveMonsterCount { get; }
        bool TryStartRoomBattle(in MonsterRoomBattleRequest request);
        void CancelRoomBattle();
        event Action<int> RoomBattleStarted;
        event Action<int> RoomBattleCleared;
        event Action<int, string> RoomBattleFailed;
    }

    /// <summary>
    /// Server 房间战斗统筹：按剩余预算采购当前波次、一次性规划全部种类/位置、一次 Spawn/可靠广播。
    /// 不负责房间检测、门、奖励、Monster Tick、Health 或 View。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterBattleManager : MonoBehaviour, IGameRuntimeService, IRoomBattleController
    {
        [SerializeField] private MonsterRuntimeService _monsterRuntimeService;
        [SerializeField, Min(0f)] private float _battleStartDelay = 2f;
        [SerializeField, Min(0f)] private float _normalWaveDelay = 1.5f;
        [SerializeField, Min(0f)] private float _rushWaveDelay = 0.5f;
        [SerializeField, Min(0)] private int _rushRemainingBudgetThreshold = 100;
        [SerializeField, Min(1)] private int _maxMonstersPerRoom = 512;
        [SerializeField, Min(0f)] private float _monsterMinimumSpacing = 1.5f;
        [SerializeField, Min(0f)] private float _groundHeightTolerance = 0.25f;
        [SerializeField, Min(1)] private int _spawnAttemptsPerPoint = 48;
        [SerializeField, Min(1)] private int _maxSampleFailures = 3;
        [SerializeField, Min(0f)] private float _sampleRetryDelay = 0.5f;

        private readonly RoomGroundPointSampler _groundSampler = new();
        private readonly MonsterPlayerTargetBuffer _players = new();
        private readonly List<ushort> _waveConfigs = new();
        private readonly List<MonsterSpawnPlan> _wavePlans = new();
        private readonly List<Vector3> _groundProbe = new(1);

        private NetworkManager _networkManager;
        private MonsterSpawnSelector _selector;
        private MonsterRandomGroundSpawnPlanner _spawnPlanner;
        private Coroutine _waveRoutine;
        private int _remainingBudget;
        private int _waveBudgetAfterSelection;
        private int _difficulty;
        private int _spawnedCount;
        private int _maxActiveMonsters;

        public static MonsterBattleManager Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsBattleActive { get; private set; }
        public int CurrentRoomId { get; private set; } = -1;
        public int AliveMonsterCount => IsBattleActive ? _monsterRuntimeService.AliveMonsterCount : 0;

        public event Action<int> RoomBattleStarted;
        public event Action<int> RoomBattleCleared;
        public event Action<int, string> RoomBattleFailed;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized) return UniTask.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            if (Instance != null && Instance != this)
                throw new InvalidOperationException("重复 MonsterBattleManager。");
            if (_monsterRuntimeService == null) _monsterRuntimeService = GetComponent<MonsterRuntimeService>();
            if (_monsterRuntimeService == null || !_monsterRuntimeService.IsInitialized)
                throw new InvalidOperationException("MonsterBattleManager 必须在 MonsterRuntimeService 后初始化。");
            _networkManager = NetworkManager.Singleton;
            if (_networkManager == null || !_networkManager.IsListening)
                throw new InvalidOperationException("MonsterBattleManager 初始化时网络尚未启动。");
            ValidateSettings();
            Instance = this;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public bool TryStartRoomBattle(in MonsterRoomBattleRequest request)
        {
            EnsureInitialized();
            if (!_networkManager.IsServer || IsBattleActive) return false;
            ValidateRequest(request);

            RoomSpawnRegion region = request.RoomView.SpawnRegion;
            var groundRandom = new System.Random(unchecked(request.Seed * 486187739 + 49979687));
            if (!_groundSampler.TrySampleMany(region, 1, 1f, 0f, groundRandom,
                    _groundProbe, _spawnAttemptsPerPoint))
            {
                RoomBattleFailed?.Invoke(request.RoomId, "房间采样区域内找不到 Ground。");
                return false;
            }

            region.GetWorldXZBounds(out Vector2 quantizationOrigin, out Vector2 quantizationSize);
            float groundY = _groundProbe[0].y;

            try
            {
                _selector = new MonsterSpawnSelector(_monsterRuntimeService.Catalogs.Spawns, request.Seed);
                _spawnPlanner = new MonsterRandomGroundSpawnPlanner(region, _groundSampler,
                    unchecked(request.Seed * 16777619 + 86028121), _monsterMinimumSpacing,
                    groundY, _groundHeightTolerance, _spawnAttemptsPerPoint);
                _monsterRuntimeService.BeginRoom(quantizationOrigin, quantizationSize, groundY, request.RoomView.SpatialData,request.RoomView.transform);
            }
            catch (Exception exception)
            {
                _selector = null;
                _spawnPlanner = null;
                RoomBattleFailed?.Invoke(request.RoomId, exception.Message);
                Debug.LogError($"[{nameof(MonsterBattleManager)}] 房间启动失败：{exception}");
                return false;
            }

            CurrentRoomId = request.RoomId;
            _maxActiveMonsters = request.MaxActiveMonsters;
            _remainingBudget = request.Budget;
            _difficulty = request.Difficulty;
            _spawnedCount = 0;
            _waveBudgetAfterSelection = request.Budget;
            IsBattleActive = true;
            _waveRoutine = StartCoroutine(WaveSpawnRoutine());
            RoomBattleStarted?.Invoke(CurrentRoomId);
            return true;
        }

        private IEnumerator WaveSpawnRoutine()
        {
            if (_battleStartDelay > 0f) yield return new WaitForSeconds(_battleStartDelay);
            bool firstWave = true;
            int sampleFailures = 0;

            while (_spawnedCount < _maxMonstersPerRoom)
            {
                if (!firstWave)
                {
                    int threshold = _maxActiveMonsters / 2;
                    yield return new WaitUntil(() => !IsBattleActive || AliveMonsterCount <= threshold);
                    if (!IsBattleActive) yield break;
                    float delay = _remainingBudget > _rushRemainingBudgetThreshold
                        ? _rushWaveDelay : _normalWaveDelay;
                    if (delay > 0f) yield return new WaitForSeconds(delay);
                }

                int available = _maxActiveMonsters - AliveMonsterCount;
                if (available <= 0)
                {
                    yield return null;
                    continue;
                }
                int waveCapacity = Math.Min(available, _maxMonstersPerRoom - _spawnedCount);
                if (_waveConfigs.Count == 0)
                {
                    _waveBudgetAfterSelection = _selector.Select(
                        _remainingBudget, _difficulty, waveCapacity, _waveConfigs);
                    if (_waveConfigs.Count == 0) break;
                }

                PlayerManager players = PlayerManager.Instance;
                if (players == null || !players.IsInitialized)
                {
                    FailCurrentBattle("PlayerManager 尚未准备完成。");
                    yield break;
                }
                MonsterPlayerTargetCapture.Capture(players, _players);
                if (!_spawnPlanner.TryBuildPlans(_waveConfigs, _players, _wavePlans))
                {
                    sampleFailures++;
                    if (sampleFailures >= _maxSampleFailures)
                    {
                        FailCurrentBattle($"当前波次连续 {sampleFailures} 次无法采样完整 Ground 落点。");
                        yield break;
                    }
                    if (_sampleRetryDelay > 0f) yield return new WaitForSeconds(_sampleRetryDelay);
                    continue;
                }

                try
                {
                    // 一个波次只调用一次：种类和位置共同进入同一个可靠 SpawnBatch。
                    _monsterRuntimeService.Spawn(_wavePlans);
                }
                catch (Exception exception)
                {
                    FailCurrentBattle($"当前波次 Spawn 失败：{exception.Message}");
                    Debug.LogError($"[{nameof(MonsterBattleManager)}] 波次生成失败：{exception}");
                    yield break;
                }
                _remainingBudget = _waveBudgetAfterSelection;
                _spawnedCount += _waveConfigs.Count;
                _waveConfigs.Clear();
                sampleFailures = 0;
                firstWave = false;
            }

            yield return new WaitUntil(() => !IsBattleActive || AliveMonsterCount == 0);
            if (IsBattleActive) CompleteCurrentBattle();
        }

        public void CancelRoomBattle()
        {
            if (!IsInitialized || !IsBattleActive) return;
            if (_waveRoutine != null) StopCoroutine(_waveRoutine);
            _waveRoutine = null;
            if (_monsterRuntimeService.IsRoomActive) _monsterRuntimeService.EndRoom();
            ResetBattleState();
        }

        private void CompleteCurrentBattle()
        {
            int roomId = CurrentRoomId;
            _waveRoutine = null;
            _monsterRuntimeService.EndRoom();
            ResetBattleState();
            RoomBattleCleared?.Invoke(roomId);
        }

        private void FailCurrentBattle(string reason)
        {
            int roomId = CurrentRoomId;
            _waveRoutine = null;
            if (_monsterRuntimeService.IsRoomActive) _monsterRuntimeService.EndRoom();
            ResetBattleState();
            RoomBattleFailed?.Invoke(roomId, reason);
        }

        private void ResetBattleState()
        {
            IsBattleActive = false;
            CurrentRoomId = -1;
            _remainingBudget = 0;
            _waveBudgetAfterSelection = 0;
            _difficulty = 0;
            _spawnedCount = 0;
            _maxActiveMonsters = 0;
            _selector = null;
            _spawnPlanner = null;
            _waveConfigs.Clear();
            _wavePlans.Clear();
            _groundProbe.Clear();
            _players.Clear();
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return UniTask.CompletedTask;
            CancelRoomBattle();
            RoomBattleStarted = null;
            RoomBattleCleared = null;
            RoomBattleFailed = null;
            _networkManager = null;
            if (Instance == this) Instance = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        private static void ValidateRequest(in MonsterRoomBattleRequest request)
        {
            if (request.RoomId < 0) throw new ArgumentOutOfRangeException(nameof(request.RoomId));
            if (request.RoomView == null) throw new ArgumentNullException(nameof(request.RoomView));
            if (request.Budget < 0) throw new ArgumentOutOfRangeException(nameof(request.Budget));
            if (request.Difficulty < 0) throw new ArgumentOutOfRangeException(nameof(request.Difficulty));
            if (request.MaxActiveMonsters < 1)
                throw new ArgumentOutOfRangeException(nameof(request.MaxActiveMonsters));
        }

        private void ValidateSettings()
        {
            if (_maxMonstersPerRoom < 1 || _spawnAttemptsPerPoint < 1 || _maxSampleFailures < 1 ||
                !MonsterMath.IsNonNegativeFinite(_battleStartDelay) ||
                !MonsterMath.IsNonNegativeFinite(_normalWaveDelay) ||
                !MonsterMath.IsNonNegativeFinite(_rushWaveDelay) ||
                !MonsterMath.IsNonNegativeFinite(_monsterMinimumSpacing) ||
                !MonsterMath.IsNonNegativeFinite(_groundHeightTolerance) ||
                !MonsterMath.IsNonNegativeFinite(_sampleRetryDelay))
                throw new InvalidOperationException("MonsterBattleManager 波次配置非法。");
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized) throw new InvalidOperationException("MonsterBattleManager 尚未初始化。");
        }

        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
