# 导航连接掩码 AOT 迁移量化

## 迁移目标

本次只将运行时导航网格的八方向连接掩码构建循环 `BuildConnections` 从 HotFix 迁入 `AOT_Core`。

保持不变的部分：

- `BuildWalkable` 可行走性缓存构建；
- 静态与动态障碍数据维护；
- Density Cost 与 Additional Cost 刷新；
- FlowField Source 准备、Integration、Direction 求解；
- 拓扑版本判定和刷新时机；
- 斜向移动禁止穿墙角的规则；
- 原有 Profiler Marker 名称和包裹范围。

## 修改边界

迁移前，`RuntimeNavigationGrid.RefreshTopologyCache()` 在 HotFix 内直接遍历全部网格，并为每个可行走单元计算八方向连接位掩码。

迁移后，原位置直接调用：

```csharp
NavigationTopologyKernel.BuildConnections(
    width,
    height,
    _walkableCache,
    _connectionMask);
```

实际循环位于 `AOT_Core/Navigation/NavigationTopologyKernel.cs`。输入、输出数组均由原调用方持有并复用，没有新增逐次分配、拷贝或中间集合。

## 迁移前基线

采样文件：`ProfilerCaptures/2026-10-20-1024-topology-markers.data`

采样条件：1024 怪、300 帧、Player 普通 Profiler、相同导航 Marker。

| 指标 | 迁移前 |
| --- | ---: |
| `Monster.NavigationGrid.Topology.Refresh` | 5 次，平均 5.833 ms，最大 7.327 ms |
| `Monster.NavigationGrid.Topology.BuildConnections` | 5 次，平均 5.616 ms，最大 7.128 ms |
| `Monster.NavigationGrid.Topology.BuildWalkable` | 5 次，平均 0.216 ms |
| `BuildConnections` 占 Topology Refresh | 96.3% |
| GC Alloc | 该循环 0 B |

`BuildConnections` 的 5.616 ms/event 是这次迁移可影响的理论 CPU 预算，不代表最终一定能全部回收。它是后续 A/B 测量的比较基准。

## 调用时机说明

基线的 5 次拓扑刷新中：

- 3 次由 Density Update 首次访问导航 Kernel 时触发；
- 2 次由 FlowField Update 的拓扑重建路径触发。

因此对比应以稳定的 `Monster.NavigationGrid.Topology.BuildConnections` 与 `Monster.NavigationGrid.Topology.Refresh` 为主，不能只看某一层 `FlowField.Build` 的平均值。

## 一致性与量化口径

重新采样时应保持：

- 1024 个存活怪物；
- 300 帧；
- 相同地图、障碍配置和压测流程；
- Player 普通 Profiler；
- 对比 Marker 的总调用数、平均耗时、最大耗时和 GC Alloc；
- 同时记录发生拓扑更新帧的 Active CPU，避免 `WaitForTargetFPS` 干扰。

本次构建完成后，在下方补充包版本和新采样实测结果。

## 本次构建

- 内容修订号：12
- Player 路径：`Builds/StandaloneWindows64/0.1.0/_plus.exe`
- 构建方式：Development + Connect With Profiler，复用现有缓存的最小增量构建
- Player 构建结果：Succeeded，0 error，9 warning
- Player 增量构建耗时：87.76 秒
- Player 构建输出大小：1,473,811,026 bytes
- Profiler：`profiler-enable=1`，启动后可由 Unity Profiler 连接
- AOT 产物核验：IL2CPP 输出中已生成 `NavigationTopologyKernel_BuildConnections`，归属 `AOT_Core`

## 迁移一致性检查

在 Unity 编译完成后，使用迁移前的原始循环作为参照，对以下四种 37 × 29 网格逐格比较连接掩码：

- 全部可行走；
- 规则分布障碍；
- 固定种子的随机障碍；
- 全部阻挡。

共比较 4,292 个单元，差异数为 0。Unity 脚本编译错误为 0。

## 迁移后实测

### 捕获信息

- 原始文件：`ProfilerCaptures/2026-10-21-1024-connections-aot.data`
- 捕获帧：3949–4248，共 300 帧
- 文件大小：44,544,332 B
- SHA-256：`C3F88BDABFA18306FACDA824AADEEF8E1AF4B6BE697674654C44EC80C078FD9A`
- `MonsterViewHandle.Update`：307,200 次，即 1024 怪 × 300 帧
- Topology 刷新：5 次，与迁移前基线一致

### AOT 迁移直接收益

| 指标 | 迁移前 | 迁移后 | 变化 |
| --- | ---: | ---: | ---: |
| `BuildConnections` 单次平均 | 5.616 ms | 0.1117 ms | -5.5043 ms，降低 98.0%，约 50.3 倍 |
| `BuildConnections` 最大 | 7.128 ms | 0.1187 ms | -7.0093 ms，降低 98.3% |
| `Topology.Refresh` 单次平均 | 5.833 ms | 0.3131 ms | -5.5199 ms，降低 94.6% |
| `Topology.Refresh` 最大 | 7.327 ms | 0.3304 ms | -6.9966 ms，降低 95.5% |
| `BuildWalkable` 单次平均 | 0.216 ms | 0.2009 ms | -0.0151 ms，基本不变 |
| `BuildConnections` GC Alloc | 0 B | 0 B | 无新增分配 |

迁移前 ConnectionMask 全量重建占 `Topology.Refresh` 的 96.3%；迁移后只占约 35.7%。剩余时间主要是仍在 HotFix 中的 `BuildWalkable`，符合本次只迁移连接掩码循环的限定范围。

### 整帧收益

| 指标 | 迁移前 | 迁移后 | 变化 |
| --- | ---: | ---: | ---: |
| 整体 Active CPU 平均 | 5.417 ms | 4.477 ms | -17.3% |
| Active CPU P95 | 10.943 ms | 9.946 ms | -9.1% |
| Active CPU P99 | 17.960 ms | 11.709 ms | -34.8% |
| Active CPU 最大 | 22.228 ms | 13.452 ms | -39.5% |
| Topology 帧 Active CPU 平均 | 17.647 ms | 10.537 ms | -7.111 ms，降低 40.3% |
| Topology 帧 Active CPU 最大 | 22.228 ms | 11.780 ms | -10.448 ms，降低 47.0% |
| 普通逻辑 Tick Active CPU 平均 | 9.530 ms | 8.850 ms | -7.1% |

当前捕获在 300 帧内出现 54 个逻辑 Tick，迁移前捕获为 63 个，因此整体每帧平均值会受到两次采样覆盖时长差异影响。`BuildConnections`、`Topology.Refresh` 与 Topology 帧数据都是相同事件口径，是本次迁移最可靠的直接量化结果。

迁移后五个 Topology 帧的 `BuildConnections` 分别为 0.1109、0.1081、0.1187、0.1111、0.1095 ms，波动很小。五个 Topology 帧的 Active CPU 为 9.701–11.780 ms，全部落在 80 FPS 的 12.5 ms CPU 预算以内；迁移前 Topology 帧平均 17.647 ms，已经超过 60 FPS 的 16.67 ms 预算。

### 迁移后的新高峰

本次 Active CPU 最大帧为 4081，耗时 13.452 ms，没有发生 Topology 重建。`GameplayNetworkTickDriver.Update` 在该帧显示 8.809 ms，但它是整个逻辑 Tick 的外层调度 Marker，并不代表纯网络逻辑消耗了 8.809 ms。其主要子项为：

| 子项 | 帧 4081 耗时 |
| --- | ---: |
| `Monster.Simulation.EntityTick` | 3.666 ms |
| `Monster.View.ApplyAuthority` | 2.108 ms |
| `Physics.SyncColliderTransform` | 0.935 ms |
| `Monster.Presentation.Capture` | 0.891 ms |
| `Monster.Presentation.Apply` | 0.622 ms |
| `Monster.Presentation.Serialize` | 0.214 ms |
| `Monster.Simulation.SeparationBuild` | 0.178 ms |
| TickDriver 自身时间 | 0.174 ms |

300 帧中 TickDriver 外层 Marker 平均为 0.954 ms/帧；只看 54 个实际逻辑 Tick 时平均为 5.285 ms/次，而 TickDriver 自身平均只有 0.113 ms/次、最大 0.202 ms。迁移完成后，下一阶段的 CPU 优化重点已转为 EntityTick、Authority 应用、Collider 同步和表现层批处理，而不是网络调度器本身。

### 表现层与动画旁证

| Marker | 迁移前 | 迁移后 |
| --- | ---: | ---: |
| `MonsterViewHandle.Update` | 1.026 ms/帧 | 0.900 ms/帧 |
| `Animators.Update` | 0.075 ms/帧 | 0.0646 ms/帧 |
| `UpdateAllSkinnedMeshes` | 0.030 ms/帧 | 0.0250 ms/帧 |
| `Voice.Capture` | 0.065 ms/帧 | 0.0512 ms/帧 |

这些项目不属于本次代码修改，下降只能视为采样波动或整体负载变化，不能记作 AOT 迁移的直接收益。

### GC 状态

- GC Alloc：平均 704.83 B/帧，P95 734 B，P99 758 B，最大 774 B；迁移前为 706.5 B/帧，基本不变。
- `UpdateFunction.Invoke`：195,546 B / 2,691 次，仍是主要来源。
- `UnityTransport.Update`：12,000 B / 300 次。
- `NetworkBehaviourUpdate`：2,120 B / 53 次。
- `GameplayNetworkTickDriver.Update`：1,440 B / 36 次，即每次 40 B。
- Voice：80 B / 2 次，未出现历史上的 200 KB 周期分配。
- 增量 GC：300 帧累计 0.314 ms，平均约 0.0011 ms/帧；帧 3962 有一次 0.1118 ms 的单点高峰，没有完整 GC 回收。

本次迁移没有引入 GC，也不会改善原有约 705 B/帧的网络更新分配；CPU 与 GC 的变化边界清晰。

## 历史两轮量化数据冻结

| 轮次 | 构建 | 原始捕获 | 怪物 / 帧数 | `BuildConnections` | Topology 帧 Active CPU |
| --- | --- | --- | ---: | ---: | ---: |
| A：HotFix 连接掩码 | 0.1.0 rev.11 | `ProfilerCaptures/2026-10-20-1024-topology-markers.data` | 1024 / 300 | 平均 5.616 ms，最大 7.128 ms | 平均 17.647 ms，最大 22.228 ms |
| B：AOT 连接掩码 | 0.1.0 rev.12 | `ProfilerCaptures/2026-10-21-1024-connections-aot.data` | 1024 / 300 | 平均 0.1117 ms，最大 0.1187 ms | 平均 10.537 ms，最大 11.780 ms |

两轮均通过 `MonsterViewHandle.Update = 307,200` 次确认是 1024 怪 × 300 帧。以上数据作为固定历史基线，后续追加采样不覆盖。

## TickDriver 细分量化准备

rev.12 捕获中最慢帧 4081 的 `GameplayNetworkTickDriver.Update` 为 8.809 ms，其中已经可见：

| 子项 | 耗时 |
| --- | ---: |
| `Monster.Simulation.EntityTick` | 3.666 ms |
| `Monster.View.ApplyAuthority` | 2.108 ms |
| `Physics.SyncColliderTransform` | 0.935 ms |
| `Monster.Presentation.Capture` | 0.891 ms |
| `Monster.Presentation.Apply` | 0.622 ms |
| `Monster.Presentation.Serialize` | 0.214 ms |
| `Monster.Simulation.SeparationBuild` | 0.178 ms |
| TickDriver 未被子 Marker 覆盖的自身时间 | 0.174 ms |

下一轮增加以下第一层 Marker，不进入逐怪粒度：

- `Gameplay.TickDriver.Schedule`：累计墙钟时间并计算本帧应派发的 Tick 数；
- `Gameplay.TickDriver.Dispatch`：实际派发零到多个 Gameplay Tick；
- `Gameplay.TickClock.TickAdvanced`：玩家同步和动态巡逻障碍订阅者；
- `Gameplay.TickClock.TickCompleted`：怪物、武器与表现同步订阅者。

> 历史口径说明：以上 Marker 与下方 rev.13 数据来自两段式 Tick 时钟。2026-10-25 起时钟改为固定阶段，新的采样应使用 `Gameplay.TickClock.PreSimulation`、`ActorSimulation`、`WorldSimulation`、`CombatSimulation`、`PostSimulation`、`Replication` 和 `Completed`。历史数值原样保留，不换算成新阶段数据。

下一轮采样需要记录四个 Marker 的调用数、平均、P95、最大、GC Alloc，并核对 `Dispatch` 是否出现单帧多 Tick。只有当 `TickAdvanced` 或 `TickCompleted` 仍有显著未解释时间时，才继续拆订阅者 Marker。

量化包准备状态：

- 内容修订号：13；
- HotFix DLL + Addressables 内容更新耗时：38.20 秒；
- Unity 编译错误：0；
- 已在构建后的 `HotFix.Gameplay.Network.dll.bytes` 中核验四个 Marker 名称；
- Player：继续使用 `Builds/StandaloneWindows64/0.1.0/_plus.exe`；
- 未重新构建 IL2CPP Player，因为本轮没有修改 AOT 或 Player 原生内容。

### rev.13 实测数据

- 原始文件：`ProfilerCaptures/2026-10-21-1024-tickdriver-markers.data`
- 捕获帧：3945–4244，共 300 帧
- 文件大小：44,598,620 B
- SHA-256：`067B02CE5204A26BCDCDDC4E4DA198C53660656C8B949C3706819D8D062FA27B`
- `MonsterViewHandle.Update`：307,200 次，即 1024 怪 × 300 帧
- Gameplay Tick：61 次；没有单帧补跑多个 Tick，最大为 1 Tick/帧

#### TickDriver 阶段拆分

| Marker | 调用数 | 平均耗时 | 最大耗时 | 说明 |
| --- | ---: | ---: | ---: | --- |
| `Gameplay.TickDriver.Schedule` | 300 | 0.0020 ms/帧 | 0.0040 ms | 时间累计与 Tick 数计算 |
| `Gameplay.TickDriver.Dispatch` | 300 | 5.1754 ms/有效 Tick | 8.6002 ms | Tick 订阅者的外层包络 |
| `Gameplay.TickClock.TickAdvanced` | 61 | 0.0685 ms/Tick | 0.1387 ms | 玩家同步与动态巡逻障碍 |
| `Gameplay.TickClock.TickCompleted` | 61 | 5.1053 ms/Tick | 8.5218 ms | 怪物、武器与表现同步 |
| `Dispatch` 自身时间 | 61 个有效 Tick | 0.0016 ms/Tick | 0.0025 ms | 不包含子 Marker |

`TickCompleted` 占有效 Tick 中 `Dispatch` 平均耗时的 98.64%。Schedule、Dispatch 自身和 TickAdvanced 都不是 5–8 ms 尖峰来源。

#### TickCompleted 完整归因

| 直接子项 | 调用数 | 平均每 Tick | 最大 | 占 TickCompleted |
| --- | ---: | ---: | ---: | ---: |
| `Monster.Simulation.EntityTick` | 61 | 2.4759 ms | 4.0231 ms | 48.50% |
| `Monster.View.ApplyAuthority` | 61 | 1.1659 ms | 2.0151 ms | 22.84% |
| `Physics.SyncColliderTransform` | 61 | 0.5463 ms | 0.9992 ms | 10.70% |
| `Monster.Presentation.Capture` | 30 | 0.2223 ms/Tick | 0.8798 ms | 4.35% |
| `Monster.Simulation.SeparationBuild` | 61 | 0.2190 ms | 0.3659 ms | 4.29% |
| `Monster.Presentation.Apply` | 30 | 0.1533 ms/Tick | 0.6113 ms | 3.00% |
| `Monster.Simulation.DensityUpdate` | 61 | 0.1164 ms | 0.9886 ms | 2.28% |
| `Monster.Simulation.FlowFieldUpdate` | 61 | 0.0840 ms | 1.7168 ms | 1.65% |
| `Monster.Presentation.Serialize` | 30 | 0.0611 ms/Tick | 0.2432 ms | 1.20% |
| `Monster.Navigation.Obstacles.Tick` | 61 | 0.0135 ms | 0.0439 ms | 0.27% |
| `TickCompleted` 自身未覆盖时间 | 61 | 0.0463 ms | 0.0712 ms | 0.91% |

前三项 EntityTick、ApplyAuthority、Collider 同步合计占 82.0%；再加表现帧 Capture、Apply、Serialize 后约占 90.6%。`GameplayNetworkTickDriver.Update` 的高数值来自它调度的玩法系统，不是 TickDriver 或网络调度本身的 CPU 开销。

#### 高峰解释

本轮 TickDriver 最大帧 4067：

- Active CPU：14.102 ms；
- TickDriver：8.607 ms；
- TickCompleted：8.522 ms；
- EntityTick：3.880 ms；
- ApplyAuthority：1.823 ms；
- FlowFieldUpdate：1.717 ms；
- Collider 同步：0.680 ms；
- Topology Refresh：0.528 ms，其中 AOT `BuildConnections` 为 0.190 ms。

该帧是多个合法工作阶段叠加形成的内容尖峰，TickDriver 自身不是瓶颈。

整段捕获的 Active CPU 最大值为帧 3969 的 17.965 ms，但该帧 TickDriver 只有 4.095 ms，同时出现 `Application.InvokeFocusChanged`、`FindObjectsOfType`、额外 916 B GC 和较大的无 Marker PlayerLoop 时间空洞，符合切换或点击 Player 窗口引起的焦点事件，不作为稳定玩法逻辑高峰。

#### rev.13 GC

- GC Alloc：平均 708.61 B/帧，P95/P99 774 B，常态与 rev.12 的 704.83 B/帧基本一致；
- 最大 1,690 B 出现在上述焦点切换帧；
- `UpdateFunction.Invoke`：195,546 B / 2,691 次；
- `UnityTransport.Update`：12,000 B / 300 次；
- `NetworkBehaviourUpdate`：2,440 B / 61 次；
- `TickAdvanced`：1,600 B / 40 次，即原先归在 TickDriver 下的 40 B/次订阅者分配；
- Voice：80 B / 2 次；
- 增量 GC：累计 0.215 ms，平均 0.0007 ms/帧，最大 0.0018 ms；没有完整 GC 回收。

新增 Profiler Marker 自身没有产生 GC，原有高频约 700 B/帧分配模式没有变化。

## 巡回障碍 A/B 测试

### A 组：巡回障碍启用

- 内容修订号：13；
- 原始捕获：`ProfilerCaptures/2026-10-21-1024-tickdriver-markers.data`；
- `Patrol_North`、`Patrol_East` 启用并跟踪移动；
- 两个动态障碍在 61 个 Gameplay Tick 中执行 122 次范围计算；
- 实际占用格变化 4 帧，触发 4 次 Topology Refresh；
- `Obstacle.Tick`：平均 0.0135 ms/Tick，最大 0.0439 ms；
- `Topology.Refresh`：变化帧平均 0.3832 ms，最大 0.5276 ms；
- `FlowField.Build`：变化帧平均 1.1859 ms，最大 1.6953 ms；
- `Physics.SyncColliderTransform`：有效 Tick 平均 0.5463 ms，最大 0.9992 ms；
- Active CPU：平均 5.0313 ms，P95 10.1250 ms，P99 11.8623 ms。

### B 组：巡回障碍禁用

- 内容修订号：14；
- `Room_Grid_Start/NavigationDemo/PatrolWalls` 的 `activeSelf=false`；
- `Patrol_North`、`Patrol_East` 均为 `activeInHierarchy=false`；
- 静态房间内容、Player、HotFix DLL和Profiler Marker保持不变；
- 只执行 Addressables 内容更新，耗时 20.04 秒；
- Catalog Hash：`78c9fdebbb37c0e9b259e9ff3613bf88`；
- 原始捕获：`ProfilerCaptures/2026-10-21-1024-patrol-disabled.data`；
- 原始帧范围：7985–8284，共 300 帧；文件大小 45,026,312 B；
- SHA-256：`CC6B4347A8F3E6F5735331FCAFFD4BBA790FA5F6D3370BA12743E6C4334EE0D8`；
- `MonsterViewHandle.Update`：307,200 次，即 1024 怪 × 300 帧；
- Gameplay Tick：60 次，没有单帧补跑多个 Tick；
- `CalculateCells`、`ApplyBlockedCells`、`Topology.Refresh`、`BuildConnections`：均为 0 次；
- `Obstacle.Tick`：平均 0.0015 ms/Tick，最大 0.0029 ms；
- `FlowField.Build`：2 次，平均 0.7013 ms，最大 0.7058 ms；这两次不是巡回障碍拓扑变化触发；
- `Physics.SyncColliderTransform`：有效 Tick 平均 0.5901 ms，最大 1.1597 ms；
- Active CPU：平均 5.3097 ms，P95 10.5168 ms，P99 12.1308 ms，最大 13.8708 ms；
- 主线程完整帧：平均 6.6794 ms，P95 10.7238 ms，P99 12.1977 ms，最大 14.1859 ms；
- `WaitForTargetFPS`：平均 1.3697 ms，P95 4.6814 ms，最大 7.3115 ms；
- GC Alloc：平均 708.19 B/帧，P95/P99 774 B，最大 814 B；增量 GC 平均 0.0008 ms，最大 0.0028 ms，没有完整 GC 回收。

对比时重点检查 `CalculateCells`、`ApplyBlockedCells`、Topology/FlowField 重建次数、`Physics.SyncColliderTransform`、Tick Active CPU以及整体帧分布。若 CPU 差值明显小于体感帧率差值，则需要将差值转向 GPU 绘制、阴影或可见怪物分布分析。

### 巡回障碍 A/B 结果

| 指标 | A：巡回障碍启用 | B：巡回障碍禁用 | B 相对 A |
| --- | ---: | ---: | ---: |
| 主线程完整帧平均 | 6.8114 ms | 6.6794 ms | -0.1320 ms（-1.9%） |
| HUD 等价平均帧率 | 146.8 FPS | 149.7 FPS | +2.9 FPS |
| Active CPU 平均 | 5.0313 ms | 5.3097 ms | +0.2784 ms（采样反而更慢） |
| Active CPU P95 | 10.1250 ms | 10.5168 ms | +0.3918 ms |
| Tick 帧 Active CPU | 9.5952 ms | 9.8469 ms | +0.2517 ms |
| 非 Tick 帧 Active CPU | 3.8665 ms | 4.1754 ms | +0.3089 ms |
| `CalculateCells` | 122 次 | 0 次 | 完全消失 |
| `ApplyBlockedCells` | 8 次 | 0 次 | 完全消失 |
| `Topology.Refresh` | 4 次，0.3832 ms/次 | 0 次 | 完全消失 |
| `FlowField.Build` | 4 次，1.1859 ms/次 | 2 次，0.7013 ms/次 | 减少 2 次 |
| `Obstacle.Tick` | 0.0135 ms/Tick | 0.0015 ms/Tick | -88.9% |

禁用巡回障碍后，动态范围计算、占格应用和拓扑刷新都按预期退出，但整体平均帧时间只改善 0.132 ms，且 Active CPU 在本轮采样中反而慢 0.278 ms。说明巡回障碍主要制造少量 Topology / FlowField 事件尖峰，不是持续下降 30–40 FPS 的来源；其平均成本低于正常采样波动和本轮其他系统的整体变慢幅度。

### HUD 无法稳定 200 FPS 的同口径复核

HUD 并不读取 Profiler 的 Active CPU，而是每 0.25 秒累计 `Time.unscaledDeltaTime`，再以“帧数 / 总墙钟时间”显示 FPS。因此 CPU 逻辑、渲染提交、Present 等待和周期 Tick 都会进入 HUD 数值。200 FPS 要求完整帧平均不超过 5.000 ms。

| 轮次 | 主线程完整帧平均 | HUD 等价平均帧率 | Active CPU 平均 | Tick / 300 帧 |
| --- | ---: | ---: | ---: | ---: |
| rev.11：连接掩码仍在 HotFix | 6.9299 ms | 144.3 FPS | 5.3990 ms | 63 |
| rev.12：连接掩码迁入 AOT | 5.9124 ms | 169.1 FPS | 4.4774 ms | 54 |
| rev.13：新增 Tick Marker、巡回障碍启用 | 6.8114 ms | 146.8 FPS | 5.0313 ms | 61 |
| rev.14：巡回障碍禁用 | 6.6794 ms | 149.7 FPS | 5.3097 ms | 60 |

rev.14 的 60 个 Tick 帧平均为 9.9691 ms，240 个非 Tick 帧平均为 5.8569 ms；即使完全没有 Gameplay Tick，当前完整帧也已经超过 5 ms。每 5 帧左右再叠加一个约 10 ms 的固定 Tick，0.25 秒窗口自然稳定在约 150 FPS，而不是 200 FPS。

这也排除了“连接掩码 AOT 迁移导致持续回退”：rev.14 整段没有发生任何 `Topology.Refresh` 或 `BuildConnections`，迁入 AOT 的循环一次都没有执行。历史 Profiler 数据同样显示 rev.12 相比 rev.11 的完整帧平均从 6.9299 ms 降到 5.9124 ms，而不是变慢。

rev.12 到 rev.14 没有出现一个新的单点大 Marker，而是多个未改逻辑同时变慢：`BehaviourUpdate` 2.3722 → 2.7353 ms/帧，URP `DoRenderLoop_Internal` 1.1455 → 1.3836 ms/帧，1024 个 `MonsterViewHandle.Update` 0.9003 → 1.0255 ms/帧，TickDriver 外层 0.9543 → 1.0676 ms/帧，Transport 0.1077 → 0.1414 ms/帧，Voice 0.0703 → 0.1003 ms/帧。静态场景内容可能解释其中约 0.238 ms/帧的主线程渲染提交增量，但不能解释全部差值；其余更符合采样时系统负载、CPU频率、Profiler传输和场景状态共同造成的宽泛波动，而不是某个 AOT Kernel 回归。

## 结论

本次 AOT 迁移达到预期并明显超过“只能回收一部分解释器开销”的保守估计：连接掩码重建从 5.616 ms 降到 0.1117 ms，单次稳定回收约 5.50 ms CPU，Topology 帧重新进入 80 FPS CPU 预算。rev.13 Marker 进一步确认 TickDriver 的调度与时钟计算合计远低于 0.1 ms，5–8 ms 包络几乎全部来自 TickCompleted 下游玩法系统。后续不应继续围绕 TickDriver 或连接掩码循环做高优先级优化，性能重点应转向大规模怪物逻辑 Tick 的 Entity、Authority、Collider 与表现层同步。
