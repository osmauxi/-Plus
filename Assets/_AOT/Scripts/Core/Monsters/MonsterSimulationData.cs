using UnityEngine;

namespace ProjectGame.Core.Monsters
{
    public enum MonsterAttackPhase : byte
    {
        Ready,
        Windup,
        Recovery,
    }

    public struct MonsterHealthData
    {
        public float CurrentHealth;
    }

    public struct MonsterStatusData
    {
        public float SpeedMultiplier;
        public uint SlowEndTick;
        public uint StunEndTick;
    }

    public struct MonsterAttackData
    {
        public MonsterAttackPhase Phase;
        public uint PhaseEndTick;
    }

    public struct MonsterPresentationData
    {
        public byte AttackSequence;
        public byte HitSequence;
    }

    /// <summary>
    /// EntityTick 只读配置。它不携带 HotFix Catalog 或攻击物理对象。
    /// </summary>
    public readonly struct MonsterSimulationConfig
    {
        public readonly float MoveSpeed;
        public readonly float AttackRangeSqr;
        public readonly uint WindupTicks;
        public readonly uint RecoveryTicks;
        public readonly float AttackDamage;
        public readonly ushort AttackProfileIndex;

        public MonsterSimulationConfig(
            float moveSpeed,
            float attackRangeSqr,
            uint windupTicks,
            uint recoveryTicks,
            float attackDamage,
            ushort attackProfileIndex)
        {
            MoveSpeed = moveSpeed;
            AttackRangeSqr = attackRangeSqr;
            WindupTicks = windupTicks;
            RecoveryTicks = recoveryTicks;
            AttackDamage = attackDamage;
            AttackProfileIndex = attackProfileIndex;
        }
    }

    /// <summary>
    /// AOT Kernel 输出的近战请求。Unity Physics 和攻击配置对象留在 HotFix 边界外。
    /// </summary>
    public readonly struct MonsterAttackRequestData
    {
        public readonly int StorageIndex;
        public readonly ushort AttackProfileIndex;
        public readonly float Damage;

        public MonsterAttackRequestData(
            int storageIndex,
            ushort attackProfileIndex,
            float damage)
        {
            StorageIndex = storageIndex;
            AttackProfileIndex = attackProfileIndex;
            Damage = damage;
        }
    }
}
