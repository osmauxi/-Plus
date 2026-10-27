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

### 8.1 实际产物核验（2026-10-28）

本轮已经完成完整的 HybridCLR、Addressables 和 Development IL2CPP Player 构建，并使用最终 Player 完成双端压测：

- Unity `2022.3.44f1c1`，`StandaloneWindows64` Development Build；
- `AOT_Core.cpp` 中已生成 `MonsterQuadTreeKernel.Build`、`Add`、`Remove`、`Relocate` 和 `QuerySweptCircle` 原生符号；
- `Assets/HybridCLRGenerate/link.xml` 已保留 `MonsterQuadTreeKernel` 和 `MonsterQuadTreeNodeData`；
- `GameAssembly.dll` SHA-256：`2A42DA2DC04235512EFE6A8711BA3B16E0F9655D5E4D7472E2D6AC844F57CFEB`；
- `global-metadata.dat` SHA-256：`7B59E85779572B3FFB951C68CC34D85DB0BB43108D2000DAFC9442EF8E962DAB`；
- Catalog 指纹：`69ce53a68b210a6e796f7fd7e50920c1`；
- Editor 一致性验证再次通过，覆盖空树、5012 固定随机数据、稀疏 Slot、边界与越界、16/17 分裂阈值、最大深度以及 Add/Remove/Relocate；
- Unity 当前编译错误为 0，最终 Player 的 Host/Client 18 轮均正常启动和结束。

`_plus.exe` 的 SHA-256 仍为 `C78C8067850AEA9F5ACA5D0CDC9201E5CE24E22C176B4C2F515982C8E8C5B322`。这是 Unity Player 壳文件未变化，不代表原生游戏代码未更新；本轮以 `GameAssembly.dll`、IL2CPP 原生符号、Catalog 和运行结果共同识别 AOT 构建。

## 9. A/B 量化和通过条件

### A0：当前 HotUpdate 基线

固定：

- 5012 怪物；
- 相同地图、Seed、路线、武器、TickRate；
- 相同预热时间和采样时长；
- Host/Client 各 3 次；
- 正常 HybridCLR、Addressables、Player 启动；
- 不打开 Deep Profiling。

#### A0 冻结结果（2026-10-27）

- 源码基线提交：`f408135`；
- 压测 RunId：`20261027-130023-32cb3592`；
- 采样模式：`dual-pass-foreground-v1`，不保存完整 Profiler；
- 5012 Host/Client 各 3 次均通过前台占比、双端对齐和工作负载验证；
- Build：平均 `5.909 ms/次`，三次为 `5.726 / 6.365 / 5.636 ms`，平均频率 `28.54 Hz`；
- Query：平均 `0.0068 ms/次`，平均频率 `188.83 Hz`；
- EntityTick：平均 `10.671 ms/次`，Build 约占 `55.4%`；
- 候选数平均 `0.79～0.96`，访问节点平均 `17.73～17.97`，射击 `127～128` 次。

本轮作为 A1 的自动比较基线。A1 必须保持相同 Seed、路线、采样时长、重复次数与轻量采样模式；完整 Profiler 只用于独立诊断，不与 A0 直接计算性能差值。

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
- 以 A0 平均值换算，A1 Build `≤ 2.95 ms` 为超过 50% 收益，`≥ 4.73 ms` 为不足 20% 收益；
- Build 下降明显但 Host 仍个位数 FPS：下一优先级转向 EntityTick、Tick 补跑和其它线性系统；
- Query 已在 0.01 ms 量级：不因为 Query 小幅变化替换查询算法。

#### A1 冻结结果（2026-10-28）

- AOT 适配器源码提交：`157d818`；
- 压测 RunId：`20261027-161455-47937465`；
- 对照 RunId：`20261027-130023-32cb3592`；
- 两轮均使用 `dual-pass-foreground-v1`、Seed `12345`、路线 `square-12m-fire-v3`、5 秒预热、30 秒采样、每档 3 次，不保存完整 Profiler；
- 1024 / 2048 / 5012 的 Host、Client 均为 3/3 有效，采样期间前台占比均为 100%；
- 5012 档两轮均为 5012 个 Host 存活怪物、5012 个 Client Replica 和 5012 个激活 View。

自动汇总将两轮标记为“Catalog 指纹不兼容”：A0 为 `f54c125628c921e992e790a03ea65a5b`，A1 为 `69ce53a68b210a6e796f7fd7e50920c1`。本次 Catalog 变化来自 AOT/HotFix DLL 与 Player 的预期重建，因此不能要求待比较的两个版本具有相同 Catalog；手工 A/B 仍以相同运行配置、工作负载和明确的构建身份进行。A1 的 `run-metadata.json` 同时记录了 `gitDirty=true`，原因是采样时生成后的 DLL 与链接文件尚未提交。它不影响本轮直接 Marker 的算法结论，但降低了作为长期自动回归基线的可复现等级。

##### 四叉树直接 Marker

以下平均值按三次 Host 前台轮的 Marker 样本数加权，Build 包含 HotFix 输入缓冲写入和 AOT 内核执行，不把跨程序集适配成本移到 Marker 外。

| 怪物数 | Build A0 (ms/次) | Build A1 (ms/次) | Build 变化 | Query A0 (ms/次) | Query A1 (ms/次) | Query 变化 |
|---:|---:|---:|---:|---:|---:|---:|
| 1024 | 1.3622 | 0.1028 | -92.5%，13.25 倍 | 0.00757 | 0.00074 | -90.2%，10.20 倍 |
| 2048 | 2.7757 | 0.2070 | -92.5%，13.41 倍 | 0.00749 | 0.00082 | -89.1%，9.16 倍 |
| 5012 | 5.9101 | 0.4947 | -91.6%，11.95 倍 | 0.00685 | 0.00083 | -87.9%，8.25 倍 |

5012 档的 Build 三次分别从 `5.726 / 6.365 / 5.636 ms` 变为 `0.495 / 0.494 / 0.495 ms`。A1 的 Build P95 单帧为 `0.538 / 0.539 / 0.546 ms`，A0 为 `6.229 / 7.711 / 5.815 ms`。结果不仅大幅下降，而且三次重复之间非常稳定。

| 5012 工作负载/结构指标 | A0 | A1 | 变化与判断 |
|---|---:|---:|---|
| Build 频率 | 28.54 Hz | 27.80 Hz | -2.6%，属于采样工作量小幅波动 |
| Query 频率 | 188.83 Hz | 184.15 Hz | -2.5%，负载近似一致 |
| Host 移动距离，三次平均 | 304.98 m | 306.81 m | +0.6%，路线负载一致 |
| 射击次数，三次合计 | 383 | 397 | +3.7%，A1 并未降低武器压力 |
| 每次 Query 访问节点 | 17.84 | 17.92 | +0.4%，树结构和遍历规模稳定 |
| 每次 Query 候选数 | 0.89 | 0.52 | 绝对值都低于 1；受射击相位和位置影响，不作为加速证据 |

候选数没有要求跨独立运行逐次相等；正确性由固定输入的一致性验证负责。运行时访问节点数几乎不变，并且 Build/Query 次数、路线和射击压力接近，说明这次收益不是通过降低四叉树工作量获得。

##### 5012 整体性能

| 指标 | A0 | A1 | 变化 |
|---|---:|---:|---:|
| Host 帧间隔，全部帧加权 | 17.117 ms | 10.617 ms | -38.0% |
| Host Main Thread，全部帧加权 | 17.055 ms | 10.578 ms | -38.0% |
| Host 帧间隔，三次原始均值 | 15.189 / 23.687 / 14.872 ms | 10.555 / 10.774 / 10.524 ms | A1 波动显著收敛 |
| Host Main Thread P95，三次平均 | 33.69 ms | 25.54 ms | -24.2% |
| `Monster.Simulation.EntityTick` | 10.673 ms/次 | 9.945 ms/次 | -6.8% |
| `Monster.Simulation.SeparationBuild` | 1.193 ms/次 | 1.068 ms/次 | -10.5%，不视为 AOT 直接收益 |
| Host GPU Frame Time | 2.682 ms | 2.537 ms | -5.4%，不是本次 CPU 迁移目标 |
| Host GC Alloc，按帧加权 | 1827 B/帧 | 1669 B/帧 | -8.7%，没有新增分配回归 |
| Client 帧间隔，全部帧加权 | 8.197 ms | 8.224 ms | +0.3%，5012 Client 基本不变 |

A0 的第二次 Host 轮为 `23.687 ms`，明显慢于另外两次，因此不能把整帧 `-38.0%` 全部解释为 AOT 的确定收益。即使使用三次中位重复比较，Host 平均帧间隔也从 `15.189 ms` 降到 `10.555 ms`，下降 `30.5%`。直接 Build Marker 的 `-91.6%`、三次重复稳定性以及所有怪物档位一致的缩放趋势，是本次迁移最可靠的证据。

##### 评估结论

1. **AOT 迁移通过。** 5012 Build 为 `0.4947 ms/次`，明显低于 `2.95 ms` 的“收益超过 50%”门槛；实际下降 `91.6%`。四叉树应保留在 `AOT_Core`，不回退到 HotFix 解释执行。
2. **正确性与运行负载通过。** 固定数据 parity 为 0 差异，Player 双端 18 轮完成；访问节点、Build/Query 频率和路线负载没有系统性下降。
3. **Query 不再是优化对象。** A1 Query 只有约 `0.00083 ms/次`，继续优化四叉树查询不会形成可见整帧收益。
4. **Build 已退出主要热点。** 5012 A1 的主要 Marker 已转为 EntityTick `9.945 ms/次`、Presentation Capture `1.864 ms/次`、Presentation Apply `1.392 ms/次` 和 Separation Build `1.068 ms/次`；四叉树 Build 只剩约 `0.495 ms/次`。
5. **可以进入统一空间快照阶段。** 本轮足以作为《怪物统一空间快照执行方案》的 S0 性能参考；下一阶段应合并 Density 与 Separation 的重复全量采集，而不是继续改四叉树算法或立即替换为 Grid。
6. **长期回归基线仍应补强。** 在生成物提交后，建议补一轮干净工作区确认样本；同时调整自动兼容判断，使 Catalog/构建指纹用于识别版本而不是要求 A/B 两轮完全相同。当前 A1 可用于架构决策，但自动报表不应把它误称为“相同构建回归”。

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
