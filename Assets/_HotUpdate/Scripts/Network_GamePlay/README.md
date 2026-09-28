# Gameplay 通用网络框架

本程序集是 Player、Weapon、Projectile、AreaEffect 等 Gameplay 网络系统共同依赖的底层，程序集名为 `HotFix.Gameplay.Network`。它只依赖 NGO 与 Collections，不反向依赖 `HotFix.Gameplay`，因此不会把任何具体玩法带入基础层。

## 当前结构

```text
GameNetworkRuntime (Unity 场景服务，HotFix.Gameplay)
  -> GameplayNetworkBootstrap (NGO 生命周期 + 一次性时间锚点)
    -> GameplayNetworkRuntime
      -> NetworkSimulationClock
      -> NetworkMessageTransport
      -> NetworkTransportStats
  -> GameplayNetworkTickDriver (独立 Update 固定步驱动)
    -> Clock.TickAdvanced (Player 等模拟)
    -> Clock.TickCompleted (所有 Player 完成后的 Weapon / Projectile)
```

`NetworkSimulationConfig` 只保存会话级 `TickRate`，默认 30 Hz，支持 20~120 Hz。各 Peer 和 Player 配置必须一致，但可以独立于 NGO `TickRate` 设置；目前保留 Player 配置并在 Spawn 时校验，不做运行中变频或跨端频率协商。Delivery 由业务层选择 `NetworkDeliveryClass` 语义，再由 Transport 集中映射成 NGO `NetworkDelivery`。

## 独立 Tick 与启动对时

`GameplayNetworkTickDriver` 挂在 GameRoot 的 GameNetworkRuntime 对象上，由后者独占初始化和关闭。它以执行顺序 100 在 `Update` 收集完本地输入后累计 `Time.unscaledDeltaTime`，用 double 累计器按 `1d / TickRate` 派发固定步。初始化所在帧不重复累计锚点之前的帧时间，`LateUpdate` 表现仍读取本帧模拟结果。

每帧最多补 4 个连续 Tick，多余的完整步时间计入 `DroppedTimeSeconds` 并丢弃，保留不足一步的余数；不会直接跳 Tick 编号，也不会放大单步模拟 dt。正式运行仅 Driver 调用 `Clock.AdvanceOneTick()`；测试可以调用 `Driver.Advance(seconds)` 注入帧时间。

Bootstrap 只在连接完成后读取一次 NGO LocalTime/ServerTime 的秒数，并按 Gameplay TickRate 换算锚点。Server 的 `CurrentTick` 与 `EstimatedServerTick` 相同；Client 的前者采用 LocalTime 起点，后者采用 ServerTime 起点，之后随独立 Tick 等速推进。Bootstrap 不再订阅 `NetworkTickSystem.Tick`，Player 不再直接读取 NGO 时间。

`EstimatedServerTick` 是启动对齐后的本地估计，不是持续同步的权威时间。本轮不实现周期性 TickSync、漂移校正或自适应追帧。持续低帧率/长暂停可能使各端时钟偏移；Player 仍按现有 Reconcile/Hard Resync 处理，不能据此保证弱网下 Hard Resync 为零。

生命周期顺序为 Runtime/Transport → PlayerSync 协议 → TickDriver；关闭时先停 Driver，再关 PlayerSync 和 Runtime。生产管线等待所有 RuntimeReady 后才 Spawn 玩家；跨进程测试 Harness 也等待客户端连接及其 RuntimeReady 消息。

## PlayerSync 接入边界

PlayerSync 已接入通用 Clock 与 Transport，但以下玩家专属能力仍留在 Player 模块：

- 输入冗余、迟到输入重定时与 Hold；
- Prediction、Reconciliation、Rollback/Replay；
- Full/Delta Snapshot 与 Baseline；
- Remote Interpolation。

全局 `NetworkSimulationClock` 只表示会话 Tick，不能被单个玩家重置。`PlayerSimulationClock` 现在是玩家专用模拟游标：Server 每 Tick 对齐全局会话时间，Owner 在 Hard Resync 后可以独立回到权威 Tick，不会影响 Weapon 或其他玩家。

Owner 的初始确认点、Observer 的初始展示 Tick 取 `EstimatedServerTick`；Server Warp 取 `CurrentTick`，确保下一次权威模拟仍是连续的下一 Tick。输入/快照内容、发送预算、Owner 预测回滚及 Observer 的 LateUpdate 插值保持原有规则。

## 新业务接入方式

业务系统从 `GameNetworkRuntime.Gameplay` 获取共享 Runtime：

```csharp
GameplayNetworkRuntime runtime = GameNetworkRuntime.Gameplay;
runtime.Clock.TickAdvanced += HandleGameplayTick;
runtime.Transport.RegisterHandler(MessageName, HandleMessage);
```

发送时由业务决定消息格式和语义：

```csharp
runtime.Transport.SendToServer(
    WeaponMessageNames.FireCommand,
    writer,
    NetworkDeliveryClass.Command);
```

统计从 `runtime.Stats` 读取，按消息名提供发送次数和 Payload 字节数。业务层仍需自行维护拒绝命令数、缺失 Baseline、预测误差等策略指标。

Runtime 同时提供会话内稳定的常用 ID 视图：`ConnectedClientIds` 表示当前 Peer 可见的全部连接，`RemoteClientIds` 表示 Server/Host 侧排除 `ServerClientId` 后的远端玩家，另有 `LocalClientId`、`ServerClientId` 和 `HasRemoteClients`。这些列表只在连接或断开时维护，广播热路径无需重复筛选或分配临时集合。

Weapon 第一版应直接建立自己的 `FireCommand`、`WeaponSnapshot` 与 `WeaponReplication`，暂时不要把 Player 的 Sequence/Baseline/Delta 实现抽成泛型公共库；等两个真实实现证明存在重复后再上提。

## 验证入口

- `GameplayNetworkFrameworkTests`：秒数换算、服务器估计游标、回绕及玩家游标隔离。
- `GameplayNetworkTickDriverTests`：20/30/60/144 FPS 与不同 TickRate 组合、不规则帧余数、追帧上限、Tick 回绕、模拟/后处理事件顺序和启停。
- `PlayerSyncPipelineTests`：原有输入、预测、回滚和插值回归；补充 Hard Resync 只重置 Owner 游标、下一次全局 Tick 仍继续预测的验证。
- `GameplayNetworkRuntimeTests`（PlayMode）：NGO 60 Hz / Gameplay 30 Hz 的 Host 玩家真实 Spawn、Warp、再次 Spawn、timeScale=0；初始化失败后重试、重复初始化、拒绝重复 Runtime 及会话关闭/重启。
- `GameplayNetworkPeerTests`（PlayMode）：同进程两个真实 NGO/UTP Peer 的连接门槛与启动锚点、输入上行、Owner/Observer Full/Delta 下行、端点重新注册清理 Baseline、Owner 路由校验和独立停表。此项验证真实传输及协议端点，不替代跨进程完整玩家预测测试。
- `PlayerSyncNetworkTestHarness`：跨进程 Baseline / Typical / Adverse；NGO 使用 60 Hz，Gameplay 使用 30 Hz。

Unity 菜单 `Tools/ProjectGame/Run Gameplay Clock EditMode Tests` 仅运行 Clock、Driver 和 Player Pipeline，结果写入 `Temp/PlayerSyncNetworkTest/tick-editmode-results.xml`。`Run Gameplay Clock PlayMode Tests` 运行上述两个 PlayMode Fixture，结果写入同目录的 `tick-playmode-results.xml`。PlayMode 测试应在没有活动网络会话的编辑模式中启动。

原有 `Run Player Sync EditMode Tests` 仍运行整个 Gameplay EditMode 程序集，结果保留在 `editmode-results.xml`，不会被专用回归覆盖。跨进程 Baseline / Typical / Adverse 需单独运行，不能由这些单进程测试推断弱网表现。
