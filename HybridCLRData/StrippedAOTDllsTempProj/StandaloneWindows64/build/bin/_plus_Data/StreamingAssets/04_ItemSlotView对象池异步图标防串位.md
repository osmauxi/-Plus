# ItemSlotView：对象池、异步图标与防串位

## 1. 方法/架构介绍

滚动列表常用对象池复用格子。对象池减少 Instantiate/Destroy，但会产生一个特殊竞态：

```text
格子对象绑定“步枪”并开始加载步枪图标
格子被回收
同一个对象重新绑定“手枪”并开始加载手枪图标
步枪图标最后才完成
```

如果异步回调只持有 `this`，最后完成的步枪请求会把手枪格子的图片覆盖掉，形成“串位”。

当前 `ItemSlotView` 使用 `_bindVersion` 解决：每次绑定、禁用或销毁都递增版本；异步加载只在自己捕获的版本仍等于当前版本时写入 UI。

它同时保存 `_iconHandle`，确保格子换图、回收或销毁时归还 Addressables 引用。

## 2. 源码展示

节选自 `Assets/_HotUpdate/Scripts/UI/WeaponChoseUI/ItemSlotView.cs`。

### 2.1 每次绑定先使旧请求失效并释放旧图标

```csharp
public void Bind(ItemSlotData data)
{
    _bindVersion++;
    ReleaseIcon();

    _itemId = data.Id;
    _nameText.text = data.Name;
    LoadIconAsync(data.IconPath, _bindVersion).Forget();
}
```

### 2.2 对象池回收时同样使请求失效

```csharp
private void OnDisable()
{
    _bindVersion++;
    ReleaseIcon();
    _rectTransform.DOKill();
    _rectTransform.localScale = _originalScale;
    _highlightFrame.gameObject.SetActive(false);
}
```

### 2.3 异步完成后检查自己是否仍属于当前绑定

```csharp
private async UniTask LoadIconAsync(
    string iconAddress,
    int bindVersion)
{
    AsyncOperationHandle<Sprite> handle =
        Addressables.LoadAssetAsync<Sprite>(iconAddress);
    _iconHandle = handle;

    try
    {
        await handle.ToUniTask();
        if (bindVersion != _bindVersion)
            return;

        _iconImage.sprite = handle.Result;
        _iconImage.enabled = true;
    }
    catch (Exception exception)
    {
        if (bindVersion != _bindVersion)
            return;

        if (handle.IsValid())
            Addressables.Release(handle);

        _iconHandle = default;
        Debug.LogError($"ItemSlot 图标加载失败：{iconAddress}\n{exception}", this);
    }
}
```

### 2.4 句柄释放与视觉清空放在同一个方法

```csharp
private void ReleaseIcon()
{
    if (_iconHandle.IsValid())
        Addressables.Release(_iconHandle);

    _iconHandle = default;
    _iconImage.sprite = null;
    _iconImage.enabled = false;
}
```

## 3. 源码解释

### 为什么 `OnDisable` 必须递增版本

对象池回收通常只是 `SetActive(false)`，不会调用 `OnDestroy`。如果只在销毁时取消，隐藏格子的旧加载仍会继续，随后可能写回一个已经被重新启用并绑定新数据的对象。

`OnDisable` 是池化对象结束本轮使用的真正生命周期边界。

### 为什么要先递增再释放

释放正在加载的句柄可能使等待以异常或失败状态结束。先递增版本后，旧异步流程在 `catch` 中能识别自己已经过期，不为预期中的回收打印错误。

### 为什么版本参数必须按值传入

调用时：

```csharp
LoadIconAsync(data.IconPath, _bindVersion)
```

参数保存的是本次绑定的快照。异步方法若只在完成时读取 `_bindVersion`，就没有“旧值”可用于比较。

### 为什么高亮也在回收时复位

对象池复用的不只是数据，还包括全部视觉状态。缩放 Tween、高亮框、旧 Sprite 和按钮 ID 都可能污染下一次绑定。池化组件应把“退出使用”视为一次完整清理。

## 4. 底层拓展说明

### 4.1 版本号与 CancellationToken 的取舍

对象池格子不会销毁，因此 `GetCancellationTokenOnDestroy()` 无法覆盖复用场景。可以为每次 Bind 建立新的 `CancellationTokenSource`，但仍需要明确释放句柄。

对于单个整数就能表达的“最新绑定获胜”，版本号更简单，也不会频繁创建和 Dispose CTS。

### 4.2 句柄字段只能代表当前请求

`_iconHandle` 是当前绑定拥有的句柄。旧请求完成时不能随意把 `_iconHandle` 清空，否则可能把新请求的所有权信息抹掉。

因此竞态代码要区分：

- 局部变量 `handle`：本次异步请求创建的句柄；
- 字段 `_iconHandle`：组件当前接管的句柄。

复杂场景可以比较句柄身份，或在成功写回时才把局部句柄提升为字段所有权。

### 4.3 占位图与失败图

当前实现加载期间禁用 Image。产品环境通常会区分三个视觉状态：

```text
Loading：显示占位图或骨架屏
Ready：显示真实 Sprite
Failed：显示缺失图标并允许重试
```

不要让失败状态继续显示上一个物品的旧图标。

### 4.4 对象池刷新必须完整

父级 `ItemSelectView.RefreshGrid` 每次复用格子时会执行：

```csharp
slot.Bind(items[i]);
slot.SetHighlight(items[i].Id == selectedId);
```

分类切换不能只改名字；必须同时刷新数据、图标请求、点击 ID 和高亮。这也是“完整 Bind”方法存在的意义。

## 5. 应用示例

### 示例：带占位图和版本检查的头像格子

```csharp
private int _version;
private AsyncOperationHandle<Sprite> _avatarHandle;

public void BindAvatar(string address)
{
    int version = ++_version;
    ReleaseAvatar();
    _image.sprite = _loadingSprite;
    _image.enabled = true;
    LoadAvatarAsync(address, version).Forget(Debug.LogException);
}

private async UniTask LoadAvatarAsync(string address, int version)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(address);
    try
    {
        await handle.ToUniTask();
        if (version != _version)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
            return;
        }

        _avatarHandle = handle;
        _image.sprite = handle.Result;
    }
    catch
    {
        if (version == _version)
            _image.sprite = _failedSprite;

        if (handle.IsValid())
            Addressables.Release(handle);
    }
}
```

这个版本采用“成功后才把局部句柄交给字段”的所有权转移方式，适合请求并发更复杂的列表。

