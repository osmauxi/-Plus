using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// Remote Observer 的权威快照环形缓冲与视觉插值时间轴 
    /// 表现时间故意落后最新 Snapshot 若干 Tick，用缓存吸收网络抖动；不执行输入预测，也不做外推 
    /// </summary>
    public sealed class PlayerRemoteInterpolation
    {
        // 按权威 Tick 单调写入的固定容量快照数组；满后覆盖最旧元素 
        private readonly PlayerSimulationState[] _buffer;
        // 插值延迟、缓冲容量、TickRate 和传送判定距离等参数 
        private readonly PlayerSyncConfig _config;
        // 环形数组中“逻辑最旧快照”对应的物理下标 
        private int _start;
        // 当前缓冲内有效快照数量，范围为 0~_buffer.Length 
        private int _count;
        // 当前表现 Tick 相对 _latestTick 的偏移；通常为负，-3 表示表现落后最新快照 3 Tick 
        private double _renderOffset;
        // 最近接受的权威 Snapshot Tick；用于拒绝重复/乱序快照和计算新快照跨度 
        private uint _latestTick;
        // 标记是否已收到第一份快照并建立表现时间轴 
        private bool _initialized;

        /// <summary>当前插值缓冲内有效快照数量 </summary>
        public int BufferedSnapshotCount => _count;

        /// <summary>创建 Observer 插值器并按配置一次性分配快照缓冲 </summary>
        public PlayerRemoteInterpolation(PlayerSyncConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _buffer = new PlayerSimulationState[config.RemoteSnapshotBufferCapacity];
        }

        /// <summary>
        /// 接收一份已还原的完整权威状态 
        /// 拒绝非递增 Tick；检测大距离传送；维护环形缓冲并保持表现时间轴连续 
        /// </summary>
        public void PushSnapshot(in PlayerSimulationState state)
        {
            // Observer 只接受严格递增 Tick，避免旧包让表现时间轴倒退 
            if (_count > 0 && !TickMath.IsNewer(state.Tick, _latestTick))
                return;

            if (_count > 0)
            {
                // 与最后一份快照距离过大时视为传送；旧轨迹不能与新位置做跨地图插值 
                PlayerSimulationState previous = Logical(_count - 1);
                float teleportDistance = _config.RemoteTeleportDistance;

                if (Vector3.SqrMagnitude(state.Position - previous.Position) > teleportDistance * teleportDistance)
                    Reset();
            }

            // latest 向前推进多少 Tick，renderOffset 就相对新的 latest 再向后移动多少 
            // 因此“表现对应的绝对权威 Tick”保持不变，新 Snapshot 到达不会直接拉动视觉位置 
            if (_initialized)
                _renderOffset -= TickMath.Distance(state.Tick, _latestTick);

            if (_count < _buffer.Length)
            {
                // 缓冲未满：写到当前逻辑尾部 
                _buffer[PhysicalIndex(_count)] = state;
                _count++;
            }
            else
            {
                // 缓冲已满：覆盖最旧槽位，并把新的逻辑起点向前移动一格 
                _buffer[_start] = state;
                _start = (_start + 1) % _buffer.Length;
            }

            _latestTick = state.Tick;

            if (!_initialized)
            {
                // 第一份快照建立表现时间轴，目标是固定落后最新权威时间 RemoteInterpolationDelayTicks 
                _renderOffset = -_config.RemoteInterpolationDelayTicks;
                _initialized = true;
            }
        }

        /// <summary>
        /// 按渲染帧 deltaTime 推进表现时间，并在缓冲内寻找包围目标 Tick 的两份快照进行插值 
        /// 返回 false 表示尚未收到任何快照；缓冲边界不足时返回最旧或最新状态，不做外推 
        /// </summary>
        public bool TrySample(float deltaTime, out PlayerSimulationState state)
        {
            if (!_initialized || _count == 0)
            {
                state = default;
                return false;
            }

            // 表现时间每帧向前推进，但最多追到“最新 Tick - 配置延迟”，不会直接追上网络头部 
            double targetOffset = -_config.RemoteInterpolationDelayTicks;
            _renderOffset = Math.Min(targetOffset, _renderOffset + deltaTime * _config.SimulationTickRate);

            // oldest/latest 是当前缓冲覆盖的时间范围 
            PlayerSimulationState oldest = Logical(0);
            PlayerSimulationState latest = Logical(_count - 1);
            // 如果目标比 oldest 还老，将其夹到 oldest；避免读取已经被环形缓冲覆盖的历史 
            double desiredFromLatest = Math.Max(_renderOffset, -TickMath.Distance(latest.Tick, oldest.Tick));

            if (desiredFromLatest >= 0d || _count == 1)
            {
                // 目标已经到达最新状态，或只有一个样本时，停在 latest；当前实现不做速度外推 
                state = latest;
                return true;
            }

            // targetAge 表示目标表现 Tick 比 latest 老多少 Tick；从新向旧查找包围区间 
            double targetAge = -desiredFromLatest;
            for (int i = _count - 2; i >= 0; i--)
            {
                PlayerSimulationState from = Logical(i);
                PlayerSimulationState to = Logical(i + 1);
                double fromAge = TickMath.Distance(latest.Tick, from.Tick);
                double toAge = TickMath.Distance(latest.Tick, to.Tick);

                // 目标年龄不落在 [toAge, fromAge] 时继续向更旧区间搜索 
                if (targetAge > fromAge || targetAge < toAge)
                    continue;

                double range = fromAge - toAge;
                // t=0 取 from，t=1 取 to；range 为 0 时保守取 from 
                float t = range <= 0d ? 0f : (float)((fromAge - targetAge) / range);
                state = Interpolate(from, to, t);
                return true;
            }

            // 理论上 desiredFromLatest 已被 oldest 夹住；兜底返回 oldest，避免采样失败造成表现对象不更新 
            state = oldest;
            return true;
        }

        /// <summary>清空全部快照和表现时间轴；传送、Despawn 或重新初始化时使用 </summary>
        public void Reset()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _start = 0;
            _count = 0;
            _renderOffset = 0d;
            _latestTick = 0;
            _initialized = false;
        }

        /// <summary>按从旧到新的逻辑下标读取快照 </summary>
        private PlayerSimulationState Logical(int index) => _buffer[PhysicalIndex(index)];
        /// <summary>把逻辑下标映射到环形数组物理下标 </summary>
        private int PhysicalIndex(int logicalIndex) => (_start + logicalIndex) % _buffer.Length;

        /// <summary>
        /// 连续数值做线性/球面插值；生命与移动模式等离散状态选择离目标更近的一端 
        /// Tick 也随离散端选择，仅作为表现状态标记，不重新写入网络历史 
        /// </summary>
        private static PlayerSimulationState Interpolate(in PlayerSimulationState from, in PlayerSimulationState to, float t)
        {
            // 先选择离采样时刻更近的一端，保留 Life/Reaction/Combat/Locomotion、
            // ActionState 计时器/序号和 AimBodyTurn 迟滞等离散数据；这些值绝不能做数值插值 
            PlayerSimulationState result = t < 0.5f ? from : to;
            result.Position = Vector3.LerpUnclamped(from.Position, to.Position, t);
            result.Rotation = Quaternion.SlerpUnclamped(from.Rotation, to.Rotation, t);
            result.Velocity = Vector3.LerpUnclamped(from.Velocity, to.Velocity, t);
            result.AngularSpeed = Mathf.LerpUnclamped(from.AngularSpeed, to.AngularSpeed, t);
            result.AimDirection = InterpolateDirection(from.AimDirection, to.AimDirection, t);
            result.StaminaState.Current = Mathf.LerpUnclamped(from.StaminaState.Current, to.StaminaState.Current, t);
            result.StaminaState.RecoveryDelayRemaining = Mathf.LerpUnclamped(
                from.StaminaState.RecoveryDelayRemaining,
                to.StaminaState.RecoveryDelayRemaining,
                t);
            return result;
        }

        /// <summary>
        /// 在角度空间插值 Aim，避免跨越 ±180° 时绕远路 
        /// 任一端为零时按最近端离散选择，避免从“无瞄准”人为生成中间方向 
        /// </summary>
        private static Vector2 InterpolateDirection(Vector2 from, Vector2 to, float t)
        {
            bool hasFrom = from.sqrMagnitude > 0.000001f;
            bool hasTo = to.sqrMagnitude > 0.000001f;

            // Aim 开始/结束属于离散语义，不在零向量和有效方向之间制造假方向 
            if (!hasFrom || !hasTo)
                return t < 0.5f ? (hasFrom ? from.normalized : Vector2.zero) : (hasTo ? to.normalized : Vector2.zero);

            float fromYaw = Mathf.Atan2(from.x, from.y) * Mathf.Rad2Deg;
            float toYaw = Mathf.Atan2(to.x, to.y) * Mathf.Rad2Deg;
            float yaw = Mathf.LerpAngle(fromYaw, toYaw, t) * Mathf.Deg2Rad;

            return new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
        }
    }
}
