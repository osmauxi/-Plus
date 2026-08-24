using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>单次玩家移动模拟所需的完整输入意图。</summary>
    public readonly struct PlayerLocomotionInput
    {
        /// <summary>XZ 世界平面的移动意图，长度应在 0~1；Motor 会再次应用移动死区。</summary>
        public Vector3 WorldMove { get; }
        /// <summary>XZ 世界平面的瞄准方向；有效输入应为单位方向，零向量表示没有瞄准目标。</summary>
        public Vector3 AimDirection { get; }
        /// <summary>本 Tick 是否持续按住瞄准。</summary>
        public bool AimHeld { get; }
        /// <summary>本 Tick 是否持续按住冲刺。</summary>
        public bool SprintHeld { get; }
        /// <summary>本 Tick 是否持续按住射击；状态机会按固定冷却产生离散 ShotSequence。</summary>
        public bool FireHeld { get; }
        /// <summary>每次 Reload 按下递增一次的边沿序号；不是换弹状态，也不能每 Tick 自动递增。</summary>
        public ushort ReloadRequestSequence { get; }

        public PlayerLocomotionInput(
            Vector3 worldMove,
            Vector3 aimDirection,
            bool aimHeld,
            bool sprintHeld,
            bool fireHeld = false,
            ushort reloadRequestSequence = 0)
        {
            WorldMove = worldMove;
            AimDirection = aimDirection;
            AimHeld = aimHeld;
            SprintHeld = sprintHeld;
            FireHeld = fireHeld;
            ReloadRequestSequence = reloadRequestSequence;
        }

        public static PlayerLocomotionInput None =>
            new(Vector3.zero, Vector3.zero, false, false, false, 0);
    }

    /// <summary>当前运动的表现阶段，不参与 Gameplay 状态决策。</summary>
    public enum MotionPhase : byte
    {
        Idle = 0,
        Start = 1,
        Move = 2,
        Stop = 3,
        Pivot = 4,
    }

    /// <summary>
    /// Pivot 开始时锁存的目标移动方向，使用角色当时的局部坐标。
    /// None 同时表示当前没有处于 Pivot；方向必须进入快照，避免 Owner 与 Remote 选择不同动画。
    /// </summary>
    public enum PlayerPivotDirection : byte
    {
        None = 0,
        Forward = 1,
        Backward = 2,
        Left = 3,
        Right = 4,
    }

    /// <summary>
    /// 中层向PlayerMotor提交的单Tick运动意图。
    /// </summary>
    public readonly struct PlayerMotionCommand
    {
        public Vector3 WorldMove { get; }
        public Vector3 DesiredFacingDirection { get; }

        public PlayerMotionCommand(Vector3 worldMove, Vector3 desiredFacingDirection)
        {
            WorldMove = worldMove;
            DesiredFacingDirection = desiredFacingDirection;
        }

        public static PlayerMotionCommand None => new(Vector3.zero, Vector3.zero);
    }

    /// <summary>PlayerMotor 单次模拟结束后的只读运动事实。</summary>
    public struct PlayerMotionState
    {
        public Vector3 Position { get; internal set; }
        public Vector3 Velocity { get; internal set; }
        public Vector3 DesiredVelocity { get; internal set; }
        public Vector3 Acceleration { get; internal set; }
        public Vector3 MoveDirection { get; internal set; }
        public Vector3 FacingDirection { get; internal set; }
        public Vector3 DesiredFacingDirection { get; internal set; }
        public Vector3 LocalVelocity { get; internal set; }
        public float Speed { get; internal set; }
        public float NormalizedSpeed { get; internal set; }
        public float AngularSpeed { get; internal set; }
        public MotionPhase Phase { get; internal set; }
        public PlayerPivotDirection PivotDirection { get; internal set; }
        public bool HasMoveInput { get; internal set; }
        public bool IsMoving { get; internal set; }
        public bool IsPivoting { get; internal set; }
    }

    /// <summary>可完整恢复 PlayerMotor 的预测运行时状态。</summary>
    public readonly struct PlayerMotorRuntimeState
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Velocity { get; }
        public float AngularSpeed { get; }
        public PlayerPivotDirection PivotDirection { get; }

        public PlayerMotorRuntimeState(
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            float angularSpeed,
            PlayerPivotDirection pivotDirection)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularSpeed = angularSpeed;
            PivotDirection = pivotDirection;
        }
    }
}

namespace ProjectGame.HotFix.Gameplay.Player.State
{
    /// <summary>顶层生命分支；Dead 会压制 Reaction、Combat 与 Locomotion。</summary>
    public enum PlayerLifeState : byte
    {
        Alive = 0,
        Dead = 1,
    }

    /// <summary>移动意图分支；Aim 优先于 Sprint，实际是否可用还受生命和受击状态限制。</summary>
    public enum PlayerLocomotionMode : byte
    {
        Free = 0,
        Aim = 1,
        Sprint = 2,
    }

    /// <summary>高于 Combat 的短时反应分支；第一版仅包含普通受击。</summary>
    public enum PlayerReactionMode : byte
    {
        Normal = 0,
        HitReaction = 1,
    }

    /// <summary>战斗动作分支；Reloading 优先于 Firing，具体武器执行由后续系统消费动作序号。</summary>
    public enum PlayerCombatMode : byte
    {
        Ready = 0,
        Firing = 1,
        Reloading = 2,
    }

    /// <summary>
    /// 参与预测、回滚及网络序列化的 Gameplay 状态。
    /// </summary>
    public struct PlayerControlState : INetworkSerializable
    {
        // 四个正交分支组成当前高层状态。保持为值类型字段，便于完整快照、Delta 和回滚严格比较。
        public PlayerLifeState LifeState;
        public PlayerReactionMode ReactionMode;
        public PlayerCombatMode CombatMode;
        public PlayerLocomotionMode LocomotionMode;

        // 以下属性统一应用跨分支约束，调用方不要只比较单个枚举后绕过 Dead/Hit 的压制规则。
        public bool IsAlive => LifeState == PlayerLifeState.Alive;
        public bool IsDead => LifeState == PlayerLifeState.Dead;
        public bool IsHitReacting => IsAlive && ReactionMode == PlayerReactionMode.HitReaction;
        public bool IsReloading => IsAlive && CombatMode == PlayerCombatMode.Reloading;
        public bool IsFiring => IsAlive && CombatMode == PlayerCombatMode.Firing;
        public bool CanUseLocomotion => IsAlive && !IsHitReacting;
        public bool IsAiming => CanUseLocomotion && LocomotionMode == PlayerLocomotionMode.Aim;
        public bool IsSprinting => CanUseLocomotion && LocomotionMode == PlayerLocomotionMode.Sprint;

        public static PlayerControlState CreateDefault()
        {
            return new PlayerControlState
            {
                LifeState = PlayerLifeState.Alive,
                ReactionMode = PlayerReactionMode.Normal,
                CombatMode = PlayerCombatMode.Ready,
                LocomotionMode = PlayerLocomotionMode.Free,
            };
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref LifeState);
            serializer.SerializeValue(ref ReactionMode);
            serializer.SerializeValue(ref CombatMode);
            serializer.SerializeValue(ref LocomotionMode);
        }
    }

    /// <summary>
    /// 当前固定 Tick 交给 PlayerStateMachine 的最小事实集合。
    /// 不携带 Transform、Animator 或网络对象，保证 Owner Replay 与 Server Authority 使用完全相同的输入语义。
    /// </summary>
    public readonly struct PlayerStateInput
    {
        /// <summary>移动向量经过 Movement DeadZone 后是否仍有效。</summary>
        public bool HasMoveInput { get; }
        /// <summary>持续瞄准意图。</summary>
        public bool AimHeld { get; }
        /// <summary>持续冲刺意图。</summary>
        public bool SprintHeld { get; }
        /// <summary>持续射击意图。</summary>
        public bool FireHeld { get; }
        /// <summary>累计换弹请求序号；与运行状态中的 LastReloadRequestSequence 比较以检测新边沿。</summary>
        public ushort ReloadRequestSequence { get; }

        public PlayerStateInput(
            bool hasMoveInput,
            bool aimHeld,
            bool sprintHeld,
            bool fireHeld = false,
            ushort reloadRequestSequence = 0)
        {
            HasMoveInput = hasMoveInput;
            AimHeld = aimHeld;
            SprintHeld = sprintHeld;
            FireHeld = fireHeld;
            ReloadRequestSequence = reloadRequestSequence;
        }
    }

    /// <summary>
    /// 影响后续 Tick 决策的受击、射击与换弹运行记忆。
    /// 每个字段都必须随 PlayerSimulationState 保存、网络同步并参与预测误差比较；禁止迁移到 Animator 或 MonoBehaviour 私有计时器。
    /// </summary>
    public struct PlayerActionRuntimeState : INetworkSerializable
    {
        // 剩余受击占用 Tick。大于 0 时 Reaction=HitReaction，并压制 Combat 与移动。
        public ushort HitTicksRemaining;
        // 剩余换弹占用 Tick。大于 0 时 Combat=Reloading；开始 Tick 已计入，所以创建时通常写入 totalTicks-1。
        public ushort ReloadTicksRemaining;
        // 下一次允许递增 ShotSequence 前的冷却 Tick；每次固定模拟最多递减 1。
        public ushort FireCooldownTicks;
        // 确定性射击事件序号。消费者按序号变化触发一次表现/武器执行，不能把它当累计弹药数。
        public uint ShotSequence;
        // 确定性受击事件序号。重复受击会递增并刷新持续时间。
        public uint HitSequence;
        // 状态机已消费的最新换弹请求；Neutral Input 也必须保留该值，避免回到 0 后误判为新请求。
        public ushort LastReloadRequestSequence;

        /// <summary>
        /// 按固定协议顺序序列化所有会影响未来决策的字段。
        /// 增删或调整顺序会改变网络协议，客户端与服务器必须同时更新。
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HitTicksRemaining);
            serializer.SerializeValue(ref ReloadTicksRemaining);
            serializer.SerializeValue(ref FireCooldownTicks);
            serializer.SerializeValue(ref ShotSequence);
            serializer.SerializeValue(ref HitSequence);
            serializer.SerializeValue(ref LastReloadRequestSequence);
        }
    }
}

namespace ProjectGame.HotFix.Gameplay.Player.Stamina
{
    /// <summary>可预测、可回滚、可网络序列化的玩家体力状态。</summary>
    public struct PlayerStaminaState : INetworkSerializable
    {
        public float Current;
        public float RecoveryDelayRemaining;
        public bool IsExhausted;

        public float Normalized(float maxStamina)
        {
            return maxStamina <= 0f ? 0f : Mathf.Clamp01(Current / maxStamina);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Current);
            serializer.SerializeValue(ref RecoveryDelayRemaining);
            serializer.SerializeValue(ref IsExhausted);
        }
    }
}
