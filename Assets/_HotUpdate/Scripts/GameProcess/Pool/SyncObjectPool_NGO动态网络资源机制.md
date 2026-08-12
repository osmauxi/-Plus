# SyncObjectPool：NGO 动态网络资源机制

> 适用模块：`SyncObjectPool`  
> 重点：`NetworkPrefabsList`、`AddNetworkPrefab`、`PrefabIdHash`、`NetworkObjectId`、`INetworkPrefabInstanceHandler`、`ForceSamePrefabs`、Addressables、热更新与 Mod。

---

## 1. SyncObjectPool 到底扩展了 NGO 的什么

SyncObjectPool 没有重新实现 NGO。

它只扩展两个边界：

```text
Addressables
    ↓
决定 NetworkPrefab 什么时候进入/退出本地资源世界

PrefabHandler
    ↓
决定 NGO 创建/销毁本地实例时改走 ObjectPool
```

真正的：

```text
NetworkObjectId
Ownership
NetworkBehaviour
RPC
NetworkVariable
Spawn / Despawn 网络协议
```

仍然由 NGO 管理。

---

## 2. NetworkPrefab 注册究竟是什么

Server Spawn 一个 NetworkObject 时，并不会把：

```text
Prefab
Mesh
Material
Texture
C# 脚本
```

通过网络发给 Client。

Client 必须本地已经拥有这个资源，并且 NGO 必须知道：

```text
“网络消息描述的这种 Prefab 类型”
    ↓
“对应本地哪个 Prefab”
```

因此需要 NetworkPrefab Registry。

可以把它理解成：

```text
Prefab Identity
    ↓
Local NetworkPrefab
```

这就是 `NetworkPrefabsList` / runtime NetworkPrefabs 的主要意义。

---

## 3. NGO 可以用三张“概念表”理解

### 3.1 NetworkPrefab Registry：类型表

```text
Prefab Identity
    ↓
NetworkPrefab
```

负责：

> 第一次收到 Spawn 时，我应该创建什么类型？

由：

```text
NetworkPrefabsList
+
AddNetworkPrefab / RemoveNetworkPrefab
```

共同维护当前运行时可用的 NetworkPrefab 集合。

---

### 3.2 Prefab Handler Registry：实例工厂表

```text
NetworkPrefab
    ↓
INetworkPrefabInstanceHandler
```

负责：

> 知道是什么 Prefab 后，本地实例应该怎么创建和销毁？

默认：

```text
Instantiate
Destroy
```

注册 Handler 后：

```text
ObjectPool.Get
ObjectPool.Release
```

---

### 3.3 Spawned Object Registry：实例表

Spawn 完成后 NGO 会为运行时对象分配：

```text
NetworkObjectId
```

概念上：

```text
NetworkObjectId
    ↓
Runtime NetworkObject
```

之后 RPC、NetworkVariable 等网络消息主要通过这个运行时身份找到具体实例。

---

## 4. PrefabIdHash 与 NetworkObjectId

可以记一句：

> **Prefab 身份解决“创建谁”；NetworkObjectId 解决“之后找谁”。**

例如同一个 Archer Prefab：

```text
Archer #1 → NetworkObjectId = 87
Archer #2 → NetworkObjectId = 88
Archer #3 → NetworkObjectId = 89
```

它们属于同一种 NetworkPrefab，但运行时是三个不同的网络实例。

对象池中的 GameObject 被复用后，下一次 Spawn 可以得到新的 `NetworkObjectId`。

---

## 5. 一个重要的 NGO 2.13 细节：不要在动态注册前依赖 PrefabIdHash

当前 NGO 2.13 的 `NetworkObject.PrefabIdHash` 语义是：

```text
对象已经注册为 NetworkPrefab
    → 返回 Prefab Hash

尚未注册
    → 返回 0
```

因此动态 Addressable Prefab 的正确顺序应当是：

```text
Addressables Load
    ↓
取得 NetworkObject
    ↓
AddNetworkPrefab
    ↓
此时 NGO 才正式认识该 Prefab
```

不要这样做：

```text
先读 PrefabIdHash
    ↓
要求非 0
    ↓
再 AddNetworkPrefab
```

因为它可能把一个合法但尚未注册的动态 Prefab 错误判定为非法。

如果只是为了调试，可以在 `AddNetworkPrefab` 成功后再读取 `PrefabIdHash`。

---

## 6. 为什么不全部提前放进 NetworkPrefabsList

静态项目完全可以：

```text
NetworkManager
    ↓
NetworkPrefabsList
        Player
        Enemy
        Projectile
        Item
```

如果所有 NetworkPrefab 在网络启动前就已经存在，这种方式简单而稳定。

但当前项目的时序是：

```text
LobbyScene
    ↓
StartHost / StartClient
    ↓
NGO 已经启动
    ↓
进入 GameRuntime
    ↓
才根据配置 / 地图 / 热更 / Mod 确定需要哪些 NetworkPrefab
```

例如一个后下载的 Mod：

```text
ModBoss.prefab
```

在连接建立时根本没有被 Addressables 加载出来，因此无法提前作为一个运行时 GameObject 注册进 NGO。

这就是动态 `AddNetworkPrefab` 的主要理由。

---

## 7. 动态注册不是为了省 NetworkPrefabList 的内存

NetworkPrefab Registry 和 Handler Registry 本身只是运行时映射关系。

相对于：

```text
Mesh
Texture
Animation
VFX
大量预热实例
```

它们的开销很小。

所以动态：

```text
AddNetworkPrefab
RemoveNetworkPrefab
```

不是为了节省几个映射条目的内存。

真正目标是：

> **让 NGO 对这个资源的“可用状态”与 Addressables / Pool 的资源生命周期保持一致。**

---

## 8. ForceSamePrefabs

### ForceSamePrefabs = true

倾向于：

```text
连接前
Server 与 Client 已经准备好一致的 NetworkPrefab 集合
```

适合：

```text
固定 NetworkPrefab 集
静态内容
连接前一次性准备
```

### ForceSamePrefabs = false

允许：

```text
连接建立以后
继续动态 Add / Remove NetworkPrefab
```

因此适合：

```text
Addressables
DLC
热更新
Mod
按阶段加载的 Gameplay NetworkPrefab
```

但代价是：

> NGO 不再替你保证运行期间所有端的 Prefab 集始终完全一致。

一致性需要业务层自己控制。

---

## 9. AddNetworkPrefab 是本地操作，不会自动同步到所有客户端

Server：

```csharp
PrefabHandler.AddNetworkPrefab(prefab);
```

不会自动让 Client 也获得这个 Prefab。

必须是：

```text
Server
    Load + Add

Client A
    Load + Add

Client B
    Load + Add
```

因此真正安全的流程应当是：

```text
Server 决定 RequiredNetworkPoolIds
    ↓
通知 / 配置驱动所有端
    ↓
所有端 PreparePoolsAsync
    ↓
Addressables Load
    ↓
AddNetworkPrefab
    ↓
AddHandler
    ↓
Prewarm
    ↓
Client Ready
    ↓
Server 等待全部 Ready
    ↓
Server 才 Spawn
```

---

## 10. AddNetworkPrefab 做了什么

在当前进程中：

```text
这个 Prefab 原来只是一个普通的已加载 GameObject
```

执行：

```csharp
_networkManager.PrefabHandler.AddNetworkPrefab(prefabObject);
```

之后：

```text
NGO runtime NetworkPrefabs
```

开始认识这个 Prefab。

此后收到对应 Spawn 消息时，NGO 才知道应该使用哪个本地 NetworkPrefab。

注意：

> “注册 NetworkPrefab”不是直接建立一条跨网络对象引用。

而是 Server 与 Client 各自在本地建立兼容的 Prefab 类型映射，网络协议依靠双方一致的 Prefab 身份正确重建实例。

---

## 11. AddHandler 做了什么

没有 Handler：

```text
收到 Spawn
    ↓
找到 NetworkPrefab
    ↓
NGO 默认 Instantiate
```

加入：

```csharp
_networkManager.PrefabHandler.AddHandler(prefabObject, handler);
```

以后：

```text
收到 Spawn
    ↓
找到 NetworkPrefab
    ↓
发现存在自定义 Handler
    ↓
Handler.Instantiate(...)
    ↓
ObjectPool.Get()
```

Handler 只替换：

```text
本地实例如何取得
```

并没有替换 NGO 后续网络管理。

Handler 返回 `NetworkObject` 后，NGO 仍继续：

```text
设置 NetworkObjectId
设置 OwnerClientId
初始化 NetworkBehaviour
同步初始 NetworkVariable
调用 OnNetworkSpawn
```

---

## 12. 当前 PooledPrefabInstanceHandler

逻辑非常简单：

```csharp
public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
{
    return _owner.RentInstance(_entry, position, rotation);
}

public void Destroy(NetworkObject networkObject)
{
    _owner.ReturnInstance(_entry, networkObject);
}
```

所以：

```text
NGO Instantiate → Pool.Get
NGO Destroy     → Pool.Release
```

这就是 NGO 对象池接管的核心。

---

## 13. Server Spawn 完整流程

Server：

```csharp
SyncObjectPool.Instance.Spawn("Enemy_Wolf", position, rotation);
```

内部：

```text
RentInstance
    ↓
ObjectPool.Get
    ↓
NetworkObject.Spawn()
```

然后 NGO 发送 Spawn 信息给客户端。

---

## 14. Client 收到 Spawn 后

Client：

```text
收到 Spawn 消息
    ↓
NGO 根据 NetworkPrefab Registry 识别 Prefab
    ↓
发现这个 Prefab 有 Handler
    ↓
Handler.Instantiate
    ↓
RentInstance
    ↓
ObjectPool.Get
    ↓
返回 NetworkObject
    ↓
NGO 继续完成 NetworkObject 初始化
```

因此 Client 不需要业务层自己调用 Spawn。

---

## 15. Despawn 完整流程

Server 主动：

```csharp
instance.Despawn(false);
```

`false` 的目的：

```text
网络上 Despawn
但不直接 Destroy GameObject
```

Server 自己主动 Rent 的实例随后：

```text
ReturnInstance
```

Client 收到 Despawn：

```text
NGO
    ↓
Handler.Destroy(networkObject)
    ↓
ReturnInstance
    ↓
ObjectPool.Release
```

Host 同时是：

```text
Server + Client
```

因此实现中会再次检查 `_rentedInstanceIds`，避免同一实例被 Return 两次。

---

## 16. 为什么动态 RemoveNetworkPrefab

当某个动态 Network Pool 被彻底释放时：

```text
所有 NetworkObject 已 Despawn
    ↓
所有实例已 Return
    ↓
RemoveHandler
    ↓
RemoveNetworkPrefab
    ↓
Pool.Clear
    ↓
Addressables.Release
```

### RemoveHandler

告诉 NGO：

> 这个 Pool 已经不能再处理该 Prefab 的 Instantiate / Destroy。

否则 Handler 仍可能指向一个已经被释放的 PoolEntry。

### RemoveNetworkPrefab

告诉 NGO：

> 这个动态 NetworkPrefab 当前已经不再属于可用网络类型集合。

### Pool.Clear

真正销毁对象池中的闲置实例。

### Addressables.Release

降低资源引用，使 Prefab / Bundle 有机会卸载。

所以：

> RemoveHandler / RemoveNetworkPrefab 主要解决生命周期一致性与安全性。

> Pool.Clear / Addressables.Release 才是主要资源释放操作。

---

## 17. 动态注册不等于频繁注册

系统支持多种策略。

### Session 级

```text
第一次需要
    ↓
Load + AddNetworkPrefab + AddHandler

整个多人 Session 一直保留

退出游戏
    ↓
统一 Remove + Release
```

优点：

```text
简单
稳定
少跨端 Ready
```

### 阶段级

```text
进入 Forest
    ↓
Load Wolf / TreeBoss
    ↓
Add

离开 Forest
    ↓
Despawn
    ↓
Remove
    ↓
Release
```

优点：

```text
资源生命周期更细
```

代价：

```text
跨端一致性要求更高
```

`ReleasePool()` 提供的是能力。

真正什么时候调用，由开发者决定。

---

## 18. 热更新 / Mod 场景

例如新 Mod 下载：

```text
Mod Config
+
Mod Addressables Catalog
+
ModBoss.prefab
```

运行时：

```text
读取新的 Config_SyncObjectPool 数据
    ↓
注册 Pool Definition
    ↓
PreparePoolAsync("ModBoss")
    ↓
Addressables Load ModBoss
    ↓
AddNetworkPrefab
    ↓
AddHandler
    ↓
所有端 Ready
    ↓
Server Spawn
```

这个流程的意义是：

> 一个已经建立的网络 Session，可以在运行时认识连接建立时根本不存在的 NetworkPrefab。

这正是动态 NetworkPrefab 对热更新和 Mod 最有价值的地方。

---

## 19. Prepare 的最终推荐顺序

```text
1. Addressables.LoadAssetAsync<GameObject>
2. 校验 GameObject 上存在 NetworkObject
3. 保存 Prefab / Handle
4. AddNetworkPrefab
5. 创建 ObjectPool
6. 创建 PooledPrefabInstanceHandler
7. AddHandler
8. Prewarm
9. IsPrepared = true
```

不需要在步骤 4 前读取 `PrefabIdHash`。

---

## 20. Release 的最终推荐顺序

```text
1. 确保 RentedCount == 0
2. RemoveHandler
3. RemoveNetworkPrefab
4. Pool.Clear
5. 清空 NetworkPrefab / PrefabObject 引用
6. Addressables.Release(handle)
7. IsPrepared = false
```

如果之后重新需要：

```text
再次 Prepare
```

即可重新：

```text
Load
AddNetworkPrefab
AddHandler
Prewarm
```

---

## 21. Rollback

Prepare 任何一步失败时按逆序回滚：

```text
Handler 已注册
    ↓ RemoveHandler

NetworkPrefab 已注册
    ↓ RemoveNetworkPrefab

ObjectPool 已创建
    ↓ Clear

Addressables Handle 已取得
    ↓ Release
```

保证失败的 PoolEntry 回到：

```text
Unprepared
```

而不是留下半注册状态。

---

## 22. Shutdown 的网络前置条件

正常情况下，SyncObjectPool Shutdown 前应当由上层流程确保：

```text
Server 已停止生成新对象
    ↓
所有 Gameplay NetworkObject 已 Despawn
    ↓
Client 已收到 Despawn 并通过 Handler Return
    ↓
所有端 RentedCount == 0
    ↓
Pool Shutdown
```

不要把客户端“强行 Destroy 仍然 IsSpawned 的 NetworkObject”当成正常退出流程。

Shutdown 中的强制清理只能作为最终兜底；真正的网络状态收束仍然应该由 Server 权威流程完成。

---

## 23. 配置重复检查

过去曾尝试通过动态注册前读取 `PrefabIdHash` 检测：

```text
两个 Pool 是否指向同一个 NetworkPrefab
```

这一方式不适合动态 Prefab，因为未注册时 Hash 可能为 0。

更合适的是在配置注册阶段检查：

```text
PoolId 唯一
PrefabAddress 唯一
```

例如：

```text
Enemy_A → Enemy/Wolf
Enemy_B → Enemy/Wolf
```

应当直接视为配置错误。

这不是 NGO 身份校验，而是对象池配置完整性校验。

---

## 24. 整套机制总结

可以用四层来记：

```text
ConfigManager
    ↓
告诉 Pool：有哪些动态资源

Addressables
    ↓
把 NetworkPrefab 加载到本地

AddNetworkPrefab
    ↓
让 NGO 认识这种网络类型

AddHandler
    ↓
让 NGO 的 Instantiate / Destroy 改走 ObjectPool

NetworkObjectId
    ↓
Spawn 后由 NGO 管理具体运行时实例
```

最终一句话：

> **NetworkPrefab Registry 是类型表，PrefabHandler 是实例工厂适配表，NetworkObjectId 对应运行时实例。**

> **SyncObjectPool 用 Addressables 动态提供资源，用 AddNetworkPrefab 动态扩展 NGO 的类型集合，再用 AddHandler 把实例创建和销毁接到 ObjectPool；真正的网络同步仍然由 NGO 管理。**
