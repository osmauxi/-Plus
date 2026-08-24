using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 玩家同步各层共享的参数集合。
    /// 序列化字段保存设计值；公开属性提供只读访问，并把秒/频率派生为 Tick 间隔。
    /// </summary>
    [Serializable]
    public sealed class PlayerSyncConfig
    {
        [Header("固定模拟")]
        // Owner Prediction 与 Server Authority 每秒共同执行的固定模拟次数，必须与 NGO TickRate 一致。
        [Tooltip("玩家预测与权威模拟每秒执行的 Tick 数。增大：响应更细腻但 CPU/带宽压力更高；减小：成本更低但离散感更明显。应与 NGO TickRate 一致。")]
        [InspectorName("模拟 Tick 率")]
        [SerializeField, Range(20, 120)] private int _simulationTickRate = 30;

        // 输入历史和预测状态历史各自可保留的 Tick 数，也是回滚最大理论覆盖窗口。
        [Tooltip("预测输入和状态历史可保留的 Tick 数。增大：可承受更长延迟但占用更多内存；减小：更省内存但更易硬校正。")]
        [InspectorName("历史容量")]
        [SerializeField, Min(64)] private int _historyCapacity = 1024;

        [Header("服务器输入容错")]
        // 缺少 Exact/Retimed 输入时，Server 最多连续沿用上一输入的 Tick 数。
        [Tooltip("丢包时服务器最多沿用上一输入的 Tick 数。增大：短时丢包更平滑但松键可能延迟；减小：更快回中立但更易顿挫。")]
        [InspectorName("输入沿用 Tick 数")]
        [SerializeField, Min(0)] private int _maxInputHoldTicks = 3;

        // Client 输入允许领先 LastProcessedTick 的安全窗口，防止异常未来 Tick 挤占历史。
        [Tooltip("客户端输入允许领先服务器的最大 Tick 数。增大：容忍更大偏差但安全边界更宽；减小：过滤更严格但误拒绝风险更高。")]
        [InspectorName("最大未来输入 Tick")]
        [SerializeField, Min(1)] private int _maxFutureInputTicks = 16;

        // 最新 Client 输入已迟到时，允许把它重标到下一权威 Tick 的最大年龄。
        [Tooltip("客户端最新输入已经迟到时，允许服务器将其重定时到下一权威 Tick 的最大迟到范围。增大：更抗高延迟但旧输入生效窗口更长；减小：更严格但更易在弱网下回中立。")]
        [InspectorName("迟到输入重定时 Tick")]
        [SerializeField, Min(0)] private int _maxLateInputRetimingTicks = 12;

        [Header("预测校正阈值")]
        // 同 Tick 预测位置与权威位置的距离超过该值时触发普通回滚。
        [Tooltip("同 Tick 位置误差超过此值时回滚。增大：校正更少但偏差更明显；减小：更精确但回滚更频繁。")]
        [InspectorName("位置误差阈值")]
        [SerializeField, Min(0f)] private float _positionErrorThreshold = 0.08f;

        // 同 Tick 预测朝向与权威朝向的夹角超过该值时触发普通回滚。
        [Tooltip("同 Tick 朝向角误差超过此值时回滚。增大：校正更少；减小：朝向更严格一致。")]
        [InspectorName("旋转误差阈值")]
        [SerializeField, Min(0f)] private float _rotationErrorThreshold = 2f;

        // 同 Tick 预测速度与权威速度的向量距离超过该值时触发普通回滚。
        [Tooltip("同 Tick 速度误差超过此值时回滚。增大：减少回滚；减小：速度更严格一致。")]
        [InspectorName("速度误差阈值")]
        [SerializeField, Min(0f)] private float _velocityErrorThreshold = 0.25f;

        // 同 Tick 预测体力与权威体力的绝对差超过该值时触发普通回滚。
        [Tooltip("体力误差超过此值时回滚。增大：减少微小校正；减小：体力更严格一致。")]
        [InspectorName("体力误差阈值")]
        [SerializeField, Min(0f)] private float _staminaErrorThreshold = 0.5f;

        [Header("网络发送")]
        // Server 每秒期望下发的权威 Snapshot 数；Controller 通过整数 Budget 调度。
        [Tooltip("服务器每秒发送权威快照的目标次数。增大：观察者更平滑但带宽更高；减小：带宽更低但插值更依赖缓冲。")]
        [InspectorName("快照发送频率")]
        [SerializeField, Range(1, 60)] private int _snapshotSendRate = 20;

        // 每个输入消息携带的输入数量：当前输入加最多两份历史输入。
        [Tooltip("每个输入包携带的当前及历史输入份数。增大：抗丢包更强但包更大；减小：包更小但更怕连续丢包。")]
        [InspectorName("输入冗余份数")]
        [SerializeField, Range(1, 3)] private int _inputRedundancy = 3;

        // 输入保持活跃且没有立即变化时，每秒期望发送的消息数。
        [Tooltip("持续输入每秒发送的目标次数。增大：输入更及时但带宽更高；减小：带宽更低但服务器更新更慢。")]
        [InspectorName("输入发送频率")]
        [SerializeField, Range(1, 60)] private int _inputSendRate = 30;

        // 完全空闲时发送中立输入的最大时间间隔，用于最终纠正丢失的松键状态。
        [Tooltip("完全静止时强制发送中立输入的间隔。增大：空闲带宽更低但纠正卡键更慢；减小：纠正更快但心跳更多。")]
        [InspectorName("空闲心跳间隔")]
        [SerializeField, Min(0.1f)] private float _idleHeartbeatSeconds = 1f;

        // Move 或 Aim 向量相对最近发送值超过该差值时，绕过频率限制立即发送。
        [Tooltip("移动或瞄准方向变化超过该值时立即发送。增大：过滤更多微变但更新更迟；减小：更灵敏但包更多。")]
        [InspectorName("方向立即发送阈值")]
        [SerializeField, Min(0f)] private float _immediateMoveChangeThreshold = 0.1f;

        [Header("观察者插值")]
        // Observer 表现时间故意落后最新权威时间的秒数。
        [Tooltip("远端渲染落后权威时间的秒数。增大：抗抖动更强但视觉延迟更高；减小：更实时但更易卡顿。")]
        [InspectorName("远端插值延迟")]
        [SerializeField, Min(0f)] private float _remoteInterpolationDelay = 0.1f;

        // PlayerRemoteInterpolation 可同时保存的完整状态数量。
        [Tooltip("远端快照环形缓冲容量。增大：覆盖更长抖动但占用更多内存；减小：更省内存但更易耗尽缓冲。")]
        [InspectorName("远端快照容量")]
        [SerializeField, Min(4)] private int _remoteSnapshotBufferCapacity = 32;

        // 连续两份 Observer Snapshot 位移超过该距离时视为传送并清空旧轨迹。
        [Tooltip("相邻权威快照位移超过该距离时视为传送并清空旧插值历史。增大：较大位移仍会平滑插值；减小：更容易直接跳到新位置。")]
        [InspectorName("远端传送判定距离")]
        [SerializeField, Min(0.1f)] private float _remoteTeleportDistance = 5f;

        // 两个 Full Snapshot 之间最多发送的 Delta 数；0 表示禁用 Delta。
        [Tooltip("两个完整关键帧之间最多允许发送多少个增量快照。20Hz快照下填9约等于每0.5秒一次完整关键帧。设为0表示始终发送完整快照。")]
        [InspectorName("关键帧间最大增量数")]
        [SerializeField, Range(0, 60)] private int _maxDeltaSnapshotsBetweenKeyframes = 9;

        /// <summary>服务器每秒目标快照数。</summary>
        public int SnapshotSendRate => _snapshotSendRate;
        /// <summary>预测端和服务器每秒执行的固定模拟 Tick（固定同步步）数。</summary>
        public int SimulationTickRate => _simulationTickRate;
        /// <summary>输入历史和状态历史各自拥有的槽位数量。</summary>
        public int HistoryCapacity => _historyCapacity;
        /// <summary>缺失新输入时最多连续沿用上一输入的 Tick 数。</summary>
        public int MaxInputHoldTicks => _maxInputHoldTicks;
        /// <summary>客户端输入允许领先服务器已处理位置的最大 Tick 数。</summary>
        public int MaxFutureInputTicks => _maxFutureInputTicks;
        /// <summary>最新迟到输入仍可被重新安排到下一权威 Tick 的最大落后量。</summary>
        public int MaxLateInputRetimingTicks => _maxLateInputRetimingTicks;
        /// <summary>触发预测回滚的位置距离误差阈值，单位为米。</summary>
        public float PositionErrorThreshold => _positionErrorThreshold;
        /// <summary>触发预测回滚的朝向夹角误差阈值，单位为度。</summary>
        public float RotationErrorThreshold => _rotationErrorThreshold;
        /// <summary>触发预测回滚的速度向量距离误差阈值，单位为米/秒。</summary>
        public float VelocityErrorThreshold => _velocityErrorThreshold;
        /// <summary>触发预测回滚的体力绝对误差阈值。</summary>
        public float StaminaErrorThreshold => _staminaErrorThreshold;
        /// <summary>每个输入包携带的输入份数，包含当前输入和历史冗余。</summary>
        public int InputRedundancy => _inputRedundancy;
        /// <summary>移动或瞄准向量变化时绕过发送节流的差值阈值。</summary>
        public float ImmediateMoveChangeThreshold => _immediateMoveChangeThreshold;
        /// <summary>Observer（观察者）插值器可保存的最大快照数量。</summary>
        public int RemoteSnapshotBufferCapacity => _remoteSnapshotBufferCapacity;
        /// <summary>相邻权威状态被判定为传送的距离阈值，单位为米。</summary>
        public float RemoteTeleportDistance => _remoteTeleportDistance;
        /// <summary>把配置的插值延迟秒数换算成可带小数的 Tick 数。</summary>
        public float RemoteInterpolationDelayTicks => _remoteInterpolationDelay * _simulationTickRate;
        /// <summary>活跃输入在没有突变时，两次发送之间的最小 Tick 数。</summary>
        public int InputSendIntervalTicks => Mathf.Max(1, Mathf.RoundToInt((float)_simulationTickRate / _inputSendRate));
        /// <summary>完全空闲时，两次中性心跳之间的最小 Tick 数。</summary>
        public int IdleHeartbeatTicks => Mathf.Max(1, Mathf.RoundToInt(_idleHeartbeatSeconds * _simulationTickRate));
        /// <summary>由频率近似换算的快照 Tick 间隔；实际调度使用整数 Budget（累加预算）。</summary>
        public int SnapshotSendIntervalTicks => Mathf.Max(1, Mathf.RoundToInt((float)_simulationTickRate / _snapshotSendRate));
        /// <summary>两个 Full Snapshot（完整快照）之间最多发送的 Delta Snapshot（差量快照）数量。</summary>
        public int MaxDeltaSnapshotsBetweenKeyframes => _maxDeltaSnapshotsBetweenKeyframes;

        /// <summary>验证会破坏时间轴或环形历史安全边界的配置组合。</summary>
        public void Validate()
        {
            if (_simulationTickRate <= 0) throw new InvalidOperationException("模拟 Tick 率必须大于 0。");
            if (_historyCapacity < 64) throw new InvalidOperationException("历史容量不应小于 64。");
            if (_maxFutureInputTicks >= _historyCapacity) throw new InvalidOperationException("最大未来输入 Tick 必须小于历史容量。");
            if (_maxLateInputRetimingTicks >= _historyCapacity) throw new InvalidOperationException("迟到输入重定时 Tick 必须小于历史容量。");
            if (_remoteSnapshotBufferCapacity < 4) throw new InvalidOperationException("远端快照容量不应小于 4。");
        }
    }
}
