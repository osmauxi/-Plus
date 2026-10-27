# 怪物四叉树核心 AOT 迁移执行方案

## 1. 目标和迁移边界

本方案只处理 `MonsterSpatialIndex` 的纯四叉树数据内核，把它从 `HotFix.Gameplay` 的 HybridCLR 解释执行路径迁移到现有 `AOT_Core` 程序集，用来回答：5012 怪物压测中的 `Monster.SpatialIndex.Build` 有多少是解释执行放大，有多少是四叉树全量重建本身的成本。

本轮不改变：

- `MonsterRoomRuntime` 的空间索引所有权；
- `MonsterRuntimeService.QueryMonsterCandidates` 和 `QueryMonsterCircleCandidates` 的调用接口；
- Weapon 的宽相候选、窄相距离、穿透、连锁 visited 和伤害语义；
- `MonsterWorld` 的 Slot 分配、死亡、击退和位置更新规则；
- `BaseDepth=4`、`MaxDepth=6`、`SplitThreshold=16`；
- 四叉树的子节点顺序、Slot 链表顺序和越界 overflow 语义；
- 当前懒 Build 触发策略；
- Density、Separation、Flow Field 和表现层。

本轮也不把四叉树改成固定 Grid。Grid 是下一轮独立实验，必须使用 AOT 后的四叉树结果作为对照。

## 2. 当前实现和问题定位

当前实现文件：

```text
Assets/_HotUpdate/Scripts/GameProcess/Monster/Spatial/MonsterSpatialIndex.cs
Assets/_HotUpdate/Scripts/GameProcess/Monster/Runtime/MonsterRoomRuntime.cs
Assets/_HotUpdate/Scripts/GameProcess/Monster/Runtime/MonsterRuntimeService.cs
```

`MonsterSpatialIndex.EnsureBuilt()` 在第一次空间查询时执行：

```text
清空 _nextSlot / _previousSlot / _leafBySlot
创建根节点和 BaseDepth 基础树
遍历 MonsterWorld 全部 Slot
读取 IsActive 和 Position
插入活跃 Slot
按 SplitThreshold 继续细分并重新插入叶节点 Slot
```

当前四叉树内部数组：

| 数组 | 作用 | AOT 后是否保留 |
|---|---|---|
| `_nodes` | 节点边界、子节点、HeadSlot、Count、Depth、MortonCode | 保留，迁入 AOT 内核 |
| `_queryStack` | 非递归查询栈 | 保留，迁入 AOT 内核 |
| `_nextSlot` | 节点内 Slot 单向链 | 保留，迁入 AOT 内核 |
| `_previousSlot` | O(1) 移除所需的前驱链 | 保留，迁入 AOT 内核 |
| `_leafBySlot` | Slot 当前叶节点 | 保留，迁入 AOT 内核 |
| `_overflowHead` | Bounds 外 Slot 链 | 保留，迁入 AOT 内核 |

这些数组不是本轮要删除的重复数据。AOT 迁移只改变执行位置，不改变四叉树的数据结构，因此不会自动降低四叉树内存。

## 3. 迁移前必须冻结的基线

用户计划的量化顺序应固定为：

```text
A0 当前代码 + 四叉树 HotUpdate
   -> 新建 5012 Host 基线

A1 只迁移四叉树核心到 AOT
   -> 同场景、同 Seed、同路线重新压测

A2 对比 A0/A1，决定是否进入统一快照
```

旧记录 `20261026-162947-530d99d1` 只作为历史问题基线。它包含旧 Host 表现同步路径，不能直接作为 AOT 收益的唯一分母。

### 3.1 A0 需要保存的指标

至少保存以下 Marker 和运行指标：

| 指标 | 目的 |
|---|---|
| `Monster.SpatialIndex.Build` 平均/P95/最大 | 判断重建成本 |
| `Monster.SpatialIndex.Query` 平均/P95/最大 | 确认 AOT 不破坏查询 |
| Build 次数和频率 | 确认懒 Build 触发次数未改变 |
| Query 次数和频率 | 确认 Weapon 负载一致 |
| 候选数、访问节点数 | 确认裁剪质量不变 |
| `Monster.Simulation.EntityTick` | 判断四叉树收益是否被实体 Tick 淹没 |
| `Monster.Simulation.SeparationBuild` | 判断其它线性系统是否回归 |
| Host Main Thread、Active CPU、P95/P99 | 判断整帧收益 |
| TickDriver 每帧补跑 Tick 数 | 判断长帧反馈 |
| GC Alloc、数组扩容次数 | 确认 AOT 没有引入临时对象 |

旧 5012 Host 记录的参考值为：四叉树 Build 单次约 8.56--8.75 ms，Query 单次约 0.010--0.014 ms。新 A0 应先得到当前架构下的同口径数据。

## 4. 推荐的 AOT 代码拆分

不要把现有 `MonsterSpatialIndex` 整个文件直接移动到 `AOT_Core`。它引用了 `MonsterWorld`、房间生命周期和 HotFix 类型，直接移动会让 AOT 反向依赖热更程序集，也会把生命周期逻辑和紧密循环混在一起。

推荐拆成两层：

```text
HotFix.Gameplay
  MonsterSpatialIndex
    - 房间边界和生命周期
    - MonsterWorld 读取
    - Add/Remove/Relocate 的业务时机
    - AOT 输入缓冲维护
    - 对外查询接口

AOT_Core
  MonsterQuadTreeKernel
    - Node 和所有四叉树数组
    - Build
    - Add / Remove / Relocate
    - QuerySweptCircle
    - 节点分裂和 Slot 链表操作
```

### 4.1 建议新增文件

```text
Assets/_AOT/Scripts/Core/Spatial/MonsterQuadTreeKernel.cs
Assets/_AOT/Scripts/Core/Spatial/MonsterQuadTreeKernel.cs.meta
```

该文件自动归入现有：

```text
Assets/_AOT/Scripts/Core/AOT_Core.asmdef
```

HotFix 侧仍保留：

```text
Assets/_HotUpdate/Scripts/GameProcess/Monster/Spatial/MonsterSpatialIndex.cs
```

不新增新的 HotFix 程序集，不让 `AOT_Core` 引用任何 `HotFix.*` 程序集。

### 4.2 AOT 内核输入接口

第一阶段为了不改变 `MonsterWorld` 的程序集归属，内核只接收基础数组和标量参数，不接收 `MonsterWorld`：

```csharp
public sealed class MonsterQuadTreeKernel
{
    public MonsterQuadTreeKernel(
        Vector2 rootCenter,
        float rootHalfSize,
        int initialMonsterCapacity);

    public void Build(
        Vector2[] positions,
        byte[] activeMask,
        int slotCount);

    public void Add(int slot, Vector2 position);
    public void Remove(int slot);
    public void Relocate(int slot, Vector2 position);

    public int QuerySweptCircle(
        Vector2 start,
        Vector2 end,
        float radius,
        int[] results);
}
```

`BaseDepth`、`MaxDepth`、`SplitThreshold` 可以先保持 AOT 内核常量，避免第一轮因为配置跨程序集传递改变语义。后续需要多配置时再改为构造参数。

### 4.3 输入数组的过渡方式

当前 `MonsterWorld` 内部是 `MonsterMetaData[]` 和 `MonsterMotionData[]`，它们属于 HotFix。第一轮不要为了 AOT 迁移而移动整个 `MonsterWorld` 数据结构。

HotFix 适配器维护两份仅用于四叉树输入的复用缓冲：

```text
Vector2[] _spatialPositions
byte[]    _spatialActiveMask
```

在 `Build` 前按 Slot 写入：

```text
_spatialPositions[slot] = world.Motion[slot].Position
_spatialActiveMask[slot] = world.Meta[slot].IsActive ? 1 : 0
```

这一步必须计入总 Build 耗时，不能为了证明 AOT 收益而放到 Marker 外面。建议保留现有 `Monster.SpatialIndex.Build` 包络整个“输入准备 + AOT Build”。如需拆分原因，再增加子 Marker，但总 Marker 必须继续用于 A/B。

后续统一快照阶段可以让这份位置和活跃输入被多个空间模块复用，但本轮 AOT 对照不要提前引入统一快照，避免两个变量同时变化。

## 5. AOT 内核的实现要求

### 5.1 必须保持的四叉树语义

AOT 内核应逐项复制当前实现：

1. 根节点 Bounds 判断和 Overflow 链；
2. BaseDepth 基础树的创建顺序；
3. 子节点顺序：左下、右下、左上、右上；
4. `FirstChild + quadrant` 的子节点选择；
5. Slot 头插入顺序；
6. Split 后取出旧链并按原位置重新插入；
7. `Remove` 使用 `_previousSlot` 做 O(1) 链表摘除；
8. `Relocate` 先 Remove 再 Insert；
9. Query 栈访问顺序和 Overflow 最后追加顺序；
10. Query 返回候选但不做距离窄相判断。

第一轮不要做以下改变：

- 不改为连续 Cell 范围；
- 不删除 `_previousSlot`；
- 不把 Query 改成排序候选；
- 不在 AOT 内核里执行伤害、死亡或 Push；
- 不把 Unity Physics、Transform、GameObject 放进内核；
- 不使用 LINQ、委托、虚接口、托管集合或每次 Query 的临时 List。

### 5.2 数组和容量策略

AOT 内核继续使用当前预分配策略：

- 节点数组按 `FullTreeNodeCount(MaxDepth)` 一次预留；
- Query 栈与节点数组等长；
- Slot 数组按房间预期怪物总数预热；
- 容量不足时只允许复用现有倍增策略；
- 正常 Build、Query、Add、Remove、Relocate 不得产生托管分配。

不要在 AOT 迁移阶段加入 NativeArray、Job、Burst 或新的内存所有权协议。它们会把“执行模式对照”扩大成并行化和容器替换对照。

## 6. HotFix 适配器的改动顺序

### 第一步：先引入 Kernel，不改调用方

在 `MonsterSpatialIndex` 中新增一个字段：

```csharp
private readonly MonsterQuadTreeKernel _kernel;
```

先保留原有对外属性和方法：

```text
IsBuilt
BuildVersion
NodeCount
QuerySweptCircle
Add
Remove
Relocate
Clear
Invalidate
```

这样 `MonsterRoomRuntime`、`MonsterRuntimeService` 和 Weapon 无需同时修改。

### 第二步：替换 Build

`EnsureBuilt(world)` 的顺序固定为：

```text
Ensure input capacity
读取 world.Meta / world.Motion 到复用输入数组
调用 _kernel.Build(positions, activeMask, slotCount)
更新 _isBuilt 和 BuildVersion
```

`BuildVersion`、Debug 属性和 Query 统计继续由 HotFix 适配器维护，避免第一轮把调试数据协议搬到 AOT。

### 第三步：替换增量操作

`MonsterRoomRuntime` 现有调用保持不变：

```text
DamageMonster -> SpatialIndex.Remove
PushMonster   -> SpatialIndex.Relocate
Spawn         -> SpatialIndex.Add
```

AOT 内核接收经过 HotFix 适配器读取的 Position，不直接访问 `MonsterWorld`。

### 第四步：保留当前懒 Build

第一轮不要把懒 Build 改为显式 SpatialSnapshot。否则会同时改变：

- Build 发生的 Tick；
- Build 所在渲染帧；
- 子弹和范围查询的调用顺序；
- Tick 补跑时 Build 的叠加方式。

只有在 AOT A/B 完成后，才单独评估显式 Prepare 或增量更新。

## 7. 一致性验证

### 7.1 纯数据测试

使用固定房间 Bounds 和固定 Seed，分别验证：

| 场景 | 需要覆盖 |
|---|---|
| 空房间 | Query 返回 0，Build 不越界 |
| 全部活跃 | 5012 Slot 全部可查询 |
| 稀疏 Slot | 中间死亡 Slot 不影响后续 Slot |
| 越界位置 | Overflow 链可查询且不重复 |
| 边界位置 | Bounds 边缘点归属和原实现一致 |
| 深度分裂 | 16/17 个怪的 SplitThreshold 行为 |
| MaxDepth | 达到深度上限后不继续创建节点 |
| 同位置 | Query 候选稳定，AOT 不丢 Slot |
| Add/Remove | 同一 Tick 增删后查询结果一致 |
| Relocate | 跨叶节点移动后旧叶无残留 |

### 7.2 结果比较

AOT 与 HotUpdate 旧实现分别执行同一组 Query：

- `QuerySweptCircle` 的候选 Slot 集合完全一致；
- 候选数量一致；
- `LastQueryVisitedNodeCount` 在相同树结构下保持一致；
- `NodeCount`、`BuildVersion` 行为一致；
- 候选顺序也先保持一致；如果后续允许顺序变化，必须先审查 Weapon 是否隐式依赖顺序；
- `DamageMonster`、`PushMonster`、死亡和击退后的下一次 Query 不出现旧 Slot。

第一轮可以写一个 Editor/Runtime parity harness，保留旧参考实现和 AOT 实现各一份，不接入正式运行路径。

## 8. HybridCLR、Player 和产物流程

本轮修改了 `AOT_Core` 源码，不能只同步 HotFix DLL。按以下顺序执行：

1. Unity 脚本编译，确认 `AOT_Core` 和 HotFix 无循环依赖；
2. 执行 `HybridCLR > Generate > All`；
3. 执行项目现有的 `Tools > HotUpdate > Build And Sync DLLs`；
4. 确认 `AOT_Core.dll.bytes`、HotFix DLL 和 Addressables 条目来自同一轮；
5. 重新构建 Development IL2CPP Player；
6. 检查 IL2CPP 输出中存在 `MonsterQuadTreeKernel` 的 `Build`、`QuerySweptCircle` 等原生符号；
7. 运行 Host/Client 正常启动链路，确认 AOT 元数据和 HotFix DLL 加载成功；
8. 运行 5012 双端压测。

`AOT_Core` 改动后必须重建 Player，不能只用旧 Player 加载新的 HotFix DLL。`link.xml` 若使用类型保留或反射调试，需要同步加入：

```xml
<type fullname="ProjectGame.Core.Spatial.MonsterQuadTreeKernel" preserve="all" />
```

若内核没有反射入口且 IL2CPP 已确认保留，可以不依赖 link.xml 作为唯一保障，但仍应以最终符号和 Player 冒烟结果为准。

## 9. A/B 量化和通过条件

### A0：当前 HotUpdate 基线

固定：

- 5012 怪物；
- 相同地图、Seed、路线、武器、TickRate；
- 相同预热时间和采样时长；
- Host/Client 各 3 次；
- 正常 HybridCLR、Addressables、Player 启动；
- 不打开 Deep Profiling。

### A1：四叉树 AOT

只改变四叉树内核所在程序集，其他代码和压测参数完全不变。

重点对比：

| 指标 | 通过要求 |
|---|---|
| Query 候选集合 | 0 差异 |
| 死亡/击退一致性 | 0 残留 Slot |
| Build 调用次数 | 与 A0 一致或仅有采样噪声差异 |
| Query 调用次数 | 与 A0 一致 |
| 候选数/访问节点数 | 不出现系统性变化 |
| GC Alloc | 不增加 |
| Build 单次耗时 | 记录平均/P95/最大，不预设收益 |
| Host Main Thread | 记录平均/P95/P99/最大 |
| TickDriver 补跑 Tick | 不增加 |

收益解释：

- Build 下降超过 50%：解释执行是重要放大因素，继续保留四叉树 AOT；
- Build 下降 20% 以下：主要成本在全量重建、数组清理和节点写入，应优先增量或 Grid；
- Build 下降明显但 Host 仍个位数 FPS：下一优先级转向 EntityTick、Tick 补跑和其它线性系统；
- Query 已在 0.01 ms 量级：不因为 Query 小幅变化替换查询算法。

## 10. 回滚和交付物

建议将本轮拆成三个提交：

```text
1. quad-tree-aot-kernel：只新增 AOT 内核
2. quad-tree-aot-adapter：HotFix 适配器切换
3. quad-tree-aot-validation：测试、文档和量化记录
```

如果 A1 正确性或性能不通过，回滚第 2 个提交即可恢复原 HotUpdate 四叉树，不触碰 Density、Separation 和 Weapon。

本轮交付物应包括：

- AOT 内核源码和 `.meta`；
- HotFix 适配器差异；
- parity 测试结果；
- A0/A1 的 `markers.csv`、`frames.csv`、`REPORT.md`；
- AOT Core/HotFix DLL 与 Player 构建指纹；
- Build/Query/EntityTick/TickDriver/GC 对照表。

只有 A1 通过后，才进入下一份《怪物统一空间快照执行方案》，避免 AOT 和 Grid 结构变化互相污染。
