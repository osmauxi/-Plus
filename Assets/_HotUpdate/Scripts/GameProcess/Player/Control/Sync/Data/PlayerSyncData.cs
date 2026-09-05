using System;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>可持续保持的玩家输入按钮位；可组合写入同一字节 </summary>
    [Flags]
    public enum PlayerInputButtons : byte
    {
        // 没有持续按钮输入 
        None = 0,
        // 当前 Tick 持续按住瞄准 
        AimHeld = 1 << 0,
        // 当前 Tick 持续按住冲刺 
        SprintHeld = 1 << 1,
        // 当前 Tick 持续按住射击 
        FireHeld = 1 << 2,
    }

    /// <summary>
    /// 一个Tick对应的连续玩家输入 
    /// </summary>
    public struct PlayerInputCommand : INetworkSerializable
    {
        // 该输入所属的 Client 模拟 Tick；PlayerSimulationClock 由 Owner Controller 推进并分配，Server 用于时序分类 
        public uint Tick;
        // XZ 世界平面移动意图压缩为 Vector2(x,z)，长度范围由 TrySanitize 限制到 0~1 
        public Vector2 WorldMove;
        // XZ 世界平面瞄准方向压缩为 Vector2(x,z)；非零值由 TrySanitize 归一化 
        public Vector2 AimDirection;
        // 本 Tick 持续按住的输入按钮位 
        public PlayerInputButtons Buttons;
        // 本地每次按下 Reload 时递增；持续携带最新值，避免渲染帧边沿在网络 Tick 之间丢失 
        // 允许 ushort 自然回绕；服务端/状态机通过“不等于已消费值”检测新请求，不能在中立输入中擅自清零 
        public ushort ReloadRequestSequence;
        // 当前网络协议允许出现的全部按钮位；用于清除未知/保留位 
        private const PlayerInputButtons ValidButtons =
            PlayerInputButtons.AimHeld |
            PlayerInputButtons.SprintHeld |
            PlayerInputButtons.FireHeld;
        // 是否持续按住瞄准 
        public bool AimHeld => (Buttons & PlayerInputButtons.AimHeld) != 0;
        // 是否持续按住冲刺 
        public bool SprintHeld => (Buttons & PlayerInputButtons.SprintHeld) != 0;
        // 是否持续按住射击 
        public bool FireHeld => (Buttons & PlayerInputButtons.FireHeld) != 0;
        // 是否存在非零移动意图；精确 DeadZone 判定由移动层配置负责 
        public bool HasMoveInput => WorldMove.sqrMagnitude > 0f;

        /// <summary>按固定字段顺序读写输入命令；发送端与接收端必须使用相同协议版本 </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref WorldMove);
            serializer.SerializeValue(ref AimDirection);
            serializer.SerializeValue(ref Buttons);
            serializer.SerializeValue(ref ReloadRequestSequence);
        }

        /// <summary>
        /// 创建一个中立输入模板，清除移动、瞄准和持续按钮 
        /// ReloadRequestSequence 默认是 0；Server 丢包兜底和 Warp 重置调用方必须随后写回已消费序号，
        /// 否则从非零序号回到 0 会被状态机误判为新的 Reload 请求 
        /// </summary>
        public static PlayerInputCommand CreateNeutral(uint tick)
        {
            return new PlayerInputCommand
            {
                Tick = tick,
                WorldMove = Vector2.zero,
                AimDirection = Vector2.zero,
                Buttons = PlayerInputButtons.None,
                ReloadRequestSequence = 0,
            };
        }
        /// <summary>
        /// 把网络用 Vector2(x,z) 还原为移动层使用的 XZ 平面 Vector3，
        /// 并把按钮位展开为 PlayerLocomotionInput 的布尔状态 
        /// </summary>
        public PlayerLocomotionInput ToLocomotionInput()
        {
            return new PlayerLocomotionInput(
                new Vector3(WorldMove.x, 0f, WorldMove.y),
                new Vector3(AimDirection.x, 0f, AimDirection.y),
                AimHeld,
                SprintHeld,
                FireHeld,
                ReloadRequestSequence);
        }
        /// <summary>
        /// 复制当前输入内容并替换 Tick；用于 Server Hold、RetimedLate 和历史重放，不修改原结构体 
        /// </summary>
        public PlayerInputCommand WithTick(uint tick)
        {
            PlayerInputCommand copy = this;
            copy.Tick = tick;
            return copy;
        }
        /// <summary>
        /// 把不可信 Client 输入转换为权威模拟可使用的安全副本 
        /// NaN/Infinity 无法修复，返回 false；过长 Move 会 Clamp，非零 Aim 会归一化，未知 Button 位会清除 
        /// 因此“长度大于 1/非单位方向/未知按钮位”是可修正数据，不会导致方法返回 false 
        /// </summary>
        public bool TrySanitize(out PlayerInputCommand sanitized)
        {
            sanitized = this;

            if (!IsFinite(WorldMove) || !IsFinite(AimDirection))
                return false;

            //世界移动输入最大只能为1 
            sanitized.WorldMove = Vector2.ClampMagnitude(WorldMove, 1f);

            //Aim表达的是方向，不允许Client通过长度携带其它信息 
            sanitized.AimDirection = AimDirection.sqrMagnitude > 0.000001f
                ? AimDirection.normalized
                : Vector2.zero;

            //清掉当前协议未定义的Button位 
            sanitized.Buttons &= ValidButtons;

            return true;
        }

        /// <summary>检查 Vector2 两个分量都不是 NaN 或正负 Infinity </summary>
        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x) &&
                   !float.IsInfinity(value.x) &&
                   !float.IsNaN(value.y) &&
                   !float.IsInfinity(value.y);
        }
    }

    /// <summary>
    /// 一个确定 Tick 的完整可恢复权威玩家状态 
    /// </summary>
    public struct PlayerSimulationState : INetworkSerializable
    {
        // 该状态对应的固定模拟 Tick 
        public uint Tick;
        // 世界坐标位置 
        public Vector3 Position;
        // 世界坐标旋转 
        public Quaternion Rotation;
        // CharacterController 真实位移反算的水平速度 
        public Vector3 Velocity;
        // 绕 Y 轴的有符号角速度（度/秒） 
        public float AngularSpeed;

        // XZ 世界平面的最终瞄准方向 它独立于 Root Rotation，使站立 Aim 时上半身仍能偏转 
        public Vector2 AimDirection;
        // Aim Root 是否处于迟滞跟随阶段；它是迟滞记忆，会影响后续 Tick 的朝向决策，必须随回滚恢复 
        public bool IsAimBodyTurning;
        // Pivot 进入时锁存的角色局部目标方向；None 表示当前没有 Pivot 
        public PlayerPivotDirection PivotDirection;
        // Pivot 爆发是会影响后续 Tick 位移的短时状态，必须随权威快照与回滚恢复 
        public float PivotBoostTimeRemaining;
        public float PivotBoostSpeedBonus;

        // 生命状态和 Free/Aim/Sprint 等离散控制模式 
        public PlayerControlState ControlState;
        // 当前体力、恢复延迟和耗尽锁定状态 
        public PlayerStaminaState StaminaState;
        // 受击、射击冷却、换弹计时与动作序号；全部会影响未来 Tick 或事件去重，不能只在表现层保存 
        public PlayerActionRuntimeState ActionState;

        /// <summary>按固定字段顺序读写一份完整可恢复状态 </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref AngularSpeed);
            serializer.SerializeValue(ref AimDirection);
            serializer.SerializeValue(ref IsAimBodyTurning);
            serializer.SerializeValue(ref PivotDirection);
            serializer.SerializeValue(ref PivotBoostTimeRemaining);
            serializer.SerializeValue(ref PivotBoostSpeedBonus);
            serializer.SerializeValue(ref ControlState);
            serializer.SerializeValue(ref StaminaState);
            serializer.SerializeValue(ref ActionState);
        }
    }

    /// <summary>
    /// uint Tick比较工具 
    /// 利用无符号整数自然溢出处理uint.MaxValue -> 0 
    /// 要求两个Tick之间的实际距离不超过int.MaxValue 
    /// </summary>
    public static class TickMath
    {
        /// <summary>在 uint 回绕语义下判断 a 是否严格新于 b </summary>
        public static bool IsNewer(uint a, uint b) => unchecked((int)(a - b)) > 0;

        /// <summary>在 uint 回绕语义下判断 a 是否严格旧于 b </summary>
        public static bool IsOlder(uint a, uint b) => unchecked((int)(a - b)) < 0;

        /// <summary>在 uint 回绕语义下判断 a 是否新于或等于 b </summary>
        public static bool IsNewerOrEqual(uint a, uint b) => a == b || IsNewer(a, b);

        /// <summary>计算从 older 向前推进到 newer 的 Tick 数；调用方必须保证参数语义顺序正确 </summary>
        public static uint Distance(uint newer, uint older) => unchecked(newer - older);
    }

    /// <summary>快照是完整关键帧，还是依赖 Baseline 的增量帧 </summary>
    public enum PlayerSnapshotKind : byte
    {
        // 可独立恢复完整状态，并成为后续 Delta 的 Baseline 
        Full = 0,
        // 只携带相对 Baseline 变化的字段 
        Delta = 1,
    }

    /// <summary>Delta Snapshot 中实际序列化了哪些 PlayerSimulationState 字段 </summary>
    [Flags]
    public enum PlayerStateDirtyMask : ushort
    {
        None = 0,

        Position = 1 << 0,
        Rotation = 1 << 1,
        Velocity = 1 << 2,
        AngularSpeed = 1 << 3,
        ControlState = 1 << 4,
        StaminaState = 1 << 5,
        AimDirection = 1 << 6,
        AimBodyTurning = 1 << 7,
        // 动作计时和事件序号任一变化都需要进入 Delta；否则 Owner Replay 可能从错误冷却/换弹进度开始 
        ActionState = 1 << 8,
        // Pivot 是离散短时阶段；方向必须随快照同步，否则 Remote 无法选择对应过渡动画 
        PivotDirection = 1 << 9,
        // 爆发剩余时间和锁存速度共同决定未来位移，作为一个原子字段组同步 
        PivotBoost = 1 << 10,

        All = Position |
              Rotation |
              Velocity |
              AngularSpeed |
              ControlState |
              StaminaState |
              AimDirection |
              AimBodyTurning |
              ActionState |
              PivotDirection |
              PivotBoost,
    }

    /// <summary>
    /// Server -> Client 的实际快照网络包 
    /// Full：包含完整状态，并成为后续Delta的Baseline 
    /// Delta：只包含相对于Baseline发生变化的字段 
    /// </summary>
    public struct PlayerSnapshotPacket : INetworkSerializable
    {
        // Full 或 Delta；决定解码是否需要 Baseline 
        public PlayerSnapshotKind Kind;

        /// <summary>
        /// 当前权威状态Tick
        /// </summary>
        public uint Tick;

        /// <summary>
        /// Delta所依赖的上一个权威状态Tick
        /// Full时等于自身Tick
        /// </summary>
        public uint BaselineTick;

        // 指示本包 State 中哪些字段真正进入网络；Full 固定为 All 
        public PlayerStateDirtyMask DirtyMask;

        /// <summary>
        /// Full时保存完整状态；
        /// Delta时只有DirtyMask对应字段会真正进入网络 
        /// </summary>
        public PlayerSimulationState State;

        // 当前包是否可独立还原完整状态 
        public bool IsFull => Kind == PlayerSnapshotKind.Full;

        /// <summary>
        /// 序列化 Packet Header 和 DirtyMask 指定字段 
        /// State.Tick 与 Header.Tick 相同，因此不重复占用网络字节 
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Kind);
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref BaselineTick);
            serializer.SerializeValue(ref DirtyMask);
            DirtyMask &= PlayerStateDirtyMask.All;

            if ((DirtyMask & PlayerStateDirtyMask.Position) != 0)
                serializer.SerializeValue(ref State.Position);

            if ((DirtyMask & PlayerStateDirtyMask.Rotation) != 0)
                serializer.SerializeValue(ref State.Rotation);

            if ((DirtyMask & PlayerStateDirtyMask.Velocity) != 0)
                serializer.SerializeValue(ref State.Velocity);

            if ((DirtyMask & PlayerStateDirtyMask.AngularSpeed) != 0)
                serializer.SerializeValue(ref State.AngularSpeed);

            if ((DirtyMask & PlayerStateDirtyMask.AimDirection) != 0)
                serializer.SerializeValue(ref State.AimDirection);

            if ((DirtyMask & PlayerStateDirtyMask.AimBodyTurning) != 0)
                serializer.SerializeValue(ref State.IsAimBodyTurning);

            if ((DirtyMask & PlayerStateDirtyMask.PivotDirection) != 0)
                serializer.SerializeValue(ref State.PivotDirection);

            if ((DirtyMask & PlayerStateDirtyMask.PivotBoost) != 0)
            {
                serializer.SerializeValue(ref State.PivotBoostTimeRemaining);
                serializer.SerializeValue(ref State.PivotBoostSpeedBonus);
            }

            if ((DirtyMask & PlayerStateDirtyMask.ControlState) != 0)
                serializer.SerializeValue(ref State.ControlState);

            if ((DirtyMask & PlayerStateDirtyMask.StaminaState) != 0)
                serializer.SerializeValue(ref State.StaminaState);

            if ((DirtyMask & PlayerStateDirtyMask.ActionState) != 0)
                serializer.SerializeValue(ref State.ActionState);

            //Tick已经存在于Packet Header，不重复传输 
            State.Tick = Tick;
        }

        /// <summary>由完整权威状态创建可独立解码的 Full Snapshot </summary>
        public static PlayerSnapshotPacket CreateFull(in PlayerSimulationState state)
        {
            return new PlayerSnapshotPacket
            {
                Kind = PlayerSnapshotKind.Full,
                Tick = state.Tick,
                BaselineTick = state.Tick,
                DirtyMask = PlayerStateDirtyMask.All,
                State = state,
            };
        }

        /// <summary>比较当前状态与固定 Baseline，创建只携带变化字段的 Delta Snapshot </summary>
        public static PlayerSnapshotPacket CreateDelta(in PlayerSimulationState state, in PlayerSimulationState baseline)
        {
            PlayerStateDirtyMask mask = ResolveDirtyMask(state, baseline);

            return new PlayerSnapshotPacket
            {
                Kind = PlayerSnapshotKind.Delta,
                Tick = state.Tick,
                BaselineTick = baseline.Tick,
                DirtyMask = mask,
                State = state,
            };
        }

        /// <summary>
        /// Full 直接返回自身 State；Delta 要求传入 Tick 与 BaselineTick 完全一致的基准状态，
        /// 再把 DirtyMask 指定字段覆盖到基准上，得到一份完整 PlayerSimulationState 
        /// </summary>
        public bool TryResolve(in PlayerSimulationState baseline, out PlayerSimulationState resolved)
        {
            if (IsFull)
            {
                resolved = State;
                resolved.Tick = Tick;
                return true;
            }

            if (baseline.Tick != BaselineTick)
            {
                resolved = default;
                return false;
            }

            resolved = baseline;
            resolved.Tick = Tick;

            if ((DirtyMask & PlayerStateDirtyMask.Position) != 0)
                resolved.Position = State.Position;

            if ((DirtyMask & PlayerStateDirtyMask.Rotation) != 0)
                resolved.Rotation = State.Rotation;

            if ((DirtyMask & PlayerStateDirtyMask.Velocity) != 0)
                resolved.Velocity = State.Velocity;

            if ((DirtyMask & PlayerStateDirtyMask.AngularSpeed) != 0)
                resolved.AngularSpeed = State.AngularSpeed;

            if ((DirtyMask & PlayerStateDirtyMask.AimDirection) != 0)
                resolved.AimDirection = State.AimDirection;

            if ((DirtyMask & PlayerStateDirtyMask.AimBodyTurning) != 0)
                resolved.IsAimBodyTurning = State.IsAimBodyTurning;

            if ((DirtyMask & PlayerStateDirtyMask.PivotDirection) != 0)
                resolved.PivotDirection = State.PivotDirection;

            if ((DirtyMask & PlayerStateDirtyMask.PivotBoost) != 0)
            {
                resolved.PivotBoostTimeRemaining = State.PivotBoostTimeRemaining;
                resolved.PivotBoostSpeedBonus = State.PivotBoostSpeedBonus;
            }

            if ((DirtyMask & PlayerStateDirtyMask.ControlState) != 0)
                resolved.ControlState = State.ControlState;

            if ((DirtyMask & PlayerStateDirtyMask.StaminaState) != 0)
                resolved.StaminaState = State.StaminaState;

            if ((DirtyMask & PlayerStateDirtyMask.ActionState) != 0)
                resolved.ActionState = State.ActionState;

            return true;
        }
        /// <summary>
        /// 逐字段比较当前状态与 Baseline，返回需要进入 Delta 的位掩码 
        /// 连续浮点值采用 Unity Approximately；离散控制状态严格比较 
        /// </summary>
        private static PlayerStateDirtyMask ResolveDirtyMask(in PlayerSimulationState state, in PlayerSimulationState baseline)
        {
            PlayerStateDirtyMask mask = PlayerStateDirtyMask.None;

            if (state.Position != baseline.Position)
                mask |= PlayerStateDirtyMask.Position;

            if (state.Rotation != baseline.Rotation)
                mask |= PlayerStateDirtyMask.Rotation;

            if (state.Velocity != baseline.Velocity)
                mask |= PlayerStateDirtyMask.Velocity;

            if (!Mathf.Approximately(state.AngularSpeed, baseline.AngularSpeed))
                mask |= PlayerStateDirtyMask.AngularSpeed;

            if (state.AimDirection != baseline.AimDirection)
                mask |= PlayerStateDirtyMask.AimDirection;

            if (state.IsAimBodyTurning != baseline.IsAimBodyTurning)
                mask |= PlayerStateDirtyMask.AimBodyTurning;

            if (state.PivotDirection != baseline.PivotDirection)
                mask |= PlayerStateDirtyMask.PivotDirection;

            if (!Mathf.Approximately(state.PivotBoostTimeRemaining, baseline.PivotBoostTimeRemaining) ||
                !Mathf.Approximately(state.PivotBoostSpeedBonus, baseline.PivotBoostSpeedBonus))
                mask |= PlayerStateDirtyMask.PivotBoost;

            if (state.ControlState.LifeState != baseline.ControlState.LifeState ||
                state.ControlState.ReactionMode != baseline.ControlState.ReactionMode ||
                state.ControlState.CombatMode != baseline.ControlState.CombatMode ||
                state.ControlState.LocomotionMode != baseline.ControlState.LocomotionMode)
                mask |= PlayerStateDirtyMask.ControlState;

            if (!Mathf.Approximately(state.StaminaState.Current, baseline.StaminaState.Current) ||
                !Mathf.Approximately(state.StaminaState.RecoveryDelayRemaining, baseline.StaminaState.RecoveryDelayRemaining) ||
                state.StaminaState.IsExhausted != baseline.StaminaState.IsExhausted)
                mask |= PlayerStateDirtyMask.StaminaState;

            // ActionState 使用严格比较：Tick 计时器和序号是离散确定性数据，不能用 Approximately 
            if (state.ActionState.HitTicksRemaining != baseline.ActionState.HitTicksRemaining ||
                state.ActionState.ReloadTicksRemaining != baseline.ActionState.ReloadTicksRemaining ||
                state.ActionState.FireCooldownTicks != baseline.ActionState.FireCooldownTicks ||
                state.ActionState.ShotSequence != baseline.ActionState.ShotSequence ||
                state.ActionState.HitSequence != baseline.ActionState.HitSequence ||
                state.ActionState.LastReloadRequestSequence != baseline.ActionState.LastReloadRequestSequence)
                mask |= PlayerStateDirtyMask.ActionState;

            return mask;
        }
    }

    /// <summary>
    /// 固定容量、零 Tick GC 的 O(1) 环形历史缓存 
    /// </summary>
    public sealed class TickRingBuffer<T> where T : struct
    {
        // 每个物理槽位当前对应的完整 Tick；用于区分取模碰撞后的旧数据 
        private readonly uint[] _ticks;
        // 与 _ticks 同下标保存的结构体值 
        private readonly T[] _values;
        // 标记槽位是否有效；避免 Tick=0 与默认数组值混淆 
        private readonly bool[] _occupied;

        /// <summary>环形历史可同时保留的最大元素数 </summary>
        public int Capacity { get; }

        /// <summary>按固定容量一次性分配 Tick、Value 和 Occupied 三组数组 </summary>
        public TickRingBuffer(int capacity)
        {
            if (capacity <= 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Tick 缓存容量必须大于 1 ");

            Capacity = capacity;
            _ticks = new uint[capacity];
            _values = new T[capacity];
            _occupied = new bool[capacity];
        }

        /// <summary>按 tick % Capacity 写入；同物理槽位的更旧 Tick 会被覆盖 </summary>
        public void Store(uint tick, in T value)
        {
            int index = GetIndex(tick);
            _ticks[index] = tick;
            _values[index] = value;
            _occupied[index] = true;
        }

        /// <summary>仅当槽位有效且保存的完整 Tick 相同，才返回对应值 </summary>
        public bool TryGet(uint tick, out T value)
        {
            int index = GetIndex(tick);

            if (_occupied[index] && _ticks[index] == tick)
            {
                value = _values[index];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>判断指定完整 Tick 当前是否仍保存在环形槽位中 </summary>
        public bool Contains(uint tick)
        {
            int index = GetIndex(tick);
            return _occupied[index] && _ticks[index] == tick;
        }

        /// <summary>移除指定 Tick；槽位已被其他 Tick 覆盖时不会误删 </summary>
        public bool Remove(uint tick)
        {
            int index = GetIndex(tick);

            if (!_occupied[index] || _ticks[index] != tick)
                return false;

            _occupied[index] = false;
            _values[index] = default;
            return true;
        }

        /// <summary>清空有效标记和值；_ticks 可保留，因为 _occupied=false 时不会被读取 </summary>
        public void Clear()
        {
            Array.Clear(_occupied, 0, _occupied.Length);
            Array.Clear(_values, 0, _values.Length);
        }

        /// <summary>把任意 uint Tick 映射到固定容量数组下标 </summary>
        private int GetIndex(uint tick) => (int)(tick % (uint)Capacity);
    }

}
