using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Player;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public sealed class MonsterPlayerTargetBuffer
    {
        public const int MaximumCapacity = 4;
        private readonly MonsterPlayerTarget[] _items = new MonsterPlayerTarget[MaximumCapacity];
        public int Count { get; private set; }
        public MonsterPlayerTarget this[int index] => (uint)index < (uint)Count ? _items[index] : throw new IndexOutOfRangeException();
        internal MonsterPlayerTarget[] Items => _items;

        public void Clear() => Count = 0;

        public void Add(ulong clientId, Vector2 position)
        {
            if (Count >= MaximumCapacity) throw new InvalidOperationException("Monster 索敌快照最多容纳四名玩家。");
            for (int i = 0; i < Count; i++)
                if (_items[i].ClientId == clientId) throw new InvalidOperationException($"重复玩家 ClientId={clientId}。");
            _items[Count++] = new MonsterPlayerTarget(clientId, position);
        }

        public void Capture(IReadOnlyList<MonsterPlayerTarget> targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (targets.Count > MaximumCapacity) throw new ArgumentException("Monster 索敌快照最多容纳四名玩家。", nameof(targets));
            Clear();
            for (int i = 0; i < targets.Count; i++) Add(targets[i].ClientId, targets[i].Position);
        }
    }

    /// <summary>每个 Simulation Tick 只遍历一次 PlayerManager/Transform。</summary>
    public static class MonsterPlayerTargetCapture
    {
        public static void Capture(PlayerManager manager, MonsterPlayerTargetBuffer destination)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();
            IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (player == null || !player.isActiveAndEnabled || !player.IsSpawned) continue;
                Vector3 position = player.transform.position;
                destination.Add(player.ClientId, new Vector2(position.x, position.z));
            }
        }
    }

    public readonly struct MonsterAttackRequest
    {
        public readonly int Slot;
        public readonly ushort ConfigIndex;
        public readonly ulong TargetClientId;
        public readonly uint Tick;
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;
        public readonly float Radius;
        public readonly float Distance;
        public readonly LayerMask TargetMask;
        public readonly float Damage;

        public MonsterAttackRequest(int slot, ushort configIndex, ulong targetClientId, uint tick,
            Vector3 origin, Vector3 direction, float radius, float distance, LayerMask targetMask, float damage)
        {
            Slot = slot;
            ConfigIndex = configIndex;
            TargetClientId = targetClientId;
            Tick = tick;
            Origin = origin;
            Direction = direction;
            Radius = radius;
            Distance = distance;
            TargetMask = targetMask;
            Damage = damage;
        }
    }

    /// <summary>纯状态机只输出命中请求；Physics 与 Health 桥接由组合层实现。</summary>
    public interface IMonsterAttackResolver
    {
        void Execute(in MonsterAttackRequest request);
    }

    public sealed class NullMonsterAttackResolver : IMonsterAttackResolver
    {
        public static readonly NullMonsterAttackResolver Instance = new NullMonsterAttackResolver();
        private NullMonsterAttackResolver() { }
        public void Execute(in MonsterAttackRequest request) { }
    }

    internal static class MonsterTargetSystem
    {
        internal static void Tick(MonsterWorld world, MonsterPlayerTargetBuffer players)
        {
            MonsterMetaData[] meta = world.Meta;
            MonsterMotionData[] motion = world.Motion;
            MonsterTargetData[] target = world.Target;
            MonsterPlayerTarget[] playerItems = players.Items;
            int playerCount = players.Count;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!meta[slot].IsActive) continue;
                float nearestDistance = float.MaxValue;
                int nearest = -1;
                Vector2 position = motion[slot].Position;
                for (int i = 0; i < playerCount; i++)
                {
                    float distance = (playerItems[i].Position - position).sqrMagnitude;
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    nearest = i;
                }
                target[slot] = nearest < 0 ? new MonsterTargetData { DistanceSqr = float.MaxValue } :
                    new MonsterTargetData
                    {
                        ClientId = playerItems[nearest].ClientId,
                        Position = playerItems[nearest].Position,
                        DistanceSqr = nearestDistance,
                        HasTarget = true,
                    };
            }
        }
    }

    internal static class MonsterAttackSystem
    {
        internal static void Tick(MonsterWorld world, MonsterRuntimeCatalog configs,
            MonsterAttackCatalog attackProfiles, IMonsterAttackResolver resolver, uint tick, float groundY)
        {
            MonsterMetaData[] meta = world.Meta;
            MonsterMotionData[] motion = world.Motion;
            MonsterTargetData[] target = world.Target;
            MonsterAttackData[] attack = world.Attack;
            MonsterPresentationData[] presentation = world.Presentation;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!meta[slot].IsActive) continue;
                ref readonly MonsterRuntimeConfig config = ref configs.Get(meta[slot].ConfigIndex);
                switch (attack[slot].Phase)
                {
                    case MonsterAttackPhase.Ready:
                        if (!target[slot].HasTarget || target[slot].DistanceSqr > config.AttackRangeSqr) break;
                        attack[slot].Phase = MonsterAttackPhase.Windup;
                        attack[slot].PhaseEndTick = unchecked(tick + config.WindupTicks);
                        presentation[slot].AttackSequence++;
                        break;
                    case MonsterAttackPhase.Windup:
                        if (!MonsterMath.HasReached(tick, attack[slot].PhaseEndTick)) break;
                        Execute(slot, meta[slot].ConfigIndex, tick, motion[slot], target[slot],
                            config, attackProfiles.Get(config.AttackProfileIndex), resolver, groundY);
                        attack[slot].Phase = MonsterAttackPhase.Recovery;
                        attack[slot].PhaseEndTick = unchecked(tick + config.RecoveryTicks);
                        break;
                    case MonsterAttackPhase.Recovery:
                        if (!MonsterMath.HasReached(tick, attack[slot].PhaseEndTick)) break;
                        attack[slot].Phase = MonsterAttackPhase.Ready;
                        attack[slot].PhaseEndTick = 0;
                        break;
                    default:
                        throw new InvalidOperationException($"Monster Slot={slot} AttackPhase 非法。");
                }
            }
        }

        private static void Execute(int slot, ushort configIndex, uint tick,
            in MonsterMotionData motion, in MonsterTargetData target,
            in MonsterRuntimeConfig config, in MonsterAttackProfile profile,
            IMonsterAttackResolver resolver, float groundY)
        {
            Quaternion rotation = Quaternion.Euler(0, motion.Yaw, 0);
            Vector3 root = new Vector3(motion.Position.x, groundY, motion.Position.y);
            resolver.Execute(new MonsterAttackRequest(slot, configIndex, target.ClientId, tick,
                root + rotation * profile.OriginOffset, rotation * Vector3.forward,
                profile.Radius, profile.Distance, profile.TargetMask, config.AttackDamage));
        }
    }

    internal static class MonsterMoveSystem
    {
        internal static void Tick(MonsterWorld world, MonsterRuntimeCatalog configs)
        {
            MonsterMetaData[] meta = world.Meta;
            MonsterMotionData[] motion = world.Motion;
            MonsterTargetData[] target = world.Target;
            MonsterAttackData[] attack = world.Attack;
            MonsterMoveData[] move = world.Move;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!meta[slot].IsActive) continue;
                if (attack[slot].Phase != MonsterAttackPhase.Ready || !target[slot].HasTarget)
                {
                    move[slot].DesiredVelocity = Vector2.zero;
                    continue;
                }
                Vector2 offset = target[slot].Position - motion[slot].Position;
                ref readonly MonsterRuntimeConfig config = ref configs.Get(meta[slot].ConfigIndex);
                move[slot].DesiredVelocity = offset.sqrMagnitude > 0.000001f
                    ? offset.normalized * config.MoveSpeed : Vector2.zero;
            }
        }
    }

    internal static class MonsterMotorSystem
    {
        internal static void Tick(MonsterWorld world, float deltaTime)
        {
            MonsterMetaData[] meta = world.Meta;
            MonsterMoveData[] move = world.Move;
            MonsterMotionData[] motion = world.Motion;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                if (!meta[slot].IsActive) continue;
                Vector2 velocity = move[slot].DesiredVelocity;
                motion[slot].Velocity = velocity;
                motion[slot].Position += velocity * deltaTime;
                if (velocity.sqrMagnitude > 0.000001f)
                    motion[slot].Yaw = Mathf.Atan2(velocity.x, velocity.y) * Mathf.Rad2Deg;
            }
        }
    }

    /// <summary>服务器端批处理调度：Target → Attack → Move → Motor。</summary>
    public sealed class MonsterSimulation
    {
        private readonly MonsterRuntimeCatalog _configs;
        private readonly MonsterAttackCatalog _attackProfiles;
        private readonly IMonsterAttackResolver _attackResolver;

        public MonsterSimulation(MonsterRuntimeCatalog configs, MonsterAttackCatalog attackProfiles,
            IMonsterAttackResolver attackResolver = null)
        {
            _configs = configs ?? throw new ArgumentNullException(nameof(configs));
            _attackProfiles = attackProfiles ?? throw new ArgumentNullException(nameof(attackProfiles));
            _attackResolver = attackResolver ?? NullMonsterAttackResolver.Instance;
        }

        public void Tick(MonsterWorld world, MonsterPlayerTargetBuffer players, uint tick, float deltaTime,
            float groundY = 0f)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (!MonsterMath.IsPositiveFinite(deltaTime)) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!MonsterMath.IsFinite(groundY)) throw new ArgumentOutOfRangeException(nameof(groundY));
            MonsterTargetSystem.Tick(world, players);
            MonsterAttackSystem.Tick(world, _configs, _attackProfiles, _attackResolver, tick, groundY);
            MonsterMoveSystem.Tick(world, _configs);
            MonsterMotorSystem.Tick(world, deltaTime);
        }
    }
}
