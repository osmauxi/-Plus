# 同步快照与 GC 复用优化方案

> 目标：优先消除高频同步链路中的托管数组分配和重复快照复制，同时保留网络反序列化边界的长度、数量和协议校验。本文区分已完成修改与后续方案，未复测数据不作为实际收益。

## 1. 已完成修改

### 1.1 怪物表现快照

`MonsterPresentationPackingSystem.Capture` 和 `MonsterNetworkCodec.ReadPresentation` 生成的数组现在由 `MonsterPresentationFrame` 直接接管，不再在构造函数中重新分配并逐元素复制。

512怪时预计每份快照减少：

- 一份 `MonsterPackedState[512]`：约 4,096 B；
- 一份 `byte[64]` ActiveMask：约 64 B；
- 对应数组头和对齐开销。

合计约 4.2 KB/快照。快照频率为 15次/s 时，预计减少约63 KB/s托管分配。该数字是按布局推算，必须重新连接Player Profiler确认。

### 1.2 其他重复物化

- `EffectSet` 不再 Clone 已由调用方创建的Effects和AcquisitionOrder数组；
- `EffectRollOffer` 直接持有生成结果，不再复制数组并创建 `ReadOnlyCollection` 包装；
- `EffectRollService.ResolveOptions` 直接以数组作为 `IReadOnlyList` 返回；
- `PlayerWeaponController` 直接遍历 `NetworkList` 重建Effect，不再先复制中间数组；
- `MonsterSpawnBatch` 直接接管调用方生成的数组；
- 怪物配置目录直接接管装配器生成的数组，避免List→List→Array的二次物化；
- `MonsterViewWorld.ApplySpawnBatch` 不再为每批怪物生成临时PoolId数组；
- `MonsterSpawnSelector` 复用按配置项统计的计数数组；
- 地图生成、房间快照和Effect Roll的单目标RPC复用ClientId数组；
- 房间快照一次构建PlayerId和PlayerRoom数组，删除同一快照内重复生成和排序PlayerId数组；
- Effect Roll的EffectId/Level RPC数组按候选数量复用，不再每次各创建一块数组；
- 连锁闪电复用访问集合与节点List，弹体命中集合在弹体销毁后返还集合池；
- 清空特殊Effect Owner时直接遍历并清空字典，不再复制一份OwnerId数组。

### 1.3 怪物快照双缓冲

- `MonsterRuntimeService`持有服务端Capture的Write/Published两份Frame；
- `MonsterNetworkTransport`持有客户端Decode的Write/Published两份Frame；
- `CaptureInto`与`ReadPresentationInto`覆盖已有数组；
- SpawnBatch已知Slot总数时同时预扩容两份对应方向的Frame；容量意外不足时才按倍数增长，序列化仍只写有效Mask长度与ActiveCount；
- 新增`Monster.Presentation.Capture / Serialize / Decode / Apply`四个Profiler Marker。

代码已完成并通过Unity编译，GC收益仍需Player Profiler复测后填写为实测值。

## 2. 当前同步模块审计

| 模块 | 当前数据形式 | 高频托管分配判断 | 后续动作 |
|---|---|---|---|
| 怪物表现同步 | 完整ActiveMask + ActiveStates双缓冲 | 稳定容量下不再创建数组和Frame对象 | Player Profiler复测 |
| 玩家输入/快照 | 值类型Packet + Native `FastBufferWriter` + 预分配RingBuffer | 当前未发现每Tick托管数组 | 保持并通过Marker复测 |
| 武器表现同步 | 服务内复用List，直接写Native Writer | 当前没有Frame数组复制 | 只监控List扩容和高峰事件量 |
| 房间状态同步 | RPC要求多个托管数组 | 低频，但一次快照有多块数组 | 按Revision缓存快照数组 |
| 地图构建同步 | 每代地图一个稳定BuildPlan | 非每Tick路径 | 保持现状 |
| Effect Roll同步 | RPC需要EffectId和Level数组 | 已按候选数量复用 | 保持并复测 |
| Voice同步 | 复用采集、编码和接收缓冲 | 512怪样本仅160 B总分配 | 保持现状 |

RPC参数需要连续数组、反序列化需要拥有接收数据、历史EffectSet需要保存版本，这些数组不是无意义的防御性复制，不能仅通过删除复制来消除。应通过复用、缓存或协议改造处理。

## 3. 怪物快照零 GC 实施方案

### 3.1 数据容器

`MonsterPresentationFrame`已经改为可重复填充的内部缓冲：

```csharp
public sealed class MonsterPresentationFrame
{
    private byte[] _activeMask;
    private MonsterPackedState[] _activeStates;
    public int SlotCount { get; private set; }
    public int ActiveCount { get; private set; }
    public uint ServerTick { get; private set; }

    internal void EnsureCapacity(int slotCapacity, int activeCapacity);
}
```

数组容量只在房间开始或怪物容量增长时扩展。协议读写必须使用 `SlotCount`、`ActiveCount` 和实际Mask长度，不能序列化整块Capacity。

### 3.2 服务端双缓冲

`MonsterRuntimeService`持有`_authorityWriteFrame`和`_authorityPublishedFrame`：

1. `CaptureInto(_writeFrame)` 覆盖数组内容；
2. 交换两个Frame引用；
3. 使用 `_publishedFrame` 完成本机Replica Apply、事件通知和网络序列化；
4. 下一次快照只覆盖新的 `_writeFrame`。

双缓冲避免当前快照在同一次发布过程中被覆盖。生命周期契约为：Frame及其数组只保证到下一次同方向快照发布之前有效。需要长期保存的观察者必须复制自己真正需要的数据。

### 3.3 客户端双缓冲

`MonsterNetworkTransport`为接收方向持有独立的`_receiveWriteFrame` / `_receivePublishedFrame`：

```csharp
MonsterNetworkCodec.ReadPresentationInto(
    ref reader,
    _receiveWriteFrame);
```

完成协议边界的长度、SlotCount、ActiveCount和剩余字节校验后交换缓冲，再提交给Replica。服务器Capture缓冲与客户端Receive缓冲不能共用，Host模式下两个方向的生命周期不同。

### 3.4 序列化

继续使用 `FastBufferWriter(Allocator.Temp)`。它使用Native临时内存，不属于托管GC。发送多个远端客户端时对同一PublishedFrame重复序列化即可，不创建托管副本。

### 3.5 预期结果

当前实现完成后，稳定容量下的怪物快照链路目标为：

- `MonsterPresentationPackingSystem.Capture`：0 B GC；
- `MonsterNetworkCodec.ReadPresentationInto`：0 B GC；
- `MonsterPresentationFrame`：房间运行期间0个新对象；
- 只在怪物容量首次超过预留值时发生扩容分配。

## 4. 其他同步模块的复用策略

### 4.1 玩家同步

当前玩家同步已经接近目标形态：Input、State和Packet均为结构体，历史缓冲在初始化时一次分配。不要引入Frame类或数组快照。只需增加/保留以下Marker：

- `PlayerSync.SerializeInput`；
- `PlayerSync.SerializeSnapshot`；
- `PlayerSync.ResolveDelta`；
- `PlayerSync.Receive`。

若后续出现GC，优先检查NGO命名消息内部、异常日志字符串和Dictionary首次扩容，而不是复制玩家State。

### 4.2 武器表现同步

`WeaponPresentationService` 已复用5个事件List，并在发送完成后Clear。保持当前模式：

- 在房间开始时按设计上限设置Capacity，避免高弹幕场景首次扩容；
- 同一组List直接序列化给所有远端客户端；
- 不创建“不可变WeaponFrame”及其数组副本；
- 接收端逐项解码并立即分发，继续避免接收数组。

### 4.3 房间状态同步

房间快照不是每Tick发送，但一次会创建RoomIds、States、Visited、PlayerIds和PlayerRooms数组。建议增加按 `_stateRevision` 缓存的 `RoomFlowSnapshotBuffers`：

- Revision未变化时，广播和单客户端补发复用同一组数组；
- Revision变化时覆盖容量足够的缓存；
- 如果继续使用NGO数组RPC，数组Length必须等于有效数量；长度经常变化时按常见长度缓存，或改用 `FastBufferWriter` 显式写Count。

### 4.4 地图构建同步

MapBuildPlan按Generation创建一次，不属于高频GC。继续复用目标ClientId List即可，不需要对象池化BuildPlan。只有在Profiler证明重复重发造成显著分配时，再缓存其序列化字节。

### 4.5 Effect和配置快照

EffectSet会进入仓库并被历史Shot引用，不能覆盖旧数组；AcquisitionOrder数组属于真实版本快照。已经删除构造函数中的第二次Clone，但保留创建快照时的一次数组物化。

Effect Roll只保存当前Offer，发送时立即序列化，因此RPC的EffectId和Level数组可以由`PlayerWeaponController`按候选数量长期复用。若候选数量不变，后续Offer不再产生这两块数组分配。

## 5. 验证标准

使用同一512怪场景采集至少300帧，并检查：

| 指标 | 当前基线 | 目标 |
|---|---:|---:|
| `GameplayNetworkTickDriver.Update`总GC | 486,896 B / 299帧 | 接近固定网络底噪 |
| 大额快照帧 | 约9.25 KB | 不再出现怪物快照分配 |
| `Monster.Presentation.Capture` | 当前未单独打点 | 0 B GC |
| `Monster.Presentation.Decode` | 当前未单独打点 | 0 B GC |
| `NetworkPostLateUpdate` | 195,546 B / 299帧 | 单独记录，不混入业务收益 |
| 增量GC最大耗时 | 0.0022 ms | 不因修改上升 |

建议在实施双缓冲时新增Capture、Apply、Serialize、Decode四个Marker，避免再次只能从顶层 `GameplayNetworkTickDriver.Update` 推断归属。
