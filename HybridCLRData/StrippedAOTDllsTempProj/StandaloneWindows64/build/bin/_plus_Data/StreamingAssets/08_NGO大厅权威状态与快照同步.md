# NGO 大厅：权威状态、NetworkList 与快照同步

## 1. 方法/架构介绍

Lobby 联机数据采用服务器权威模型：

```text
客户端点击
  → 发送 ServerRpc 请求
  → 服务器校验发送者和 Config ID
  → 服务器修改 NetworkList<LobbyPlayerState>
  → NGO 同步列表变化
  → 客户端收到 OnListChanged
  → 重新构建可见展位快照
  → 同时刷新展位 UI 与 Avatar 模型
```

`LobbyPlayerState` 是同步单位；`NetworkList<LobbyPlayerState>` 是权威集合；`LobbyOverviewCoordinator` 把网络集合转换为按 StandIndex 排列的固定快照。

离线模式不伪造 NetworkList，而是使用 `_localDraft`。无论数据来自网络还是本地，最终都进入同一份 `_visibleStates`，下游渲染代码不需要维护两套流程。

## 2. 源码展示

### 2.1 定义可同步结构体

节选自 `LobbyPlayerState.cs`：

```csharp
public struct LobbyPlayerState :
    INetworkSerializable,
    IEquatable<LobbyPlayerState>
{
    public ulong ClientId;
    public FixedString64Bytes PersistentPlayerId;
    public FixedString32Bytes PlayerName;
    public int StandIndex;
    public int CharacterId;
    public int WeaponId;
    public int ItemId;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PersistentPlayerId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref StandIndex);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref WeaponId);
        serializer.SerializeValue(ref ItemId);
        serializer.SerializeValue(ref IsReady);
    }
}
```

### 2.2 NetworkList 自动同步玩家集合

```csharp
public NetworkList<LobbyPlayerState> LobbyPlayers =
    new NetworkList<LobbyPlayerState>();

public override void OnNetworkSpawn()
{
    LobbyPlayers.OnListChanged += HandleLobbyPlayersChanged;
}

private void HandleLobbyPlayersChanged(
    NetworkListEvent<LobbyPlayerState> changeEvent)
{
    OnLobbyDataChanged?.Invoke();
}
```

### 2.3 ServerRpc 使用真实发送者而不是客户端传入的 ClientId

```csharp
[ServerRpc(RequireOwnership = false)]
public void ChangeWeaponServerRpc(
    int newWeaponId,
    ServerRpcParams rpcParams = default)
{
    ValidateWeaponId(newWeaponId);
    ulong senderId = rpcParams.Receive.SenderClientId;

    for (int i = 0; i < LobbyPlayers.Count; i++)
    {
        if (LobbyPlayers[i].ClientId != senderId)
            continue;

        LobbyPlayerState state = LobbyPlayers[i];
        if (state.IsReady)
            return;

        state.WeaponId = newWeaponId;
        LobbyPlayers[i] = state;
        break;
    }
}
```

### 2.4 结构体修改后必须写回列表

```csharp
LobbyPlayerState state = LobbyPlayers[i];
state.IsReady = !state.IsReady;
LobbyPlayers[i] = state;
```

### 2.5 网络列表转换为固定展位快照

节选自 `LobbyOverviewCoordinator`：

```csharp
Array.Clear(_visibleStates, 0, _visibleStates.Length);
LocalPlayerStandIndex = -1;

foreach (LobbyPlayerState player in _networkManager.LobbyPlayers)
{
    if ((uint)player.StandIndex >= (uint)_visibleStates.Length)
        throw new InvalidOperationException("服务器下发了无效展位");

    if (_visibleStates[player.StandIndex].HasValue)
        throw new InvalidOperationException("展位被多个玩家占用");

    _visibleStates[player.StandIndex] = player;
}
```

### 2.6 同一快照驱动 UI 和模型

```csharp
for (int i = 0; i < _visibleStates.Length; i++)
{
    _standManager.RenderStand(
        i,
        _visibleStates[i],
        i == LocalPlayerStandIndex,
        showReadyState);

    _avatarResManager.ApplyStandState(i, _visibleStates[i]);
}
```

## 3. 源码解释

### `INetworkSerializable` 与 MessagePack 不同

NGO 使用 `BufferSerializer` 写入实时网络包；Config 使用 MessagePack 读取构建产物。两者都涉及序列化，但生命周期和协议完全不同。

不要给 `LobbyPlayerState` 加 MessagePack 就认为 NGO 会自动使用它；NGO 读取的是 `NetworkSerialize`。

### 为什么使用 `FixedString`

NGO 高频同步结构体不适合直接携带任意托管 `string`。`FixedString32Bytes` 和 `FixedString64Bytes` 容量固定、可以在原生容器与网络序列化中使用，也让包大小上限更明确。

容量按 UTF-8 字节而不是中文字符数计算。项目把玩家名限制为 29 个 UTF-8 字节，为内部编码和终止信息留出空间。

### 为什么结构体必须重新赋值

`LobbyPlayers[i]` 返回的是值副本：

```csharp
var state = LobbyPlayers[i];
state.WeaponId = newWeaponId;
```

只修改副本不会触发 NetworkList 脏标记。执行：

```csharp
LobbyPlayers[i] = state;
```

才会更新权威集合并同步变化。

### 为什么渲染层使用固定数组快照

`NetworkList` 的顺序不应该隐式等于展位顺序。服务器显式分配 `StandIndex`，客户端将玩家放入对应数组位置。这样玩家断线、重连或列表顺序变化时，展位仍然稳定。

### 离线与在线共用渲染出口

离线 `_localDraft` 被放入 `_visibleStates[0]`；在线权威数据也被放入 `_visibleStates`。最终都调用 `RenderVisibleStates`，避免“单机能显示但联网不显示”这种双分支漂移。

## 4. 底层拓展说明

### 4.1 服务器权威不仅是“代码运行在 ServerRpc”

服务器还必须验证：

- 请求发送者是否只能修改自己；
- ID 是否存在于服务器 Config；
- 玩家准备后是否禁止继续换装；
- 展位索引是否唯一且在范围内；
- 房间状态是否允许该操作。

当前代码通过 `rpcParams.Receive.SenderClientId` 定位玩家，避免信任客户端自行传入的 ClientId。

### 4.2 `IEquatable` 影响变化判断

同步结构体实现全部字段比较，有利于框架判断值是否真正变化。新增同步字段时，应同时更新：

1. 字段声明；
2. `NetworkSerialize` 顺序；
3. `Equals` 比较；
4. 默认玩家创建；
5. 快照和 UI/模型消费逻辑。

### 4.3 序列化字段顺序是网络协议

Host 与 Client 程序版本不一致时，如果字段顺序或类型不同，会读出错误数据。NGO 项目通常要求参与同一房间的客户端使用兼容构建版本。

### 4.4 PersistentPlayerId 需要可信身份来源

当前持久 ID 存在 PlayerPrefs，并作为连接 Payload 发送。它足以演示断线重连映射，但客户端可以修改或伪造 PlayerPrefs。

正式账号系统中，重连身份应来自服务器签名 Token、平台账号或认证服务，不能只凭客户端自报字符串恢复敏感数据。

### 4.5 全量刷新与增量刷新

当前收到任何 `NetworkList` 变化后重建最多四个展位，逻辑简单且成本很低。玩家数量很大时，可以使用 `NetworkListEvent` 的 Index/Type 做增量更新，但复杂度也会显著增加。

## 5. 应用示例

### 示例：同步玩家当前展示动作 ID

新增字段：

```csharp
public int EmoteId;
```

同步方法追加在末尾：

```csharp
serializer.SerializeValue(ref EmoteId);
```

相等比较加入：

```csharp
EmoteId == other.EmoteId
```

服务器 RPC：

```csharp
[ServerRpc(RequireOwnership = false)]
public void ChangeEmoteServerRpc(
    int emoteId,
    ServerRpcParams rpcParams = default)
{
    ValidateEmoteId(emoteId);
    int index = FindPlayerIndex(rpcParams.Receive.SenderClientId);
    LobbyPlayerState state = LobbyPlayers[index];
    state.EmoteId = emoteId;
    LobbyPlayers[index] = state;
}
```

客户端快照刷新后，根据 Config 把 `EmoteId` 映射成 Animator Trigger 或状态名。网络仍然只传整数，不传动画资源路径。

