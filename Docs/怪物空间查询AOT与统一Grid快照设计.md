# 怪物空间查询 AOT 与统一 Grid 快照设计

本设计的可执行拆分见：

- [怪物四叉树核心 AOT 迁移执行方案](怪物四叉树核心AOT迁移执行方案.md)；
- [怪物统一空间快照执行方案](怪物统一空间快照执行方案.md)。

## 1. 结论先行

当前 5012 怪物压测中的 `Monster.SpatialIndex.Build` 高耗时，不能直接归因于四叉树算法本身。它至少包含三类成本：

1. `HotFix.Gameplay` 在 Player 中由 HybridCLR 解释执行，紧密循环、数组访问、结构体字段访问和方法调用会被放大；
2. 四叉树重建本身需要清空链表数组、建立基础节点、遍历所有 Slot、插入怪物并在高密节点继续细分，这部分是算法和内存写入成本；
3. 怪物每个逻辑 Tick 都会让索引失效，第一次空间查询才触发延迟 Build；渲染帧变长时，TickDriver 还可能在同一渲染帧补跑多个 Tick，把多个 Build 叠加到一帧。

因此，在修改四叉树结构或直接改成 Grid 之前，先做一次 AOT 对照是合理的。它可以回答一个关键问题：当前 8.5 ms 左右的 Build 中，有多少是解释执行放大，有多少是四叉树的数据结构和全量重建必然成本。

但 AOT 对照不能替代算法分析。即使全部迁入 AOT，5012 个 Slot 的全量插入、数组清空和节点链表维护仍然存在。AOT 只会降低执行方式带来的额外成本，不会把 `O(怪物数)` 的重建变成 `O(1)`。

## 2. 为什么应该先做 AOT 对照

项目已有 Flow Field 的直接证据：同一重建次数和同一输入构成下，`FlowField.Build` 从 HotUpdate 的 9.829 ms 降到 AOT 的 0.832 ms，下降 91.5%；Build 帧的额外成本也从 9.711 ms 降到 1.230 ms。这个结果说明，当前项目中的纯数据紧密循环确实可能被 HybridCLR 解释执行明显放大。

四叉树 Build 具备相似特征，但不能直接套用 Flow Field 的收益比例。它还包含以下不可被 AOT 消除的工作：

- 每次 Build 清空 `_nextSlot`、`_previousSlot` 和 `_leafBySlot`；
- 固定创建 BaseDepth 的基础节点；
- 遍历 `MonsterWorld` 的全部 Slot 并读取 Active/Position；
- 将每个活跃 Slot 写入链表，并在超过 `SplitThreshold` 时继续细分；
- 对高密叶节点重复分配节点和重新插入节点内的 Slot。

所以应先得到这组数据，而不是先假设“8.5 ms 都是热更解释器造成的”：

| 对照项 | 需要固定的条件 | 需要观察的指标 |
|---|---|---|
| HotUpdate 当前实现 | 当前 commit、同一场景、Seed、路线、武器和 TickRate | Build/Query、Main Thread、TickDriver、EntityTick、GC |
| AOT 仅迁移空间索引内核 | 其余逻辑保持不变 | Build 单次耗时、Build 帧额外耗时、Query 单次耗时 |
| AOT 后的完整 5012 压测 | 与前一轮完全相同 | 平均/P95/P99 帧时、补跑 Tick 数、Build 频率 |

判断方式建议如下：

- Build 下降超过约 50%，说明执行模式是重要放大因素，应先完成 AOT 边界和数据布局，再评估 Grid；
- Build 只下降约 20% 或更少，说明主要成本在全量重建和内存写入，应优先做增量更新、快照复用或 Grid；
- Build 下降明显但 Host 仍保持个位数 FPS，说明 `EntityTick`、Tick 补跑和其他线性系统仍是主因，不能把整体收益全部归因于空间索引；
- Query 已经只有约 0.01 ms 量级时，AOT 不应成为替换 Query 算法的理由，重点应放在 Build 频率和 Build 数据量。

## 3. 当前数据对四叉树开销的解释

`20261026-162947-530d99d1` 的 5012 Host 数据显示：

- 单次 Build 约 8.56--8.75 ms；
- 单次 Query 约 0.010--0.014 ms；
- Build 频率约 15.65--24.23 Hz；
- Query 频率约 50.95--115.47 Hz；
- Build 摊销约 6.34--6.91 ms/逻辑 Tick。

这已经证明“频繁子弹 Query 导致每颗子弹重建”不是当前逻辑。索引在怪物移动后被标脏，当前 Tick 第一次查询触发一次 Build，同 Tick 后续查询复用结果。范围伤害和连锁查询接入房间空间服务后，也复用同一份已建立索引。

Build 仍然是明显成本。若一帧补跑四个逻辑 Tick，最坏情况下可以在一个渲染帧中观察到四次 Build；这解释了为什么 Build 的 P95 单帧可以达到约 35 ms。它会参与 CPU 追赶反馈，但不能单独解释 5012 Host 约 134--153 ms 的主线程帧。

### 3.1 5012 档重复遍历量化

统一快照的主要收益不是“少分配几份数组”，而是把同一 Tick 内的全量 Slot 扫描合并。以 `20261026-162947-530d99d1` 的 5012 Host 首次重复为例，30 秒内记录到约 791 次 Monster Simulation Tick、131 次 Density Capture 和 533 次 QuadTree Build；该档全部 5012 个 Slot 都处于活跃状态。

当前空间准备阶段的全量 Slot 遍历可以拆成：

```text
Separation.Capture       791 × 5012
Separation.BuildBuckets  791 × 5012
Density.Capture          131 × 5012
```

也就是 30 秒约 1713 个 `5012` 规模的遍历单元。统一 Occupancy Snapshot 后，Density 和 Separation 共用一次预移动快照：

```text
OccupancySnapshot.Build  791 × 5012
```

在遍历次数上可直接消除：

```text
1713 - 791 = 922 个 5012 规模遍历单元
约 462 万次 Slot 访问，约占这组空间准备遍历的 53.8%
```

这里的 53.8% 是“空间准备阶段”的遍历减少，不是整帧耗时下降。`Separation.CalculatePairs` 的成对距离计算仍然保留；`MonsterSimulation.EntityTick` 仍必须逐个更新怪物；四叉树仍独立构建自己的节点和 Slot 链。若把 EntityTick 和 533 次四叉树 Build 也放入同一组粗略的全量 Slot 遍历统计，约可从 1522 万次降到 1060 万次，约 30.4%，但这只是遍历量上限估计，不能直接换算成 FPS。

Density 的 `Capture` 在该档单次约 2.43 ms，读取共享 `CellCount` 后可以删除这段独立的 `MonsterWorld -> WorldToCell` 扫描；不过统一快照需要在 Separation 的已有采集过程中计算空间归属，实际收益仍需扣除 FineCell/CoarseBucket 归属计算的新增成本。四叉树单次 Build 约 8.56 ms，不会因 Density/Separation 合并自动消失，因此统一快照和四叉树 AOT 必须分开实测。

## 4. “一次建桶、多业务复用”到底是什么意思

这里的“一次建桶”指每个空间快照只做一次 Slot 到空间桶的归属计算，所有系统读取同一份快照。它不表示 Density、Separation、Projectile 和范围伤害分别创建自己的 Grid，也不表示为每一种半径预建一套完全独立的网格。

共享快照可以抽象成：

```text
MonsterWorld.Motion / ActiveMask
             |
             v
MonsterRoomRuntime.PrepareSpatialSnapshot(tick)
             |
             +-- Slot -> Cell 归属
             +-- Cell -> Slot 链表或连续范围
             +-- CellCount / 粗粒度聚合计数
             +-- 可选：邻接 Cell 缓存
             v
同一 Tick 的查询只读这份快照
  Projectile       -> QuerySweptCircle
  Range Effect     -> QueryCircle
  Lightning        -> QueryNearest / QueryCircle
  Density          -> CellCount
  Separation       -> NeighborCells
```

查询半径不同，通常只影响需要访问的 Cell 范围，不需要复制一份 Grid。比如半径 1.5 米访问中心 Cell 周边一圈，半径 8 米访问更大的 Cell 矩形或环形范围；拿到候选 Slot 后，各业务继续做自己的精确距离、方向、伤害和去重判断。

如果确实需要不同粒度，也建议在一次建桶过程中生成层级元数据，而不是让业务各自重建：

- 细粒度 Cell：服务弹丸、近距离 Separation 和精确邻域查询；
- 粗粒度 Cell：由多个细 Cell 聚合得到，服务 Density、区域激活和快速空区域判断；
- 邻接元数据：记录相邻 Cell 的连续范围或固定邻居列表，服务 Separation 和最近邻查询。

这仍然是一次 Slot 归属、一次基础数据准备，差别只是额外生成少量 `CellCount` 或聚合数组。只有当房间尺寸、对象类型或更新频率完全不同，才考虑独立索引；当前怪物房间不满足这个条件。

## 5. 建议的运行逻辑

### 5.1 Tick 内的空间快照时序

```text
PreSimulation
  -> 动态墙等地图状态更新

ActorSimulation
  -> 玩家位置和玩家动作提交

WorldSimulation
  -> MonsterSimulation 更新全部 MonsterWorld.Position
  -> 标记空间快照 Dirty

PhysicsSync
  -> 提交 Unity Physics Transform

MeleeCombat
  -> 结算怪物近战
  -> 处理可能影响怪物 Active/Position 的增量

SpatialSnapshot
  -> 对 Dirty 快照执行一次 Build/Rebuild
  -> 或应用本 Tick 的 Slot 迁移、死亡和新增增量

CombatSimulation
  -> Projectile、范围伤害、风暴云、连锁闪电只读同一快照
  -> Damage/Push 通过 RoomRuntime 更新 World 和索引增量

Replication
  -> 捕获最终状态和表现帧
```

当前代码还没有单独的 `SpatialSnapshot` 阶段，仍然是 `MonsterSpatialIndex.Invalidate()` 后由第一次查询懒触发 Build。正式统一快照时，建议在 `MeleeCombat` 之后、`CombatSimulation` 之前增加显式准备点，或者让 `CombatSimulation` 的第一个房间查询负责调用一次 `PrepareSpatialSnapshot`。显式阶段更容易量化 Build，也能避免不同系统第一次查询的时机改变 Build 所在帧。

### 5.2 同 Tick 内的死亡和击退

空间快照不能只保存位置，还需要处理 Active 和位置变化：

- `DamageMonster` 让怪物死亡：从 Cell 链表摘除，或写入 `RemovedSlots` 覆盖层；
- `PushMonster` 改变位置：从旧 Cell 摘除并插入新 Cell，或写入 `MovedSlots`；
- 新增 Slot：插入对应 Cell，不能等到下一 Tick 才让查询看见；
- 查询返回候选后仍需检查 Active，避免同 Tick 已死亡对象继续造成伤害。

当前四叉树已经通过 `Remove`、`Relocate` 和查询时的 `TryGetMonsterPosition` 保证了这些语义。未来 Grid 需要保留同样的边界，不能为了“一次建桶”而牺牲同 Tick 的命中正确性。

## 6. 大致实现路径

### 第一步：AOT 对照，不改变算法语义

先把四叉树的纯数据内核从房间对象依赖中拆出来。AOT 内核不直接引用 `MonsterRoomRuntime` 或 `MonsterWorld`，而是接收：

- `PositionX[]`、`PositionY[]`；
- `ActiveMask[]`；
- `SlotCount`；
- 房间 Bounds；
- Build 参数 `BaseDepth`、`MaxDepth`、`SplitThreshold`；
- 调用方提供的节点、链表和 Query 临时数组。

HotUpdate 层继续负责读取 `MonsterWorld`、维护房间生命周期和处理 Damage/Push；AOT 层只负责 Build、Relocate、Remove 和 Query。这样才能真正测量 AOT 内核，不会把 HotUpdate 的对象访问和边界转换混进结果。

### 第二步：用统一接口包住当前四叉树

房间服务提供稳定接口，例如：

```csharp
int QuerySweptCircle(Vector2 start, Vector2 end, float radius, int[] results);
int QueryCircle(Vector2 center, float radius, int[] results);
int QueryNearest(Vector2 center, float radius, int[] results);
int GetCellCount(int cellId);
int QueryNeighborCells(int cellId, int[] cells);
```

Weapon、Density 和 Separation 只依赖接口，不再访问四叉树字段、节点深度或内部链表。当前已完成的 `MonsterRuntimeService.QueryMonsterCircleCandidates` 是这个方向的第一步。

### 第三步：在相同接口后加入 Grid 快照实现

固定 Grid 的核心数据可以采用连续数组：

```text
cellHead[cell]
cellCount[cell]
nextSlot[slot]
cellOfSlot[slot]
positionX[slot]
positionY[slot]
active[slot]
```

Build 时遍历一次活跃 Slot，把 Slot 插入对应 Cell。Query 时根据扫掠圆的 AABB 或 Cell 范围访问候选 Cell，再由调用方做精确几何判断。若需要粗粒度统计，可以在同一遍 Build 中累加 `coarseCellCount`，不再重新遍历全部 Slot。

### 第四步：迁移 Density 与 Separation

范围伤害和连锁查询已经先迁入房间服务。下一步才迁移 Density 和 Separation：

- Density 使用 `cellCount` 或粗粒度聚合计数；
- Separation 使用当前 Cell 加邻接 Cell 的 Slot 链表；
- Projectile 和范围 Effect 使用同一份候选 Cell；
- 房间服务统一决定 Build、Dirty、增量迁移和清理时机。

迁移完成后，`MonsterSeparationSystem` 不再拥有自己的独立空间桶，Density 也不再单独扫描全量 Slot。各模块保留自己的计算规则，但不再各自维护空间事实。

### 第五步：AOT 与 Grid 分别复测

最终至少保留三组结果：当前四叉树 HotUpdate、四叉树 AOT、Grid AOT。三组都要使用相同的 5012 场景和压测路线，比较：

- Build/Rebuild 单次耗时和每 Tick 摊销；
- Query 单次耗时、候选数和访问 Cell/Node 数；
- EntityTick、Separation、Density 和 TickDriver；
- 主线程平均/P95/P99、补跑 Tick 数和丢弃时间；
- GC Alloc、数组扩容次数和网络表现帧频率。

只有在这三组数据齐全后，才能判断收益来自执行模式、数据结构，还是减少了重复空间工作。

## 7. 当前阶段边界

- 当前建议先做四叉树 AOT 对照，再决定是否把底层替换成 Grid；
- 当前范围伤害和连锁查询已经复用房间空间查询入口，但底层仍是四叉树；
- 当前不删除子弹的 Collider/GameObject；这属于表现生命周期改造，和空间查询内核可以分开验证；
- 当前不新增 marker，沿用已有 `Monster.SpatialIndex.Build`、`Monster.SpatialIndex.Query` 和系统级 Marker 完成 AOT 对照；
- Grid 的目标是共享一次空间事实，不是让每个系统各自创建一套不同粒度的 Grid。

