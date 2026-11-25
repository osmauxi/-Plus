# 怪物三阶段执行后端与可变 Slot 迁移方案

更新时间：2026-11-23

## 1. 文档目标

当前怪物系统已经完成面向对象 SoA 阶段，下一步计划进入 Job/Burst，再评估 Pure ECS。割草尸潮玩法还提出了一个更底层的需求，怪物需要在同一个房间内持续生成和死亡，数组容量不能随着历史生成总数无限增长。

本文件同时固定两件事：

1. 三种执行后端的边界和切换方式；
2. 怪物身份与物理存储位置分离后的可变 Slot 方案。

可变 Slot 需要在 Job/Burst 之前完成基础迁移。Job/Burst 和 ECS 都依赖稳定、明确的实体身份与连续存储边界。

## 2. 三阶段执行后端

三阶段比较的是数据存储和执行方式。玩法规则、网络协议、表现事件和基准测试保持同一套语义。

```text
Managed SoA
    托管数组 + 主线程循环

Manual Native SoA
    NativeArray + 手写 Job + Burst

Entities ECS
    Entity/Component/Chunk + ECS System + Job/Burst
```

### 2.1 Managed SoA

这是当前基线实现：

- `MonsterWorld` 保存怪物各字段数组；
- `MonsterSimulation` 负责房间级编排；
- `MonsterSimulationKernel` 执行主线程数据循环；
- `MonsterNetworkGroupKernel` 负责网络群组；
- `MonsterSpatialIndex` 负责四叉树查询；
- VAT、网络编码和表现事件使用现有协议。

Managed SoA 的结果用于验证后续两种后端的行为一致性。

### 2.2 Manual Native SoA

这一阶段保留当前房间生命周期和业务边界：

- 纯数据计算使用持久化 `NativeArray`；
- Job 输入和输出采用固定布局值类型；
- Burst 只处理不依赖托管对象和 Unity 主线程 API 的计算；
- Unity Physics、伤害提交、Effect、网络和表现事件继续在主线程完成；
- 主线程负责 Job 依赖、结果回写和结构变化。

Job/Burst 阶段可以大量复用当前 SoA 的字段布局、规则函数、网络编码和表现打包。

### 2.3 Entities ECS

Pure ECS 采用独立的 ECS World 和 Component 存储：

- 怪物状态放入 Entity Component；
- 怪物系统使用 `ISystem`、`IJobEntity` 或 `IJobChunk`；
- ECS Chunk 提供连续组件存储；
- ECS System 负责系统依赖和调度；
- 计算通常仍使用 Burst；
- 网络和 VAT 表现通过适配层读取 ECS 输出。

ECS 可以和 Job/Burst 一起使用。三阶段的区别是数据组织方式和系统生命周期，而不是是否使用 Burst。

当前工程的 `Packages/manifest.json` 尚未显式引入 Unity Entities。进入 ECS 阶段时需要单独增加 Entities 相关包、程序集和测试入口。

## 3. 策略模式边界

策略选择放在房间级别，房间创建时确定一次。每个怪物循环中不执行接口分派，也不在 Tick 内反复判断当前模式。

```text
MonsterRoomRuntime
        │
        ▼
MonsterSimulationBackend
        ├── ManagedSoABackend
        ├── ManualNativeSoABurstBackend
        └── EntitiesEcsBackend
```

策略共享两个边界结构：

```text
MonsterSimulationTickInput
    玩家目标
    活动实体
    FlowField 数据
    怪物配置
    Tick 和 DeltaTime

MonsterSimulationTickOutput
    运动结果
    攻击请求
    死亡结果
    表现事件
    网络状态变化
```

共享规则包括目标选择、移动约束、攻击条件、伤害结果、量化协议和表现事件语义。内部数组、Chunk 查询、Job 组织和实体生命周期由各后端自行管理。

## 4. 当前 Slot 模型

当前 `MonsterWorld` 把 Slot 同时当成三种东西：

1. 怪物在房间内的逻辑身份；
2. SoA 数组索引；
3. 网络、弹丸命中和 VAT 表现使用的索引。

当前行为如下：

- `Create` 只把 `_slotCount` 递增后追加到数组尾部；
- `Kill` 将 `IsActive` 设为 `false`；
- `_activeSlots` 删除死亡 Slot 时会把后续活动 Slot 向前移动；
- `_slotCount` 只会在房间结束时清零；
- `MonsterSpatialIndex`、群组内核、表现打包、Replica、VAT 和子弹命中都使用 Slot；
- `MonsterEntityIdBase + slot` 直接作为目标实体 ID。

这个模型让现有网络和表现逻辑简单可靠，但历史生成数量会决定数组高水位。死亡怪物越多，后续全量扫描越容易扫描大量无效 Slot。

## 5. 直接交换整行数据的风险

如果死亡 Slot 直接与最后一个数组元素交换，需要同时交换：

- `Meta`；
- `Motion`；
- `Health`；
- `Status`；
- `Attack`；
- `Presentation`；
- 目标解析缓存；
- 分离力缓存；
- 占用快照缓存；
- 四叉树的 Slot 链接；
- 群组的 `groupBySlot` 和 `slotIndexInGroup`；
- 量化状态缓存；
- VAT 实例状态；
- 客户端 Replica 和网络成员表；
- 弹丸碰撞结果和待处理 Effect 中的目标引用。

当前代码中 Slot 仍然是网络身份，因此移动数组数据会让同一个 Slot 在前后两个时刻代表不同怪物。弹丸候选列表、待处理攻击、网络旧包和群组成员表都可能因此命中错误目标。

死亡处理发生在弹丸碰撞和 Effect 结算链路中。碰撞循环尚未结束时立即交换整行数据，会让同一个 Tick 的后续候选结果失去原有语义。

因此当前阶段不直接把死亡 Slot 和数组最后一项交换。

## 6. 推荐的最终模型

把逻辑身份和物理存储位置分开：

```text
MonsterEntityId
    房间内唯一身份，生成后不复用

StorageIndex
    当前在 SoA 或 ECS 存储中的物理位置，可以复用和交换
```

核心映射为：

```text
storageIndex -> MonsterEntityId
MonsterEntityId -> storageIndex
```

死亡时执行以下流程：

1. 使用 `MonsterEntityId` 记录事件和网络目标；
2. 立即标记当前 StorageIndex 为死亡，后续碰撞逻辑不再接受该实体；
3. 当前 Tick 的碰撞、Effect 和攻击提交结束；
4. 在 Tick 边界执行最后一个活动 StorageIndex 到死亡位置的 SwapBack；
5. 更新两个映射表和所有存储索引表；
6. 重新提交空间索引和网络群组；
7. 对外发送稳定的 `MonsterEntityId`，不发送内部 StorageIndex。

新生成的怪物获得新的 `MonsterEntityId`，复用已经空出的 StorageIndex。这样数组容量由同时存活数量决定，物理数据可以连续，网络身份也不会因为交换而改变。

## 7. 分阶段迁移顺序

### 7.1 第一阶段，先修复活动列表删除成本

当前 `MonsterWorld.Kill` 删除 `_activeSlots` 时会移动整个尾部区间。第一步只把它改为活动列表的 SwapBack：

```text
deadActiveIndex = activeIndexByStorage[deadStorage]
lastStorage = activeSlots[aliveCount - 1]
activeSlots[deadActiveIndex] = lastStorage
activeIndexByStorage[lastStorage] = deadActiveIndex
aliveCount--
```

这一步不改变外部 Slot 身份，不改变网络协议，也不移动 SoA 数据。它是低风险的 O(1) 死亡处理优化，应最先完成。

### 7.2 第二阶段，引入实体身份结构

增加明确的实体身份：

- `MonsterEntityId`；
- `StorageIndex`；
- `storageIndexByEntityId`；
- `entityIdByStorageIndex`。

这一阶段先保持数据仍按原 Slot 存储，只更新子弹目标、攻击事件、死亡事件和网络身份的使用方式。等身份边界稳定后再进行物理数组压缩。

### 7.3 第三阶段，建立密集存储

将 `MonsterWorld` 的数组改为按 StorageIndex 保存。活动实体直接使用连续的 StorageIndex 范围，死亡时在 Tick 末尾执行 SwapBack。

需要同步迁移：

- `MonsterSimulationKernel`；
- `MonsterOccupancySnapshotKernel`；
- `MonsterSpatialIndex` 和四叉树 Slot 链；
- `MonsterNetworkGroupKernel`；
- `_packedStateBySlot`；
- `MonsterPresentationPackingSystem`；
- `MonsterReplica`；
- `MonsterVATRenderWorld`；
- `ProjectileSimulation` 和 `WeaponRuntimeService` 的目标引用。

### 7.4 第四阶段，调整网络和表现身份

群组成员表发送 `MonsterEntityId`。群组姿态包继续使用成员表顺序，不需要在每个姿态状态中重复发送 EntityId。

客户端收到成员表后建立：

```text
MonsterEntityId -> ViewStorageIndex
```

客户端 VAT 数组同样使用密集 ViewStorageIndex。死亡和新生成只更新映射与活动范围，不再要求整个房间 SlotCount 单调增长。

### 7.5 第五阶段，接入三种执行后端

完成身份和密集存储后，再接入：

1. Managed SoA 后端；
2. Manual Native SoA + Job/Burst 后端；
3. Entities ECS + Job/Burst 后端。

三种后端都输出同一套 EntityId 事件和网络表现结果。

## 8. 网络协议注意事项

当前群组成员表发送的是 Slot。迁移后需要发送 EntityId，成员表版本仍然负责判断成员集合是否一致。

姿态快照可以继续只发送量化状态，因为接收端已经通过成员表获得了 EntityId 顺序。这样不会在每个姿态状态中增加重复身份字段。

新生成实体不能复用旧 EntityId。网络旧包、群组旧快照和延迟到达的命中事件都必须能通过 EntityId 判断是否仍属于当前实体。

如果未来需要有限宽度的 EntityId，可以使用房间代次和实体序号组合，但当前阶段优先使用单调递增的 32 位或 64 位 ID，先保证语义正确。

## 9. 对性能的预期影响

### 9.1 当前全部存活时

10024 怪全部存活时，密集存储不会自动减少计算量。活动数量和物理存储数量相同，主要收益来自：

- 死亡删除从 O(N) 变为 O(1)；
- 后续新增不再触发历史 Slot 高水位增长；
- 四叉树、占用快照和群组刷新可以按活动存储范围工作。

### 9.2 大量死亡后

当房间内同时存活数量明显低于历史生成数量时，收益会更加明显：

- 占用快照不再扫描大量死亡 Slot；
- 网络群组刷新不再检查历史死亡 Slot；
- 四叉树重建不再遍历无效尾部；
- 表现层和 Replica 不再维护不断增长的 Slot 数组；
- 连续 StorageIndex 更适合后续 Job/Burst 和 ECS Chunk。

### 9.3 迁移期间的成本

迁移初期会增加：

- EntityId 与 StorageIndex 的映射访问；
- SwapBack 时的多数组行复制；
- 网络成员表身份转换；
- 客户端 EntityId 到 ViewStorageIndex 的查找。

在 10024 怪全部存活、死亡数量很少的场景中，预期收益有限，映射成本可能抵消部分收益。该方案的主要价值在高频生成、持续死亡和大量历史 Slot 的割草尸潮场景。

## 10. 验收指标

每个迁移阶段都需要验证：

- 10024 怪全存活时平均帧时和 P95 不回退；
- 同 Tick 多个怪物死亡时没有错误命中；
- 弹丸、范围 Effect、闪电和近战攻击不会作用到新实体；
- 群组成员表版本和姿态顺序保持一致；
- Host、Client 的 EntityId、Replica 和 VAT 数量一致；
- 连续生成和死亡后数组容量不再随历史生成总数线性增长；
- 四叉树查询、群组刷新和表现打包只处理当前活动存储范围；
- Managed SoA、Job/Burst 和 ECS 后端输出一致。

## 11. 最终建议

当前最适合的路线是：

1. 先把 `_activeSlots` 删除改为 O(1) SwapBack；
2. 立即建立 `MonsterEntityId` 与 `StorageIndex` 的概念边界；
3. 在 Tick 结束点执行死亡实体的密集存储 SwapBack；
4. 网络和弹丸事件使用 EntityId，内部计算使用 StorageIndex；
5. 完成这层基础改动后，再开始 Manual Native SoA + Job/Burst；
6. 最后建立 Entities ECS 后端，并继续复用相同的 EntityId、网络和表现输出协议。

这个顺序可以同时解决割草尸潮的 Slot 高水位问题，并为后续 Job/Burst 和 ECS 提供统一的实体身份与连续数据基础。

## 12. 外部身份索引不会自动减少遍历

`MonsterEntityId -> StorageIndex` 解决的是身份稳定和随机访问。它本身不会减少任何全量循环。

当前代码中已经存在两类不同的循环：

### 12.1 已经按活动列表处理的循环

怪物模拟内核和姿态打包已经使用 `ActiveSlots`。这类循环的工作量接近当前存活数量，Slot 稳定性不是它们的主要问题。

```text
for activeIndex in ActiveCount
    storage = ActiveSlots[activeIndex]
    读取 storage 对应的 SoA 状态
```

引入密集 StorageIndex 后，这类循环可以直接变为：

```text
for storageIndex in 0 .. AliveCount
    读取连续的 SoA 行
```

循环内不需要再次判断 `IsActive`。

### 12.2 当前仍然扫描 SlotCount 的循环

这些路径才是 Slot 高水位会影响的地方：

- `MonsterOccupancySnapshotKernel.Build` 扫描 `slotCount` 并判断 `meta[slot].IsActive`；
- `MonsterNetworkGroupKernel.Refresh` 扫描 `slotCount` 并判断成员是否活跃；
- `MonsterSpatialIndex.EnsureBuilt` 扫描 `world.SlotCount`；
- `MonsterPresentation.Apply` 扫描客户端的整个 `SlotCount`；
- `MonsterViewWorld.SyncReplica` 扫描整个 `SlotCount`；
- `MonsterVATRenderWorld.Update` 扫描 `_slotCount` 并跳过非活动实例。

如果只添加外部索引而保留这些循环，它们仍然会遍历历史 Slot，并在每一项上做活跃状态判断。真正的优化需要把这些接口改为以 `AliveCount` 或密集成员表为输入。

### 12.3 服务器状态应用

密集存储后的服务器模拟过程如下：

```text
for storageIndex = 0 .. AliveCount - 1
    EntityId = entityIdByStorageIndex[storageIndex]
    直接读取 Meta/ Motion/ Status/ Attack 的连续行
    计算并写回当前行
```

死亡实体在当前 Tick 结束前仍然保留自己的 StorageIndex，但会被标记为待移除。当前 Tick 的碰撞和 Effect 结算完成后，执行：

```text
deadIndex = pendingDeathIndex
lastIndex = AliveCount - 1
movedEntityId = entityIdByStorageIndex[lastIndex]
复制 lastIndex 的全部 SoA 字段到 deadIndex
entityIdByStorageIndex[deadIndex] = movedEntityId
storageIndexByEntityId[movedEntityId] = deadIndex
AliveCount--
```

下一 Tick 从 `0 .. AliveCount - 1` 处理，已经不需要扫描死亡行。

### 12.4 随机事件访问

弹丸命中、范围 Effect、近战请求和网络接收属于随机访问路径。它们先使用 EntityId 查找 StorageIndex：

```text
storageIndex = storageIndexByEntityId[entityId]
读取或修改对应的 SoA 行
```

这个查找只发生在事件边界，不放入每只怪物的主循环。SwapBack 后只需要更新被移动实体的映射，外部 EntityId 不变化。

### 12.5 网络和客户端状态应用

群组成员表对外保存 EntityId。姿态快照继续按成员表顺序传输，因此不需要在每个姿态状态中重复发送 EntityId。

客户端收到成员表后建立 `EntityId -> ViewStorageIndex`。应用群组姿态时只遍历本群组成员数量：

```text
for memberIndex = 0 .. groupMemberCount - 1
    entityId = groupEntityIds[memberIndex]
    viewIndex = viewStorageIndexByEntityId[entityId]
    ApplyPose(viewIndex, states[memberIndex])
```

完整帧也需要从当前的 Slot 掩码模型改成活动实体列表或密集状态列表。继续保留按历史 Slot 扫描的完整帧格式，会让客户端仍然承担高水位遍历。

### 12.6 对性能的实际含义

- 10024 怪全部存活时，`AliveCount` 和当前 `SlotCount` 接近，密集化主要带来更少的死亡处理和更连续的内存访问；
- 大量死亡后，`AliveCount` 明显低于历史 `SlotCount` 时，占用快照、空间索引、网络群组和客户端表现循环才会产生明显下降；
- EntityId 查找会给随机命中增加一次 O(1) 映射访问；
- 密集主循环不再执行每项 `IsActive` 判断；
- SwapBack 只在死亡实体提交阶段执行，不会增加每 Tick 的固定复制成本；
- 仍然可以保留 `ActiveSlots` 作为过渡结构，直到所有 SlotCount 循环完成密集化。

所以外部索引层是身份正确性的基础，密集 StorageIndex 和按 AliveCount 改造循环才是遍历优化本身。两层需要一起迁移，但可以分阶段完成。

## 13. 第一阶段实现结果

`MonsterWorld.Kill` 已将 `_activeSlots` 的死亡删除从尾部整体前移改为 O(1) SwapBack：

- 读取死亡实体在活动列表中的位置；
- 取活动列表最后一项填入死亡位置；
- 更新被移动实体的 `_activeIndexBySlot`；
- 将死亡实体的索引标记为 `-1`；
- 减少 `_aliveCount`。

本阶段只改变活动列表的排列，不移动 `Meta`、`Motion`、`Health`、`Status`、`Attack` 和 `Presentation` 数组中的实体数据。Slot 身份、网络协议、四叉树索引、群组成员表和 VAT 索引保持原有语义。

因此这一阶段的收益集中在死亡批次处理，尤其是同一 Tick 大量怪物死亡时。后续仍需单独完成 EntityId 与 StorageIndex 分离，才能进一步压缩历史 Slot 高水位和全量扫描范围。

## 14. 第二阶段实现结果，接入身份查询链路

当前已经在 `MonsterWorld` 增加按 Slot 保存的 EntityId 表和 EntityId 到 Slot 的查询表。现阶段 EntityId 仍使用与旧协议兼容的 `MonsterEntityIdBase + Slot` 生成方式，底层数组位置没有移动，网络包格式也没有改变。

以下链路已经停止直接计算 `MonsterEntityIdBase + slot`：

- 弹丸命中目标和穿透命中集合；
- 弹丸窄相候选目标过滤；
- 闪电跳跃目标；
- 执行 Effect；
- Crowd Control 目标；
- 执行特效的目标身份。

这些路径现在统一通过 `MonsterRuntimeService.GetMonsterEntityId` 和 `ResolveMonsterSlot` 访问身份层。后续底层 StorageIndex 发生交换时，武器模块不需要重新修改目标身份逻辑。

本阶段的映射表仍然保留死亡实体的身份到房间结束，保证当前 Tick 内排队的 Effect 和延迟结算仍能解析到原 Slot，并由 `IsActive` 判断目标是否已经死亡。进入密集 StorageIndex 阶段后，映射删除和死亡提交会移动到 Tick 边界统一处理。

Unity 编译、工程编译和 HotFix DLL 同步均已通过。下一阶段需要把 EntityId 映射继续接入底层 StorageIndex 的 SwapBack。

## 15. 第三阶段实现结果，生成与群组网络接入 EntityId

本轮把稳定身份继续接入怪物网络层，网络包不再把本地存储 Slot 当成远端身份。

### 15.1 SpawnBatch

`MonsterSpawnData` 现在显式携带 `EntityId`、`ConfigIndex` 和量化姿态。`MonsterSpawnBatch` 不再保存 `StartSlot`。客户端表现槽位从本地当前计数继续追加，网络身份由批次字段建立映射。

这样即使服务器后续因为 SwapBack 改变底层存储位置，生成包仍然只描述哪只怪物出生，不会把服务器数组下标传播给客户端。

### 15.2 GroupMembership

服务器群组内核仍然用 Slot 维护空间分组和姿态采集，因为这些是权威端的本地数组访问。发送成员表时，房间运行时把成员 Slot 转换为 `EntityId`，网络编码和客户端缓存都使用身份。

客户端收到群组成员表后，通过 `EntityId -> 本地表现 Slot` 字典解析姿态写入目标数组。群组快照仍然只传输按成员表顺序排列的量化状态，因此快照包的成员状态布局没有变化。

### 15.3 Despawn

死亡通知现在传输 `EntityId`。客户端先用身份表解析本地 Slot，再关闭对应的表现状态。生成包尚未到达时收到群组或死亡通知，身份查找失败会跳过当前成员，后续可靠包到达后由新的状态继续建立。

### 15.4 当前边界

上一阶段只替换了跨网络的身份语义，没有移动底层 SoA 数据。服务器模拟、四叉树和群组内核仍以 Slot 访问数组，客户端表现数组暂时仍按生成顺序追加。本阶段已经把服务器 StorageIndex 的 SwapBack 接入身份映射和发布前的索引重建。

## 16. 第四阶段实现结果，服务器密集 StorageIndex

本轮完成了服务器端所有 SoA 行的密集化。`MonsterWorld` 的数组有效区现在始终是 `0 .. AliveCount - 1`，这个整数只代表当前 Tick 内部的 `StorageIndex`。

### 16.1 死亡提交时机

战斗阶段只把死亡行标记为 inactive，并记录待移除的 StorageIndex。这样同一个 Tick 内的弹丸穿透、范围 Effect、闪电链和近战请求仍然可以使用本 Tick 开始时的索引，不会因为中途搬行让后续事件指向另一只怪。

Replication 阶段开始前统一调用 `CompactDead`。它从密集区尾部寻找仍然存活的行，复制整行 `Meta`、`Motion`、`Health`、`Status`、`Attack`、`Presentation` 和 EntityId 到死亡位置，然后更新被移动实体的身份映射。

### 16.2 索引重建

发生搬行后，四叉树和网络群组都被清空。下一次四叉树查询按新的密集 StorageIndex 重建，下一次群组刷新按新的密集 StorageIndex 重新分组。这样不需要在每个 SwapBack 分支里同时修补多个空间结构。

### 16.3 本地完整表现帧

本地完整表现帧不再使用历史 Slot 的活动掩码。服务器按密集顺序写入 `EntityId` 和 `MonsterPackedState`，Host Replica 先把 EntityId 解析成自己的表现槽位，再应用姿态。客户端表现槽位仍可保持自己的生命周期顺序，因此服务器搬行不会改变客户端 VAT 实例的身份。

### 16.4 当前可观察结果

- 主循环和空间索引的有效范围由存活数量决定；
- 死亡数量增加后不会继续抬高服务器数组遍历上限；
- EntityId 在搬行前后不变；
- 武器碰撞、范围 Effect、群组快照和本地表现帧继续通过身份解析或当前 Tick 的 StorageIndex 工作；
- 搬行成本集中在死亡提交阶段，存活期间没有额外复制。

占用快照、网络群组刷新和四叉树重建已经按密集区直接遍历，不再为每一行重复判断 `IsActive`。死亡行只会在战斗阶段被空间索引主动移除，提交压缩后下一次构建的有效区全部是活跃实体。

本轮使用 Unity Editor 内存验证创建 4 只怪物，击杀第 2 只后执行 CompactDead。结果为存活数量 3，尾部 StorageIndex 失效，原尾部实体移动到第 2 行，身份查询返回新的 StorageIndex 1。

## 17. 第五阶段实现结果，完整收口 Slot 语义

本轮把剩余调用链按数据所属层次完成了命名迁移。服务器权威数据中的整数索引统一称为 `StorageIndex`，客户端表现数据中的整数索引统一称为 `ViewSlot`。

服务器侧的 `MonsterWorld.StorageCount`、`ActiveStorageIndices` 和 `ResolveStorageIndex` 明确表达当前密集 SoA 行。空间索引、占用快照、分组内核、寻路目标、姿态打包、近战请求、弹丸候选、范围效果和武器目标解析都使用 `StorageIndex` 命名。网络协议继续只携带 `EntityId`，因此命名迁移没有把物理索引泄露回网络层。

客户端 Replica 和 VAT 渲染世界使用 `ViewSlotCount`、`ViewSlot`。ViewSlot 是客户端自己的表现数组位置，服务端 SwapBack 不会改变它。完整帧先按 `EntityId` 找到 ViewSlot，再把量化姿态写入本地表现数组。

`MonsterRoomRuntime.Spawn` 在创建新实体前执行一次 `CompactDead`。如果上一轮战斗产生了待回收行，先完成密集化并清空空间索引和网络群组，再从新的 `StorageCount` 尾部追加新实体。这样分批生成不会把死亡行重新夹回有效区。

当前代码中仍保留的 `slot` 文本只属于无关 UI 槽位或历史说明，不再作为怪物服务器索引 API 的名称。VAT 和 Replica 的局部数组也不再被称为服务器 Slot。

## 18. 批量死亡压缩修正与链路烟测

本轮链路烟测发现，原先按待死亡索引逐个从尾部交换的实现，在同一批次同时包含中间行和尾行时，会先缩短有效区，再跳过已经落到有效区外的尾行记录，导致 `StorageCount` 多保留一行死亡数据。

`MonsterWorld.CompactDead` 现改为两步：

1. 先为本批次所有死亡实体清除 `EntityId -> StorageIndex` 映射；
2. 从旧有效区头到尾扫描，把活跃行按读写指针连续写回前缀，并同步更新被移动实体的身份映射。

该实现仍然只在存在待提交死亡时运行。没有死亡时不会扫描；提交时最多扫描一次旧有效区，且不会把本 Tick 中途的索引变化暴露给战斗请求。

已在 Unity Editor 中完成以下验证：

- 中间行与尾行同时死亡，压缩后存活数量、行顺序和两条死亡身份映射正确；
- 压缩后追加新实体，新实体从当前 `StorageCount` 继续写入且获得新的 EntityId；
- 压缩后再次死亡并搬移多行，活跃实体的身份映射与密集行同步；
- 全部实体死亡后压缩到 0，再生成实体复用行 0，旧 EntityId 不会重新解析。
- 压缩后清空并重建网络群组，群组成员不再保留旧的 StorageIndex。

## 19. 9000 怪物三次压测验收

RunId：`20261123-155220-16ba2ce8`

三次 Host 和三次 Client 采样全部有效，前台占比均为 100%，双端时间对齐均在 100 ms 内。三次运行的权威存活数均为 9000，客户端 Replica 和 ViewSlot 均为 `9000/9000`，没有出现实体数量丢失或表现槽位缺失。

Host 三次平均帧间隔为 `25.483 ms`、`22.713 ms`、`28.221 ms`，Client 三次平均帧间隔为 `17.224 ms`、`16.236 ms`、`18.996 ms`。`Monster.Replication.Publish` 的三次 Host 平均每次耗时约 `5.35 ms`，网络发送速率约 `942～959 KiB/s`，三次结果稳定。

这组压测确认 StorageIndex 和 ViewSlot 的稳态调用链已经通过。批量死亡、SwapBack、EntityId 重映射、重新生成和群组重建已经通过 Unity Editor 定向烟测。9000 怪物压测本身没有产生死亡，因此不把死亡复用场景混入稳态帧时结论。

当前 Slot 重构阶段标记为通过，可以进入下一阶段工作。

## 20. 三阶段策略模式第一步

本轮把模拟执行后端从房间编排中抽出固定策略边界。

### 20.1 策略接口

`IMonsterSimulationBackend` 只包含一次模拟 Tick 所需的计算能力。房间层通过 `MonsterSimulationTickInput` 组织当前 Tick 的数据引用，策略返回 `MonsterSimulationTickOutput`：

- 输入包含已经完成导航格缓存的 `MonsterWorld`、玩家目标、Occupancy 快照、Tick、DeltaTime 和 GroundY；
- 运动与状态结果仍然写回 `MonsterWorld` 的 SoA 数组；
- 输出只携带跨阶段消费的近战请求数组和请求数量；
- 近战阶段由 `MonsterRoomRuntime` 消费输出，并统一调用 `MonsterMeleePhysicsResolver`；
- 提供寻路调试数据和 Separation 查询；
- 接收房间生命周期的容量扩展和诊断重置。

这两个结构是无分配的只读值类型。房间层在构造输入前完成玩家导航格缓存和占用快照，策略只消费这些输入并写回 World 的模拟结果。它们当前保存的是托管数组和房间对象的引用，因此先完成调用边界整理，再为 Native SoA 后端建立对应的 Native 数据视图。不能把当前结构直接当作 Burst 输入。

接口不包含网络发送、VAT、对象池、Transform 创建或房间开始结束逻辑。这样三个后端共享玩法语义，表现和网络仍由外层模块统一消费。

### 20.2 策略选择位置

`MonsterRuntimeService` 在房间级保存 `MonsterSimulationBackendKind`。`MonsterRoomRuntime` 创建时通过 `MonsterSimulationBackendFactory` 固化一次后端实例，之后每个 Tick 只发生一次整 Tick 策略调用，不在逐怪循环中做接口分派。

当前已接入的实现是 `ManagedSoA`，对应现有 `MonsterSimulation`。`ManualNativeSoABurst` 和 `EntitiesEcs` 已进入统一枚举，但实现尚未接入，选择它们会在房间创建时明确报出未支持状态，不会伪装成另一种后端。

### 20.3 后续接入顺序

1. 保持 `ManagedSoA` 作为行为基线，冻结接口和量化口径；
2. 新增 `ManualNativeSoABurst`，复用同一套输入边界和输出事件；
3. 用相同场景、Seed、射击路线和采样窗口完成两种后端的行为对照；
4. 再新增 `EntitiesEcs`，让 ECS 适配器只负责把 Chunk 数据映射到同一输出边界。

第一步只改变调用边界，没有改变 ManagedSoA 的数据布局和模拟算法。当前 `MonsterRoomRuntime.Tick` 创建输入结构，策略返回输出结构，`ResolveCombat` 在下一个阶段消费输出。模拟策略不再持有攻击配置和 Unity Physics 解析器，后续 Native SoA 后端只需生成同语义的攻击请求。
