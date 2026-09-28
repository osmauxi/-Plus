using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Runtime;
using ProjectGame.HotFix.Gameplay.Weapon.Effects;
using ProjectGame.HotFix.Gameplay.Weapon.Effects.Special;
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
            if (rows == null) 
                throw new InvalidOperationException("Config_Weapon 尚未加载。");
            foreach (var pair in rows)
            {
                var row = pair.Value;
                WeaponConfigRules.Validate(row);
                if (pair.Key != row.WeaponID) 
                    throw new InvalidOperationException("Weapon 表键与 WeaponID 不一致。");
                if (!row.Enabled) 
                    continue;
                var stats = new WeaponStatSnapshot
                {
                    Id = checked((ushort)(row.WeaponID + 1)), 
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
            if (_definitions.Count == 0) 
                throw new InvalidOperationException("Weapon 表没有启用任何武器。");
        }

        public WeaponDefinition Get(int weaponId) => _definitions.TryGetValue(weaponId, out var definition)
            ? definition : throw new InvalidOperationException($"武器 {weaponId} 未配置或未启用。");
    }

    /// <summary>
    /// 场景级组合根，拥有仓储、子弹世界和生命周期,不拥有任何玩家的可回滚状态,只管子弹的状态
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponRuntimeService : MonoBehaviour, IGameRuntimeService, IWeaponEventSource,
        IWeaponSpecialEffectCommandSink
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _worldMask;
        public static WeaponRuntimeService Instance { get; private set; }
        public bool IsInitialized { get; private set; }
        public WeaponCatalog Catalog { get; private set; }
        public WeaponEffectCatalog EffectCatalog { get; private set; }
        public EffectRollService EffectRolls { get; private set; }
        public int ActiveProjectileCount => _world.Count;
        public int ActiveShotCount => _shots.Count;
        //给外部事件的接口，通过Simulation内部委托触发，作为一层内部与外部的解耦
        //不让外部直接依赖Simulation
        public event Action<ShotContext> ShotFired;
        public event Action<ProjectileSpawn> ProjectileSpawned;
        public event Action<ProjectileImpact> ProjectileImpact;
        public event Action<ProjectileState> ProjectileRemoved;
        public event Action<WeaponSpecialVfxEvent> SpecialEffectVfx;

        private readonly WeaponStatSnapshotRepository _stats = new();
        private readonly EffectSetRepository _effectSets = new();
        private readonly ShotRepository _shots = new();
        private readonly ProjectileWorld _world = new();
        private readonly List<ProjectileSpawn> _spawnBuffer = new(16);
        private readonly Dictionary<ulong, StormCloudState> _stormClouds = new();
        private readonly List<DamageAreaState> _damageAreas = new();
        private readonly Dictionary<ulong, Dictionary<ushort, float>> _shieldCapacities = new();
        private readonly List<int> _effectTargets = new(64);
        private WeaponStatModifierCalculator _effectCalculator;
        private WeaponSpecialEffectManager _specialEffects;
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
            if(Instance != null && Instance != this) 
                throw new InvalidOperationException("重复 WeaponRuntimeService。");
            _network = GameNetworkRuntime.Gameplay;
            if(_network == null || !_network.IsInitialized) 
                throw new InvalidOperationException("Weapon 必须在 GameNetworkRuntime 后初始化。");
            Catalog = new WeaponCatalog(ConfigManager.Instance.GetTable<Config_Weapon>(), _stats, _network.Clock.TickDeltaTime);
            EffectCatalog = new WeaponEffectCatalog(
                ConfigManager.Instance.GetTable<Config_Effect>(),
                ConfigManager.Instance.GetTable<Config_Modifier>(),
                ConfigManager.Instance.GetTable<Config_EffectRoll>());
            EffectRolls = new EffectRollService(EffectCatalog);
            _effectCalculator = new WeaponStatModifierCalculator(EffectCatalog);
            _specialEffects = new WeaponSpecialEffectManager(EffectCatalog, _effectSets);
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
            _specialEffects.ValidateRegistrations();

            foreach(var row in ConfigManager.Instance.GetTable<Config_Weapon>().Values)
            {
                if (!row.Enabled) 
                    continue;
                if (!WeaponConfigRules.IsSelectable(row.WeaponID)) 
                    throw new InvalidOperationException($"武器 {row.WeaponID} 缺少 Lobby 资源行。");
            }
            if ((_targetMask.value & _worldMask.value) != 0 || _targetMask.value == 0 || _worldMask.value == 0)
                throw new InvalidOperationException("Weapon 的 Target/World LayerMask 必须非空且不重叠。");
            _builder = new ShotBuilder(_shots, _world, _specialEffects);
            _simulation = new ProjectileSimulation(_world, _shots, _stats,
                new ProjectileSimulationConfig { TargetMask = _targetMask, WorldMask = _worldMask },
                _specialEffects);
            _simulation.Impact += HandleImpact;
            _simulation.Removed += HandleRemoved;
            _network.Clock.TickCompleted += Tick;
            Instance = this;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public PlayerEffectLoadout CreatePlayerEffectLoadout()
        {
            if (!IsInitialized) throw new InvalidOperationException("WeaponRuntimeService 尚未初始化。");
            return new PlayerEffectLoadout(EffectCatalog);
        }

        /// <summary>
        /// 服务端创建不可覆盖的新EffectSet与属性快照，供后续ShotContext持有
        /// 把玩家当前的可变Loadout正式提交成权威世界可引用的快照
        /// </summary>
        public WeaponEffectApplication RegisterPlayerEffectState(
            ulong ownerEntityId,
            WeaponDefinition definition,
            PlayerEffectLoadout loadout)
        {
            if (!IsInitialized)
               throw new InvalidOperationException("WeaponRuntimeService 尚未初始化。");
            if (!_network.Transport.IsServer) 
                throw new InvalidOperationException("只有服务端可以注册权威 Effect 快照。");
            if (definition == null || loadout == null) 
                throw new ArgumentNullException();
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
            if (!IsInitialized || _network == null || !_network.Transport.IsServer) return;
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
            if (!IsInitialized) 
                throw new InvalidOperationException("WeaponRuntimeService 尚未初始化。");
            if (definition == null || loadout == null) 
                throw new ArgumentNullException();
            EffectSet set = loadout.AcquisitionCount == 0 ? EffectSet.Empty : loadout.CreateSnapshot(1);
            return _effectCalculator.Calculate(definition.Stats, set, definition.Stats.Id,
                _network.Clock.TickDeltaTime);
        }

        // 只允许 PlayerWeaponController 在服务端权威 Tick 确认新序号后调用。
        internal void CommitShot(ulong ownerEntityId, in WeaponRuntimeState state, uint tick,
            Vector3 origin, Vector3 direction)
        {
            if (!IsInitialized || !_network.Transport.IsServer) 
                return;
            //根据权威ID拿到对应数据快照和Effect效果
            WeaponStatSnapshot stats = _stats.Get(state.StatSnapshotId);
            EffectSet effects = state.EffectSetId == 0 ? EffectSet.Empty : _effectSets.Get(state.EffectSetId);
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
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null || !monsters.IsRoomActive) return;
            int targetSlot = ToMonsterSlot(command.TargetEntityId);
            float executeThreshold = GetExecuteThreshold(command.EffectSetId);
            ApplyLightning(monsters, targetSlot, command.Origin, command.Damage,
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
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null) return;
            TryExecuteMonster(monsters, ToMonsterSlot(command.TargetEntityId),
                command.HealthThreshold);
        }

        public void ApplyCrowdControl(in CrowdControlEffectCommand command)
        {
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            int slot = ToMonsterSlot(command.TargetEntityId);
            if (monsters == null ||
                !monsters.TryGetMonsterPosition(slot, out Vector3 position)) return;
            monsters.StunMonster(slot, command.Duration, _currentTick);
            PublishSpecialVfx(WeaponSpecialVfxType.KineticBoost,
                command.TargetEntityId, 0,
                position, default, 0.7f, command.Duration);
        }

        /// <summary>分裂弹仍属于同一个 Shot；这里扩展引用计数并发布普通出生事件。</summary>
        public void SpawnProjectiles(in SplitProjectileEffectCommand command)
        {
            if (!IsInitialized || !_network.Transport.IsServer || command.SplitCount == 0) return;
            ShotContext shot = command.Context.Shot;
            WeaponStatSnapshot stats = command.Context.Stats;
            Vector3 forward = command.Parent.Velocity.sqrMagnitude > Mathf.Epsilon
                ? command.Parent.Velocity.normalized : shot.AimDirection.normalized;
            float speed = command.Parent.Velocity.magnitude;
            if (speed <= Mathf.Epsilon) speed = stats.ProjectileSpeed;

            _shots.RetainProjectiles(shot.ShotId, command.SplitCount);
            for (int i = 0; i < command.SplitCount; i++)
            {
                float t = command.SplitCount == 1 ? 0.5f : i / (float)(command.SplitCount - 1);
                float angle = Mathf.Lerp(-command.SpreadAngle * 0.5f,
                    command.SpreadAngle * 0.5f, t);
                Vector3 direction = (Quaternion.AngleAxis(angle, Vector3.up) * forward).normalized;
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
                    Generation = checked((byte)(command.Parent.Generation + 1)),
                    Flags = command.Parent.Flags & ProjectileFlags.Critical,
                };
                _specialEffects.DispatchSpawn(ref projectile,
                    new ProjectileSpawnEffectContext(shot, stats));
                _world.Add(projectile);
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
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null || !monsters.IsRoomActive) return;
            float executeThreshold = GetExecuteThreshold(command.EffectSetId);
            PublishSpecialVfx(WeaponSpecialVfxType.Shockwave,
                0, 0,
                command.Position, default, command.Radius, 0.6f);
            CollectMonsters(monsters, command.Position, command.Radius, _effectTargets);
            for (int i = 0; i < _effectTargets.Count; i++)
            {
                int slot = _effectTargets[i];
                if (!monsters.TryGetMonsterPosition(slot, out Vector3 position)) continue;
                Vector3 offset = position - command.Position;
                float forceRatio = 1f - Mathf.Clamp01(offset.magnitude / command.Radius);
                DamageMonster(monsters, slot, command.Damage, executeThreshold);
                Vector2 direction = new Vector2(offset.x, offset.z).normalized;
                monsters.PushMonster(slot, direction * (command.Force * forceRatio * 0.1f));
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

        private void Tick(uint tick)
        {
            if (!_network.Transport.IsServer) 
                return;
            _currentTick = tick;
            //强行更新玩家本Tick的移动，再进行子弹Tick
            Physics.SyncTransforms();
            _simulation.Tick(_network.Clock.TickDeltaTime);
            TickStormClouds(_network.Clock.TickDeltaTime);
            TickDamageAreas(_network.Clock.TickDeltaTime);
        }

        private static int ToMonsterSlot(ulong entityId) =>
            checked((int)(entityId - MonsterRuntimeService.MonsterEntityIdBase));

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

        private static void CollectMonsters(MonsterRuntimeService monsters, Vector3 position,
            float radius, List<int> results)
        {
            results.Clear();
            float radiusSqr = radius * radius;
            for (int slot = 0; slot < monsters.MonsterSlotCount; slot++)
                if (monsters.TryGetMonsterPosition(slot, out Vector3 monsterPosition) &&
                    (monsterPosition - position).sqrMagnitude <= radiusSqr)
                    results.Add(slot);
        }

        private void ApplyLightning(MonsterRuntimeService monsters, int targetSlot,
            Vector3 origin, float damage, byte jumpCount, byte branchCount,
            float searchRadius, bool applyInitialDamage, float executeThreshold)
        {
            if (applyInitialDamage)
            {
                bool hasInitialPosition =
                    monsters.TryGetMonsterPosition(targetSlot, out Vector3 initialPosition);
                DamageMonster(monsters, targetSlot, damage, executeThreshold);
                if (hasInitialPosition)
                    PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                        0, 0, origin + Vector3.up * 1.5f, initialPosition, 0f, 0.2f);
            }

            var visited = new HashSet<int> { targetSlot };
            var nodes = new List<LightningNode> { new(origin, 0) };
            float radiusSqr = searchRadius * searchRadius;
            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                LightningNode node = nodes[nodeIndex];
                if (node.Depth >= jumpCount) continue;
                for (int branch = 0; branch < branchCount; branch++)
                {
                    int nearestSlot = -1;
                    float nearestDistance = radiusSqr;
                    Vector3 nearestPosition = default;
                    for (int slot = 0; slot < monsters.MonsterSlotCount; slot++)
                    {
                        if (visited.Contains(slot) ||
                            !monsters.TryGetMonsterPosition(slot, out Vector3 candidatePosition))
                            continue;
                        float distance = (candidatePosition - node.Position).sqrMagnitude;
                        if (distance >= nearestDistance) continue;
                        nearestSlot = slot;
                        nearestDistance = distance;
                        nearestPosition = candidatePosition;
                    }
                    if (nearestSlot < 0) break;
                    visited.Add(nearestSlot);
                    DamageMonster(monsters, nearestSlot, damage, executeThreshold);
                    PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                        0, 0, node.Position, nearestPosition, 0f, 0.2f);
                    nodes.Add(new LightningNode(nearestPosition, checked((byte)(node.Depth + 1))));
                }
            }
        }

        private void TickStormClouds(float deltaTime)
        {
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null || !monsters.IsRoomActive) return;
            foreach (KeyValuePair<ulong, StormCloudState> pair in _stormClouds)
            {
                StormCloudState cloud = pair.Value;
                cloud.TimeUntilStrike -= deltaTime;
                if (cloud.TimeUntilStrike > 0f ||
                    !TryGetPlayerPosition(pair.Key, out Vector3 ownerPosition))
                    continue;
                cloud.TimeUntilStrike += cloud.StrikeInterval;
                CollectMonsters(monsters, ownerPosition, cloud.Radius, _effectTargets);
                if (_effectTargets.Count == 0) continue;
                int slot = _effectTargets[UnityEngine.Random.Range(0, _effectTargets.Count)];
                if (!monsters.TryGetMonsterPosition(slot, out Vector3 targetPosition)) continue;
                DamageMonster(monsters, slot, cloud.StrikeDamage, cloud.ExecuteThreshold);
                PublishSpecialVfx(WeaponSpecialVfxType.LightningArc,
                    0, pair.Key, ownerPosition + Vector3.up * 4f,
                    targetPosition, 0f, 0.2f);
                if (cloud.OverloadJumpCount > 0)
                    ApplyLightning(monsters, slot, targetPosition, cloud.OverloadDamage,
                        cloud.OverloadJumpCount, 3, 5f, false, cloud.ExecuteThreshold);
            }
        }

        private void TickDamageAreas(float deltaTime)
        {
            MonsterRuntimeService monsters = MonsterRuntimeService.Instance;
            if (monsters == null || !monsters.IsRoomActive)
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

                CollectMonsters(monsters, area.Position, area.Radius, _effectTargets);
                for (int targetIndex = 0; targetIndex < _effectTargets.Count; targetIndex++)
                {
                    int slot = _effectTargets[targetIndex];
                    DamageMonster(monsters, slot, area.DamagePerTick, area.ExecuteThreshold);
                    monsters.SlowMonster(slot, Mathf.Max(0.1f, 1f - area.SlowRatio), 1.2f, _currentTick);
                }
            }
        }

        private float GetExecuteThreshold(ushort effectSetId) =>
            ExecutionerEffectSystem.GetThreshold(
                effectSetId == 0 ? EffectSet.Empty : _effectSets.Get(effectSetId));

        private void DamageMonster(MonsterRuntimeService monsters, int slot,
            float damage, float executeThreshold)
        {
            if (!monsters.DamageMonster(slot, damage) || executeThreshold <= 0f) return;
            TryExecuteMonster(monsters, slot, executeThreshold);
        }

        private void TryExecuteMonster(MonsterRuntimeService monsters, int slot, float threshold)
        {
            if (!monsters.TryGetMonsterHealthRatio(slot, out float healthRatio) ||
                healthRatio > threshold ||
                !monsters.TryGetMonsterPosition(slot, out Vector3 position)) return;
            monsters.DamageMonster(slot, float.MaxValue);
            PublishSpecialVfx(WeaponSpecialVfxType.Execution,
                MonsterRuntimeService.MonsterEntityIdBase + (uint)slot, 0,
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
                _network.Clock.TickCompleted -= Tick;
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
