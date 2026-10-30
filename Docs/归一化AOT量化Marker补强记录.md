# 归一化 AOT 量化 Marker 补强记录

## 1. 同步基线

本次修改前已执行远端同步：

- 分支：`LogicCore`
- 远端：`origin/LogicCore`
- 结果：远端已是最新，本地无待合并提交
- 本地未提交内容：之前新增的宏观重构文档，不参与本次代码修改

## 2. 原有 EntityTick Marker

`MonsterSimulation.Tick` 原本已经存在：

```text
Monster.Simulation.EntityTick
```

它覆盖整个活跃怪物循环，但不区分循环内部的主要阶段，因此只能证明 EntityTick 总成本，不能判断耗时来自目标读取、攻击状态机、导航速度、分离还是墙体约束。

本次保留总 Marker，并增加以下子 Marker：

```text
Monster.Simulation.EntityTick.Target
Monster.Simulation.EntityTick.Attack
Monster.Simulation.EntityTick.Navigation
Monster.Simulation.EntityTick.Separation
Monster.Simulation.EntityTick.Constraint
```

这些 Marker 按每个活跃 Slot 的主要阶段包裹，目的只是一次性拆分 EntityTick，不继续细化到单个数学表达式，避免 5012 怪物下 Marker 本身对热路径造成明显干扰。

## 3. 复制与网络发送 Marker

已有的编码 Marker：

```text
Monster.Presentation.Serialize
Monster.Presentation.Decode
```

本次增加发布、应用和网络发送链路：

```text
Monster.Replication.Publish
Monster.Replication.Capture
Monster.Replication.ApplyLocal
Monster.Replication.SendPresentation
Monster.Replication.ReceiveApply
Monster.Network.SendPresentation
Monster.Network.SendPresentation.Transport
Monster.Network.ReceivePresentation
```

含义如下：

| Marker | 统计范围 |
|---|---|
| `Monster.Replication.Publish` | Host 发布一次表现帧的完整流程 |
| `Monster.Replication.Capture` | 从 `MonsterWorld` 生成表现帧 |
| `Monster.Replication.ApplyLocal` | Host 本地应用已发布帧 |
| `Monster.Replication.SendPresentation` | Host 创建 Writer、编码并发送表现帧 |
| `Monster.Replication.ReceiveApply` | 本地 Replica 应用表现帧并通知表现层 |
| `Monster.Network.SendPresentation` | 网络发送入口的完整成本 |
| `Monster.Network.SendPresentation.Transport` | 底层 Transport 发送成本 |
| `Monster.Network.ReceivePresentation` | Client 接收、解码、交换帧并交给 Replica 的完整成本 |

`Monster.Presentation.Serialize` 会嵌套在 `Monster.Network.SendPresentation` 中，后续需要通过嵌套 Marker 区分编码成本和 Transport 成本，不能将各 Marker 直接相加当作总帧耗时。

## 4. Tick 阶段 Marker 纳入采样

`NetworkSimulationClock` 原本已经为各阶段创建 Marker，但 Benchmark Recorder 没有采集。本次将以下阶段加入采样清单：

```text
Gameplay.TickClock.WorldSimulation
Gameplay.TickClock.PhysicsSync
Gameplay.TickClock.MeleeCombat
Gameplay.TickClock.CombatSimulation
Gameplay.TickClock.Replication
```

它们用于解释轻量业务 Marker 未覆盖的 Host Main Thread 成本，尤其需要关注：

- `WorldSimulation` 是否与 `Monster.Simulation.EntityTick` 相近；
- `PhysicsSync` 是否仍有不必要的 Transform 同步；
- `CombatSimulation` 是否包含子弹系统未标记的状态更新或事件处理；
- `Replication` 是否明显高于 `Capture + Serialize + SendPresentation` 子 Marker 之和。

## 5. 下一轮量化重点

下一轮仍使用归一化 AOT 后的相同压测条件，重点观察：

1. `EntityTick.Target/Attack/Navigation/Separation/Constraint` 的累计耗时和每帧 P95；
2. `Replication.Capture/ApplyLocal/SendPresentation` 的比例；
3. `Network.SendPresentation.Transport` 与 `Presentation.Serialize` 的差值；
4. `Gameplay.TickClock` 阶段总耗时和业务 Marker 是否存在未覆盖成本；
5. Host 5012 的 P95/P99 长帧是否与 Replication 或某个 Tick 阶段同步出现。

## 6. 解释限制

本次 Marker 只能定位同步 CPU 路径，不能单独证明根因：

- `EntityTick` 子 Marker 位于循环内部，采样时需要同时观察总 Marker；
- `Replication` 和 `Network` Marker 存在嵌套关系，不能相加；
- 当前没有完整 Profiler 调用树，不能用轻量 Marker 代替完整 Profiler；
- 如果长帧只出现在少量帧，需要结合逐帧 CSV 的长帧行和 Marker 峰值判断。

## 7. 路径 3 修复记录

基于 `20261029-151522-d3b8f494` 的 Target/Constraint 热点分析，已继续落地：

- Target 的 Cell 级 Flow/恢复缓存与批量 AOT 解析；
- Constraint 的房间矩阵缓存和纯数据 AOT 内核；
- `Target.Cache`、`Target.Recovery`、`Constraint.Kernel` 三个补充 Marker。

具体改动、修复前量化、口径调整和待补的修复后数据见：

《Target 与 Constraint 路径 3 优化修复报告》

该报告明确保留了修复后量化空栏；在 Unity 生成物、运行时 parity 和 5012 同口径压测完成前，不把预期收益写成实测收益。

