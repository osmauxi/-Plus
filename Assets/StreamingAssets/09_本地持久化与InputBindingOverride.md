# 本地持久化：JSON、版本归一化与 Input Binding Override

## 1. 方法/架构介绍

配置表与用户设置虽然都需要存储，但需求不同：

| 对比项 | Config | 用户设置 |
| --- | --- | --- |
| 写入者 | 编辑器构建管线 | 玩家运行中的客户端 |
| 数据量 | 较大、表格化 | 很小 |
| 是否需要人工排错 | 通常不需要看二进制 | 经常需要查看和删除 |
| 当前格式 | MessagePack | JSON |
| 当前位置 | Addressables 内容 | `Application.persistentDataPath` |

当前 Setting 数据包含：

- 数据版本号；
- 主音量、音乐音量、音效音量；
- Input System Binding Override JSON。

存储流程遵循：

```text
Load
  → 文件不存在：默认值
  → JSON 损坏：默认值
  → JSON 正常：Normalize

Save
  → Normalize
  → JsonUtility.ToJson
  → persistentDataPath/user_settings.json
```

## 2. 源码展示

### 2.1 可序列化数据与版本字段

节选自 `GameUserSettingsData.cs`：

```csharp
[Serializable]
public sealed class GameUserSettingsData
{
    public int Version = 1;
    public AudioSettingsData Audio = new AudioSettingsData();
    public string InputBindingOverridesJson = string.Empty;

    public static GameUserSettingsData CreateDefault()
    {
        return new GameUserSettingsData();
    }

    public void Normalize()
    {
        Version = Mathf.Max(Version, 1);
        Audio ??= new AudioSettingsData();
        Audio.Normalize();
        InputBindingOverridesJson ??= string.Empty;
    }
}
```

### 2.2 数值范围归一化

```csharp
public void Normalize()
{
    MasterVolume = Mathf.Clamp01(MasterVolume);
    MusicVolume = Mathf.Clamp01(MusicVolume);
    SfxVolume = Mathf.Clamp01(SfxVolume);
}
```

### 2.3 从 persistentDataPath 读取

节选自 `SettingSaveService.cs`：

```csharp
_filePath = Path.Combine(
    Application.persistentDataPath,
    "user_settings.json");
```

```csharp
public GameUserSettingsData Load()
{
    if (!File.Exists(_filePath))
        return GameUserSettingsData.CreateDefault();

    try
    {
        string json = File.ReadAllText(_filePath);
        GameUserSettingsData data =
            JsonUtility.FromJson<GameUserSettingsData>(json);

        if (data == null)
            throw new InvalidDataException("JSON 未生成有效对象");

        data.Normalize();
        return data;
    }
    catch (Exception exception)
    {
        Debug.LogWarning($"设置文件读取失败，将使用默认设置。\n{exception}");
        return GameUserSettingsData.CreateDefault();
    }
}
```

### 2.4 保存前再次归一化

```csharp
public void Save(GameUserSettingsData data)
{
    data.Normalize();
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(_filePath, json);
}
```

### 2.5 Input System 自己负责 Override JSON

节选自 `InputRebindService.cs`：

```csharp
public string SaveBindingOverridesAsJson()
{
    return _inputActions.SaveBindingOverridesAsJson();
}
```

```csharp
public bool ApplyBindingOverrides(string json)
{
    CancelRebind();
    _inputActions.RemoveAllBindingOverrides();

    try
    {
        if (!string.IsNullOrEmpty(json))
            _inputActions.LoadBindingOverridesFromJson(json);

        return true;
    }
    catch (Exception exception)
    {
        _inputActions.RemoveAllBindingOverrides();
        Debug.LogWarning($"按键 Override JSON 损坏，已恢复默认按键。\n{exception}");
        return false;
    }
}
```

## 3. 源码解释

### 为什么读取和保存都要 Normalize

读取时归一化可以修复旧文件缺字段、手改越界或 `null` 数据。

保存前再次归一化可以保证任何调用方都无法把非法值写入磁盘。数据不变量集中在数据类内部，而不是依赖每个 Slider 或按钮都做对。

### 为什么损坏时返回默认值

用户设置不是不可替代存档。因一个损坏 JSON 阻止游戏启动，代价高于丢失音量和键位偏好，因此这里选择记录警告并恢复默认。

关键进度存档则应保留备份、校验和迁移日志，不能简单吞掉错误。

### 为什么 Binding Override 保存成一个字符串

Input System 已经定义了 Override 的 JSON 格式。项目无需把每个 Binding 手工复制到自己的 DTO，只需把官方 JSON 作为一个字段嵌入设置文件。

加载前先 `RemoveAllBindingOverrides`，可以避免旧内存状态与文件内容叠加。

### 为什么改键时要临时 Disable Action

正在监听新输入时，如果目标 Action 仍启用，按下的新按键可能同时触发实际游戏行为。当前代码记录 `_actionWasEnabled`，开始改键时 Disable，结束后恢复原状态。

## 4. 底层拓展说明

### 4.1 `persistentDataPath` 的意义

它是 Unity 为当前应用提供的可写持久目录。具体路径随平台变化，不应在代码中硬编码 Windows 用户目录。

`Assets` 与 Addressables 构建内容在玩家包中通常不可写；用户数据必须写到平台允许的持久目录。

### 4.2 JSON 写入不是天然原子操作

当前数据很小，`File.WriteAllText` 通常足够，但应用在写入中途崩溃仍可能留下截断文件。更稳健的方式是先写临时文件再替换：

```csharp
string tempPath = _filePath + ".tmp";
string backupPath = _filePath + ".bak";

File.WriteAllText(tempPath, json);
if (File.Exists(_filePath))
    File.Replace(tempPath, _filePath, backupPath);
else
    File.Move(tempPath, _filePath);
```

启动时可以在主文件损坏后尝试读取 `.bak`。

### 4.3 版本号必须配合迁移代码

只有 `Version` 字段不会自动迁移。新增数据结构后应明确：

```csharp
switch (data.Version)
{
    case 1:
        MigrateV1ToV2(data);
        goto case 2;
    case 2:
        break;
    default:
        throw new InvalidDataException("未知设置版本");
}
```

小型设置也可以采取“缺字段使用默认值”，但删除、改名或改变含义时仍需要迁移。

### 4.4 什么时候改用 MessagePack

当本地数据量很大、加载频繁且不需要人工查看时，可以使用 MessagePack。对于几十行设置 JSON，二进制带来的收益很小，可读性更有价值。

格式选择应由数据用途决定，而不是项目已经安装了哪个序列化库。

### 4.5 本地文件不是安全存储

JSON、MessagePack 和 PlayerPrefs 都可以被玩家修改。不要把付费状态、服务器权限或反作弊判定只保存在本地文件。

### 4.6 当前输入重构边界

Setting 使用自己的 `InputActionAsset` 保存 Override，当前不修改 Gameplay 输入读取。未来统一输入系统时，要确保 Gameplay 实际使用同一资产或消费同一份 Override，否则设置界面显示的按键不会影响游戏。

## 5. 应用示例

### 示例：V2 增加鼠标灵敏度

数据类：

```csharp
[Serializable]
public sealed class GameUserSettingsData
{
    public int Version = 2;
    public AudioSettingsData Audio = new AudioSettingsData();
    public string InputBindingOverridesJson = string.Empty;
    public float MouseSensitivity = 1f;

    public void Normalize()
    {
        Audio ??= new AudioSettingsData();
        Audio.Normalize();
        InputBindingOverridesJson ??= string.Empty;
        MouseSensitivity = Mathf.Clamp(MouseSensitivity, 0.1f, 5f);
    }
}
```

迁移：

```csharp
private static void Migrate(GameUserSettingsData data)
{
    if (data.Version < 2)
    {
        data.MouseSensitivity = 1f;
        data.Version = 2;
    }

    data.Normalize();
}
```

应用顺序：

```text
读取 JSON
  → 迁移旧版本
  → Normalize
  → 应用音频
  → 应用 Binding Override
  → 应用鼠标灵敏度
  → 必要时把迁移结果重新保存
```

