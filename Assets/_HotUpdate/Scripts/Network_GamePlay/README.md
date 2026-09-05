# Gameplay 通用网络框架

本程序集是 Player、Weapon、Projectile、AreaEffect 等 Gameplay 网络系统共同依赖的底层，程序集名为 `HotFix.Gameplay.Network`。它只依赖 NGO 与 Collections，不反向依赖 `HotFix.Gameplay`，因此不会把任何具体玩法带入基础层。

## 当前结构

```text
GameNetworkRuntime (Unity 场景服务，HotFix.Gameplay)
  -> GameplayNetworkBootstrap (唯一 NGO Tick 订阅者)
    -> GameplayNetworkRuntime
      -> NetworkSimulationClock
      -> NetworkMessageTransport
      -> NetworkTransportStats
```

`NetworkSimulationConfig` 当前只保存会话级 `TickRate`，并在启动时强制校验它与 NGO `TickRate` 一致。Delivery 由业务层选择 `NetworkDeliveryClass` 语义，再由 Transport 集中映射成 NGO `NetworkDelivery`。

## PlayerSync 接入边界

PlayerSync 已接入通用 Clock 与 Transport，但以下玩家专属能力仍留在 Player 模块：

- 输入冗余、迟到输入重定时与 Hold；
- Prediction、Reconciliation、Rollback/Replay；
- Full/Delta Snapshot 与 Baseline；
- Remote Interpolation。

全局 `NetworkSimulationClock` 只表示会话 Tick，不能被单个玩家重置。`PlayerSimulationClock` 现在是玩家专用模拟游标：Server 每 Tick 对齐全局会话时间，Owner 在 Hard Resync 后可以独立回到权威 Tick，不会影响 Weapon 或其他玩家。

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

Weapon 第一版应直接建立自己的 `FireCommand`、`WeaponSnapshot` 与 `WeaponReplication`，暂时不要把 Player 的 Sequence/Baseline/Delta 实现抽成泛型公共库；等两个真实实现证明存在重复后再上提。
