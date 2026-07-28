# HotFix 程序集结构

## 依赖规则

所有项目热更程序集统一使用 `HotFix` 前缀。依赖只能从入口、页面和场景逻辑流向更底层的数据、网络与 AOT 基础设施，禁止底层程序集反向引用 UI。

```text
AOT_Bootstrap
    └─ 反射加载 HotFix.Entry

HotFix.Entry
    ├─ HotFix.Gameplay
    └─ HotFix.Config

HotFix.Lobby.UI
    ├─ HotFix.Lobby.World
    ├─ HotFix.Lobby.Network
    ├─ HotFix.Settings
    └─ HotFix.Config

HotFix.Lobby.World
    ├─ HotFix.Lobby.Network
    └─ HotFix.Config

HotFix.Lobby.Network
    └─ HotFix.Config

HotFix.Gameplay
    ├─ AOT_Core
    └─ HotFix.Config
```

## 程序集职责

| 程序集 | 职责 |
|---|---|
| `AOT_Bootstrap` | 初始化 Addressables、加载 AOT 元数据和全部热更 DLL，再反射调用热更入口 |
| `AOT_Core` | 稳定的音频、事件、对象池、相机震动和 NGO 通用组件 |
| `HotFix.Config` | 配置结构、MessagePack 解析器、配置注册和 Addressables 配表加载 |
| `HotFix.Entry` | 注册 MessagePack Resolver、初始化配置并进入大厅 |
| `HotFix.Lobby.Network` | 大厅玩家同步结构、连接审批、RPC、准备状态和转场发起 |
| `HotFix.Lobby.World` | 展位布局、3D 玩家模型、装备模型、挂点和大厅世界交互 |
| `HotFix.Settings` | 设置数据、持久化、音频应用和 Input System 重绑 |
| `HotFix.Lobby.UI` | Overview、ItemSelect、Setting 页面和大厅 UI 状态导航 |
| `HotFix.DebugTools` | GM 面板与调试命令注册 |
| `HotFix.Gameplay` | 当前正式游戏场景逻辑的临时统一程序集，等待后续游戏代码重构 |

## HybridCLR 与 Addressables

`ProjectSettings/HybridCLRSettings.asset` 中登记的每一个热更 asmdef，都必须在 `Assets/_HotUpdate/DLLS` 中拥有同名的 `.dll.bytes` 文件。

`Tools/HotUpdate/Build And Sync DLLs` 会执行以下操作：

1. 编译 HybridCLR 热更程序集。
2. 复制全部 DLL 为 `.dll.bytes`。
3. 将 DLL 移入 `HotfixDLLs` Addressables Group。
4. 统一设置 `Hotfix_DLL` 标签。
5. 移除已经不属于 HybridCLR 配置的旧热更 Addressables 条目。

新增或删除热更程序集后必须运行一次该命令。

## 后续游戏场景拆分

`HotFix.Gameplay` 当前只是 GameProcess 的过渡容器。正式重构时应按照实际领域拆分，例如：

- `HotFix.Gameplay.Session`
- `HotFix.Gameplay.Player`
- `HotFix.Gameplay.Combat`
- `HotFix.Gameplay.World`
- `HotFix.Gameplay.UI`

拆分时由具体功能程序集依赖 `AOT_Core`、`HotFix.Config` 或未来的跨场景 Session 数据程序集，不允许重新依赖 `HotFix.Lobby.UI` 或 `HotFix.Lobby.World`。
