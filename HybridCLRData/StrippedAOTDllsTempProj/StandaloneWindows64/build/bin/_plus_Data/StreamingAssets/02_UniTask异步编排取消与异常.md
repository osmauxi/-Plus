# UniTask：异步编排、取消与异常

## 1. 方法/架构介绍

UniTask 的价值不是把 `Coroutine` 全部换成 `async`，而是让 Unity 的异步资源、PlayerLoop 和业务顺序使用同一种控制流表达。

当前项目中的典型用法：

- `ConfigManager`：等待 Addressables 批量加载完成后再进入 LobbyScene。
- `AvatarResManager`：按角色 → 武器 → 道具顺序更新一个展位。
- `ItemSlotView`：等待 Sprite 加载，但阻止对象池复用后的旧结果写回。
- `BootstrapRunner`：按 Catalog → 下载 → AOT 元数据 → 热更 DLL 的顺序启动。

需要区分三个概念：

1. 异步顺序：`await` 保证当前方法后续代码等待结果。
2. 生命周期取消：对象销毁或页面退出后不再需要结果。
3. 业务竞态：请求 A 先发后到，请求 B 后发先到；即使对象没有销毁，A 也已经过期。

取消令牌主要处理第 2 点，版本号主要处理第 3 点，两者不能简单互相替代。

## 2. 源码展示

### 2.1 把 Addressables Handle 转为 UniTask

节选自 `ConfigManager`：

```csharp
var handle = Addressables.LoadAssetsAsync<TextAsset>("Config", callback);
await handle.ToUniTask();
```

### 2.2 按顺序更新角色、武器和道具

节选自 `AvatarResManager.UpdateStationAsync`：

```csharp
if (characterChanged)
{
    // 等待角色生成并绑定挂点
}

if (station.WeaponId != state.WeaponId)
    await UpdateWeaponAsync(station, state.WeaponId, revision);

if (revision != station.Revision)
    return;

if (station.ItemId != state.ItemId)
    await UpdateItemAsync(station, state.ItemId, revision);
```

武器依赖角色上的 `CharacterSocketProvider`，所以这里不能并行加载后直接挂载。顺序本身就是业务依赖。

### 2.3 从同步事件启动异步工作

```csharp
int revision = ++station.Revision;
UpdateStationAsync(standIndex, value, revision).Forget();
```

`ApplyStandState` 是同步入口，因此使用 `.Forget()` 启动异步链，而不是把整个调用链强制改成返回 `UniTask`。

### 2.4 统一捕获 Addressables 加载异常

```csharp
private static async UniTask<bool> TryCompleteLoadAsync(
    AsyncOperationHandle<GameObject> handle,
    string resourceDescription)
{
    try
    {
        await handle.ToUniTask();
        return handle.Status == AsyncOperationStatus.Succeeded;
    }
    catch (Exception exception)
    {
        Debug.LogError(
            $"[AvatarResManager] {resourceDescription} 加载失败：{exception.Message}");
        return false;
    }
}
```

## 3. 源码解释

### `UniTask` 与 `async void`

普通异步方法优先返回 `UniTask` 或 `UniTask<T>`，这样调用者能等待、捕获异常或组合任务。

`async void` 只适合 Unity 消息或按钮回调这类框架规定为 `void` 的边界。当前 `BootstrapRunner.Start()` 是这种情况，它立刻进入一个返回 `UniTask` 的 `StartPipelineAsync()`，并在管线内部捕获异常。

### `.Forget()` 的责任

`.Forget()` 表示调用者明确不等待结果。它不等于“不需要处理错误”。如果异步方法内部并未覆盖所有异常，建议提供异常回调：

```csharp
UpdateStationAsync(index, state, revision)
    .Forget(exception => Debug.LogException(exception, this));
```

项目中的 Addressables 等待异常已经在 `TryCompleteLoadAsync` 中转换为 `false`，但配置缺失、非法枚举或必须组件漏挂仍可能抛出。这类错误在开发期保留为显式异常有利于定位。

### 为什么不能只依赖 `await`

假设玩家连续选择武器 1、2、3：

```text
请求 1 ─────────────── 完成
请求 2 ───── 完成
请求 3 ──────── 完成
```

每个请求内部都正确 `await`，但请求之间仍然并发。如果请求 1 最晚完成且没有版本检查，它会覆盖最新的武器 3。

## 4. 底层拓展说明

### 4.1 UniTask 与 Unity PlayerLoop

`Task` 默认依赖 .NET TaskScheduler；UniTask 可以直接把延续注册到 Unity PlayerLoop 的指定阶段，减少为 Unity 主线程异步场景创建托管 Task 的成本。

常见等待方式：

- `UniTask.Yield()`：下一次 PlayerLoop 继续。
- `UniTask.Delay(...)`：计时等待，可选择是否受 `timeScale` 影响。
- `handle.ToUniTask()`：把 Addressables 操作接入 UniTask。
- `GetCancellationTokenOnDestroy()`：GameObject 销毁时发出取消。

### 4.2 取消不自动释放资源

取消等待只意味着调用方不再继续等待，不保证 Addressables 引用计数自动归还。资源句柄仍应在 `finally` 或统一释放方法中处理。

### 4.3 生命周期令牌与业务令牌

```csharp
CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
await handle.ToUniTask(cancellationToken: destroyToken);
```

这能阻止组件销毁后继续执行 UI 写回，但无法判断同一个组件是否已经绑定了新数据。对象池中的 ItemSlot 没有销毁，因此仍需要 `_bindVersion`。

### 4.4 并行只适合无依赖步骤

如果两个加载互不依赖，可以：

```csharp
await UniTask.WhenAll(loadA, loadB);
```

Avatar 中武器与道具都需要角色挂点，因此至少要等角色完成。角色完成后，武器和道具理论上可以并行，但还要分别设计句柄归属、错误恢复和版本检查；在资源数很少时，顺序执行通常更容易保证正确性。

## 5. 应用示例

### 示例 A：带生命周期取消并复制出托管数据

```csharp
private async UniTask<byte[]> LoadConfigBytesAsync(
    string address,
    CancellationToken cancellationToken)
{
    var handle = Addressables.LoadAssetAsync<TextAsset>(address);
    try
    {
        await handle.ToUniTask(cancellationToken: cancellationToken);
        byte[] source = handle.Result.bytes;
        var copy = new byte[source.Length];
        Buffer.BlockCopy(source, 0, copy, 0, source.Length);
        return copy;
    }
    finally
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }
}
```

返回的是独立托管 `byte[]`，因此释放 TextAsset 句柄后仍可反序列化。Sprite、Prefab 等 UnityEngine.Object 若需要长期使用，则不能在返回前释放，应该由使用者持有句柄直到换图或销毁。

### 示例 B：版本号防止过期结果写回

```csharp
private int _requestVersion;

public void Refresh(string address)
{
    int version = ++_requestVersion;
    RefreshAsync(address, version).Forget(Debug.LogException);
}

private async UniTask RefreshAsync(string address, int version)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(address);
    await handle.ToUniTask();

    if (version != _requestVersion)
    {
        Addressables.Release(handle);
        return;
    }

    // 当前请求接管 handle，并负责在下一次 Refresh 时释放。
    _currentHandle = handle;
    _image.sprite = handle.Result;
}
```

关键不是 `int` 本身，而是“只有最新请求有权写入状态；过期请求必须归还自己创建的资源”。
