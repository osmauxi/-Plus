# AvatarResManager：实例化、资源所有权与竞态防御

## 1. 方法/架构介绍

`AvatarResManager` 的任务不是简单调用一次 `InstantiateAsync`，而是维护“每个展位当前拥有哪些资源”。每个展位可能同时拥有：

- 一个角色实例；
- 一个挂在角色 Socket 上的武器实例；
- 一个挂在角色 Socket 上的道具实例；
- 三个对应的 Addressables 句柄；
- 已经显示的 ID、期望显示的 ID 和异步请求版本号。

它采用四个关键方法保证正确性：

1. 每个展位独立保存运行时状态，不把多个玩家的句柄混在一起。
2. 区分 `DesiredXxxId` 与已经加载完成的 `XxxId`。
3. 每次请求递增 `Revision`，旧请求完成后发现版本不符就放弃写回。
4. 过期请求仍然释放自己创建的 Addressables 实例。

这套结构适用于角色换装、展柜商品、载具预览、宠物展示等“同一位置会被快速切换”的场景。

## 2. 源码展示

### 2.1 每个展位独立保存实际状态、期望状态和句柄

节选自 `Assets/_HotUpdate/Scripts/UI/AvatarResManager.cs`：

```csharp
private sealed class StationRuntime
{
    public ulong ClientId = ulong.MaxValue;
    public int CharacterId = -1;
    public int WeaponId = -1;
    public int ItemId = -1;
    public int Revision;

    public ulong DesiredClientId = ulong.MaxValue;
    public int DesiredCharacterId = -1;
    public int DesiredWeaponId = -1;
    public int DesiredItemId = -1;

    public GameObject CharacterInstance;
    public GameObject WeaponInstance;
    public GameObject ItemInstance;

    public AsyncOperationHandle<GameObject> CharacterHandle;
    public AsyncOperationHandle<GameObject> WeaponHandle;
    public AsyncOperationHandle<GameObject> ItemHandle;

    public CharacterSocketProvider SocketProvider;
}
```

### 2.2 同样的目标状态不重复发起加载

```csharp
bool isCurrent = station.DesiredClientId == value.ClientId
    && station.DesiredCharacterId == value.CharacterId
    && station.DesiredWeaponId == value.WeaponId
    && station.DesiredItemId == value.ItemId;

if (isCurrent)
    return;

station.DesiredClientId = value.ClientId;
station.DesiredCharacterId = value.CharacterId;
station.DesiredWeaponId = value.WeaponId;
station.DesiredItemId = value.ItemId;

int revision = ++station.Revision;
UpdateStationAsync(standIndex, value, revision).Forget();
```

### 2.3 角色请求完成后校验版本

```csharp
AsyncOperationHandle<GameObject> handle =
    Addressables.InstantiateAsync(
        ResolveSkinAddress(state.CharacterId),
        anchor);

bool loadSucceeded = await TryCompleteLoadAsync(
    handle,
    $"角色 CharacterId={state.CharacterId}");

if (revision != station.Revision)
{
    ReleaseLoadedHandle(handle);
    return;
}

if (!loadSucceeded)
{
    ReleaseLoadedHandle(handle);
    return;
}

station.CharacterHandle = handle;
station.CharacterInstance = handle.Result;
station.CharacterId = state.CharacterId;
BindCharacterComponents(station);
```

### 2.4 角色完成后再加载依赖它的装备

```csharp
if (station.WeaponId != state.WeaponId)
    await UpdateWeaponAsync(station, state.WeaponId, revision);

if (revision != station.Revision)
    return;

if (station.ItemId != state.ItemId)
    await UpdateItemAsync(station, state.ItemId, revision);
```

### 2.5 成功与失败句柄采用不同释放方法

```csharp
private static void ReleaseLoadedHandle(
    AsyncOperationHandle<GameObject> handle)
{
    if (!handle.IsValid())
        return;

    if (handle.Status == AsyncOperationStatus.Succeeded)
        Addressables.ReleaseInstance(handle);
    else
        Addressables.Release(handle);
}
```

## 3. 源码解释

### 为什么需要实际状态和期望状态

假设角色 1 正在加载，网络列表又刷新了一次，但玩家仍然选择角色 1。如果只看 `CharacterId`，它还是 `-1`，系统会重复发起加载。`DesiredCharacterId` 在请求发起时立即写入，可以过滤重复快照。

`CharacterId` 只在资源真正成功并被当前请求接管后写入，表示场景中实际展示的内容。

### `Revision` 如何阻止旧请求覆盖新请求

每次状态变化都执行 `++station.Revision`。异步方法捕获当时的版本：

```text
请求 A 捕获 revision=3
请求 B 捕获 revision=4
A 最后完成，发现当前 Revision 已经是 4
→ A 释放自己的实例，不写入 StationRuntime
```

这个检查必须放在每个可能挂起的 `await` 之后。只在方法开头检查没有意义，因为状态可能在等待期间变化。

### 为什么换角色时先释放装备

武器和道具是角色子节点。直接释放角色会连带销毁 Unity 层级中的子物体，但 Addressables 对武器、道具仍各自持有实例句柄。先通过 `ReleaseEquipment` 归还它们的句柄，再释放角色，才能保持引用计数平衡。

### 为什么装备要等角色完成

武器挂点来自：

```csharp
station.CharacterInstance
    .GetComponent<CharacterSocketProvider>()
    .GetEquipmentSocket(equipmentSlot);
```

角色尚未生成时没有 Socket，因此这个依赖不能省略。

## 4. 底层拓展说明

### 4.1 版本号不是线程锁

Unity Addressables 的结果通常回到主线程 PlayerLoop。这里并不是多线程同时写字段，而是多个异步延续在不同帧按不确定顺序恢复。版本号解决的是逻辑时序，不是 CPU 数据竞争。

### 4.2 取消令牌仍然有价值

`Revision` 能判定结果过期，但通常不能真正停止底层 AssetBundle 下载。可以为页面销毁增加 CancellationToken，减少后续业务执行；无论是否取消，句柄仍要释放。

### 4.3 当前实现的重试边界

当前代码在请求发起时写入 `DesiredXxxId`。如果加载失败，下一次传入完全相同的玩家状态会被 `isCurrent` 提前返回，因此不会自动重试。

需要自动重试时，可以增加加载状态：

```csharp
private enum ResourceLoadState
{
    Empty,
    Loading,
    Ready,
    Failed
}
```

只有 `Loading` 或 `Ready` 且目标相同才过滤重复请求；`Failed` 允许重试，或在失败时把对应 `DesiredXxxId` 重置为 `-1`。

### 4.4 配置整数应在边界校验

当前 `WeaponEquipAnim` 会限制到 Rifle/Pistol，有利于尽早暴露坏表。`EquipmentSlot` 也建议在转换时校验：

```csharp
private static EquipmentSlot ParseEquipmentSlot(int value, string fieldName)
{
    if (!Enum.IsDefined(typeof(EquipmentSlot), value))
        throw new ArgumentOutOfRangeException(fieldName, value, "装备挂点不存在");

    return (EquipmentSlot)value;
}
```

配置错误属于开发错误，应该在配置边界直接报出，而不是静默挂到错误节点。

### 4.5 完整切换与渐进切换

当前换角色时先释放旧角色，所以加载期间展位为空。这是“完整切换”：资源占用低，逻辑简单。

如果希望无缝切换，可以先在隐藏节点加载新角色，全部装备成功后一次性交换新旧实例，再释放旧资源。代价是切换期间同时持有两套模型，且失败回滚逻辑更复杂。

## 5. 应用示例

### 示例：增加大厅宠物资源

给 `StationRuntime` 增加：

```csharp
public int PetId = -1;
public int DesiredPetId = -1;
public GameObject PetInstance;
public AsyncOperationHandle<GameObject> PetHandle;
```

加载流程遵循同样的所有权规则：

```csharp
private async UniTask UpdatePetAsync(
    StationRuntime station,
    int petId,
    int revision,
    Transform petAnchor)
{
    ReleasePet(station);

    var handle = Addressables.InstantiateAsync(
        ResolvePetAddress(petId),
        petAnchor);

    bool succeeded = await TryCompleteLoadAsync(handle, $"宠物 PetId={petId}");
    if (revision != station.Revision || !succeeded)
    {
        ReleaseLoadedHandle(handle);
        return;
    }

    station.PetHandle = handle;
    station.PetInstance = handle.Result;
    station.PetId = petId;
}
```

同时必须把 `ReleasePet` 接入 `ReleaseStation`。只实现加载、不实现对称释放，是资源管理中最常见的错误。

