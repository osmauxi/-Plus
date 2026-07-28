# Config 数据管线：从 Excel 到运行时强类型字典

## 1. 方法/架构介绍

当前项目的 Config 管线把编辑器阶段的高成本工作提前完成，使运行时只做三件事：加载 `.bytes`、MessagePack 反序列化、按类型保存字典。

完整流程如下：

```text
Assets/DesignData/Excels/*.xlsx
  → ExcelDataReader 读取每个 Sheet
  → 生成 Config_Xxx.cs
  → Unity 编译生成类
  → 反射创建 Dictionary<int, Config_Xxx>
  → MessagePackSerializer.Serialize
  → Assets/AddressableResources/Config/Config_Xxx.bytes
  → Addressables Label: Config
  → ConfigRegister.ParseAndRegister
  → ConfigManager.GetTable<Config_Xxx>()
```

这种管线的核心不是 Excel，而是“编辑器负责转换，运行时消费最终格式”。运行时不会解析 `.xlsx`，因此不需要携带 ExcelDataReader，也不会把表头推断、字符串转换和反射成本带到玩家设备上。

## 2. 源码展示

### 2.1 Excel 的三行 Schema 约定

节选自 `Assets/Editor/Tools/ExcelToMessagePackGenerator.cs`：

```csharp
string desc = table.Rows[0][col].ToString();
string varName = table.Rows[1][col].ToString();
string typeName = table.Rows[2][col].ToString();
```

约定为：

| 行 | 内容 | 示例 |
| --- | --- | --- |
| 第 1 行 | 中文说明 | 武器名称 |
| 第 2 行 | C# 字段名 | `Name` |
| 第 3 行 | C# 类型 | `string` |
| 第 4 行起 | 实际数据 | 步枪 |

第一列还约定为 `int` ID，用作字典 Key。

### 2.2 根据表头生成强类型类

```csharp
csBuilder.AppendLine("[MessagePackObject]");
csBuilder.AppendLine($"public class {className}{inheritance}");
csBuilder.AppendLine("{");

for (int col = 0; col < table.Columns.Count; col++)
{
    string desc = table.Rows[0][col].ToString();
    string varName = table.Rows[1][col].ToString();
    string typeName = table.Rows[2][col].ToString();

    if (string.IsNullOrWhiteSpace(varName))
        continue;

    csBuilder.AppendLine($"    /// <summary> {desc} </summary>");
    csBuilder.AppendLine($"    [Key({col})]");
    csBuilder.AppendLine($"    public {typeName} {varName};");
}
```

生成结果示例：

```csharp
[MessagePackObject]
public class Config_Lobby_Weapons
{
    [Key(0)] public int WeaponID;
    [Key(1)] public string Name;
    [Key(2)] public string ModleName;
    [Key(3)] public string IconName;
    [Key(4)] public string Description;
    [Key(5)] public int WeaponSpawnSlot;
    [Key(6)] public int WeaponEquipAnim;
}
```

### 2.3 反射创建实例并填充字段

```csharp
Type configType = Type.GetType(className + ", _HotUpdate.Config");
Type dictType = typeof(Dictionary<,>)
    .MakeGenericType(typeof(int), configType);
IDictionary dataDict = Activator.CreateInstance(dictType) as IDictionary;

object configInstance = Activator.CreateInstance(configType);
FieldInfo field = configType.GetField(varName);
object parsedValue = TypeParsers[typeName].Invoke(cellValue);
field.SetValue(configInstance, parsedValue);

dataDict.Add(keyId, configInstance);
```

### 2.4 输出 MessagePack 二进制

```csharp
byte[] bytes = MessagePackSerializer.Serialize(dictType, dataDict);
string outPath = bytesOutputFolderPath + className + ".bytes";
File.WriteAllBytes(outPath, bytes);
```

### 2.5 自动生成运行时路由

节选自生成的 `ConfigRegister.cs`：

```csharp
case "Config_Lobby_Weapons":
    var dict = MessagePackSerializer.Deserialize<
        Dictionary<int, Config_Lobby_Weapons>>(bytes);
    ConfigManager.Instance.RegisterTable(dict);
    break;
```

### 2.6 运行时按 Label 加载并按 Type 保存

```csharp
var handle = Addressables.LoadAssetsAsync<TextAsset>("Config", asset =>
{
    ConfigRegister.ParseAndRegister(asset.name, asset.bytes);
});

await handle.ToUniTask();
```

```csharp
public void RegisterTable<T>(Dictionary<int, T> dict)
{
    _allConfigs[typeof(T)] = dict;
}

public Dictionary<int, T> GetTable<T>()
{
    Type type = typeof(T);
    if (_allConfigs.TryGetValue(type, out object dictObject))
        return dictObject as Dictionary<int, T>;

    Debug.LogError($"找不到配置表 {type.Name}");
    return null;
}
```

## 3. 源码解释

### 为什么生成类和导出数据要分两步

导出二进制时，工具通过：

```csharp
Type.GetType("Config_Lobby_Weapons, _HotUpdate.Config")
```

寻找已经编译完成的类型。因此新增或修改表头后，需要先生成 C#，等待 Unity 编译，再导出 `.bytes`。当前编辑器面板给出的完整顺序是：

```text
1. 读表生成 C# 脚本
2. 导出二进制数据
3. 生成 AOT 静态解析代码
4. 编译并同步热更 DLL
```

只修改数据、不修改表头时，已有类没有变化，只需重新导出二进制并重新构建对应 Addressables 内容。

### 为什么按 `Type` 保存表

`Dictionary<Type, object>` 允许不同的强类型字典共存：

```text
typeof(Config_Lobby_Skins)   → Dictionary<int, Config_Lobby_Skins>
typeof(Config_Lobby_Weapons) → Dictionary<int, Config_Lobby_Weapons>
```

读取时不需要字符串查找或逐项转换，调用方能直接获得强类型字段和 IDE 补全。

### 为什么需要生成 `ConfigRegister`

Addressables 回调拿到的是资源名和 `byte[]`，而泛型反序列化需要在编译期知道具体类型。生成的 `switch` 是资源名到泛型类型的静态路由，也让 AOT 工具能够看到具体的 `Dictionary<int, Config_Xxx>` 使用。

### `asset.name` 是隐含协议

运行时路由依赖 `.bytes` 的资源名必须正好等于生成类名，例如：

```text
Config_Lobby_Weapons.bytes
asset.name == "Config_Lobby_Weapons"
```

修改 Address 不一定影响 `asset.name`，但重命名文件或生成类会影响路由。新增表后应重新生成 `ConfigRegister`。

## 4. 底层拓展说明

### 4.1 编辑器反射换取运行时性能

管线在编辑器使用 `Activator.CreateInstance`、`FieldInfo.SetValue` 和字符串解析。这些操作比直接赋值慢，但它们只发生在制表阶段。

运行时直接反序列化强类型字典，避免对每个单元格再次反射。

### 4.2 数值解析应固定 Culture

当前工具使用 `float.Parse(val)`。不同系统区域可能把 `.` 或 `,` 当作不同的小数分隔符。构建机上建议固定：

```csharp
float.Parse(val, CultureInfo.InvariantCulture);
int.Parse(val, CultureInfo.InvariantCulture);
```

否则同一份 Excel 在不同开发机或 CI 环境可能得到不同结果。

### 4.3 空单元格意味着保留类型默认值

当前逻辑遇到空值会 `continue`：

```csharp
if (string.IsNullOrWhiteSpace(varName) ||
    string.IsNullOrWhiteSpace(cellValue))
    continue;
```

于是 `int` 为 0、`bool` 为 false、引用类型为 null。需要区分“允许为空”和“漏填”时，应在表头增加必填约束，并在导出阶段阻止产出坏表。

### 4.4 重复 ID 会立即失败

`dataDict.Add(keyId, configInstance)` 在重复 ID 时抛异常。这比后写覆盖前写更适合配置表，因为重复主键通常是数据错误。

### 4.5 当前错误提示中的 Label 名称不一致

运行时实际加载的 Label 是 `Config`，但 `GetTable` 的错误日志仍提示检查 `ConfigData`。排错时应以 `LoadAllConfigsAsync` 的 `Config` 为准，并统一日志文字。

### 4.6 必填表建议直接抛异常

当前 `GetTable<T>` 找不到表时记录错误并返回 null，调用方随后可能在别处空引用。更利于定位的写法：

```csharp
public Dictionary<int, T> GetTable<T>()
{
    if (_allConfigs.TryGetValue(typeof(T), out object value))
        return (Dictionary<int, T>)value;

    throw new KeyNotFoundException(
        $"配置表 {typeof(T).Name} 尚未注册。请检查 Config Label 与加载顺序。");
}
```

## 5. 应用示例

### 示例：新增 Lobby 宠物表

Excel Sheet：`Lobby_Pets`

| 宠物 ID | 名称 | 模型地址 | 图标地址 |
| --- | --- | --- | --- |
| PetID | Name | ModelAddress | IconAddress |
| int | string | string | string |
| 1 | 小机器人 | Lobby/Pets/Robot | Lobby/Icons/Robot |

执行流程：

1. 运行“读表生成 C# 脚本”，得到 `Config_Lobby_Pets.cs`。
2. 等待 `_HotUpdate.Config` 编译完成。
3. 导出 `.bytes`，得到 `Config_Lobby_Pets.bytes`。
4. 重新生成 `ConfigRegister.cs` 和 MPC Resolver。
5. 给 `.bytes` 资源配置 `Config` Label。
6. 运行时读取：

```csharp
Dictionary<int, Config_Lobby_Pets> pets =
    ConfigManager.Instance.GetTable<Config_Lobby_Pets>();

Config_Lobby_Pets robot = pets[1];
string modelAddress = robot.ModelAddress;
```

新增表成功的标准不是“文件生成了”，而是编辑器生成、程序集编译、二进制导出、静态 Formatter、Addressables Label 和运行时注册全部形成闭环。
