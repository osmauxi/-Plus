# Target 与 Constraint 路径 3 优化修复报告

> 状态：代码修复、AOT/HotFix 生成物重建和 Player 复测已完成；当前批次可作为功能正确性与轻量性能基线，但跨批次严格回归仍受 Catalog 指纹差异限制。
> 
> 本文记录最新归一化 AOT 量化中的问题判断、路径 3 的实现、Marker 口径以及修复后的实测结果。当前结果来自 `20261101-071326-a1a508c6`，所有结果均标注采样口径和可比性边界。

## 1. 本次分析使用的量化结果

来源：`C:\Users\SK\Downloads\SUMMARY(1).md`

- RunId：`20261029-151522-d3b8f494`
- 对照提交：`b58ee905c207`
- Unity：`2022.3.44f1c1`
- 采样：完整 Profiler，1024/2048/5012，Host/Client，每组 3 次有效样本
- 数据限制：工作区有未提交修改；与报告中的旧基线存在 Catalog 指纹和采样模式不兼容，不能把两轮绝对值直接当成严格 A/B 结论

5012 Host 的关键结果：

| 指标 | 报告汇总值 | 结论 |
|---|---:|---|
| Main Thread 平均 / P95 (ms) | 9.40～9.75 / 23.79～24.08 | 长帧主要在 CPU 主线程 |
| `WorldSimulation` 平均每帧 (ms) | 3.519 | 是主要 Tick 阶段 |
| `EntityTick` 平均每帧 (ms) | 3.269 | 占 WorldSimulation 大部分 |
| `EntityTick.Target` 平均每帧 (ms) | 0.982 | 目标读取/恢复是第一大子阶段 |
| `EntityTick.Constraint` 平均每帧 (ms) | 0.867 | 墙体约束是第二大子阶段 |
| `Replication` 平均每帧 (ms) | 0.813 | 不是当前 EntityTick 主因 |

Target 与 Constraint 合计约 `1.849 ms/frame`，约占 EntityTick 的 `56.6%`。这里的百分比只用于本轮内部热点排序，不用于跨 Catalog 基线的绝对收益承诺。

四叉树同期数据仍显示：5012 Host Build 约 `0.454 ms/次`，Query 约 `0.0008 ms/次`，平均候选数约 `0.3～0.7`，因此本轮不把四叉树重建列为 EntityTick 长帧的主修复对象。

## 2. 根因判断

### 2.1 Target 的旧路径

旧实现对每个活跃怪物执行一次 `TryGetFlowTarget`，其中重复发生：

1. 从 Cell Index 重新拆分坐标；
2. 读取当前 Cell 的 Flow 方向和 Source；
3. 读取玩家位置和玩家 Walkable 状态；
4. 对非法 Cell 再执行一次附近可达 Flow Cell 搜索；
5. 把局部 Flow 方向转换到世界坐标。

第 5 项对每只怪物重复，而 Flow 数据实际是按 Cell 共享的。非法 Cell 的恢复搜索也可能被同一 Cell 内的多只怪物重复触发。

### 2.2 Constraint 的旧路径

旧实现对每个活跃怪物使用 `Transform.InverseTransformVector`、`Transform.TransformVector` 和 `RoomSpatialUtility.WorldToCell`，并按完整速度、X 轴滑动、Z 轴滑动、纯 Flow 回退的顺序重复进行坐标变换与 Cell 判断。

这部分算法本身不复杂，主要问题是：

- 每个候选都跨 HotFix/Unity Transform 边界；
- 同一个房间的矩阵和网格尺寸重复读取；
- 纯数据计算没有独立的 AOT 内核，后续无法直接做 Burst/Job 对照。

## 3. 已实施的路径 3

### 3.1 Target：按 Cell 缓存，按 Slot 批量解析

新增：

```text
Assets/_AOT/Scripts/Core/Navigation/FlowTargetKernel.cs
```

核心数据：

- `FlowTargetCellData[]`：每个导航 Cell 缓存 Valid/Recovery 标志、SourceIndex、世界 Flow 方向和恢复位置；
- `MonsterTargetResult[]`：按 Monster Slot 写入本 Tick 的目标结果；
- `Vector2[]`：按 Slot 缓存怪物位置，供纯 AOT 批处理读取。

运行逻辑变为：

```text
FlowField 或拓扑版本变化
  -> RefreshFlowCellCache：每个 Cell 只做一次 Flow/方向/坐标转换

本 Tick
  -> PrepareRecoveryCache：每个非法 Cell 最多做一次附近可达搜索
  -> FlowTargetKernel.ResolveBatch：只按 ActiveSlots 计算距离、标志和结果
  -> EntityTick 直接读取 MonsterTargetResult
```

缓存只在 `_flowFieldDataVersion` 变化时重建。恢复缓存按 Cell 记录版本，避免同一 Tick 同一非法 Cell 被多次搜索。

### 3.2 Constraint：房间矩阵预计算 + 纯数组合法性检查

新增：

```text
Assets/_AOT/Scripts/Core/Navigation/MovementConstraintKernel.cs
```

`RoomGridTransformData` 在进入 Tick 时从房间根节点生成一次，缓存：

- World→Local 的 X/Z 矩阵项和地面高度偏移；
- Local→World 的 X/Z 矩阵项；
- 房间半尺寸和 CellSize 的倒数。

`MovementConstraintKernel.Constrain` 保留原候选顺序和判定语义：

```text
完整 desired velocity
  -> X 轴滑动
  -> Z 轴滑动
  -> 纯 navigation velocity 回退
  -> 零速度
```

内核只接收 `byte[] walkable`、宽高、位置和速度，不再在热路径中调用 Transform 或 `RoomSpatialUtility`。

### 3.3 玩家目标缓存的输入整理

`MonsterPlayerTargetBuffer` 新增按玩家的 `Positions` 和 `CellWalkable` 数组。玩家位置仍由现有采集适配器负责读取，但 Target 内核不再逐怪物穿过 `Items` 对象数组读取位置。

## 4. 本次新增 Marker

保留原有总 Marker 和主要阶段 Marker，并新增：

```text
Monster.Simulation.EntityTick.Target.Cache
Monster.Simulation.EntityTick.Target.Recovery
Monster.Simulation.EntityTick.Constraint.Kernel
```

解释：

- `Target.Cache`：Flow Cell 级缓存重建。正常情况下只在 FlowField 数据版本变化时出现，不应按每个 EntityTick 高频出现；
- `Target.Recovery`：非法 Cell 的恢复缓存和 Slot 位置准备。可用于判断恢复搜索是否成为新的尖峰；
- `Constraint.Kernel`：纯数据约束内核，嵌套在原有 `Constraint` Marker 中，用于区分坐标/Transform 成本是否被移除。

`Monster.Simulation.EntityTick` 总 Marker 已调整为覆盖：

```text
Target.Cache -> Target.Recovery -> Target batch -> 活跃怪物循环
```

这是为了保持总 Marker 对“一个完整 EntityTick 阶段”的解释一致。后续量化时不能把总 Marker 与子 Marker简单相加，因为存在嵌套和阶段重叠。

## 5. 预期优化方向（不是实测结论）

### Target

- 从“每个怪物读取并转换一次 Flow Cell”变为“每个 Cell 转换一次、每个 Slot 只做轻量结果解析”；
- 非法 Cell 恢复搜索由按怪物重复变为按 Cell 版本缓存；
- 目标数据访问从 `MonsterPlayerTarget[]` 对象数组改为紧凑的 `Vector2[] + byte[]`。

### Constraint

- 删除每个候选速度的 Transform API 调用；
- 删除每个候选速度的临时 `Vector3` 构造路径；
- 将坐标变换和合法性判断下沉到可独立 AOT 的纯数据函数，为后续 Burst/Job 提供直接输入。

本轮不预估 FPS 百分比。原因是 Target/Constraint 仍然位于 5012 Host 的 EntityTick 中，最终收益取决于：非法 Cell 比例、FlowField 重建频率、分离速度撞墙比例、Profiler Marker 开销以及脚本后端是否实际使用 AOT 生成物。

## 6. 修复后量化结果

本轮 RunId 为 `20261101-071326-a1a508c6`，Windows Development Player，1024×768、Performant、VSync 0，Seed 12345，三档怪物数量各 3 次重复，预热 5 秒、实际采样 30 秒，Host/Client 分别前台采样。18/18 个角色样本有效，5012 Host/Client 的 Slot、View 和工作负载校验均通过；本轮 Catalog 指纹为 `271e3c5aa7d3ab5904f93b77ac542dfd`。

最新批次的角色前台聚合值：

| 怪物数 | 角色 | 平均帧时 (ms) | P95 (ms) | P99 (ms) | Main Thread 平均 (ms) | GPU 平均 (ms) | GC Alloc 平均 (B/帧) |
|---:|---|---:|---:|---:|---:|---:|---:|
| 1024 | Host | 2.855 | 5.124 | 6.840 | 2.847 | 0.578 | 1,865 |
| 1024 | Client | 3.237 | 4.141 | 4.659 | 3.229 | 0.770 | 1,365 |
| 2048 | Host | 3.566 | 8.575 | 9.965 | 3.557 | 0.960 | 1,925 |
| 2048 | Client | 4.178 | 5.178 | 5.879 | 4.168 | 1.287 | 1,157 |
| 5012 | Host | 6.945 | 18.026 | 19.748 | 6.932 | 2.410 | 2,053 |
| 5012 | Client | 7.153 | 9.183 | 9.951 | 7.141 | 2.674 | 1,485 |

5012 Host 的路径 3 Marker（3 次重复的代表区间）：

| Marker | 平均单次 (ms) | 平均每帧 (ms) | P95 单帧 (ms) |
|---|---:|---:|---:|
| `EntityTick` | 5.865～6.104 | 1.169～1.381 | 5.895～6.238 |
| `EntityTick.Target` | 0.041～0.042 | 0.008～0.009 | 0.040～0.042 |
| `Target.Cache` | 0.775～0.807 | 0.012～0.015 | 仅在缓存重建帧出现 |
| `Target.Recovery` | 0.467～0.480 | 0.093～0.109 | 0.471～0.492 |
| `EntityTick.Constraint` | 约 0.77～0.81（含子阶段） | 约 0.15～0.18 | 约 0.77～0.81 |
| `Constraint.Kernel` | 约 0.47～0.49 | 约 0.09～0.11 | 约 0.47～0.49 |

`Target.Cache` 的调用次数约 70～72 次/30 秒，符合“按 Flow 数据版本重建”的设计；不能将 `EntityTick` 与子 Marker 直接相加，因为子 Marker 嵌套在总 Marker 内。

四叉树同期量化：`Monster.SpatialIndex.Build` 约 0.464～0.479 ms/次，`Monster.SpatialIndex.Query` 约 0.00077～0.00080 ms/次；平均候选数约 0.75、平均访问节点约 18、候选占活跃怪物约 0.015%。这足以作为当前实现的轻量绝对基线，但尚未与同 Catalog 的旧全 Slot 实现做严格 A/B。

与 `20261031-035525-43747a99` 的 5012 Host 对照如下。两轮采样模式相同，但 Catalog 指纹不同，因此只用于趋势观察：

| 指标 | 20261031 观察值 | 20261101 实测 | 变化（观察性） |
|---|---:|---:|---:|
| `EntityTick` 平均单次 | 5.586 ms | 约 5.964 ms | +6.8% |
| Host 平均帧时 | 6.808 ms | 6.945 ms | +2.0% |
| Host P95 / P99 | 17.632 / 19.719 ms | 18.026 / 19.748 ms | +2.2% / +0.1% |
| Host Main Thread 平均 | 约 6.80 ms | 6.932 ms | 仅作观察 |
| GC Alloc 平均 | 旧批次口径不同 | 2,053 B/帧 | 不作严格比较 |
| QuadTree Build / Query | 约 0.454 / 0.0008 ms | 约 0.464 / 0.0008 ms | 基本稳定 |

本轮已完成以下 parity 检查：

1. Target 的 HasTarget、TargetWalkable、TargetDistanceSqr、FlowDirection 与旧实现一致；
2. 非法 Cell 的恢复 Cell、恢复 Source 和脱困方向一致；
3. Constraint 四种候选顺序和越界结果一致；
4. EntityTick 活跃 Slot 数量、攻击状态和移动结果不出现变化；
5. QuadTree 候选集合和重建频率不因本轮修改变化。

## 7. 当前验证状态

- 已完成：代码实现、Marker 接入、总 Marker 口径修正、AOT/HotFix 生成物重建和 Windows Player 打包；
- 已完成：三档怪物数量、Host/Client 双端前台采样，18/18 有效，目标、约束、Slot/View 数量和工作负载校验通过；
- 已完成：`MonsterVATRenderProfileCatalog` 的 `Zombie_Standard` 键与运行时配置一致，最新批次没有再出现 Catalog 未注册或 Replica SlotCount 错误；
- 当前可确认：路径 3 已落到可量化的独立内核和缓存边界，`Target.Cache` 没有按怪物数量高频重复执行，`Constraint.Kernel` 保持在约 0.1 ms/帧量级；
- 当前不能确认：相对旧实现的严格加速百分比。原因是最新 Catalog 指纹与 `20261031` 及更早批次不一致，基线兼容性检查仍应标记为“不兼容：Catalog 指纹”；
- 已知非采样故障：5012 Host 结束进程时仍会出现 `TMP_SubMeshUI` 与 `NetworkPrefabRegistry.ReleaseAll` 的 `NullReferenceException`，该模式在前两次批次也存在，发生于 teardown，不计入性能样本，但应作为独立清理问题跟踪。

下一步若要形成严格回归基线，应固定构建提交、Catalog 指纹和采样配置后重跑至少 3 次；在此之前，本轮数据用于验证功能正确性、Marker 是否可解释以及当前四叉树/EntityTick 的绝对量级。
