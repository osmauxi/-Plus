using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Player;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 单个怪物房的纯 C# 组合根。MonoBehaviour/网络层只负责调用 BeginRoom、Spawn、Tick、CaptureFrame、EndRoom。
    /// HealthRuntime 可与更高层共享，生命周期不归本类所有。
    /// </summary>
    public sealed class MonsterRoomRuntime : IDisposable
    {
        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterPoseQuantizer _quantizer;
        private readonly HealthRuntime _health;
        private readonly MonsterSimulation _simulation;
        private readonly MonsterPlayerTargetBuffer _players = new MonsterPlayerTargetBuffer();
        private readonly bool _stepHealthInternally;
        private float _groundY;
        private bool _disposed;

        public MonsterWorld World { get; }
        public MonsterHealthBinding HealthBinding { get; }
        public bool IsRoomActive { get; private set; }

        public MonsterRoomRuntime(MonsterRuntimeCatalog configs, MonsterAttackCatalog attackProfiles,
            MonsterPoseQuantizer quantizer, HealthRuntime health, IMonsterAttackResolver attackResolver = null,
            int initialCapacity = 64, ulong healthEntityIdBase = 0, bool stepHealthInternally = true)
            : this(configs, attackProfiles, quantizer, health, _ => attackResolver,
                initialCapacity, healthEntityIdBase, stepHealthInternally)
        {
        }

        public static MonsterRoomRuntime CreateWithAttackResolverFactory(
            MonsterRuntimeCatalog configs, MonsterAttackCatalog attackProfiles,
            MonsterPoseQuantizer quantizer, HealthRuntime health,
            Func<MonsterHealthBinding, IMonsterAttackResolver> attackResolverFactory,
            int initialCapacity = 64, ulong healthEntityIdBase = 0, bool stepHealthInternally = true)
        {
            if (attackResolverFactory == null) throw new ArgumentNullException(nameof(attackResolverFactory));
            return new MonsterRoomRuntime(configs, attackProfiles, quantizer, health,
                attackResolverFactory, initialCapacity, healthEntityIdBase, stepHealthInternally);
        }

        private MonsterRoomRuntime(MonsterRuntimeCatalog configs, MonsterAttackCatalog attackProfiles,
            MonsterPoseQuantizer quantizer, HealthRuntime health,
            Func<MonsterHealthBinding, IMonsterAttackResolver> attackResolverFactory,
            int initialCapacity, ulong healthEntityIdBase, bool stepHealthInternally)
        {
            _configs = configs ?? throw new ArgumentNullException(nameof(configs));
            if (attackProfiles == null) throw new ArgumentNullException(nameof(attackProfiles));
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _stepHealthInternally = stepHealthInternally;
            for (int i = 0; i < configs.Count; i++)
            {
                ref readonly MonsterRuntimeConfig config = ref configs.Get(checked((ushort)i));
                attackProfiles.Get(config.AttackProfileIndex);
            }
            World = new MonsterWorld(initialCapacity);
            HealthBinding = new MonsterHealthBinding(World, health, healthEntityIdBase);
            IMonsterAttackResolver attackResolver = attackResolverFactory?.Invoke(HealthBinding);
            _simulation = new MonsterSimulation(configs, attackProfiles, attackResolver);
        }

        public void BeginRoom(int expectedTotalMonsterCount = 0, float groundY = 0f)
        {
            EnsureUsable();
            if (expectedTotalMonsterCount < 0) throw new ArgumentOutOfRangeException(nameof(expectedTotalMonsterCount));
            if (!MonsterMath.IsFinite(groundY)) throw new ArgumentOutOfRangeException(nameof(groundY));
            if (IsRoomActive) throw new InvalidOperationException("Monster 房间已经开始，请先 EndRoom。");
            // 顺序固定：先释放仍引用旧 Slot 的 Health，再允许 World 从 Slot 0 复用。
            HealthBinding.Clear();
            World.Clear();
            World.EnsureCapacity(expectedTotalMonsterCount);
            _players.Clear();
            _groundY = groundY;
            IsRoomActive = true;
        }

        public MonsterSpawnBatch Spawn(IReadOnlyList<MonsterSpawnPlan> plans)
        {
            EnsureActive();
            if (plans == null) throw new ArgumentNullException(nameof(plans));
            int startSlot = World.SlotCount;
            World.EnsureCapacity(checked(startSlot + plans.Count));
            try
            {
                for (int i = 0; i < plans.Count; i++)
                {
                    MonsterSpawnPlan plan = plans[i];
                    ref readonly MonsterRuntimeConfig config = ref _configs.Get(plan.ConfigIndex);
                    int slot = World.Create(plan.ConfigIndex, plan.Position, plan.Yaw);
                    HealthBinding.Register(slot, config.HealthProfileId);
                }
            }
            catch
            {
                // Batch 尚未发布，允许原子回滚尾部；正常运行中已发布 Slot 仍永不复用。
                for (int slot = startSlot; slot < World.SlotCount; slot++)
                    if (HealthBinding.TryGetHealth(slot, out _)) HealthBinding.Unregister(slot);
                World.RollbackUnpublishedTail(startSlot);
                throw;
            }
            return MonsterSpawnBatch.FromPlans(startSlot, plans, _quantizer);
        }

        public void Tick(PlayerManager manager, uint tick, float deltaTime)
        {
            EnsureActive();
            MonsterPlayerTargetCapture.Capture(manager, _players);
            TickCapturedTargets(tick, deltaTime);
        }

        public void Tick(IReadOnlyList<MonsterPlayerTarget> players, uint tick, float deltaTime)
        {
            EnsureActive();
            _players.Capture(players);
            TickCapturedTargets(tick, deltaTime);
        }

        public MonsterPresentationFrame CaptureFrame(uint serverTick)
        {
            EnsureActive();
            return MonsterPresentationPackingSystem.Capture(World, serverTick, _quantizer);
        }

        public void EndRoom()
        {
            EnsureUsable();
            if (!IsRoomActive) return;
            HealthBinding.Clear();
            World.Clear();
            _players.Clear();
            _groundY = 0f;
            IsRoomActive = false;
        }

        private void TickCapturedTargets(uint tick, float deltaTime)
        {
            _simulation.Tick(World, _players, tick, deltaTime, _groundY);
            if (_stepHealthInternally) _health.Step(tick);
        }

        private void EnsureActive()
        {
            EnsureUsable();
            if (!IsRoomActive) throw new InvalidOperationException("Monster 房间尚未 BeginRoom。");
        }

        private void EnsureUsable()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MonsterRoomRuntime));
        }

        public void Dispose()
        {
            if (_disposed) return;
            EndRoom();
            HealthBinding.Dispose();
            _disposed = true;
        }
    }
}
