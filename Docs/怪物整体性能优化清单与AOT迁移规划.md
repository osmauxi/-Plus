# 怪物整体性能优化清单与 AOT 迁移规划

更新时间：2026-11-19

## 1. 当前结论

当前怪物系统的主要问题不是某一个网络发送函数，而是 30 Hz Gameplay Tick 的单 Tick 成本已经接近预算，导致 `GameplayNetworkTickDriver` 在一帧内连续执行多个 Tick。

30 Hz 的逻辑 Tick 预算为：

```text
1 / 30 = 33.333 ms
```

最新 `20261118-143108-7c51391d` 的三次 Host 结果中：

- `Gameplay.TickDriver.Dispatch`：约 60～63 ms/帧；
- 每个渲染帧执行约 2.14～2.21 个 Gameplay Tick；
- `EntityTick`：约 16.6～16.7 ms/Tick；
- `Replication.Publish`：约 9.0 ms/Tick；
- `WorldSimulation + Replication`：约 28～29 ms/Tick；
- 加上其他 PlayerLoop 阶段后，单 Tick 很容易超过 33.333 ms。

因此第一目标是把完整单 Tick 压回预算以内，使驱动器稳定回到一 Tick/帧。网络带宽和发送 CPU 已经不是第一优先级，但网络发送仍然是明显的 GC 来源。

## 2. 当前链路与热点

```text
GameplayNetworkTickDriver.Update
└─ Advance
   └─ Dispatch
      ├─ WorldSimulation
      │  └─ MonsterRoomRuntime.Tick
      │     ├─ OccupancySnapshot.Build
      │     ├─ MonsterSimulation.Tick
      │     │  ├─ FlowField / Density / Separation
      │     │  └─ EntityTick
      │     │     ├─ Target
      │     │     ├─ Attack
      │     │     ├─ Navigation
      │     │     ├─ Separation
      │     │     └─ Constraint
      │     └─ SpatialIndex.Invalidate
      ├─ MeleeCombat
      └─ Replication
         ├─ CaptureFrame / ApplyLocal
         └─ PublishRemoteGroups
            ├─ RefreshNetworkGroups
            ├─ DueGroup 排序
            ├─ CaptureGroupFrame
            ├─ GroupMembership
            ├─ GroupSnapshot.Write
            └─ SendUnnamedMessage
```

当前已经下沉到 AOT 的部分主要是四叉树、占用快照、密度、流场、分离和移动约束内核；但这些内核仍然大量使用托管数组和普通 C# 类，还没有形成统一的 Burst 数据通道。

## 3. P0：先把单 Tick 压回预算

### 3.1 `MonsterSimulation.EntityTick`

这是当前最大的 CPU 区域。现有细 Marker 显示 Navigation、Separation、Constraint 和 Attack 都有稳定成本，且外层循环仍在 HotFix 中逐怪执行。

优化顺序：

1. 保留现有 AOT Kernel，先把 `EntityTick` 外层逐怪循环拆成纯数据阶段；
2. 把 `MonsterMetaData`、`MonsterMotionData`、攻击状态、目标结果和运行时配置整理成连续值类型数组；
3. 让 AOT/Burst 内核一次处理 `ActiveSlots`；
4. 将攻击请求写入预分配的 NativeList 或固定容量请求缓冲；
5. HotFix 只负责 Tick 编排、玩家输入、配置和最终战斗事件。

这一步潜在收益最大，但改动风险也最高，应在群组网络优化之后作为第二阶段。

### 3.2 `MonsterNetworkGroupIndex.Refresh`

当前实现每次刷新都会：

- 扫描整个 `world.SlotCount`；
- 对活跃怪物计算四叉树基础区域码；
- 通过 `Dictionary<uint, Region>` 管理区域；
- 通过 `List<Group>` 和 `Group` 引用类型管理群组；
- 每 6 Tick 刷新所有 Region 的层级；
- 成员迁移时使用 `Array.IndexOf` 查找组内位置。

这是最适合优先下沉 AOT 的网络模块。重点不是把现有类搬到 AOT，而是改成 SoA：

```text
slot -> regionCode
slot -> groupIndex
group -> memberOffset / memberCount
group -> interval / nextPoseTick / membershipRevision
region -> groupRange / interval
dueGroups -> groupIndex[]
```

建议的 AOT 内核接口：

```text
RefreshMembership(
    positions,
    activeSlots,
    playerPositions,
    currentTick,
    groupState,
    membershipOutput)
```

HotFix 只保留：

- 房间生命周期；
- 网络客户端列表；
- 群组批次发送；
- 事件和诊断。

### 3.3 避免成员迁移的线性查找

当前 `Group.Remove` 使用 `Array.IndexOf`，成员迁移时最坏是组容量级查找。应增加：

```text
slotGroupIndex[slot]
slotIndexInGroup[slot]
```

采用末项填补后更新两个索引，使 Remove 变为 O(1)。这项不需要先做 Burst，属于低风险直接优化。

### 3.4 避免每 Tick 全量排序

当前 `_duePoseGroups.Sort(...)` 每个复制 Tick 都执行。群组数量目前不算最大热点，但它属于稳定的重复工作。

建议顺序：

1. 先测排序累计耗时；
2. 若排序成为明显占比，按 `IntervalTicks` 建立 2/3/6 三个相位桶；
3. 每个 Tick 只扫描当前相位和已过期桶；
4. 在预算内按到期时间和层级消费。

## 4. P1：表现数据 Capture / Apply

### 4.1 `MonsterPresentationPackingSystem.CaptureInto`

当前完整表现 Capture：

- 扫描整个 `SlotCount`；
- 判断 Active；
- 调用 `MonsterPoseQuantizer.Pack`；
- 写入 ActiveMask 和 PackedState。

细 Marker 显示 `Capture.StateLoop` 是主要成本，`BeginWrite` 几乎没有成本。

建议：

- 直接消费预先维护的 `ActiveSlots`，避免再次扫描整个 Slot 数组；
- 将量化 Pack 改为 AOT/Burst 纯值函数；
- 使用连续状态数组和批量写入；
- 将 `MonsterPackedState` 移到 AOT 共享数据程序集，作为网络和表现共同协议类型。

### 4.2 `CaptureGroupInto`

群组 Capture 是网络发布热路径，适合做成独立 AOT Kernel：

```text
PackGroupStates(memberSlots, positions, presentationEvents, quantizer, destinationStates)
```

`BeginWrite` 不需要优化，重点是成员循环和量化。当前细 Marker 已证明群组初始化不是成本来源。

### 4.3 `MonsterReplica.Apply` 与 `ApplyGroupSnapshot`

客户端 Apply 同样是纯数组循环，适合下沉 AOT：

- 完整帧 Apply：ActiveMask、Previous/Current Pose、事件序列；
- 群组 Apply：成员表索引、Tick 顺序、插值起点和目标姿态。

客户端目前不是 Host 长帧主因，但这会降低客户端 CPU 和未来网络规模增长的风险。

## 5. P1：网络 GC 与发包调用次数

最新原始 Profiler 显示：

- `GroupSnapshot.Write`：GC 0；
- `GroupMembership.Write`：GC 0；
- `GroupSnapshot.Send` / `SendUnnamedMessage`：约 126 B/次；
- Host 约 1214 条发送消息/s；
- 约产生 153 KB/s 的托管分配。

因此 `SendUnnamedMessage` 的主要问题是 GC，不是 CPU。当前 `FastBufferWriter(Allocator.Temp)` 没有显示为这部分托管 GC，分配主要发生在 NGO 的 Unnamed 消息入队路径。

建议：

1. 先保持现有包大小限制，统计不同消息类型的调用数；
2. 将成员表变更和首个姿态尽可能合并；
3. 在不触发底层分片的前提下扩大快照批次；
4. 目标是减少发送调用次数，而不是继续微调单次 Writer 写入；
5. 每减少 1000 条发送消息/s，理论上约减少 126 KB/s 托管分配。

旧 `SendNamedMessage` 没有同一 Catalog、同一 Profiler 口径的 GC 原始对照，因此暂时不能宣称 Unnamed 相比 Named 的绝对 GC 降幅。当前只能确认 Unnamed 仍然是重要 GC 来源。

## 6. AOT 迁移边界

### 6.1 适合下沉到 AOT/Burst 的内容

- `MonsterNetworkGroupIndex` 的区域码、成员表和 Tick 调度核心；
- `MonsterPackedState`、量化和批量 Pack；
- `MonsterReplica` 的完整帧与群组 Apply 循环；
- `MonsterSimulation` 的目标、移动、攻击和约束数据循环；
- 已存在的 Occupancy、QuadTree、Density、FlowField、Separation、Constraint Kernel。

### 6.2 应留在 HotFix 的内容

- `MonsterRuntimeService` 生命周期和房间编排；
- `MonsterRoomRuntime` 的场景对象、Transform、PlayerManager 和导航环境绑定；
- `MonsterNetworkTransport` 的 NGO 发送入口；
- 网络客户端列表、事件、UI 和调试接口；
- VAT 的 `Material`、`Graphics.DrawMeshInstancedIndirect` 和 Unity 渲染 API。

### 6.3 不建议的迁移方式

- 不要把当前带 `Dictionary`、`List<Group>`、`Transform` 和 NGO 调用的整个 `MonsterNetworkGroupIndex` 原样移动到 AOT；
- 不要为了“进入 AOT”引入一层泛化 Provider/Manager；
- 不要在没有 NativeArray 数据边界之前直接加 Burst 特性；
- 不要先改 TickRate 来掩盖单 Tick 成本。

正确顺序是：先确定数据布局，再提取 AOT Kernel，最后把 NativeArray/Burst 接入。AOT 只是编译归属变化，真正的性能收益来自连续数据、减少托管对象和批量循环。

## 7. 推荐执行顺序

### 阶段一：低风险直接收益

1. 给 Group 成员增加 O(1) 删除索引；
2. 维护 ActiveSlots，减少全量 Slot 扫描；
3. 统计并减少 GroupMembership / GroupSnapshot 发送调用；
4. 保持 `Gameplay.TickDriver.Dispatch`、EntityTick、Publish 和 GC Marker。

### 阶段二：群组 AOT Kernel

1. 把区域、成员和调度状态改为 SoA；
2. 将 Refresh、层级判定、DueGroup 生成下沉 AOT；
3. HotFix 接收纯数组结果并完成 Capture/Batch/Transport；
4. 用相同 Player、Catalog、10024 怪物、3 次重复验证。

### 阶段三：表现 Pack / Apply AOT Kernel

1. 迁移 `MonsterPackedState` 和量化函数；
2. 迁移全量 Capture、Group Capture；
3. 迁移 Host ApplyLocal 和 Client GroupApply；
4. 对比 Host Tick、Client GroupApply、GC 和消息数量。

### 阶段四：MonsterSimulation 外层循环

1. 先把状态数组和配置改成 Burst 可读布局；
2. 再拆分 Target、Attack、Navigation、Separation、Constraint；
3. 最后处理近战请求、Unity Physics 和 HotFix 事件边界。

## 8. 每阶段验收指标

每次只接受同一 Player、同一 Catalog、同一路线、同一怪物数量的对比：

- `Gameplay.TickDriver.Dispatch` 平均和 P95；
- Tick 样本数 / 渲染帧数；
- `WorldSimulation`、`EntityTick`、`Replication.Publish` 单 Tick 时间；
- `PublishGroups`、GroupBuild、GroupCapture、Capture StateLoop；
- GC Alloc B/帧和 Gen0 次数；
- Host 发送消息/s、平均 Payload、KiB/s；
- Host/Client 存活、Replica、View 和 Source FlowField 重建一致性。

最终目标不是让某个 Marker 单独变小，而是让完整 Gameplay Tick 稳定低于 33.333 ms，使驱动器恢复一 Tick/帧，再用消息合批消除 `SendUnnamedMessage` 的托管分配。

## 9. 下一阶段具体修改计划：群组索引 AOT 重构

本阶段只处理“群组区域、成员关系、层级频率和到期调度”的核心。网络发送、协议格式和 MonsterSimulation 暂不一起改，避免一次变更多个变量。

### 9.1 当前实现与目标实现

当前入口链路：

```text
MonsterRuntimeService.PublishRemoteGroups
└─ MonsterRoomRuntime.RefreshNetworkGroups
   └─ MonsterNetworkGroupIndex.Refresh
      ├─ 扫描 world.SlotCount
      ├─ GetBaseRegionCode
      ├─ Dictionary<uint, Region>
      ├─ List<Group>
      └─ Region.RefreshTier
```

目标链路：

```text
MonsterRuntimeService.PublishRemoteGroups
└─ MonsterRoomRuntime.RefreshNetworkGroups
   └─ MonsterNetworkGroupKernel.Refresh
      ├─ 更新 slot -> region/group 映射
      ├─ 更新成员连续存储
      ├─ 更新区域频率
      └─ 输出到期 groupIndex 列表
```

HotFix 最终只消费 AOT Kernel 输出的群组视图，不再持有 `Region` 和 `Group` 引用对象。

### 9.2 第一步：冻结现状和定义数据契约

修改前先固定以下基线：

- `20261118-143108-7c51391d` 的 Host/Client 结果；
- `Monster.Network.GroupBuild`、`GroupSchedule.Refresh`、`PublishGroups`；
- 组数量、每组成员数、MembershipRevision 变化次数；
- Host 消息数、Payload、GC Alloc；
- 10024 怪物三次有效重复。

在 `Assets/_AOT/Scripts/Core/Monsters` 下新增只包含值类型的数据契约，建议包含：

```text
MonsterNetworkRegionState
MonsterNetworkGroupState
MonsterNetworkGroupPlan
MonsterNetworkGroupOutput
```

契约只放：

- RegionCode；
- GroupId；
- MemberOffset / MemberCount；
- IntervalTicks；
- NextPoseTick；
- MembershipRevision；
- PublishedMembershipRevision；
- Slot -> GroupIndex；
- Slot -> IndexInGroup。

不把 `MonsterWorld`、`Transform`、`NetworkManager`、`FastBufferWriter` 或事件放进 AOT 数据结构。

### 9.3 第二步：先做 AOT Kernel，保持行为不变

新增 `MonsterNetworkGroupKernel`，先使用预分配数组实现最终数据流，确保迁移过程中不产生每 Tick GC：

```text
Initialize(rootCenter, rootHalfSize, initialCapacity)
EnsureCapacity(slotCapacity, regionCapacity, groupCapacity)
Clear()
RefreshMembership(positions, activeSlots, activeCount, playerPositions, tick)
RefreshTiers(playerPositions, tick)
BuildDueGroups(tick)
GetGroupState(groupIndex)
GetMemberSlots(groupIndex)
```

这一阶段只替换：

- `Dictionary<uint, Region>`；
- `List<Group>`；
- `Group` 内部数组；
- `Array.IndexOf` 成员删除。

频率规则保持不变：Inner=2、Middle=3、Outer=6，区域基础划分仍然调用现有四叉树 Kernel 的 `GetBaseRegionCode`。

### 9.4 第三步：实现 O(1) 成员迁移

在 Kernel 内维护：

```text
slotGroupIndex[slot]
slotIndexInGroup[slot]
groupMemberSlots[groupIndex * GroupCapacity + localIndex]
```

迁移流程：

1. 通过 `slotGroupIndex` 找到旧组；
2. 通过 `slotIndexInGroup` 找到组内位置；
3. 用末成员填补空位；
4. 更新末成员的 `slotIndexInGroup`；
5. 写入新组尾部；
6. 更新两个组的 MembershipRevision。

这一步必须保持当前“末项填补空位”的成员顺序语义，并验证跨组后客户端成员表和姿态 Tick 不回退。

### 9.5 第四步：改 HotFix 适配层

修改文件：

- `MonsterNetworkGroupIndex.cs`：由对象容器改为 AOT Kernel 适配器，或直接删除旧实现；
- `MonsterRoomRuntime.cs`：转发位置、活跃 Slot 和玩家位置，返回 Kernel 输出；
- `MonsterRuntimeService.cs`：`PublishRemoteGroups` 改为读取 `GroupPlan` 和连续成员 Span；
- `MonsterNetworkTransport.cs`：只接收批次和客户端列表，不感知 Region/Group 对象。

这一阶段不修改：

- `MonsterNetworkCodec` 的包格式；
- Inner/Middle/Outer 频率；
- 32 KiB Tick 预算；
- 1024 B 快照批次上限；
- 客户端 Replica 协议。

### 9.6 第五步：验证 AOT Kernel 的行为一致性

必须在同一测试环境同时验证：

1. 10024 怪物全部存活；
2. GroupId 数量和成员总数一致；
3. 每个 Slot 的所属组一致；
4. MembershipRevision 变化次数一致；
5. Inner/Middle/Outer 频率分布一致；
6. DueGroup 顺序和预算截断结果一致；
7. Host/Client Replica 和 View 数量一致；
8. 网络 KiB/s、消息数和 Payload 不发生协议外变化。

性能验收：

- `Monster.Network.GroupBuild` 下降；
- `Monster.Network.GroupSchedule.Refresh` 下降；
- `PublishGroups` 下降；
- GC 不因 Kernel 迁移增加；
- `Gameplay.TickDriver.Dispatch` 的平均和 P95 下降；
- Tick/渲染帧比值向 1.0 靠近。

### 9.7 第六步：决定是否进入 NativeArray/Burst

先完成 AOT Kernel 的行为等价和轻量量化。如果 AOT 版本仍无法把单 Tick 压回 33.333 ms，再把 Kernel 的数组替换为 `NativeArray`，为 Burst Job 做准备。

NativeArray/Burst 阶段需要同步处理 MonsterWorld 的数据边界，否则每 Tick 把托管数组复制到 NativeArray 会抵消收益。此阶段再评估是否把 `MonsterMotionData`、`MonsterMetaData` 和 ActiveSlots 统一迁移到 AOT 数据容器。

### 9.8 本阶段完成条件

满足以下条件才结束群组 AOT 阶段：

- HotFix 中不再存在 `Region`、`Group` 引用对象和群组 Dictionary；
- 群组刷新和成员迁移不产生每 Tick 托管分配；
- 10024 三次有效压测均能完成 Source FlowField 重建；
- `GroupBuild`、`GroupSchedule.Refresh` 和 `PublishGroups` 有可重复的下降；
- 包体字节、频率、成员版本和客户端表现保持一致；
- 文档记录迁移前后 Tick、GC、消息数和带宽变化。

### 9.9 首轮实施结果

群组 AOT Kernel 已完成首轮落地：

- 删除 HotFix `MonsterNetworkGroupIndex`、`Region` 和 `Group` 引用实现；
- 新增 AOT `MonsterNetworkGroupKernel`，使用区域、群组、Slot 映射和连续成员数组；
- 复用四叉树 Kernel 的 BaseRegionCode 计算，保持空间划分一致；
- 成员删除改为 `slotGroupIndex + slotIndexInGroup` O(1)；
- `MonsterRuntimeService` 改为使用 groupIndex 和 Kernel 视图；
- 网络包格式、Inner/Middle/Outer 频率、批次预算和客户端协议保持不变。

首轮 10024 压测结果：正常两次 Host 平均帧时约 39.7～41.0 ms，`GroupSchedule.Refresh` 从约 3.4 ms/帧降到约 0.45 ms/帧，`PublishGroups` 从约 7.8 ms/帧降到约 2.4～2.6 ms/帧。重复 2 出现一次 95 ms 运行时异常，需要单独复测，但不影响群组 Kernel 的首轮收益判断。

## 10. 下一阶段具体修改计划：MonsterSimulation EntityTick AOT/SoA

### 10.1 阶段目标

群组索引 AOT 已经完成稳定收益验证，下一阶段不再继续微调网络群组调度，而是处理 Host 单 Tick 的最大 CPU 区域：`MonsterSimulation.EntityTick`。

当前同 Catalog 的 AOT Profiler 复测显示：

- `EntityTick` 每个逻辑 Tick 约 16～17 ms；
- `Replication.Publish` 每个逻辑 Tick 约 8～9 ms；
- `GroupSchedule.Refresh` 已降到约 0.6～0.9 ms/帧；
- 客户端平均帧时约 15～16 ms，网络带宽和消息数量保持稳定。

因此当前长帧的根因是模拟和复制总成本触发 Tick backlog。下一阶段目标是把 EntityTick 的单 Tick 成本继续压低，并让完整 Gameplay Tick 稳定落在 33.333 ms 预算以内。

### 10.2 最终数据边界

最终由 AOT Kernel 直接消费怪物模拟的热数据，HotFix 只负责生命周期和环境编排。当前数组仍由 `MonsterWorld` 持有，以保持房间生命周期和现有查询接口；数组元素已经迁移到 AOT 共享值类型，Tick 内不再复制到临时缓冲。后续进入 NativeArray/Burst 时再迁移容器所有权，不把 HotFix 对象、Transform、PlayerManager 或 NGO 类型带入 Kernel。

建议最终数据布局：

```text
Meta:        active / configIndex
Motion:      position / velocity / yaw
Status:      speedMultiplier / slowEndTick / stunEndTick
Attack:      phase / phaseEndTick / attackSequence
Presentation: hitSequence
Target:      flags / targetPosition / flowDirection / distanceSquared
ActiveSlots: 当前活跃 Slot 连续列表
Configs:     只读运行时配置数组
```

当前 `MonsterWorld` 的六组托管 SoA 数组是已有数据基础；本阶段将其所有权迁移到 AOT 模拟状态，避免每 Tick 把 HotFix 数组复制到 AOT 临时缓冲。

### 10.3 Kernel 责任划分

新增最终态 `MonsterSimulationKernel`，一次处理当前 Tick 的全部活跃 Slot：

```text
Tick
├─ ResolveTargetBatch
├─ TickAttackBatch
├─ CalculateNavigationBatch
├─ ApplySeparationBatch
├─ ApplyConstraintBatch
└─ IntegrateBatch
```

继续复用现有 AOT 内核：

- `FlowTargetKernel`：批量目标和流场方向解析；
- `SeparationKernel`：占用快照构建后的分离力；
- `MovementConstraintKernel`：可行走区域约束；
- `DensityKernel`、`MultiSourceFlowField`：密度和流场更新。

本次重点不是重新实现这些算法，而是移除 HotFix 中逐怪循环对多个数组和方法的重复边界访问，让一个 AOT Kernel 连续完成完整实体阶段。

### 10.4 HotFix 调用边界

修改后 `MonsterSimulation` 只保留以下职责：

1. 接收房间 Tick、玩家目标和导航环境；
2. 更新 Density、FlowField、Occupancy 和 Separation 输入；
3. 调用一次 `MonsterSimulationKernel.Tick`；
4. 读取预分配的近战请求缓冲并交给战斗结算。

以下逻辑移出 HotFix 的逐 Slot 路径：

- `IsStunned`、减速状态读取；
- `TickAttack` 的状态推进；
- Navigation 速度计算；
- Separation 合入；
- Constraint 约束；
- Position、Velocity、Yaw 写回。

不要保留“旧 HotFix 循环 + 新 AOT 循环”双路径，也不增加兼容适配层。切换完成后，旧循环直接删除。

### 10.5 攻击请求和事件数据

攻击请求继续使用值类型缓冲，不在 Kernel 内创建对象或调用 Unity API：

```text
AttackRequestBuffer
├─ attackerSlot
├─ targetPosition
├─ attackProfileIndex
└─ damage
```

Kernel 只负责判断攻击阶段、写入请求和推进攻击状态；近战物理解析、伤害应用和表现事件仍由房间层在 Tick 后处理。请求缓冲在房间容量变化时一次扩容，Tick 内只清空计数，不重新创建集合。

### 10.6 实施文件范围

首轮修改集中在以下文件：

- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Core/MonsterWorld.cs`：移交模拟 SoA 数据所有权；
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Simulation/MonsterSimulation.cs`：删除逐 Slot 热循环，保留编排；
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Spatial/MonsterOccupancySnapshot.cs`：提供 AOT 可直接消费的活跃 Slot 和空间数据；
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Runtime/MonsterRoomRuntime.cs`：连接 Kernel 输入、输出和攻击请求；
- `Assets/_AOT/Scripts/Core/Monsters/MonsterSimulationData.cs`：新增 AOT 共享状态、配置和攻击请求值类型；
- `Assets/_AOT/Scripts/Core/Monsters/MonsterSimulationKernel.cs`：新增实体 Tick AOT Kernel；
- `Assets/_HotUpdate/Scripts/GameProcess/Monster/Simulation/MonsterSimulation.cs`：持有一次扩容的攻击请求数组并负责战斗阶段编排。

表现 Capture、网络包格式和群组分层在本阶段保持不变，避免把模拟收益和复制协议变化混在一次量化中。

### 10.7 不采用的方案

- 不通过降低 TickRate 掩盖单 Tick 成本；
- 不在每 Tick 把 `MonsterWorld` 托管数组复制到 NativeArray；
- 不把 `Transform`、`PlayerManager` 或 `FastBufferWriter` 传入 AOT Kernel；
- 不为理论上的空状态增加额外判空和恢复分支；
- 不先优化 `SendUnnamedMessage` 的 126 B/次分配，因为它当前不是 Host 长帧的主要耗时来源；
- 不为了 Burst 强行拆出多层 Provider、Manager 或接口。

### 10.8 验收顺序

代码完成后按以下顺序验证：

1. Unity 编译和 Development Player 打包；
2. 10024 怪三次双端有效压测；
3. Host/Client 存活、Replica、View 和 Source FlowField 重建；
4. `EntityTick`、`WorldSimulation`、`Replication.Publish` 单 Tick Marker；
5. Tick/渲染帧比例和 `DroppedTimeSeconds`；
6. GC Alloc、Gen0 次数、网络消息数和带宽；
7. 与 `20261118-152803-45f62fe3` 同 Player/Catalog 结果对比。

阶段完成条件：

- EntityTick 单 Tick 成本出现可重复下降；
- 完整 Gameplay Tick 稳定低于 33.333 ms，或明确剩余成本来自复制阶段；
- 10024 三次压测不再出现持续多 Tick 追赶；
- 网络包格式、频率、成员版本和客户端表现保持一致；
- 文档记录 AOT 前后 Tick、GC、消息数和带宽变化。

### 10.9 后续顺序

EntityTick 完成后再处理 `Monster.Replication.Publish`：先让本地表现帧和群组快照复用同一份量化状态，再单独评估 `SendUnnamedMessage` 的消息合批和固定 GC。这样可以分别确认模拟、复制和底层网络的收益来源。

### 10.10 EntityTick 首轮实施结果

本阶段已经完成最终态切换：

- 新增 AOT `MonsterSimulationKernel`，统一处理 Target、Attack、Navigation、Separation、Constraint 和 Integrate；
- 怪物状态数据迁移到 AOT 共享值类型；
- `MonsterRuntimeCatalog` 生成 AOT 可直接读取的连续配置数组；
- 近战请求改为预分配 `MonsterAttackRequestData[]`，Unity Physics 仍留在 HotFix；
- 删除 HotFix 逐 Slot EntityTick 循环和 `MonsterMeleeAttackRequest` 对象路径；
- 保留现有网络包格式、群组频率和客户端协议。

同一 Player、同一 Catalog 的 10024 怪轻量压测 `20261118-163045-a7772310` 三次均有效：

| 指标 | 重复 1 | 重复 2 | 重复 3 |
|---|---:|---:|---:|
| Host 平均帧时 | 21.912 ms | 19.556 ms | 20.079 ms |
| `EntityTick` 平均每帧 | 2.429 ms | 2.165 ms | 2.211 ms |
| `WorldSimulation` 平均每帧 | 4.617 ms | 4.196 ms | 4.458 ms |
| `Gameplay.TickDriver.Dispatch` 平均每帧 | 10.863 ms | 9.769 ms | 10.262 ms |
| Client 平均帧时 | 16.952 ms | 16.358 ms | 16.262 ms |

迁移前同口径 AOT 群组压测的正常 Host 平均帧时约 39.7～41.0 ms，`EntityTick` 约 16.7～17.3 ms/帧；迁移后 Host 降至约 19.6～21.9 ms，`EntityTick` 降至约 2.2～2.4 ms/帧。三轮 Tick/渲染帧比例已回到约 1.0，未出现此前的多 Tick 追赶放大。

网络消息数仍约 1238～1265 条/s，带宽仍约 577～618 KiB/s，说明这次收益来自模拟热循环下沉，而不是减少同步内容。下一阶段转入 `Replication.Publish` 的量化状态复用；`SendUnnamedMessage` 的固定 GC 放在表现复制优化之后处理。

## 11. 下一阶段具体修改计划：Replication Publish 量化状态复用

### 11.1 阶段目标

EntityTick AOT 后，Host 已经稳定回到一 Tick/帧区间，下一阶段处理复制阶段中的重复量化和重复状态读取。当前同一个 Replication Tick 可能分别执行：

```text
MonsterRuntimeService.PublishAuthorityState
├─ CaptureFrame
│  └─ 遍历 SlotCount，Pack(position, yaw, events)
└─ PublishRemoteGroups
   └─ CaptureGroupFrame（每个到期组）
      └─ 再次 Pack(position, yaw, events)
```

本阶段目标是：每个 Replication Tick 对每个活跃 Slot 最多执行一次量化 Pack，Host 本地完整帧和远端群组快照共同读取这份结果。网络协议、成员表、Inner/Middle/Outer 频率和批次预算保持不变。

### 11.2 当前重复点与数据时序

当前 `CaptureFrame` 在 `Replication` 阶段先执行，随后 `PublishRemoteGroups` 才刷新群组和采集群组状态。怪物伤害在 `CombatSimulation` 阶段已经完成，因此复制阶段读取的是同一份最终状态；但当前两个 Capture 路径各自重新读取 `Motion`、`Presentation` 并调用 `MonsterPoseQuantizer.Pack`。

实现时必须以复制阶段开始时的当前活跃表为准。不能直接复用 Tick 开始时的 `MonsterOccupancySnapshot.ActiveSlots`，因为本 Tick 的 CombatSimulation 可能已经击杀怪物。为此先让 `MonsterWorld` 维护房间级 `ActiveSlots` 和 `ActiveCount`，在 Create/Kill 时同步更新，复制阶段直接消费这份权威活跃列表。

### 11.3 目标数据布局

在房间级维护按 Slot 索引的预分配缓存：

```text
packedStateBySlot[slot] : MonsterPackedState
activeSlots[]           : 当前活跃 Slot
activeCount             : 当前活跃数量
```

`packedStateBySlot` 只在一次 Pack 阶段写入；它不是网络包，也不改变组内成员顺序。群组仍然通过 `memberSlots[i]` 读取 `packedStateBySlot[memberSlots[i]]`，所以 MembershipRevision 和客户端映射语义保持不变。

### 11.4 AOT Pack Kernel

新增纯数据 `MonsterPresentationPackKernel`，输入：

- `activeSlots`、`activeCount`；
- `MonsterMotionData[]`；
- `MonsterPresentationData[]`；
- 量化原点和尺寸；

输出：

- `MonsterPackedState[] packedStateBySlot`。

量化器的原点、尺寸和 Pack 数学逻辑下沉为 AOT 值函数，移除每个 Slot 调用 HotFix `MonsterPoseQuantizer.Pack` 的对象边界。Pack 阶段只执行一次有限性和房间范围假设下的纯数学运算，不创建临时集合。

建议新增文件：

- `Assets/_AOT/Scripts/Core/Monsters/MonsterPresentationPackKernel.cs`；
- `Assets/_AOT/Scripts/Core/Monsters/MonsterPresentationData.cs`（若共享数据需要从表现程序集进一步拆分）。

`MonsterPackedState` 可移动到 AOT 共享数据程序集，Codec、Host Capture 和 Client Apply 共同引用同一个值类型。

### 11.5 Host Capture 改造

修改 `MonsterRoomRuntime` 和 `MonsterPresentationPackingSystem`：

1. 在 `PublishAuthorityState` 进入本地 Capture 或远端群组发布前，执行一次 `PackActive`；
2. `CaptureFrame` 不再调用 `quantizer.Pack`，只根据 `activeSlots` 写 ActiveMask 和 dense ActiveState；
3. `CaptureGroupInto` 不再调用 `quantizer.Pack`，按成员表顺序复制 `packedStateBySlot`；
4. 本地没有到期完整帧且没有远端客户端时，不执行 Pack；
5. 本地完整帧和群组快照需要同一 Tick 状态时，复用同一个缓存，不重复 Pack。

新增 Marker：

```text
Monster.Replication.PackActive
Monster.Presentation.Capture.CopyLoop
Monster.Presentation.GroupCapture.CopyLoop
```

保留现有 `Capture.StateLoop` 和 `GroupCapture.StateLoop`，用于对比迁移前后的“量化循环”与“缓存复制循环”成本。

### 11.6 Client Apply 的后续边界

客户端收到网络包后仍必须执行 Unpack，因为网络数据是新的量化状态。本阶段先不改变客户端插值语义，只保留现有：

- 完整帧的 ActiveMask、Previous/Current 姿态更新；
- 群组快照的 MembershipRevision、Tick 顺序和组内插值；
- AttackSequence、HitSequence 的离散事件更新。

如果 Host Capture 复用完成后 Client `GroupApply` 仍成为主要成本，再单独把 `ApplyPose` 和 GroupApply 循环迁移到 AOT 批处理。不要把 Host Pack 复用和客户端插值重写混在同一轮验证中。

### 11.7 不改动的内容

- 不修改 `MonsterGroupSnapshot` 包体结构；
- 不修改 MembershipRevision、ServerTick 和 IntervalTicks；
- 不修改 1024 B 单批次上限和 32 KiB Tick 预算；
- 不在本阶段引入增量状态包；
- 不先合并所有网络消息；
- 不把 `SendUnnamedMessage` 的 126 B/次 GC 与 Pack 优化混合判断。

### 11.8 验收指标

使用同一 Player、同一 Catalog、10024 怪、同一路线、三次轻量压测，比较：

- `Monster.Presentation.Capture.StateLoop`；
- `Monster.Network.GroupSchedule.Capture`；
- 新增 `PackActive` 与 CopyLoop；
- `Monster.Replication.Publish` 和 `PublishGroups`；
- Host 平均/P95、Tick/渲染帧比例；
- GC Alloc、网络消息数、Payload 和带宽；
- Host/Client 存活数、Replica/View 数、Source FlowField 重建。

阶段完成条件：

- 每个 Replication Tick 每个活跃 Slot 只发生一次 Pack；
- 本地完整帧与群组快照的姿态、事件序列和 Tick 结果保持一致；
- `Capture.StateLoop` 与 `GroupCapture.StateLoop` 的量化成本被 CopyLoop 替代，并出现可重复下降；
- 网络协议和客户端表现不变；
- `SendUnnamedMessage` GC 仍单独记录，不把它误判为 Pack 阶段收益。

### 11.9 后续顺序

量化状态复用完成后，再选择以下一项：

1. 若 Host 复制仍是主要 CPU，迁移 Client/Host Apply 与 Codec Pack/Unpack 到 AOT 批处理；
2. 若 CPU 已足够，单独减少发送调用次数，验证 `SendUnnamedMessage` 的 126 B/次 GC；
3. 只有在实测证明连续快照带宽仍是瓶颈时，才讨论增量状态包。

### 11.10 首轮实施结果

量化状态复用已完成：

- `MonsterWorld` 维护按 Slot 排序的 `ActiveSlots`，CombatSimulation 后仍能反映当前活跃状态；
- 新增 AOT `MonsterPresentationPackKernel`；
- 每个复制 Tick 先执行一次 `PackActive`，输出 `packedStateBySlot`；
- 本地完整帧改为 ActiveSlots + 缓存复制；
- 群组快照改为成员表 + 缓存复制；
- `MonsterPackedState` 和量化数据进入 AOT 共享值类型；
- 网络包格式、群组频率和客户端 Apply 未改变。

10024 怪轻量压测 `20261118-170147-08a9005e` 三次 Host/Client 全部有效：

| 指标 | 重复 1 | 重复 2 | 重复 3 |
|---|---:|---:|---:|
| Host 平均帧时 | 16.163 ms | 20.022 ms | 18.800 ms |
| `PackActive` 平均每帧 | 0.108 ms | 0.148 ms | 0.126 ms |
| `Capture.CopyLoop` 平均每帧 | 0.147 ms | 0.191 ms | 0.171 ms |
| `GroupCapture.CopyLoop` 平均每帧 | 0.046 ms | 0.069 ms | 0.055 ms |
| `PublishGroups` 平均每帧 | 0.571 ms | 0.804 ms | 0.672 ms |
| Client 平均帧时 | 17.260 ms | 16.131 ms | 17.346 ms |

迁移前 `GroupCapture` 约 1.1～1.25 ms/帧，本轮实际成员复制约 0.05～0.08 ms/帧；`PublishGroups` 从约 2.4～2.6 ms/帧降到约 0.57～0.80 ms/帧。网络发送仍约 570～585 KiB/s、1246～1261 条消息/s，说明只减少了重复量化和状态读取，没有改变网络内容。

三轮 Host 都保持 10024 存活、Client Replica/View 完整，Tick/渲染帧比例稳定，没有重新出现 Tick backlog。下一阶段进入 Client Apply 或单独验证 `SendUnnamedMessage` 固定 GC，二者不再与 Host Pack 混合修改。
