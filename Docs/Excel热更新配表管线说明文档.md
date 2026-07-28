# Excel 资源及热更新配表管线说明文档

## 一、管线概览

本项目的配置表管线是一套**从 Excel 数据到运行时热更读取**的完整流水线，涵盖了编辑阶段的代码生成、数据转换、AOT 安全编译，以及运行阶段的高效加载、解析、注入。管线由以下几个核心环节串联而成：

```
Excel(.xlsx) → Config_xxx.cs (C# 数据类) → MessagePack(.bytes) → Addressables Group
                                                      ↓
                              mpc 生成静态解析器 (_HotUpdate/Config/MessagePackGenerated.cs)
                                                      ↓
                              HybridCLR 编译 HotFix.dll → DLLs/*.bytes
                                                      ↓
                              运行时: Addressables 加载 → ConfigManager 解析 → GetTable<T>() 提供数据
```

整个调用链的终点在 `HotFixEntry.StartGame()`，它串联起 MessagePack 解析器注册、ConfigManager 初始化、以及场景与配表的并行加载。

---

## 二、核心文件清单

| 文件路径 | 类型 | 说明 |
|----------|------|------|
| `Assets/Editor/Tools/ExcelToMessagePackGenerator.cs` | Editor 工具 | Excel → C# 数据类 + MessagePack 二进制转换 |
| `Assets/Editor/Tools/MessagePackCompilerTool.cs` | Editor 工具 | 调用 mpc 工具生成 AOT 静态解析器 |
| `Assets/Editor/Tools/HotUpdateBuilderTool.cs` | Editor 工具 | HybridCLR 热更 DLL 编译与拷贝 |
| `Assets/_HotUpdate/Scripts/Config/ConfigManager.cs` | 运行时 | 配置表加载/缓存/查询管理器 |
| `Assets/_HotUpdate/Scripts/Config/IDataParser.cs` | 运行时 | 数据解析策略接口 |
| `Assets/_HotUpdate/Scripts/Config/MessagePackBinaryParser.cs` | 运行时 | MessagePack 二进制解析策略实现 |
| `Assets/_HotUpdate/Scripts/Config/JsonModParser.cs` | 运行时 | JSON Mod 文本解析策略（**待完整实现**） |
| `Assets/_HotUpdate/Scripts/Config/Generated/ConfigRegister.cs` | 自动生成 | 配表注册分发器 |
| `Assets/_HotUpdate/Scripts/Config/Generated/Config_Item.cs` | 自动生成 | 示例配置数据类（物品表） |
| `Assets/_HotUpdate/Scripts/Config/Generated/Config_Lobby_Skins.cs` | 自动生成 | 示例配置数据类（大厅皮肤表） |
| `Assets/_HotUpdate/Scripts/Config/MessagePackGenerated.cs` | 自动生成 | mpc 生成的 AOT 静态解析器（Formatter / Resolver） |
| `Assets/_HotUpdate/Scripts/GameProcess/HotFixEntry.cs` | 运行时 | 热更域入口，管线启动点 |
| `Assets/_HotUpdate/Scripts/GameProcess/UI/LoadingUI.cs` | 运行时 | 加载界面 UI 控制 |
| `Assets/_HotUpdate/Scripts/GameProcess/UI/LoadingView.cs` | 运行时 | 加载界面视图基类 |

---

## 三、编辑阶段（Editor Pipeline）

### 3.1 步骤一：Excel → MessagePack 数据转换

**工具脚本**: `ExcelToMessagePackGenerator.cs`

此工具负责将策划编辑的 Excel 文件转换为运行时可直接使用的格式，生成产物包括：

1. **C# 数据类**（如 `Config_Item.cs`），存放于 `Assets/_HotUpdate/Scripts/Config/Generated/`
2. **MessagePack 二进制文件**（`.bytes`），存放于 `Assets/StreamingAsset/`（隶属于 Addressables Group "Config"）
3. **ConfigRegister.cs** 自动更新，为每张表生成对应的反序列化与注册逻辑

**关键特性**：
- 数据类自动标注 `[MessagePackObject]` 和 `[Key(n)]` 特性
- Excel 第一行作为字段名，自动生成 C# 属性
- 二进制采用 MessagePack 格式，比 JSON/XML 体积更小、解析更快
- 自动更新 `ConfigRegister.cs`，实现表名到反序列化逻辑的 switch-case 路由

---

### 3.2 步骤二：MessagePack AOT 静态解析器生成

**工具脚本**: `MessagePackCompilerTool.cs`

由于项目使用 HybridCLR（IL2CPP 环境），在运行时无法动态生成 IL 代码，因此必须**在编译阶段提前生成好所有反序列化代码**。此工具调用 mpc（MessagePack-CSharp 编译工具）完成这一工作。

**关键参数**：
- **输入目录**: `Assets/_HotUpdate/Scripts/Config/Generated` —— 扫描其中的 `[MessagePackObject]` 数据类
- **输出文件**: `Assets/_HotUpdate/Scripts/Config/MessagePackGenerated.cs`
- **命名空间**: `ProjectGame.HotFix.Resolvers`

**生成产物 `MessagePackGenerated.cs` 包含**：
- `GeneratedResolver` —— 解析器组合入口，实现 `IFormatterResolver`
- `GeneratedResolverGetFormatterHelper` —— 类型查找表，将 C# 类型映射到对应 Formatter
- 每个数据类的专用 `Formatter`（如 `Config_ItemFormatter`、`Config_Lobby_SkinsFormatter`），提供 `Serialize` / `Deserialize` 方法

**工作流程**：
1. 清理旧的 `MessagePackGenerated.cs`
2. 执行 `mpc -i <input> -o <output> -n <namespace>` 命令
3. 刷新 AssetDatabase

---

### 3.3 步骤三：HybridCLR 热更 DLL 编译与拷贝

**工具脚本**: `HotUpdateBuilderTool.cs`

此工具触发 HybridCLR 的热更程序集编译，并将编译产物拷贝到 Unity 的资源目录以供后续 Addressables 加载。

**`BuildAndCopyHotUpdateDlls()` 方法流程**：
1. 获取当前构建目标平台（PC / Android / iOS）
2. 调用 `CompileDllCommand.CompileDll(target)` 执行 HybridCLR 编译
3. 从 `SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target)` 获取编译输出目录
4. 将热更程序集列表中的所有 `.dll` 文件拷贝到 `Assets/_HotUpdate/DLLs/`，并追加 `.bytes` 后缀
5. 刷新 AssetDatabase

> **注意**: 生成的 `.bytes` 文件需要挂载到 Addressables Group 中以供运行时加载。

---

## 四、运行时解析管线（Runtime Pipeline）

### 4.1 策略模式接口 `IDataParser`

**文件**: `IDataParser.cs`

```csharp
public interface IDataParser
{
    Dictionary<K, V> Parse<K, V>(byte[] rawData);
}
```

此接口是数据解析的抽象层，允许在运行时根据场景切换不同的解析策略（如标准的 MessagePack 二进制、以及未来的 JSON Mod 补丁解析）。

---

### 4.2 MessagePack 二进制解析器 `MessagePackBinaryParser`

**文件**: `MessagePackBinaryParser.cs`

**功能**: 将 Addressables 加载的原始二进制数据（MessagePack 格式）直接反序列化为 `Dictionary<K, V>`。核心逻辑仅一行：

```csharp
return MessagePack.MessagePackSerializer.Deserialize<Dictionary<K, V>>(rawData);
```

由于后续 mpc 生成的静态解析器已在 `HotFixEntry.StartGame()` 中注册，此处的反序列化可以直接使用预编译的 Formatter，避免 IL2CPP 环境下的反射限制。

---

### 4.3 JSON Mod 解析器 `JsonModParser`（**待完整实现**）

**文件**: `JsonModParser.cs`

**设计意图**: 支持以明文 JSON 格式的本地 Mod 补丁覆盖基础配表数据。

**当前状态**: 框架已搭建，但核心的 JSON 反序列化逻辑尚未实现（TODO 标记）。当前方法仅返回空字典以防御报错。未来需引入 Newtonsoft.Json 或其他 JSON 库完成解析，并与基础数据进行合并。

---

### 4.4 配表注册分发器 `ConfigRegister`

**文件**: `ConfigRegister.cs`（自动生成）

此文件由 `ExcelToMessagePackGenerator` 自动生成，包含一个 `ParseAndRegister` 方法，根据 Addressables 加载时的资源名（addressableName）来决定反序列化目标类型，并调用 `ConfigManager.Instance.RegisterTable()` 注册到全局配置字典中。

```csharp
public static void ParseAndRegister(string addressableName, byte[] bytes)
{
    switch (addressableName)
    {
        case "Config_Item":
            var dict = MessagePackSerializer.Deserialize<Dictionary<int, Config_Item>>(bytes);
            ConfigManager.Instance.RegisterTable(dict);
            break;
        // ... 每张表一个 case
    }
}
```

> **注意**: 当前是串行反序列化，未来可考虑结合 UniTask 的并行加载。

---

### 4.5 配置管理器 `ConfigManager`

**文件**: `ConfigManager.cs`

这是整个运行时管线的核心调度器，负责配置文件的全生命周期管理。

#### 核心字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `_allConfigs` | `Dictionary<Type, object>` | 所有已加载的配置表，Key 为数据类类型，Value 为 `Dictionary<int, T>` |
| `_baseDataParser` | `IDataParser` | 基础数据解析器（默认：MessagePackBinaryParser） |
| `_modDataParser` | `IDataParser` | Mod 补丁解析器（默认：JsonModParser） |

#### 核心方法

| 方法 | 说明 |
|------|------|
| `Init()` | 初始化解析管线，创建 `MessagePackBinaryParser` 和 `JsonModParser` 实例 |
| `LoadTable<T>(byte[] rawData)` | 单表加载：先用 `_baseDataParser` 解析二进制，再预留 Mod 合并接入点 |
| `RegisterTable<T>(Dictionary<int, T>)` | 将已解析的表注册到全局缓存 |
| `GetTable<T>()` | O(1) 时间复杂度获取指定配置表（无 GC） |
| `LoadAllConfigsAsync()` | 一键全量加载：通过 Addressables 批量拉取标签 `"Config"` 下的所有配表资源，利用 UniTask 的零 GC 等待环异步完成 |
| `ClearAll()` | 清空所有缓存配置 |

#### 数据加载流程

```
LoadAllConfigsAsync()
    └─ Addressables.LoadAssetsAsync<TextAsset>("Config", callback)
            └─ ConfigRegister.ParseAndRegister(asset.name, asset.bytes)
                    ├─ MessagePackSerializer.Deserialize<T>(bytes)
                    └─ ConfigManager.Instance.RegisterTable(dict)
```

提供**两种加载模式**：
1. **全量加载** (`LoadAllConfigsAsync`)：适合游戏启动时一次性拉取全部配表
2. **单表加载** (`LoadTable<T>`)：适合按需加载特定配置表（如某个 UI 界面需要的表）

---

## 五、启动调用链（HotFixEntry → 配表生效）

`HotFixEntry.cs` 是热更域的入口脚本，`StartGame()` 方法串联起整个管线：

```
1. RegisterMessagePackResolver()
   └─ StaticCompositeResolver 注册 GeneratedResolver + StandardResolver
      └─ 覆盖 MessagePackSerializer.DefaultOptions

2. ConfigManager.Instance.Init()
   └─ 创建 MessagePackBinaryParser 实例
   └─ 创建 JsonModParser 实例

3. EnterLobbyScene()
   └─ await LoadingUI.Show("正在进入大厅...")
   └─ UniTask.WhenAll(
        sceneLoadTask: Addressables.LoadSceneAsync("LobbyScene"),
        configLoadTask: ConfigManager.Instance.LoadAllConfigsAsync()
      )
   └─ LoadingUI.Hide()
   └─ NetworkManager.Singleton.StartHost()
```

**关键时序**：
- 场景加载与配置表加载**并行执行**（`UniTask.WhenAll`），最大化启动效率
- `LoadingUI.Show/Hide` 提供可视化的加载反馈
- 配置表加载完毕后，所有表已位于 `ConfigManager._allConfigs` 中，业务代码可通过 `ConfigManager.Instance.GetTable<T>()` 随时获取

---

## 六、运行时数据获取接口

任何热更业务代码通过以下方式获取配置数据：

```csharp
// 获取物品表（Config_Item）
var itemDict = ConfigManager.Instance.GetTable<Config_Item>();

// 按 ID 查询特定物品
if (itemDict.TryGetValue(1001, out Config_Item item))
{
    Debug.Log($"物品名: {item.Name}, 描述: {item.Description}");
}
```

**性能特征**：
- `GetTable<T>()`: O(1) 字典查找，零 GC
- 返回的 `Dictionary<int, T>` 是引用传递，不产生复制开销

---

## 七、已有数据表示例

### Config_Item（物品表）

| 字段 | Key | 类型 | 说明 |
|------|-----|------|------|
| ItemID | 0 | int | 物品 ID |
| Name | 1 | string | 物品名称 |
| MaxStackSize | 2 | int | 最大堆叠数量 |
| IconPath | 3 | string | 图标路径 |
| Description | 4 | string | 物品描述 |

### Config_Lobby_Skins（大厅皮肤表）

| 字段 | Key | 类型 | 说明 |
|------|-----|------|------|
| (占位) | 0 | - | **目前反序列化代码中该位为空操作** |
| Name | 1 | string | 皮肤名称 |
| ModleName | 2 | string | 模型名称 |
| IconName | 3 | string | 图标名称 |
| Description | 4 | string | 描述 |

> **注意**: `Config_Lobby_Skins` 的 Formatter 中 Key 0 位置的代码块为空（`case 0: break;`），可能是 mpc 生成的预留位，建议检查 Excel 定义确认是否需要调整。

---

## 八、待实现 / TODO 项目

### 8.1 JsonModParser JSON 反序列化（高优先级）

**位置**: `JsonModParser.Parse<K, V>()`  
**当前状态**: 方法骨架存在，JSON 反序列化逻辑为 TODO 占位  
**需完成**:
1. 引入 JSON 序列化库（推荐 Newtonsoft.Json 或 System.Text.Json）
2. 完成 `JsonConvert.DeserializeObject<Dictionary<K, V>>(jsonStr)` 的调用
3. 确保与 `MessagePackBinaryParser` 的泛型接口一致

### 8.2 Mod 合并逻辑

**位置**: `ConfigManager.LoadTable<T>()` 中的 TODO 注释  
**当前状态**: 仅有注释 `ModMergeUtility.Merge(...)` 占位  
**需完成**:
1. 实现 Mod 补丁文件的管理（从何处加载 JSON Mod 文件）
2. 实现合并算法：将 Mod 字典覆盖合并到基础字典
3. 集成到全量加载流程中

### 8.3 配置表并行反序列化

**位置**: `ConfigRegister.ParseAndRegister()`  
**当前状态**: switch-case 串行执行  
**优化方向**: 结合 UniTask，对多张配置表的反序列化任务进行并行调度，进一步缩短启动时间

### 8.4 Config_Lobby_Skins Key 0 字段

**位置**: `MessagePackGenerated.cs` 中 `Config_Lobby_SkinsFormatter.Deserialize()` 的 case 0  
**当前状态**: case 0 仅有 `break;`，未读取任何数据  
**建议**: 确认 Excel 定义中是否包含第一个字段（可能是 ID），若包含则更新 Formatter 或重新运行 mpc

### 8.5 ExcelToMessagePackGenerator 详细实现

**当前状态**: 该脚本的完整实现未在本次分析中展开（首行截断加载）  
**建议**: 确保 Excel 解析逻辑已覆盖所有需要的字段类型（int, string, float, bool 等），并支持多 Sheet 转换

---

## 九、开发工作流

### 新增一张配置表的完整流程

1. **策划**：在 Excel 文件中新增 Sheet，第一行填写字段名
2. **程序**：运行 `ExcelToMessagePackGenerator` 工具
   - 自动生成 `Config_XXX.cs` 数据类
   - 自动更新 `ConfigRegister.cs`
   - 自动生成 `Config_XXX.bytes`
3. **程序**：运行 `MessagePackCompilerTool.GenerateMPC()`
   - 重新生成 `MessagePackGenerated.cs`（包含新 Formatter）
4. **程序**：运行 `HotUpdateBuilderTool.BuildAndCopyHotUpdateDlls()`
   - 重新编译 HotFix.dll 并拷贝到 DLLs 目录
5. **程序**：在业务代码中调用 `ConfigManager.Instance.GetTable<Config_XXX>()` 获取数据

### 仅修改配置数据（不涉及结构变更）

1. 修改 Excel 文件中的数据
2. 运行 `ExcelToMessagePackGenerator`（仅更新 `.bytes` 文件）
3. 重新打包 Addressables，热更下发

---

## 十、架构示意图

```
┌─────────────────────────────────────────────────────────────┐
│                     编辑阶段 (Editor)                        │
│                                                              │
│  ExcelToMessagePackGenerator                                │
│    ├─ Excel → Config_xxx.cs (数据类)                        │
│    ├─ Excel → Config_xxx.bytes (MessagePack 二进制)         │
│    └─ 更新 ConfigRegister.cs (注册代码)                     │
│                                                              │
│  MessagePackCompilerTool (mpc)                              │
│    └─ 扫描 [MessagePackObject] → MessagePackGenerated.cs    │
│                                                              │
│  HotUpdateBuilderTool (HybridCLR)                           │
│    └─ 编译 HotFix.dll → DLLs/*.bytes                        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                   运行时 (Runtime)                            │
│                                                              │
│  HotFixEntry.StartGame()                                    │
│    ├─ RegisterMessagePackResolver()                         │
│    │     └─ GeneratedResolver + StandardResolver             │
│    ├─ ConfigManager.Init()                                  │
│    │     ├─ new MessagePackBinaryParser()                   │
│    │     └─ new JsonModParser() [待完善]                    │
│    └─ EnterLobbyScene()                                     │
│          └─ UniTask.WhenAll(                                │
│               Addressables.LoadSceneAsync("LobbyScene"),     │
│               ConfigManager.LoadAllConfigsAsync()           │
│             )                                               │
│                                                              │
│  ConfigManager.LoadAllConfigsAsync()                        │
│    └─ Addressables.LoadAssetsAsync<TextAsset>("Config")     │
│          └─ ConfigRegister.ParseAndRegister(name, bytes)    │
│               ├─ MessagePackSerializer.Deserialize<T>(bytes) │
│               └─ ConfigManager.RegisterTable(dict)          │
│                                                              │
│  业务代码调用:                                               │
│    ConfigManager.Instance.GetTable<Config_Item>()            │
└─────────────────────────────────────────────────────────────┘
```

---

## 十一、关键技术决策说明

| 决策 | 原因 |
|------|------|
| 使用 MessagePack 而非 JSON | 二进制格式体积更小、解析更快，适合移动端和大量配置数据的场景 |
| 使用 mpc 生成静态解析器 | HybridCLR（IL2CPP）不允许运行时动态生成 IL 代码，必须提前生成为 AOT 安全代码 |
| 使用策略模式 (`IDataParser`) | 支持未来扩展更多数据源（JSON Mod、XML 等），遵循开闭原则 |
| Addressables 标签 "Config" | 通过标签一键加载全部配表，避免每个表单独维护加载逻辑 |
| UniTask 并行加载 | 场景加载和配表加载并行执行，显著缩短启动等待时间 |
| 配置数据以 `Dictionary<int, T>` 存储 | ID 为 Key 的字典查询为 O(1)，适合游戏中大量按 ID 查找的场景 |

---

> **文档版本**: v1.0  
> **最后更新**: 2026-07-12  
> **维护者**: 项目组全体