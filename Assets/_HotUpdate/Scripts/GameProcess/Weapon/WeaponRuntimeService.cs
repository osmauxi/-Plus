using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Network;
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

        private readonly WeaponStatSnapshotRepository _stats = new();
        private readonly EffectSetRepository _effectSets = new();
        private readonly ShotRepository _shots = new();
        private readonly ProjectileWorld _world = new();
        private readonly List<ProjectileSpawn> _spawnBuffer = new(16);
        private IWeaponSpecialEffectCommandSink _specialEffectCommands = NoneWeaponSpecialEffectCommandSink.Instance;
        private IProjectileTargetResolver _projectileTargets = ComponentProjectileTargetResolver.Instance;
        private WeaponStatModifierCalculator _effectCalculator;
        private WeaponSpecialEffectManager _specialEffects;
        private GameplayNetworkRuntime _network;
        private ShotBuilder _builder;
        private ProjectileSimulation _simulation;

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
                _specialEffects, _projectileTargets);
            _simulation.Impact += HandleImpact;
            _simulation.Removed += HandleRemoved;
            _network.Clock.TickCompleted += Tick;
            Instance = this;
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        /// <summary>Health/对象池等组合根在初始化前注入；特殊脚本只依赖该命令端口。</summary>
        public void SetSpecialEffectCommandSink(IWeaponSpecialEffectCommandSink commands)
        {
            if (IsInitialized) throw new InvalidOperationException("特殊 Effect 命令端口必须在 Weapon 初始化前注入。");
            _specialEffectCommands = commands == null || ReferenceEquals(commands, this)
                ? NoneWeaponSpecialEffectCommandSink.Instance : commands;
        }

        /// <summary>
        /// 初始化前注入集中命中解析链。Monster 接入时应组合 MonsterProjectileHealthResolver 与旧组件兼容 Resolver。
        /// </summary>
        public void SetProjectileTargetResolver(IProjectileTargetResolver resolver)
        {
            if (IsInitialized) throw new InvalidOperationException("Projectile TargetResolver 必须在 Weapon 初始化前注入。");
            _projectileTargets = resolver ?? ComponentProjectileTargetResolver.Instance;
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
            PlayerEffectLoadout loadout,
            float baseShieldCapacity = 0f,
            ushort acquiredEffectId = 0)
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
                    definition.Stats, new EffectOwnerStatSnapshot(baseShieldCapacity));
                _specialEffects.DispatchOwnerState(ownerEntityId, EffectSet.Empty, definition.Stats);
                return new WeaponEffectApplication(EffectSet.Empty, baseCalculation);
            }

            EffectSet set = loadout.CreateSnapshot(_effectSets.AllocateId());
            //将新EffectSet放进仓库
            _effectSets.Register(set);
            //算出新的EffectOwnerStatSnapshot
            WeaponEffectCalculation calculation = _effectCalculator.Calculate(
                definition.Stats, set, _stats.AllocateRuntimeId(), _network.Clock.TickDeltaTime,
                baseShieldCapacity);
            //注册EffectOwnerStatSnapshot
            _stats.Register(calculation.WeaponStats);
            //触发特殊的装备Effect效果
            _specialEffects.DispatchOwnerState(ownerEntityId, set, calculation.WeaponStats);
            return new WeaponEffectApplication(set, calculation);
        }

        /// <summary>玩家离开或回池时移除跟随玩家的常驻特殊 Effect。</summary>
        public void RemovePlayerEffectState(ulong ownerEntityId)
        {
            if (!IsInitialized || _network == null || !_network.Transport.IsServer) return;
            _specialEffects.RemoveOwner(ownerEntityId);
        }

        /// <summary>
        /// 客户端预测只重算数值，不注册权威ID，也不会执行特殊玩法
        /// </summary>
        public WeaponEffectCalculation PreviewPlayerEffectState(
            WeaponDefinition definition,
            PlayerEffectLoadout loadout,
            float baseShieldCapacity = 0f)
        {
            if (!IsInitialized) 
                throw new InvalidOperationException("WeaponRuntimeService 尚未初始化。");
            if (definition == null || loadout == null) 
                throw new ArgumentNullException();
            EffectSet set = loadout.AcquisitionCount == 0 ? EffectSet.Empty : loadout.CreateSnapshot(1);
            return _effectCalculator.Calculate(definition.Stats, set, definition.Stats.Id,
                _network.Clock.TickDeltaTime, baseShieldCapacity);
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

        public void EmitLightning(in LightningEffectCommand command) =>
            _specialEffectCommands.EmitLightning(command);

        public void SpawnStormCloud(in StormCloudEffectCommand command) =>
            _specialEffectCommands.SpawnStormCloud(command);

        public void RemoveOwnerEffect(in OwnerEffectRemovalCommand command) =>
            _specialEffectCommands.RemoveOwnerEffect(command);

        public void AddShield(in ShieldEffectCommand command) =>
            _specialEffectCommands.AddShield(command);

        public void SetShieldCapacity(in ShieldCapacityEffectCommand command) =>
            _specialEffectCommands.SetShieldCapacity(command);

        public void TryExecute(in ExecuteEffectCommand command) =>
            _specialEffectCommands.TryExecute(command);

        public void ApplyCrowdControl(in CrowdControlEffectCommand command) =>
            _specialEffectCommands.ApplyCrowdControl(command);

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
                ProjectileSpawned?.Invoke(new ProjectileSpawn(
                    shot, projectile, stats.ProjectileSize * projectile.SizeMultiplier));
            }
        }

        public void EmitRadialImpact(in RadialImpactEffectCommand command) =>
            _specialEffectCommands.EmitRadialImpact(command);

        public void UpsertDamageArea(in PersistentAreaEffectCommand command) =>
            _specialEffectCommands.UpsertDamageArea(command);

        private void Tick(uint tick)
        {
            if (!_network.Transport.IsServer) 
                return;
            //强行更新玩家本Tick的移动，再进行子弹Tick
            Physics.SyncTransforms();
            _simulation.Tick(_network.Clock.TickDeltaTime);
        }
        private void HandleImpact(ProjectileImpact impact) => ProjectileImpact?.Invoke(impact);
        private void HandleRemoved(ProjectileState projectile) => ProjectileRemoved?.Invoke(projectile);

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
            Catalog = null; 
            EffectCatalog = null;
            EffectRolls = null;
            _effectCalculator = null;
            _specialEffects = null;
            _projectileTargets = ComponentProjectileTargetResolver.Instance;
            _simulation = null; 
            _builder = null; 
            _network = null;
            ShotFired = null; 
            ProjectileSpawned = null;
            ProjectileImpact = null; 
            ProjectileRemoved = null;
            IsInitialized = false;
            if (Instance == this) 
                Instance = null;
            return UniTask.CompletedTask;
        }
        private void OnDestroy() => ShutdownAsync(CancellationToken.None).Forget();
    }
}
