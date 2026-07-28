# MessagePack：序列化、存储、读取与 AOT

## 1. 方法/架构介绍

MessagePack 是二进制序列化格式。当前项目用它保存大量只读 Config，而不是保存可手改的用户设置。

项目中的完整职责分配：

- `Config_Xxx.cs`：定义可序列化 Schema。
- `[MessagePackObject]` 与 `[Key(n)]`：定义字段到二进制位置的映射。
- `ExcelToMessagePackGenerator`：把字典序列化为 `.bytes`。
- `ConfigRegister`：把 `.bytes` 反序列化为具体泛型字典。
- `MessagePackCompilerTool`：调用 `mpc` 生成静态 Formatter。
- `MessagePackGenerated.cs`：IL2CPP/HybridCLR 可直接调用的序列化代码。
- 启动边界：在第一次反序列化前注册 GeneratedResolver。

MessagePack 解决的是“对象与字节之间的转换”，文件存储、Addressables 下载、版本迁移和资源释放仍由其他层负责。

## 2. 源码展示

### 2.1 使用数字 Key 定义 Schema

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

### 2.2 编辑器序列化并写入磁盘

节选自 `ExcelToMessagePackGenerator`：

```csharp
byte[] bytes = MessagePackSerializer.Serialize(dictType, dataDict);
File.WriteAllBytes(
    bytesOutputFolderPath + className + ".bytes",
    bytes);
```

### 2.3 运行时反序列化

```csharp
Dictionary<int, Config_Lobby_Weapons> table =
    MessagePackSerializer.Deserialize<
        Dictionary<int, Config_Lobby_Weapons>>(bytes);
```

通用解析器中同样是这个核心调用：

```csharp
public Dictionary<K, V> Parse<K, V>(byte[] rawData)
{
    return MessagePackSerializer.Deserialize<Dictionary<K, V>>(rawData);
}
```

### 2.4 生成 AOT 静态 Formatter

节选自 `Assets/Editor/Tools/MessagePackCompilerTool.cs`：

```csharp
string inputPath = Path.Combine(
    Application.dataPath,
    "_HotUpdate", "Scripts", "Config", "Generated");

string outputPath = Path.Combine(
    Application.dataPath,
    "_HotUpdate", "Scripts", "Config", "MessagePackGenerated.cs");

string arguments =
    $"-i \"{inputPath}\" -o \"{outputPath}\" " +
    "-n ProjectGame.HotFix.Resolvers";
```

### 2.5 第一次反序列化前注册 Resolver

当前注册代码位于热更启动边界；这里只展示 MessagePack 必需部分：

```csharp
StaticCompositeResolver.Instance.Register(
    GeneratedResolver.Instance,
    StandardResolver.Instance);

var options = MessagePackSerializerOptions.Standard
    .WithResolver(StaticCompositeResolver.Instance);

MessagePackSerializer.DefaultOptions = options;
```

## 3. 源码解释

### `[MessagePackObject]`

它告诉 MessagePack 这个类型由显式标记的成员组成。未标记字段不会自动进入当前数字 Key Schema。

### `[Key(n)]`

数字 Key 通常编码为数组位置。`Key(0)` 对应第 0 项，`Key(6)` 对应第 6 项。数字模式比字符串字段名更紧凑，但数字位置成为长期兼容协议。

### 为什么序列化整个字典

运行时最常见查询是 `table[id]`。直接保存 `Dictionary<int, Config_Xxx>`，反序列化后无需再遍历 List 建索引。

### 为什么需要静态 Resolver

部分 MessagePack 运行方式会动态生成序列化代码。IL2CPP 是 AOT 编译环境，运行时不能随意生成 IL。`mpc` 在编辑器阶段把 Formatter 写成普通 C#，随后与热更程序集一起编译。

生成文件中的核心逻辑类似：

```csharp
writer.WriteArrayHeader(7);
writer.Write(value.WeaponID);
formatter.Serialize(ref writer, value.Name, options);
```

反序列化则读取数组长度并按 Key 写回字段。

### Resolver 注册顺序

`GeneratedResolver` 放在 `StandardResolver` 前面，Config 类型优先使用静态生成 Formatter，普通基础类型再由标准 Resolver 处理。

注册必须发生在：

```text
MessagePackSerializer.Deserialize(...)
```

第一次调用之前，否则编辑器可能正常、AOT 真机却失败。

## 4. 底层拓展说明

### 4.1 数字 Key 的兼容规则

较安全的演进：

- 只在末尾新增更大的 Key；
- 新字段提供合理默认值；
- 老数据缺少末尾字段时允许使用默认值。

危险操作：

- 调换两个字段的 Key；
- 复用已经删除字段的 Key 表达不同含义；
- 修改同一个 Key 的数据类型；
- 因 Excel 调整列顺序而自动改变全部 Key。

当前生成器直接使用 Excel 列索引作为 Key，因此“拖动列顺序”属于 Schema 变更，不只是排版。

### 4.2 数字 Key 与字符串 Key

数字 Key：体积更小、速度更快、对字段调整更敏感。

字符串 Key：数据包含成员名，体积更大，但字段增删和顺序变化更灵活。大量稳定配置适合数字 Key；长期跨版本存档需要更明确的迁移制度。

### 4.3 压缩不是默认必需

MessagePack 已经比 JSON 紧凑。还可以配置 LZ4：

```csharp
var options = MessagePackSerializerOptions.Standard
    .WithResolver(StaticCompositeResolver.Instance)
    .WithCompression(MessagePackCompression.Lz4BlockArray);
```

写入和读取必须使用兼容选项。小表压缩可能得不偿失；应以构建产物大小和加载耗时实测决定。

### 4.4 不要对不可信数据无限反序列化

来自远端或 Mod 的二进制应限制文件大小、表数量和集合长度，并捕获格式异常。Config 是受构建流程控制的资源，信任边界比玩家上传数据更高，但下载损坏仍需处理。

### 4.5 生成文件不可手改

`MessagePackGenerated.cs` 与 `Generated/ConfigRegister.cs` 都会被工具覆盖。需要改变行为时应修改生成器或 Schema 源，而不是编辑产物。

## 5. 应用示例

### 示例 A：保存和读取普通二进制存档

```csharp
[MessagePackObject]
public sealed class PlayerCache
{
    [Key(0)] public int Version;
    [Key(1)] public int LastCharacterId;
    [Key(2)] public long LastLoginUnixTime;
}
```

```csharp
public static void SaveCache(string path, PlayerCache data)
{
    byte[] bytes = MessagePackSerializer.Serialize(data);
    string tempPath = path + ".tmp";
    File.WriteAllBytes(tempPath, bytes);

    if (File.Exists(path))
        File.Replace(tempPath, path, path + ".bak");
    else
        File.Move(tempPath, path);
}
```

```csharp
public static PlayerCache LoadCache(string path)
{
    if (!File.Exists(path))
        return new PlayerCache { Version = 1 };

    byte[] bytes = File.ReadAllBytes(path);
    PlayerCache data = MessagePackSerializer.Deserialize<PlayerCache>(bytes);

    if (data.Version > 1)
        throw new InvalidDataException("存档版本高于当前客户端支持范围");

    return data;
}
```

### 示例 B：Config 的正确使用顺序

```csharp
RegisterGeneratedResolver();
await ConfigManager.Instance.LoadAllConfigsAsync();

Dictionary<int, Config_Lobby_Weapons> weapons =
    ConfigManager.Instance.GetTable<Config_Lobby_Weapons>();
```

顺序不能颠倒。Resolver 注册解决“如何反序列化”，Config 加载解决“字节从哪里来”，GetTable 只读取已经注册的内存字典。

