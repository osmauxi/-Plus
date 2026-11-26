using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Weapon.Effects;
using ProjectGame.HotFix.Gameplay.Weapon.Effects.Special;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 从ConfigManager中读取武器配表信息，组装WeaponDefinition
    /// </summary>
    public sealed class WeaponCatalog
    {
        private readonly Dictionary<int, WeaponDefinition> _definitions = new();
        public WeaponCatalog(IDictionary<int, Config_Weapon> rows, WeaponStatSnapshotRepository snapshots, float tickDeltaTime)
        {
            foreach (var pair in rows)
            {
                var row = pair.Value;
                if (!row.Enabled) 
                    continue;
                var stats = new WeaponStatSnapshot
                {
                    Id = (ushort)(row.WeaponID + 1), 
                    Damage = row.Damage,
                    FireRate = row.FireRate, 
                    ReloadTime = row.ReloadTime,
                    //读配表时动态转换固定Tick缓存，这里是无Effect影响下的基础值
                    FireIntervalTicks = WeaponSystem.ToTicks(1f / row.FireRate, tickDeltaTime),
                    ReloadTicks = WeaponSystem.ToTicks(row.ReloadTime, tickDeltaTime),
                    MagSize = (ushort)row.MagSize,
                    CritChance = row.CritChance, 
                    CritMultiplier = row.CritMultiplier,
                    ProjectileSpeed = row.ProjectileSpeed, 
                    ProjectileCount = (ushort)row.ProjectileCount,
                    SpreadAngle = row.SpreadAngle, 
                    BounceCount = (byte)row.BounceCount,
                    PierceCount = (byte)row.PierceCount, 
                    ProjectileSize = row.ProjectileSize,
                    ProjectileLifeTime = row.ProjectileLifeTime,
                };
                snapshots.Register(stats);
                _definitions.Add(row.WeaponID, new WeaponDefinition((ushort)row.WeaponID, stats,
                    (ushort)row.ReserveAmmo, row.AutoReload));
            }
        }

        public WeaponDefinition Get(int weaponId) => _definitions[weaponId];
    }

    /// <summary>
    /// 场景级组合根，拥有仓储、子弹世界和生命周期,不拥有任何玩家的可回滚状态,只管子弹的状态
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponRuntimeService : MonoBehaviour, IGameRuntimeService, IWeaponEventSource,
        IWeaponSpecialEffectCommandSink
    {
        private const ushort BenchmarkStressStatsId = ushort.MaxValue - 1;
        private const ushort BenchmarkStressEffectSetId = ushort.MaxValue;
        private static readonly ProfilerMarker EffectDispatchMarker =
            new("Weapon.Projectile.EffectDispatch");
        private static readonly ProfilerMarker LightningMarker =
            new("Weapon.Effect.Lightning");
        private static readonly ProfilerMarker StormCloudMarker =
            new("Weapon.Effect.StormCloud");
        private static readonly ProfilerMarker DamageAreaMarker =
            new("Weapon.Effect.DamageArea");

        [SerializeField] private LayerMask _worldMask;
        public static WeaponRuntimeService Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public WeaponCatalog Catalog { get; private set; }
        public WeaponEffectCatalog EffectCatalog { get; private set; }
        public EffectRollService EffectRolls { get; private set; }
        public int ActiveProjectileCount => _world.Count;
        public int ActiveShotCount => _shots.Count;
        public int DebugProjectileCount => _world.Count;
        //给外部事件的接口，通过Simulation内部委托触发，作为一层内部与外部的解耦
        //不让外部直接依赖Simulation
        public event Action<ShotContext> ShotFired;
        public event Action<ProjectileSpawn> ProjectileSpawned;
        public event Action<ProjectileImpact> ProjectileImpact;
        public event Action<ProjectileState> ProjectileRemoved;
        public event Action<WeaponSpecialVfxEvent> SpecialEffectVfx;

        public void GetDebugProjectile(int index, out ProjectileState projectile, out float radius)
        {
            projectile = default;
            _world.Read(index, ref projectile);
            radius = _simulation.GetDebugRadius(index);
        }

        private readonly WeaponStatSnapshotRepository _stats = new();
        private readonly EffectSetRepository _effectSets = new();
        private readonly ShotRepository _shots = new();
        private readonly ProjectileWorld _world = new();
        private readonly List<ProjectileSpawn> _spawnBuffer = new(16);
        private readonly Dictionary<ulong, StormCloudState> _stormClouds = new();
        private readonly List<DamageAreaState> _damageAreas = new();
        private readonly Dictionary<ulong, Dictionary<ushort, float>> _shieldCapacities = new();
        private readonly List<int> _effectTargets = new(64);
        private int[] _monsterQueryBuffer = Array.Empty<int>();
        private readonly HashSet<int> _lightningVisited = new();
        private readonly List<LightningNode> _lightningNodes = new(16);
        private WeaponStatModifierCalculator _effectCalculator;
        private WeaponSpecialEffectManager _specialEffects;
        private MonsterRuntimeService _monsters;
        private GameplayNetworkRuntime _network;
        private ShotBuilder _builder;
        private ProjectileSimulation _simulation;
        private uint _currentTick;
        private ulong _nextDamageAreaVfxId = 1;

        private sealed class StormCloudState
        {
            public float Radius;
            public float StrikeDamage;
            public float StrikeInterval;
            public float TimeUntilStrike;
            public float OverloadDamage;
            public byte OverloadJumpCount;
            public float ExecuteThreshold;
        }

        private sealed class DamageAreaState
        {
            public ulong VfxId;
            public ulong SourceEntityId;
            public Vector3 Position;
            public float Radius;
            public float RemainingDuration;
            public float TickInterval;
            public float TimeUntilTick;
            public float DamagePerTick;
            public float SlowRatio;
            public float ExecuteThreshold;
        }

        private readonly struct LightningNode
        {
            public readonly Vector3 Position;
            public readonly byte Depth;

            public LightningNode(Vector3 position, byte depth)
            {
                Position = position;
                Depth = depth;
            }
        }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if(IsInitialized) 
                return UniTask.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            _network = GameNetworkRuntime.Gameplay;
            Catalog = new WeaponCatalog(ConfigManager.Instance.GetTable<Config_Weapon>(), _stats, _network.Clock.TickDeltaTime);
            EffectCatalog = new WeaponEffectCatalog(
                ConfigManager.Instance.GetTable<Config_Effect>(),
                ConfigManager.Instance.GetTable<Config_Modifier>(),
                ConfigManager.Instance.GetTable<Config_EffectRoll>());
            EffectRolls = new EffectRollService(EffectCatalog);
            _effectCalculator = new WeaponStatModifierCalculator(EffectCatalog);
            _specialEffects = new WeaponSpecialEffectManager(_effectSets);
            _specialEffects.Register(new OverloadEffectSystem(this));
            _specialEffects.Register(new StormCloudEffectSystem(this));
            _specialEffects.Register(new StaticShieldEffectSystem(this));
            _specialEffects.Register(new PhotonMomentumEffectSystem(this));
            _specialEffects.Register(new EnergySiphonEffectSystem(this));
            _specialEffects.Register(new ExecutionerEffectSystem(this));
            _specialEffects.Register(new KineticBoostEffectSystem(this));
            _specialEffects.Register(new MultiSplitEffectSystem(this));
            _specialEffects.Register(new NuclearFissionEffectSystem(this));
            _specialEffects.Register(new ShockwaveEffectSystem(this));

            _monsters = MonsterRuntimeService.Instance;
            _builder = new ShotBuilder(_shots, _world, _specialEffects);
            _simulation = new ProjectileSimulation(_world, _shots,
                new ProjectileSimulationConfig { WorldMask = _worldMask },
                _monsters,
                _specialEffects);
            _simulation.Impact += HandleImpact;
            _simulation.Removed += HandleRemoved;
            _network.Clock.Subscribe(GameplayTickPhase.PhysicsSync, SyncPhysicsTransforms);
            _network.Clock.Subscribe(GameplayTickPhase.CombatSimulation, TickCombat);
            Instance = this;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public PlayerEffectLoadout CreatePlayerEffectLoadout() => new PlayerEffectLoadout(EffectCatalog);

        /// <summary>
        /// 服务端创建不可覆盖的新EffectSet与属性快照，供后续ShotContext持有
        /// 把玩家当前的可变Loadout正式提交成权威世界可引用的快照
        /// </summary>
        public WeaponEffectApplication RegisterPlayerEffectState(
            ulong ownerEntityId,
            WeaponDefinition definition,
            PlayerEffectLoadout loadout)
        {
            //没有Effect不用创建快照直接返回空的
            if (loadout.AcquisitionCount == 0)
            {
                var baseCalculation = new WeaponEffectCalculation(
                    definition.Stats, new EffectOwnerStatSnapshot(0f));
                _specialEffects.DispatchOwnerState(ownerEntityId, EffectSet.Empty, definition.Stats);
                SetShieldCapacity(new ShieldCapacityEffectCommand(
                    0, ownerEntityId, baseCalculation.OwnerStats.ShieldCapacity));
                return new WeaponEffectApplication(EffectSet.Empty, baseCalculation);
            }

            EffectSet set = loadout.CreateSnapshot(_effectSets.AllocateId());
            //将新EffectSet放进仓库
            _effectSets.Register(set);
            //算出新的EffectOwnerStatSnapshot
            WeaponEffectCalculation calculation = _effectCalculator.Calculate(
                definition.Stats, set, _stats.AllocateRuntimeId(), _network.Clock.TickDeltaTime);
            //注册EffectOwnerStatSnapshot
            _stats.Register(calculation.WeaponStats);
            //触发特殊的装备Effect效果
            _specialEffects.DispatchOwnerState(ownerEntityId, set, calculation.WeaponStats);
            SetShieldCapacity(new ShieldCapacityEffectCommand(
                0, ownerEntityId, calculation.OwnerStats.ShieldCapacity));
            return new WeaponEffectApplication(set, calculation);
        }

        /// <summary>玩家离开或回池时移除跟随玩家的常驻特殊 Effect。</summary>
        public void RemovePlayerEffectState(ulong ownerEntityId)
        {
            if (!IsInitialized || !_network.Transport.IsServer) return;
            _specialEffects.RemoveOwner(ownerEntityId);
            _stormClouds.Remove(ownerEntityId);
            _shieldCapacities.Remove(ownerEntityId);
            for (int i = _damageAreas.Count - 1; i >= 0; i--)
                if (_damageAreas[i].SourceEntityId == ownerEntityId)
                    _damageAreas.RemoveAt(i);
            PlayerHealthNetworkState health = FindPlayerHealth(ownerEntityId);
            if (health != null) health.SetBonusShieldCapacity(0f);
        }

        /// <summary>
        /// 客户端预测只重算数值，不注册权威ID，也不会执行特殊玩法
        /// </summary>
        public WeaponEffectCalculation PreviewPlayerEffectState(
            WeaponDefinition definition,
            PlayerEffectLoadout loadout)
        {
            EffectSet set = loadout.AcquisitionCount == 0 ? EffectSet.Empty : loadout.CreateSnapshot(1);
            return _effectCalculator.Calculate(definition.Stats, set, definition.Stats.Id,
                _network.Clock.TickDeltaTime);
        }

        /// <summary>
        /// 只供 Benchmark 使用的高压武器：低伤害、多弹丸、高穿透/反弹，并固定挂载两个特殊 Effect。
        /// 快照仍走正式 WeaponSystem，所以测试测到的是真实射击与子弹链路。
        /// </summary>
        public void ConfigureBenchmarkStressWeapon(PlayerWeaponController controller)
        {
            WeaponStatSnapshot baseStats = controller.Definition.Stats;
            WeaponStatSnapshot stats = baseStats;
            stats.Id = BenchmarkStressStatsId;
            stats.Damage = 0.001f;
            stats.FireRate = 5f;
            stats.ReloadTime = 0f;
            stats.FireIntervalTicks = WeaponSystem.ToTicks(1f / stats.FireRate,
                _network.Clock.TickDeltaTime);
            stats.ReloadTicks = 0;
            stats.MagSize = ushort.MaxValue;
            stats.CritChance = 0f;
            stats.CritMultiplier = 1f;
            stats.ProjectileSpeed = 32f;
            stats.ProjectileCount = 8;
            stats.SpreadAngle = 8f;
            stats.BounceCount = 8;
            stats.PierceCount = 8;
            stats.ProjectileSize = 0.22f;
            stats.ProjectileLifeTime = 2f;

            var effects = new EffectSet(
                BenchmarkStressEffectSetId,
                (1UL << (MultiSplitEffectSystem.Id & 63)) |
                (1UL << (ShockwaveEffectSystem.Id & 63)),
                new[]
                {
                    new EffectSnapshot(MultiSplitEffectSystem.Id, 1),
                    new EffectSnapshot(ShockwaveEffectSystem.Id, 1),
                },
                new[] { MultiSplitEffectSystem.Id, ShockwaveEffectSystem.Id });

            _stats.Register(stats);
            _effectSets.Register(effects);

            PlayerSyncController sync = controller.GetComponent<PlayerSyncController>();
            ushort version = unchecked((ushort)(controller.CurrentWeaponState.SnapshotVersion + 1));
            if (version == 0)
                version = 1;
            sync.ConfigureWeaponRuntime(stats, effects.Id, version);
            sync.RefillWeaponForBenchmark();
            _specialEffects.DispatchOwnerState(controller.NetworkObjectId, effects, stats);
        }

        // 只允许 PlayerWeaponController 在服务端权威 Tick 确认新序号后调用。
        internal void CommitShot(ulong ownerEntityId, in WeaponRuntimeState state, uint tick,
            Vector3 origin, Vector3 direction)
        {
            //根据权威ID拿到对应数据快照和Effect效果
            WeaponStatSnapshot stats = _stats.Get(state.StatSnapshotId);
            EffectSet effects = _effectSets.Get(state.EffectSetId);
            //触发特殊效果的开火前效果
            _specialEffects.DispatchBeforeFire(new WeaponEffectFireContext(
                ownerEntityId, tick, state.WeaponId, stats, effects));
            _spawnBuffer.Clear();
            var shot = _builder.Build(ownerEntityId, state.WeaponId, tick, state.ShotSequence,
                origin, direction, stats, state.EffectSetId, _spawnBuffer);
            _specialEffects.DispatchAfterFire(shot, stats);
            ShotFired?.Invoke(shot);

            for (int i = 0; i < _spawnBuffer.Count; i++)
                ProjectileSpawned?.Invoke(_spawnBuffer[i]);

            _spawnBuffer.Clear();
        }

        public void EmitLightning(in LightningEffectCommand command)
        {
            if (!_monsters.IsRoomActive) return;
            int targetStorageIndex = _monsters.ResolveMonsterStorageIndex(command.TargetEntityId);
            float executeThreshold = GetExecuteThreshold(command.EffectSetId);
            ApplyLightning(_monsters, targetStorageIndex, command.Origin, command.Damage,
                command.JumpCount, command.BranchCount, command.SearchRadius,
                command.ApplyInitialDamage, executeThreshold);
        }

        public void SpawnStormCloud(in StormCloudEffectCommand command)
        {
            _stormClouds[command.OwnerEntityId] = new StormCloudState
            {
                Radius = command.Radius,
                StrikeDamage = command.StrikeDamage,
                StrikeInterval = command.StrikeInterval,
                TimeUntilStrike = command.StrikeInterval,
                OverloadDamage = command.OverloadDamage,
                OverloadJumpCount = command.OverloadJumpCount,
                ExecuteThreshold = command.ExecuteThreshold,
            };
            PublishSpecialVfx(WeaponSpecialVfxType.StormCloudStart,
                command.OwnerEntityId, command.OwnerEntityId, default, default,
                command.Radius, 0f);
        }

        public void RemoveStormCloud(ulong ownerEntityId)
        {
            if (!_stormClouds.Remove(ownerEntityId)) return;
            PublishSpecialVfx(WeaponSpecialVfxType.StormCloudStop,
                ownerEntityId, ownerEntityId, default, default, 0f, 0f);
        }

        public void AddShield(in ShieldEffectCommand command) =>
            FindPlayerHealth(command.OwnerEntityId)?.AddShield(command.Amount);

        public void SetShieldCapacity(in ShieldCapacityEffectCommand command)
        {
            if (!_shieldCapacities.TryGetValue(command.OwnerEntityId,
                    out Dictionary<ushort, float> capacities))
            {
                capacities = new Dictionary<ushort, float>();
                _shieldCapacities.Add(command.OwnerEntityId, capacities);
            }
            if (command.CapacityContribution > 0f)
                capacities[command.EffectId] = command.CapacityContribution;
            else
                capacities.Remove(command.EffectId);

            float total = 0f;
            foreach (float capacity in capacities.Values) total += capacity;
            if (capacities.Count == 0) _shieldCapacities.Remove(command.OwnerEntityId);
            FindPlayerHealth(command.OwnerEntityId)?.SetBonusShieldCapacity(total);
        }

        public void TryExecute(in ExecuteEffectCommand command)
        {
            if (!_monsters.IsRoomActive) return;
            TryExecuteMonster(_monsters, _monsters.ResolveMonsterStorageIndex(command.TargetEntityId),
                command.HealthThreshold);
        }

        public void ApplyCrowdControl(in CrowdControlEffectCommand command)
        {
            if (!_monsters.IsRoomActive) return;
            int storageIndex = _monsters.ResolveMonsterStorageIndex(command.TargetEntityId);
            if (!_monsters.TryGetMonsterPosition(storageIndex, out Vector3 position)) return;
            _monsters.StunMonster(storageIndex, command.Duration, _currentTick);
            PublishSpecialVfx(WeaponSpecialVfxType.KineticBoost,
                command.TargetEntityId, 0,
                position, default, 0.7f, command.Duration);
        }

        /// <summary>分裂弹仍属于同一个 Shot；这里扩展引用计数并发布普通出生事件。</summary>
        public void SpawnProjectiles(in SplitProjectileEffectCommand command)
        {
            ShotContext shot = command.Context.Shot;
            WeaponStatSnapshot stats = command.Context.Stats;
            float speed = command.Parent.Velocity.magnitude;
            Vector3 forward = command.Parent.Velocity / speed;

            _shots.RetainProjectiles(shot.ShotId, command.SplitCount);
            for (int i = 0; i < command.SplitCount; i++)
            {
                float t = command.SplitCount == 1 ? 0.5f : i / (float)(command.SplitCount - 1);
                float angle = Mathf.Lerp(-command.SpreadAngle * 0.5f,
                    command.SpreadAngle * 0.5f, t);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                var projectile = new ProjectileState
                {
                    ProjectileId = _world.AllocateProjectileId(),
                    ShotId = shot.ShotId,
                    Position = command.Context.HitPoint + direction * 0.01f,
                    Velocity = direction * speed,
                    RemainingLifeTime = stats.ProjectileLifeTime,
                    DamageMultiplier = command.Parent.DamageMultiplier * command.DamageRatio,
                    SizeMultiplier = command.Parent.SizeMultiplier,
                    PierceRemaining = stats.PierceCount,
                    BounceRemaining = stats.BounceCount,
                    Generation = (byte)(command.Parent.Generation + 1),
                    Flags = command.Parent.Flags & ProjectileFlags.Critical,
                };
                using (EffectDispatchMarker.Auto())
                    _specialEffects.DispatchSpawn(ref projectile,
                        new ProjectileSpawnEffectContext(shot, stats));
                _world.Add(projectile, stats, shot.OwnerEntityId);
                _simulation.MarkTargetAsHit(projectile.ProjectileId, command.Context.TargetEntityId);
                ProjectileSpawned?.Invoke(new ProjectileSpawn(
                    shot,
                    projectile,
                    stats.ProjectileSize * projectile.SizeMultiplier,
                    _currentTick));
            }
        }

        public void EmitRadialImpact(in RadialImpactEffectCommand command)
        {
            if (!_monsters.IsRoomActive) return;
            float executeThreshold = GetExecuteThreshold(command.EffectSetId);
            PublishSpecialVfx(WeaponSpecialVfxType.Shockwave,
                0, 0,
                command.Position, default, command.Radius, 0.6f);
            CollectMonsters(_monsters, command.Position, command.Radius, _effectTargets);
            for (int i = 0; i < _effectTargets.Count; i++)
            {
                int storageIndex = _effectTargets[i];
                if (!_monsters.TryGetMonsterPosition(storageIndex, out Vector3 position)) continue;
                Vector3 offset = position - command.Position;
                float forceRatio = 1f - Mathf.Clamp01(offset.magnitude / command.Radius);
                DamageMonster(_monsters, storageIndex, command.Damage, executeThreshold);
                Vector2 direction = new Vector2(offset.x, offset.z).normalized;
                _monsters.PushMonster(storageIndex, direction * (command.Force * forceRatio * 0.1f));
            }
        }

        public void TrySpawnDamageArea(in PersistentAreaEffectCommand command)
        {
            Vector3 position = command.Position;
            position.y = 0.1f;
            float minimumDistanceSqr = command.Radius * command.Radius * 0.25f;
            for (int i = 0; i < _damageAreas.Count; i++)
            {
                DamageAreaState area = _damageAreas[i];
                if ((area.Position - position).sqrMagnitude < minimumDistanceSqr) return;
            }

            ulong vfxId = _nextDamageAreaVfxId++;
            float executeThreshold = _shots.TryGet(command.ShotId, out ShotContext shot)
                ? GetExecuteThreshold(shot.EffectSetId)
                : 0f;
            _damageAreas.Add(new DamageAreaState
            {
                VfxId = vfxId,
                SourceEntityId = command.SourceEntityId,
                Position = position,
                Radius = command.Radius,
                RemainingDuration = command.Duration,
                TickInterval = command.TickInterval,
                TimeUntilTick = command.TickInterval,
                DamagePerTick = command.DamagePerTick,
                SlowRatio = command.SlowRatio,
                ExecuteThreshold = executeThreshold,
            });
            PublishSpecialVfx(WeaponSpecialVfxType.RadiationArea,
                vfxId, command.SourceEntityId, position, default,
                command.Radius, command.Duration);
        }

        private void TickCombat(uint tick)
        {
            if (!_network.Transport.IsServer) 
                return;
            _currentTick = tick;
            _simulation.Tick(_network.Clock.TickDeltaTime);
            TickStormClouds(_network.Clock.TickDeltaTime);
            TickDamageAreas(_network.Clock.TickDeltaTime);
        }

        /// <summary>
        /// 在所有 Actor/World Transform 更新完成后，统一提交本 Tick 的 Unity Physics 场景。
        /// 当前主要服务动态墙等 WorldMask 碰撞体；怪物命中不依赖 Collider。
        /// </summary>
        private void SyncPhysicsTransforms(uint tick)
        {
            if (_network.Transport.IsServer)
                Physics.SyncTransforms();
        }

        private static PlayerHealthNetworkState FindPlayerHealth(ulong entityId)
        {
            PlayerManager manager = PlayerManager.Instance;
            if (manager == null) return null;
            IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (player != null && player.NetworkObjectId == entityId)
                    return player.GetComponent<PlayerHealthNetworkState>();
            }
            return null;
        }

        private static bool TryGetPlayerPosition(ulong entityId, out Vector3 position)
        {
            PlayerManager manager = PlayerManager.Instance;
            if (manager != null)
            {
                IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerRuntime player = players[i];
                    if (player == null || player.NetworkObjectId != entityId ||
                        !player.GetComponent<PlayerHealthNetworkState>().IsAlive)
                        continue;
                    position = player.transform.position;
                    return true;
                }
            }
            position = default;
            return false;
        }

        private void CollectMonsters(MonsterRuntimeService monsters, Vector3 position,
            float radius, List<int> results)
        {
            results.Clear();
            int candidateCount = QueryMonsterCircleCandidates(monsters, position, radius);
            float radiusSqr = radius * radius;
            for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
            {
                int storageIndex = _monsterQueryBuffer[candidateIndex];
                if (monsters.TryGetMonsterPosition(storageIndex, out Vector3 monsterPosition) &&
                    (monsterPosition - position).sqrMagnitude <= radiusSqr)
                    results.Add(storageIndex);
            }
        }

        private int QueryMonsterCircleCandidates(
            MonsterRuntimeService monsters,
            Vector3 position,
            float radius)
        {
            EnsureMonsterQueryCapacity(monsters.MonsterStorageCount);
            return monsters.QueryMonsterCircleCandidates(
                new Vector2(position.x, position.z),
                radius,
                _monsterQueryBuffer);
        }

        private void EnsureMonsterQueryCapacity(int storageCount)
        {
            if (_monsterQueryBuffer.Length >= storageCount)
                return;

            int capacity = Math.Max(
                storageCount,
                _monsterQueryBuffer.Length == 0 ? 64 : _monsterQueryBuffer.Length * 2);
            Array.Resize(ref _monsterQueryBuffer, capacity);
        }

        private void ApplyLightning(MonsterRuntimeService monsters, int targetStorageIndex,
            Vector3 origin, float damage, byte jumpCount, byte branchCount,
            float searchRadius, bool applyInitialDamage, float executeThreshold)
        {
            using var lightningScope = LightningMarker.Auto();
            if (applyInitialDamage)
            {
                bool hasInitialPosition =
                    monsters.TryGetMonsterPosition(targetStorageIndex, out Vector3 initialPosition);
                DamageMonster(monsters, targetStorageIndex, damage, executeThreshold);
                if (hasInitialPosition)
                    PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                        0, 0, origin + Vector3.up * 1.5f, initialPosition, 0f, 0.2f);
            }

            _lightningVisited.Clear();
            _lightningVisited.Add(targetStorageIndex);
            _lightningNodes.Clear();
            _lightningNodes.Add(new LightningNode(origin, 0));
            float radiusSqr = searchRadius * searchRadius;
            for (int nodeIndex = 0; nodeIndex < _lightningNodes.Count; nodeIndex++)
            {
                LightningNode node = _lightningNodes[nodeIndex];
                if (node.Depth >= jumpCount) continue;
                // 同一节点的分支查询范围和位置相同。分支只改变已访问集合，死亡目标会在 TryGetMonsterPosition 时被过滤。
                int candidateCount = QueryMonsterCircleCandidates(
                    monsters, node.Position, searchRadius);
                for (int branch = 0; branch < branchCount; branch++)
                {
                    int nearestStorageIndex = -1;
                    float nearestDistance = radiusSqr;
                    Vector3 nearestPosition = default;
                    for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
                    {
                        int storageIndex = _monsterQueryBuffer[candidateIndex];
                        if (_lightningVisited.Contains(storageIndex) ||
                            !monsters.TryGetMonsterPosition(storageIndex, out Vector3 candidatePosition))
                            continue;
                        float distance = (candidatePosition - node.Position).sqrMagnitude;
                        if (distance >= nearestDistance) continue;
                        nearestStorageIndex = storageIndex;
                        nearestDistance = distance;
                        nearestPosition = candidatePosition;
                    }
                    if (nearestStorageIndex < 0) break;
                    _lightningVisited.Add(nearestStorageIndex);
                    DamageMonster(monsters, nearestStorageIndex, damage, executeThreshold);
                    PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                        0, 0, node.Position, nearestPosition, 0f, 0.2f);
                    _lightningNodes.Add(new LightningNode(nearestPosition, (byte)(node.Depth + 1)));
                }
            }
        }

        private void TickStormClouds(float deltaTime)
        {
            using var stormCloudScope = StormCloudMarker.Auto();
            if (!_monsters.IsRoomActive) return;
            foreach (KeyValuePair<ulong, StormCloudState> pair in _stormClouds)
            {
                StormCloudState cloud = pair.Value;
                cloud.TimeUntilStrike -= deltaTime;
                if (cloud.TimeUntilStrike > 0f ||
                    !TryGetPlayerPosition(pair.Key, out Vector3 ownerPosition))
                    continue;
                cloud.TimeUntilStrike += cloud.StrikeInterval;
                CollectMonsters(_monsters, ownerPosition, cloud.Radius, _effectTargets);
                if (_effectTargets.Count == 0) continue;
                int storageIndex = _effectTargets[UnityEngine.Random.Range(0, _effectTargets.Count)];
                if (!_monsters.TryGetMonsterPosition(storageIndex, out Vector3 targetPosition)) continue;
                DamageMonster(_monsters, storageIndex, cloud.StrikeDamage, cloud.ExecuteThreshold);
                PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                    0, pair.Key, ownerPosition + Vector3.up * 4f,
                    targetPosition, 0f, 0.2f);
                if (cloud.OverloadJumpCount > 0)
                    ApplyLightning(_monsters, storageIndex, targetPosition, cloud.OverloadDamage,
                        cloud.OverloadJumpCount, 3, 5f, false, cloud.ExecuteThreshold);
            }
        }

        private void TickDamageAreas(float deltaTime)
        {
            using var damageAreaScope = DamageAreaMarker.Auto();
            if (!_monsters.IsRoomActive)
            {
                _damageAreas.Clear();
                return;
            }

            for (int i = _damageAreas.Count - 1; i >= 0; i--)
            {
                DamageAreaState area = _damageAreas[i];
                area.RemainingDuration -= deltaTime;
                if (area.RemainingDuration <= 0f)
                {
                    _damageAreas.RemoveAt(i);
                    continue;
                }
                area.TimeUntilTick -= deltaTime;
                if (area.TimeUntilTick > 0f) continue;
                area.TimeUntilTick += area.TickInterval;

                CollectMonsters(_monsters, area.Position, area.Radius, _effectTargets);
                for (int targetIndex = 0; targetIndex < _effectTargets.Count; targetIndex++)
                {
                    int storageIndex = _effectTargets[targetIndex];
                    DamageMonster(_monsters, storageIndex, area.DamagePerTick, area.ExecuteThreshold);
                    _monsters.SlowMonster(storageIndex, Mathf.Max(0.1f, 1f - area.SlowRatio), 1.2f, _currentTick);
                }
            }
        }

        private float GetExecuteThreshold(ushort effectSetId) =>
            ExecutionerEffectSystem.GetThreshold(_effectSets.Get(effectSetId));

        private void DamageMonster(MonsterRuntimeService monsters, int storageIndex,
            float damage, float executeThreshold)
        {
            if (!monsters.DamageMonster(storageIndex, damage) || executeThreshold <= 0f) return;
            TryExecuteMonster(monsters, storageIndex, executeThreshold);
        }

        private void TryExecuteMonster(MonsterRuntimeService monsters, int storageIndex, float threshold)
        {
            if (!monsters.TryGetMonsterHealthRatio(storageIndex, out float healthRatio) ||
                healthRatio > threshold ||
                !monsters.TryGetMonsterPosition(storageIndex, out Vector3 position)) return;
            monsters.DamageMonster(storageIndex, float.MaxValue);
            PublishSpecialVfx(WeaponSpecialVfxType.Execution,
                monsters.GetMonsterEntityId(storageIndex), 0,
                position, default, 1.8f, 0.8f);
        }

        private void HandleImpact(ProjectileImpact impact) => ProjectileImpact?.Invoke(impact);
        private void HandleRemoved(ProjectileState projectile) => ProjectileRemoved?.Invoke(projectile);

        private void PublishSpecialVfx(
            WeaponSpecialVfxType type,
            ulong instanceId,
            ulong ownerEntityId,
            Vector3 position,
            Vector3 targetPosition,
            float radius,
            float duration) =>
            SpecialEffectVfx?.Invoke(new WeaponSpecialVfxEvent(
                type, instanceId, ownerEntityId, position, targetPosition, radius, duration));

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            _specialEffects?.ClearOwners();
            if (_network != null)
            {
                _network.Clock.Unsubscribe(GameplayTickPhase.PhysicsSync, SyncPhysicsTransforms);
                _network.Clock.Unsubscribe(GameplayTickPhase.CombatSimulation, TickCombat);
            }
            if (_simulation != null)
            {
                _simulation.Impact -= HandleImpact;
                _simulation.Removed -= HandleRemoved;
            }
            _world.Clear(); 
            _shots.Clear(); 
            _stats.Clear();
            _effectSets.Clear();
            _spawnBuffer.Clear();
            _stormClouds.Clear();
            _damageAreas.Clear();
            _shieldCapacities.Clear();
            _effectTargets.Clear();
            _nextDamageAreaVfxId = 1;
            Catalog = null; 
            EffectCatalog = null;
            EffectRolls = null;
            _effectCalculator = null;
            _specialEffects = null;
            _monsters = null;
            _simulation = null; 
            _builder = null; 
            _network = null;
            ShotFired = null; 
            ProjectileSpawned = null;
            ProjectileImpact = null; 
            ProjectileRemoved = null;
            SpecialEffectVfx = null;
            IsInitialized = false;
            if (Instance == this) 
                Instance = null;
            return UniTask.CompletedTask;
        }
        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
