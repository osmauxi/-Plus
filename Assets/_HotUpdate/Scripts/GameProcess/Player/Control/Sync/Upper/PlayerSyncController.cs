using System;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using ProjectGame.HotFix.Gameplay.Network;
using ProjectGame.HotFix.Gameplay.Pooling;
using ProjectGame.HotFix.Gameplay.Player.Movement;
using ProjectGame.HotFix.Gameplay.Player.Stamina;
using ProjectGame.HotFix.Gameplay.Player.State;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 单个玩家的网络同步总控。
    ///
    /// 根据当前实例身份执行不同路径：
    ///
    /// Remote Client Owner：
    /// Input -> Prediction -> SendInput -> Reconcile
    ///
    /// Server：
    /// ReceiveInput -> Authority Simulation -> Snapshot
    ///
    /// Remote Observer：
    /// ReceiveSnapshot -> Buffered Interpolation
    ///
    /// Host Owner：
    /// 直接执行Server Authority，不额外做一遍Client Prediction。
    /// </summary>
    // Remote Observer 必须先在 LateUpdate 应用快照表现状态，随后 Presentation 和 Camera 才能取到本帧位姿。
    [DefaultExecutionOrder(-400)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerSyncController : NetworkBehaviour, IPlayerSyncEndpoint, IPoolable
    {
        /// <summary>固定 Tick（固定同步步编号）、容错、校正、发送频率与插值参数。</summary>
        [Header("同步配置")]
        [Tooltip("固定 Tick、输入容错、预测校正、发送频率和远端插值的完整配置。")]
        [InspectorName("玩家同步参数")]
        [SerializeField] private PlayerSyncConfig _config = new();

        /// <summary>自由、瞄准、冲刺及底层运动状态判定参数。</summary>
        [Header("移动模拟配置")]
        [Tooltip("构造纯 C# PlayerMotor 时使用的移动响应和运动状态参数。")]
        [InspectorName("玩家移动参数")]
        [SerializeField] private PlayerMovementConfig _movementConfig = new();

        /// <summary>冲刺消耗、恢复延迟、恢复速度和耗尽解除参数。</summary>
        [Tooltip("构造纯 C# PlayerLocomotionController 时使用的体力参数。")]
        [InspectorName("玩家体力参数")]
        [SerializeField] private PlayerStaminaConfig _staminaConfig = new();

        /// <summary>
        /// 受击、连续射击和换弹的确定性时间规则。
        /// 这些值属于 Simulation，不应从 Animator Clip 时长反推。
        /// </summary>
        [Header("玩家动作配置")]
        [Tooltip("受击持续、射击节奏和换弹时间；全部按固定模拟 Tick 推进。")]
        [InspectorName("玩家动作参数")]
        [SerializeField] private PlayerActionConfig _actionConfig = new();

        /// <summary>预测端与服务器共用的纯 C# 单 Tick（单个固定同步步）模拟入口。</summary>
        private PlayerSimulation _simulation;

        /// <summary>
        /// 当前玩家端点的纯 C# 固定步时钟；统一保存 Tick 编号并计算单步时长。
        /// 当前仍由 NGO NetworkTickSystem.Tick 驱动一次推进一步。
        /// </summary>
        private PlayerSimulationClock _simulationClock;

        /// <summary>当前网络会话共享的玩家同步消息传输与路由服务。</summary>
        private PlayerSyncTransport _transport;

        /// <summary>仅普通 Owner Client（拥有者客户端）存在的本地预测、历史记录与权威校正模块。</summary>
        private PlayerPrediction _prediction;

        /// <summary>仅服务器存在的输入缓冲和权威固定 Tick 模拟模块。</summary>
        private PlayerServerAuthority _serverAuthority;

        /// <summary>仅远端观察客户端存在的快照缓冲与延迟插值模块。</summary>
        private PlayerRemoteInterpolation _remoteInterpolation;

        /// <summary>快照发送的整数累加预算，用于从模拟频率稳定换算到快照频率，避免浮点计时漂移。</summary>
        private int _snapshotSendBudget;

        // Driver（本地输入驱动器）每帧只负责把最新输入意图提交进来。
        // 真正的 Tick（固定同步步编号）由 SyncController（同步总控）在网络 Tick 回调中分配。
        // 持续按钮保存最新值；ReloadRequestSequence 持续保存最新边沿序号，直到状态机消费。
        private PlayerInputCommand _latestLocalInput;

        /// <summary>该玩家端点是否已完成角色模块创建、传输注册和 Tick 订阅。</summary>
        private bool _initialized;

        /// <summary>是否已经订阅 NGO（Netcode for GameObjects）网络 Tick 事件，用于安全且仅一次地退订。</summary>
        private bool _subscribedToNetworkTick;

        /// <summary>服务器构造 Delta Snapshot（差量快照）时使用的最近 Full Snapshot（完整快照）基准状态。</summary>
        private PlayerSimulationState _snapshotBaseline;

        /// <summary>服务器当前是否已经建立可供差量编码使用的 Baseline（基准完整状态）。</summary>
        private bool _hasSnapshotBaseline;

        /// <summary>当前完整快照之后已经发送的差量快照数量，用于周期性插入新的关键完整快照。</summary>
        private int _deltaSnapshotsSinceKeyframe;

        #region Debug Metrics

        /// <summary>该拥有者客户端累计发送的输入包数；每包可能带历史冗余输入。</summary>
        public int InputPacketSendCount { get; private set; }

        /// <summary>服务器为该玩家累计发送的快照消息数；每个目标客户端分别计数。</summary>
        public int SnapshotSendCount { get; private set; }

        /// <summary>该客户端为此玩家接收并路由成功的权威状态数。</summary>
        public int SnapshotReceiveCount { get; private set; }

        /// <summary>预测状态与权威状态误差超阈值后执行 Restore + Replay（恢复并重演）的次数。</summary>
        public int RollbackCount => _prediction?.RollbackCount ?? 0;

        /// <summary>因服务器 Tick 超出本地预测历史而直接采用权威状态的硬同步次数。</summary>
        public int HardResyncCount => _prediction?.HardResyncCount ?? 0;

        /// <summary>最近一次回滚校正重新模拟的 Tick 数。</summary>
        public int LastReplayTickCount => _prediction?.LastReplayTickCount ?? 0;

        /// <summary>最近一次预测校验的本地位置与权威位置距离，单位为米。</summary>
        public float LastPositionError => _prediction?.LastPositionError ?? 0f;

        /// <summary>最近一次预测校验的本地朝向与权威朝向夹角，单位为度。</summary>
        public float LastRotationError => _prediction?.LastRotationError ?? 0f;

        /// <summary>最近一次预测校验的本地速度与权威速度差值大小，单位为米/秒。</summary>
        public float LastVelocityError => _prediction?.LastVelocityError ?? 0f;

        /// <summary>服务器当前缓存的、领先于已处理 Tick 的输入跨度。</summary>
        public int ServerBufferedInputTicks => _serverAuthority?.BufferedAheadTicks ?? 0;

        /// <summary>服务器接受并写入缓冲的有效输入数量。</summary>
        public int ServerAcceptedInputCount => _serverAuthority?.AcceptedInputCount ?? 0;

        /// <summary>服务器收到的重复 Tick 输入数量。</summary>
        public int ServerDuplicateInputCount => _serverAuthority?.DuplicateInputCount ?? 0;

        /// <summary>服务器拒绝的过旧输入数量。</summary>
        public int ServerOutdatedInputCount => _serverAuthority?.OutdatedInputCount ?? 0;

        /// <summary>服务器因 NaN（非数）或 Infinity（无穷值）等不可修复数据而拒绝的输入数量。</summary>
        public int ServerInvalidInputCount => _serverAuthority?.InvalidInputCount ?? 0;

        /// <summary>服务器拒绝的、超出允许未来窗口的输入数量。</summary>
        public int ServerInvalidFutureInputCount => _serverAuthority?.InvalidFutureInputCount ?? 0;

        /// <summary>服务器按目标 Tick 找到完全匹配输入并执行的次数。</summary>
        public int ServerExactInputTickCount => _serverAuthority?.ExactInputTickCount ?? 0;

        /// <summary>服务器接收时将轻微迟到输入重新安排到未来 Tick 的输入数量。</summary>
        public int ServerRetimedLateAcceptedInputCount => _serverAuthority?.RetimedLateAcceptedInputCount ?? 0;

        /// <summary>服务器模拟时实际消费 Retimed Late（重新定时的迟到输入）的 Tick 数。</summary>
        public int ServerRetimedLateInputTickCount => _serverAuthority?.RetimedLateInputTickCount ?? 0;

        /// <summary>输入短暂缺失时服务器复用最近输入的 Tick 数。</summary>
        public int ServerReusedInputTickCount => _serverAuthority?.ReusedInputTickCount ?? 0;

        /// <summary>连续缺失超过保持上限后服务器执行 Neutral（中性，即无移动、无按键）输入的 Tick 数。</summary>
        public int ServerNeutralInputTickCount => _serverAuthority?.NeutralInputTickCount ?? 0;

        /// <summary>远端插值缓冲区当前保存的权威快照数量。</summary>
        public int RemoteBufferedSnapshots => _remoteInterpolation?.BufferedSnapshotCount ?? 0;

        /// <summary>最近一次模拟、恢复或插值应用后的运动表现状态。</summary>
        public PlayerMotionState MotionState => _simulation?.MotionState ?? default;

        /// <summary>当前生命状态和移动模式。</summary>
        public PlayerControlState ControlState => _simulation?.ControlState ?? default;
        /// <summary>当前最终表现时间轴上的世界平面瞄准方向，Vector2(x,z)。</summary>
        public Vector2 AimDirection => _simulation?.AimDirection ?? Vector2.zero;
        /// <summary>Aim Root 当前是否处于迟滞跟随阶段。</summary>
        public bool IsAimBodyTurning => _simulation?.IsAimBodyTurning ?? false;
        /// <summary>
        /// Aim Root 开始跟随前允许的最大水平偏角。
        /// 表现层用它限制上半身扭转，避免动画阈值与 Simulation 判定各自维护一份数值。
        /// </summary>
        public float AimBodyTurnStartAngle => _movementConfig?.AimBodyTurnStartAngle ?? 0f;
        /// <summary>当前体力、恢复延迟和耗尽状态。</summary>
        public PlayerStaminaState StaminaState => _simulation?.StaminaState ?? default;
        /// <summary>
        /// 当前最终表现时间轴上的动作运行状态。
        /// Owner 对应预测/校正结果，Remote 对应插值选择结果，Animator Driver 无需自行判断网络身份。
        /// </summary>
        public PlayerActionRuntimeState ActionState => _simulation?.ActionState ?? default;

        /// <summary>当前体力相对最大体力的 0~1 比例。</summary>
        public float NormalizedStamina => _simulation?.NormalizedStamina ?? 0f;

        /// <summary>当前玩家模拟时钟已经完成的最新 Tick。</summary>
        public uint SimulationTick => _simulationClock?.CurrentTick ?? 0u;

        /// <summary>当前玩家固定模拟 Tick 的时长，单位为秒。</summary>
        public float SimulationTickDeltaTime => _simulationClock?.TickDeltaTime ?? 0f;

        #endregion

        /// <summary>
        /// 取得 Unity CharacterController（角色控制器），按 Body → Motor → Locomotion → Simulation 顺序
        /// 创建纯 C# 模拟链，并在网络生成前校验同步配置。
        /// </summary>
        private void Awake()
        {
            _config.Validate();

            CharacterController characterController = GetComponent<CharacterController>();
            IPlayerCharacterBody body = new CharacterControllerPlayerBody(transform, characterController);
            PlayerMotor motor = new(body, _movementConfig);
            PlayerLocomotionController locomotion = new(motor, _staminaConfig, _actionConfig);
            _simulation = new PlayerSimulation(locomotion);
            _simulationClock = new PlayerSimulationClock(_config.SimulationTickRate);
        }

        /// <summary>
        /// 网络对象生成时，按 Server（服务器）、Owner（拥有者）和 Observer（观察者）身份创建所需同步模块，
        /// 注册消息端点并订阅固定网络 Tick。
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _transport = GameNetworkRuntime.PlayerSync;

            if (_transport == null || !_transport.IsInitialized)
            {
                Debug.LogError($"[{nameof(PlayerSyncController)}] PlayerSyncTransport 尚未初始化。");
                return;
            }

            ValidateTickRate();
            uint initialTick = ResolveInitialTick();
            // NetworkObject 可能来自对象池；建立本次会话前先清掉死亡、体力、动作和 Motor 惯性。
            // ResetRuntimeState 保留对象池刚设置好的 Transform Pose。
            _simulation.ResetRuntimeState();
            _simulationClock.Reset(initialTick);
            PlayerSimulationState initialState = _simulation.CaptureState(initialTick);

            // Server（服务器）无论玩家 Owner（拥有者）是谁，都需要一份 Authority Simulation（权威模拟）。
            if (IsServer)
            {
                _serverAuthority = new PlayerServerAuthority(_simulation, _simulationClock, _config);
                _serverAuthority.Initialize(initialState);
            }

            // 普通 Remote Client Owner（远端拥有者客户端）需要 Prediction（本地预测）。
            // Host（主机）自己就是 Authority（权威端），不能在同一个 PlayerSimulation 上再跑一遍预测，否则会重复模拟。
            if (IsOwner && !IsServer)
            {
                _prediction = new PlayerPrediction(_simulation, _simulationClock, _config);
                uint confirmedTick = NetworkManager != null
                    ? unchecked((uint)NetworkManager.ServerTime.Tick)
                    : initialState.Tick;
                _prediction.Reset(initialState.Tick, confirmedTick);
            }

            // 普通 Observer（观察者）不产生输入，只需要 Snapshot（快照）插值。
            if (!IsOwner && !IsServer)
                _remoteInterpolation = new PlayerRemoteInterpolation(_config);

            _transport.RegisterEndpoint(NetworkObjectId, OwnerClientId, this);
            _latestLocalInput = PlayerInputCommand.CreateNeutral(initialState.Tick);

            if ((IsServer || IsOwner) && NetworkManager?.NetworkTickSystem != null)
            {
                // 当前 NGO NetworkTickSystem 只负责提供“推进信号”；Tick 编号和单步时长由纯 C# 时钟维护。
                // TODO(PlayerSimulationClockDriver)：二轮重构时可替换为独立时间累加器、追帧和漂移校正。
                NetworkManager.NetworkTickSystem.Tick += HandleNetworkTick;
                _subscribedToNetworkTick = true;
            }
            ResetSnapshotBaseline();
            _snapshotSendBudget = 0;
            _initialized = true;
        }

        /// <summary>网络对象销毁时退订 Tick、注销路由，并清空各身份模块保存的历史状态。</summary>
        public override void OnNetworkDespawn()
        {
            if (_subscribedToNetworkTick && NetworkManager?.NetworkTickSystem != null)
                NetworkManager.NetworkTickSystem.Tick -= HandleNetworkTick;

            _subscribedToNetworkTick = false;

            if (_initialized)
                _transport?.UnregisterEndpoint(NetworkObjectId);

            _prediction?.Reset();
            _remoteInterpolation?.Reset();

            _prediction = null;
            _serverAuthority = null;
            _remoteInterpolation = null;
            _transport = null;
            _simulationClock?.Reset();

            ResetSnapshotBaseline();
            _snapshotSendBudget = 0;
            _latestLocalInput = default;

            _initialized = false;

            ResetReusableRuntimeState();

            base.OnNetworkDespawn();
        }

        /// <summary>
        /// LocalPlayerLocomotionDriver（本地玩家移动输入驱动器）只需提交最新输入意图。
        /// Tick（固定同步步编号）由同步层统一管理，不接受外部 Tick，以免输入时钟与网络时钟分叉。
        /// </summary>
        public void SubmitLocalInput(in PlayerInputCommand input)
        {
            if (!_initialized || !IsOwner)
                return;

            _latestLocalInput.WorldMove = input.WorldMove;
            _latestLocalInput.AimDirection = input.AimDirection;
            _latestLocalInput.Buttons = input.Buttons;
            // Reload 是累计边沿，必须与持续按钮一起保存；只保留单帧 bool 会在渲染帧与网络 Tick 之间丢失。
            _latestLocalInput.ReloadRequestSequence = input.ReloadRequestSequence;
        }

        /// <summary>
        /// 修改玩家顶层生命状态；纯 C# Simulation 会把变化纳入后续预测和权威快照。
        /// 网络会话中应由服务器 Gameplay 调用；本方法保留无网络测试入口，因此自身不做 IsServer 拦截。
        /// </summary>
        public void SetLifeState(PlayerLifeState lifeState)
        {
            _simulation?.SetLifeState(lifeState);
        }

        /// <summary>
        /// 进入或刷新受击状态。网络会话中只允许服务器 Gameplay 调用；
        /// 非网络测试或尚未 Spawn 时也可直接使用。
        /// </summary>
        public bool ApplyHit()
        {
            if (_simulation == null || (IsSpawned && !IsServer))
                return false;

            // 使用模拟时钟固定步长把受击秒数转换成 Tick；禁止使用本帧 Time.deltaTime。
            return _simulation.ApplyHit(_simulationClock.TickDeltaTime);
        }

        /// <summary>每个渲染帧为远端观察者采样并应用一次平滑展示状态。</summary>
        private void LateUpdate()
        {
            if (!_initialized)
                return;

            if (!IsServer && !IsOwner)
                UpdateRemotePresentation(Time.deltaTime);
        }

        /// <summary>
        /// 当前固定步驱动入口。每次 NGO 网络 Tick 到达时先推进纯 C# 时钟，
        /// 再让服务器或普通拥有者客户端消费该 Tick，确保两条路径不再自行递增时间。
        /// </summary>
        private void HandleNetworkTick()
        {
            if (!_initialized)
                return;

            uint tick = _simulationClock.AdvanceOneTick();

            if (IsServer)
                RunServerTick(tick);
            else if (IsOwner)
                RunOwnerPredictionTick(tick);
        }

        #region Owner Prediction

        /// <summary>为最新本地输入分配 Client Tick（客户端固定同步步编号），立即预测一帧，再按发送策略尝试上行。</summary>
        private void RunOwnerPredictionTick(uint tick)
        {
            PlayerInputCommand input = _latestLocalInput;

            // Clock（时钟）负责分配 Client Tick；Prediction 只消费该 Tick 并把输入与结果写入回滚历史。
            _prediction.Predict(ref input, tick);

            TrySendOwnerInput(input);
        }

        /// <summary>
        /// 在激活/空闲发送间隔满足时发送当前输入，并按配置附带前一、前二 Tick 的历史冗余。
        /// 冗余输入用于弥补不可靠传输中的少量丢包。
        /// </summary>
        private void TrySendOwnerInput(in PlayerInputCommand current)
        {
            if (!_prediction.ShouldSend(current))
                return;

            PlayerInputCommand? previous1 = null;
            PlayerInputCommand? previous2 = null;

            if (_config.InputRedundancy >= 2 && current.Tick > 0 &&
                _prediction.TryGetInput(current.Tick - 1, out PlayerInputCommand input1))
                previous1 = input1;

            if (_config.InputRedundancy >= 3 && current.Tick > 1 &&
                _prediction.TryGetInput(current.Tick - 2, out PlayerInputCommand input2))
                previous2 = input2;

            _transport.SendInputBatch(NetworkObjectId, current, previous1, previous2);

            _prediction.MarkSent(current);
            InputPacketSendCount++;
        }

        #endregion

        #region Server Authority

        /// <summary>
        /// 用整数预算把 SimulationTickRate（每秒模拟 Tick 数）换算成 SnapshotSendRate（每秒快照数）。
        /// 例如模拟 30 Hz、快照 20 Hz 时，每 Tick 加 20，预算达到 30 就发送并减 30。
        /// </summary>
        private void TrySendSnapshot(in PlayerSimulationState state)
        {
            _snapshotSendBudget += _config.SnapshotSendRate;

            if (_snapshotSendBudget < _config.SimulationTickRate)
                return;

            _snapshotSendBudget -= _config.SimulationTickRate;
            BroadcastSnapshot(state);
        }
        /// <summary>执行一个服务器权威 Tick；Host 玩家直接注入本地输入，然后推进模拟并尝试广播快照。</summary>
        private void RunServerTick(uint tick)
        {
            // Host（主机）自己的本地玩家不经过网络 Transport（传输层），
            // 直接使用时钟已派发的服务器 Tick，并提交到 Server Input Buffer（服务器输入缓冲）。
            if (IsOwner)
            {
                PlayerInputCommand hostInput = _latestLocalInput;
                hostInput.Tick = tick;
                _serverAuthority.PushInput(hostInput);
            }

            PlayerSimulationState serverState = _serverAuthority.SimulateNextTick(tick);
            TrySendSnapshot(serverState);
        }

        /// <summary>为当前权威状态构造一次协议包，并向除服务器自身外的所有已连接客户端发送。</summary>
        private void BroadcastSnapshot(in PlayerSimulationState state)
        {
            NetworkManager networkManager = NetworkManager;

            if (networkManager == null)
                return;

            // 同一个 Player（玩家）、同一个 Server Tick（服务器固定同步步编号）只构建一次 Snapshot（快照）。
            // 所有 Client（客户端）收到相同协议数据，因此无需为每个客户端重复计算 DirtyMask（变化字段掩码）。
            PlayerSnapshotPacket packet = BuildSnapshotPacket(state);

            foreach (ulong clientId in networkManager.ConnectedClientsIds)
            {
                if (clientId == NetworkManager.ServerClientId)
                    continue;

                _transport.SendSnapshot(clientId, NetworkObjectId, packet);
                SnapshotSendCount++;
            }
        }
        /// <summary>
        /// 首包和周期性 Keyframe（关键完整帧）发送 Full Snapshot（完整快照），其余状态相对该完整基准生成 Delta Snapshot（差量快照）。
        /// </summary>
        private PlayerSnapshotPacket BuildSnapshotPacket(in PlayerSimulationState state)
        {
            bool shouldSendFull = !_hasSnapshotBaseline || _config.MaxDeltaSnapshotsBetweenKeyframes <= 0 || _deltaSnapshotsSinceKeyframe >= _config.MaxDeltaSnapshotsBetweenKeyframes;

            if (shouldSendFull)
            {
                _snapshotBaseline = state;
                _hasSnapshotBaseline = true;
                _deltaSnapshotsSinceKeyframe = 0;
                return PlayerSnapshotPacket.CreateFull(state);
            }

            _deltaSnapshotsSinceKeyframe++;
            return PlayerSnapshotPacket.CreateDelta(state, _snapshotBaseline);
        }
        /// <summary>清除服务器差量编码基准，使下一次发送必定生成完整快照。</summary>
        private void ResetSnapshotBaseline()
        {
            _snapshotBaseline = default;
            _hasSnapshotBaseline = false;
            _deltaSnapshotsSinceKeyframe = 0;
        }
        #endregion

        #region Network Endpoint

        /// <summary>
        /// Client Input（客户端输入）经过 Transport（传输层）路由后进入 Server Buffer（服务器输入缓冲）。
        /// </summary>
        public void ReceiveInputFromClient(ulong senderClientId, in PlayerInputCommand input)
        {
            if (!_initialized || !IsServer)
                return;

            // Transport（传输层）已经验证过 OwnerClientId（拥有者客户端编号），这里不重复验证。
            _serverAuthority.PushInput(input);
        }

        /// <summary>
        /// Server Snapshot（服务器快照）经过 Transport（传输层）后进入客户端。
        /// Owner（拥有者）执行 Reconciliation（权威校正）；Remote Observer（远端观察者）写入插值 Buffer（缓冲区）。
        /// </summary>
        public void ReceiveServerSnapshot(in PlayerSimulationState state)
        {
            if (!_initialized || IsServer)
                return;

            SnapshotReceiveCount++;

            if (IsOwner)
            {
                _prediction?.Reconcile(state);
                return;
            }

            _remoteInterpolation?.PushSnapshot(state);
        }

        #endregion

        #region Remote Presentation

        /// <summary>按渲染时间从远端缓冲采样状态，并只恢复展示状态，不执行本地移动模拟。</summary>
        private void UpdateRemotePresentation(float deltaTime)
        {
            if (_remoteInterpolation == null)
                return;

            if (!_remoteInterpolation.TrySample(deltaTime, out PlayerSimulationState renderState))
                return;

            _simulation.RestoreState(renderState);
        }

        /// <summary>
        /// 传送、换层或复活后把模拟器放到指定位置，并清空预测、权威、插值及快照 Baseline（基准完整状态）历史，
        /// 防止传送前的旧状态参与后续回滚或插值。
        /// </summary>
        public void ResetAfterWarp(Vector3 position, Quaternion rotation)
        {
            uint tick = IsSpawned && NetworkManager != null
                ? unchecked((uint)NetworkManager.ServerTime.Tick)
                : 0u;
            _simulationClock.Reset(tick);
            PlayerSimulationState state = _simulation.CaptureState(tick);
            state.Position = position;
            state.Rotation = rotation;
            state.Velocity = Vector3.zero;
            state.AngularSpeed = 0f;
            state.IsAimBodyTurning = false;
            state.PivotDirection = PlayerPivotDirection.None;
            state.ControlState.ReactionMode = PlayerReactionMode.Normal;
            state.ControlState.CombatMode = PlayerCombatMode.Ready;

            PlayerActionRuntimeState actionState = state.ActionState;
            // Warp 只取消正在执行的短时动作；保留 Shot/Hit Sequence 和已消费 Reload 序号，
            // 避免 Animator 重绑或中立输入把传送前事件重新播放/重新消费。
            actionState.HitTicksRemaining = 0;
            actionState.ReloadTicksRemaining = 0;
            actionState.FireCooldownTicks = 0;
            state.ActionState = actionState;

            _simulation.RestoreState(state);
            _prediction?.Reset(tick);
            _serverAuthority?.Reset(state);
            _remoteInterpolation?.Reset();
            _latestLocalInput = PlayerInputCommand.CreateNeutral(tick);
            _latestLocalInput.ReloadRequestSequence =
                state.ActionState.LastReloadRequestSequence;
            ResetSnapshotBaseline();
            _snapshotSendBudget = 0;

            if (IsOwner)
                LocalEvents.Publish<GameplayCameraSnapRequestedEvent>();
        }

        /// <summary>
        /// SyncObjectPool 在 Network Spawn 前调用。此时 Transform 已经是新出生点，
        /// 这里只重建模拟默认状态，不改写对象池设置的 Pose。
        /// </summary>
        public void OnRentFromPool()
        {
            ResetReusableRuntimeState();
        }

        /// <summary>Network Despawn 后再次兜底清理，保证下一位 Owner 不继承旧会话历史。</summary>
        public void OnReturnToPool()
        {
            ResetReusableRuntimeState();
        }

        private void ResetReusableRuntimeState()
        {
            _simulation?.ResetRuntimeState();
            _simulationClock?.Reset();
            _prediction?.Reset();
            _remoteInterpolation?.Reset();
            _prediction = null;
            _serverAuthority = null;
            _remoteInterpolation = null;
            _transport = null;
            _latestLocalInput = default;
            _snapshotSendBudget = 0;
            ResetSnapshotBaseline();
            ResetDebugMetrics();
        }

        private void ResetDebugMetrics()
        {
            InputPacketSendCount = 0;
            SnapshotSendCount = 0;
            SnapshotReceiveCount = 0;
        }

        /// <summary>
        /// 选择生成时的同步起点。普通拥有者客户端采用通常领先的 LocalTime（本地网络时间），
        /// 服务器和观察者采用 ServerTime（服务器网络时间）。
        /// </summary>
        private uint ResolveInitialTick()
        {
            if (NetworkManager == null)
                return 0u;

            // 客户端 LocalTime（本地网络时间）通常领先 ServerTime（服务器网络时间），可让输入提前进入服务器未来缓冲，
            // 而不会因正常网络传输延迟落后于服务器已处理 Tick。
            int networkTick = IsOwner && !IsServer
                ? NetworkManager.LocalTime.Tick
                : NetworkManager.ServerTime.Tick;
            return unchecked((uint)networkTick);
        }

        /// <summary>检查玩家模拟频率是否与 NGO 网络 TickRate（每秒网络 Tick 数）一致，并在不一致时给出时钟漂移警告。</summary>
        private void ValidateTickRate()
        {
            if (NetworkManager == null || NetworkManager.NetworkConfig == null)
                return;

            int networkTickRate = (int)NetworkManager.NetworkConfig.TickRate;
            if (networkTickRate != _config.SimulationTickRate)
                Debug.LogWarning($"[{nameof(PlayerSyncController)}] 玩家模拟 TickRate={_config.SimulationTickRate}，NGO TickRate={networkTickRate}。请保持一致以避免时钟漂移。", this);
        }

        #endregion
    }
}
