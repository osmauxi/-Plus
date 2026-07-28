# Addressables：目录、标签与句柄生命周期

## 1. 方法/架构介绍

Addressables 把“业务代码如何找到资源”和“资源实际存放在哪里”分开。业务代码只依赖 Address 或 Label，Catalog 决定它最终落到本地文件、AssetBundle 还是远端 CDN。

当前项目有三类典型用法：

1. 启动阶段按标签下载依赖：`AOT_DLL`、`Hotfix_DLL`。
2. Config 阶段按 `Config` 标签批量加载所有 `.bytes`。
3. Lobby 阶段按配置中的地址加载角色、武器、道具和 Sprite。

最重要的工程概念不是 `LoadAssetAsync` 本身，而是句柄所有权：

- `AsyncOperationHandle<T>` 既用于等待结果，也持有一次引用计数。
- 谁发起并持有句柄，谁负责在不再使用时释放。
- `LoadAssetAsync` 使用 `Addressables.Release(handle)`。
- `InstantiateAsync` 生成的实例使用 `Addressables.ReleaseInstance(handle)`。
- 失败、取消、对象销毁和竞态过期都必须走释放路径。

## 2. 源码展示

### 2.1 启动时初始化 Catalog 并更新远端目录

节选自 `Assets/_AOT/Scripts/Bootstrap/BootstrapRunner.cs`：

```csharp
await Addressables.InitializeAsync();

List<string> catalogsToUpdate =
    await Addressables.CheckForCatalogUpdates(false);

if (catalogsToUpdate.Count > 0)
{
    await Addressables.UpdateCatalogs(catalogsToUpdate, false);
}
```

### 2.2 按标签下载依赖并释放操作句柄

```csharp
private async UniTask DownloadDependencies(string label)
{
    var handle = Addressables.DownloadDependenciesAsync(
        label,
        autoReleaseHandle: false);

    try
    {
        while (!handle.IsDone)
        {
            await UniTask.Yield();
        }

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            throw new Exception($"下载标签 [{label}] 失败");
        }
    }
    finally
    {
        Addressables.Release(handle);
    }
}
```

### 2.3 Config 按标签批量读取 TextAsset

节选自 `Assets/_HotUpdate/Scripts/Config/ConfigManager.cs`：

```csharp
var handle = Addressables.LoadAssetsAsync<TextAsset>("Config", asset =>
{
    ConfigRegister.ParseAndRegister(asset.name, asset.bytes);
});

await handle.ToUniTask();

if (handle.Status == AsyncOperationStatus.Succeeded)
{
    Debug.Log($"配置表加载完毕：{_allConfigs.Count}");
}

Addressables.Release(handle);
```

### 2.4 加载资源与实例化资源的释放方式不同

```csharp
// 加载共享 Sprite 资源
AsyncOperationHandle<Sprite> spriteHandle =
    Addressables.LoadAssetAsync<Sprite>(iconAddress);
Addressables.Release(spriteHandle);

// 创建独立 GameObject 实例
AsyncOperationHandle<GameObject> instanceHandle =
    Addressables.InstantiateAsync(characterAddress, parent);
Addressables.ReleaseInstance(instanceHandle);
```

## 3. 源码解释

### Catalog 阶段

`InitializeAsync` 将运行时 Catalog 装入内存。`CheckForCatalogUpdates(false)` 只返回需要更新的 Catalog ID，不直接修改本地目录；`UpdateCatalogs` 才会拉取新目录。

Catalog 更新只代表“地址映射是新的”，并不代表对应 AssetBundle 已下载。真正下载依赖发生在 `DownloadDependenciesAsync(label)`。

### Label 批量加载

`LoadAssetsAsync<TextAsset>("Config", callback)` 会查找所有带 `Config` 标签且能转换为 `TextAsset` 的位置。每个资源完成时执行回调，因此 Config 注册顺序不应承载业务含义。

释放批量句柄后，原始 `TextAsset` 可以被卸载；已经被 MessagePack 转换成普通 C# 字典的数据仍由 `_allConfigs` 持有，不依赖 TextAsset 继续存活。

### 加载与实例化

`LoadAssetAsync<T>` 返回对资源对象的引用。多个调用方可能共享同一底层资源，释放只是减少引用计数。

`InstantiateAsync` 除了加载资源，还创建场景实例。`ReleaseInstance` 会销毁实例并归还加载引用。只调用 `Destroy(instance)` 会绕开 Addressables 的实例跟踪，容易留下不平衡的引用计数。

## 4. 底层拓展说明

### 4.1 Address 到资源的查找链

```text
Address / Label		传入寻址键（比如字符串 "Hero_Anbi" 或者标签 "Weapons"）
  → ResourceLocator	根据寻址键查找Catalog（资源目录表，即 catalog.json）
  → IResourceLocation	查到后给出Location，包含此资源位于哪个bundle文件，其依赖，位于本地还是远端
  → 依赖位置	递归依赖树，抓到并输出所有依赖名单(贴图/材质/Shader/预制件)
  → AssetBundle Provider	找到包含名单内容的AB包加载到内存中
  → Asset Provider		从AB包中拆包提取需要的资产
  → UnityEngine.Object	将资产组装完毕后返回
```

一次看似简单的 `LoadAssetAsync` 可能先加载 Catalog 中记录的多个依赖 Bundle，再从 Bundle 读取目标对象。句柄完成表示整条依赖链已经完成。

### 4.2 引用计数不是 C# GC

普通托管对象由 GC 判断是否可达；Addressables 资源还受到原生对象、AssetBundle 和引用计数管理。C# 字段清空并不会自动执行 `Addressables.Release`。

### 4.3 `autoReleaseHandle` 的含义

部分 API 允许 `autoReleaseHandle: true`。它只适合调用方不需要在完成后读取句柄状态或结果的操作。需要查看结果、错误或进度时，应保留句柄并在 `finally` 中手动释放。

### 4.4 当前 Config 源码的改进点

当前 `LoadAllConfigsAsync` 在 `await` 之后释放。如果等待过程抛出异常，最后一行不会执行。更稳健的版本应使用 `try/finally`：

```csharp
var handle = Addressables.LoadAssetsAsync<TextAsset>("Config", asset =>
{
    ConfigRegister.ParseAndRegister(asset.name, asset.bytes);
});

try
{
    await handle.ToUniTask();
    if (handle.Status != AsyncOperationStatus.Succeeded)
    {
        throw handle.OperationException ??
            new InvalidOperationException("Config Addressables 加载失败");
    }
}
finally
{
    if (handle.IsValid())
        Addressables.Release(handle);
}
```

## 5. 应用示例

### 示例 A：获取一个共享图标句柄并明确转移所有权

```csharp
public async UniTask<AsyncOperationHandle<Sprite>> AcquireIconAsync(
    string address)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(address);
    try
    {
        await handle.ToUniTask();
        return handle;
    }
    catch
    {
        if (handle.IsValid())
            Addressables.Release(handle);
        throw;
    }
}
```

调用方接收句柄后拥有释放责任：

```csharp
AsyncOperationHandle<Sprite> handle = await AcquireIconAsync(address);
try
{
    image.sprite = handle.Result;
    // 在真正使用期间持有 handle。
}
finally
{
    Addressables.Release(handle);
}
```

长期显示的 UI 通常把句柄保存为字段，在换图或销毁时释放；短期读取则适合上述作用域写法。

### 示例 B：生成大厅角色并由持有者释放

```csharp
private AsyncOperationHandle<GameObject> _characterHandle;

public async UniTask SpawnAsync(string address, Transform anchor)
{
    ReleaseCharacter();
    _characterHandle = Addressables.InstantiateAsync(address, anchor);
    await _characterHandle.ToUniTask();
}

private void ReleaseCharacter()
{
    if (_characterHandle.IsValid())
        Addressables.ReleaseInstance(_characterHandle);

    _characterHandle = default;
}
```

实际项目还需要加入异常处理和竞态防御，详见 AvatarResManager 与 ItemSlotView 两篇笔记。
