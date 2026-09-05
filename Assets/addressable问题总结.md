# Addressables、HybridCLR 与 Windows Player 导出问题总结

本文记录本项目从 Unity Editor 正常运行，到 Windows IL2CPP Player 能够通过 Addressables 下载热更内容、加载 HybridCLR 程序集、读取配表并进入 LobbyScene 的完整排障过程。

适用环境：Unity 2022.3.44f1c1、Addressables 1.22.3、HybridCLR、Windows Standalone、局域网 HTTP Hosting。

## 一、最终验证结果

最终 Windows Development Player 已完成以下链路：

1. 启动 BootStrapScene。
2. 初始化 Addressables，Catalog 请求超时为 15 秒。
3. 资源服务器与 Player 在同一台电脑时，把本机局域网 IP 自动改写为 `127.0.0.1`；其他电脑仍使用 Profile 中的局域网 IP。
4. 下载并加载 5 个 AOT 补充元数据程序集。
5. 按依赖顺序加载 9 个 HotFix 程序集。
6. 初始化 MessagePack 与 ConfigManager。
7. 成功加载 8 张二进制配表。
8. 通过 Addressables 加载 LobbyScene。
9. 成功进入联机大厅，未再出现 HotFix MonoBehaviour 的 Missing Script。

当前 9 个 HotFix 程序集的有效装载顺序：

```text
HotFix.Gameplay.Network
-> HotFix.Config
-> HotFix.DebugTools
-> HotFix.Settings
-> HotFix.Lobby.Network
-> HotFix.Lobby.World
-> HotFix.Gameplay
-> HotFix.Lobby.UI
-> HotFix.Entry
```

当前 AOT 补充元数据集合：

```text
mscorlib.dll
UnityEngine.CoreModule.dll
Unity.Netcode.Runtime.dll
UniTask.Addressables.dll
DOTween.dll
```

## 二、必须理解的三层产物

### 1. Player 本体

包含 IL2CPP/AOT 代码、BootStrapScene、StreamingAssets 中的 Addressables 初始化数据，以及远端 Catalog 的初始地址。

Player 不是远端资源服务器。复制给另一台电脑时必须复制整个构建目录，不能只复制 exe。

### 2. ServerData/StandaloneWindows64

这是 Addressables 远端发布目录，包含：

- Catalog JSON；
- Catalog hash；
- HotFix DLL bundle；
- AOT 元数据 bundle；
- Config、Scene、Prefab、UI 等 bundle。

局域网测试时由 Unity Addressables Hosting 托管；正式环境应部署到稳定的 HTTP/HTTPS、CDN 或对象存储。

### 3. HybridCLR 生成物

主要包括：

- `HybridCLRData/HotUpdateDlls/<Target>`：热更 DLL；
- `HybridCLRData/AssembliesPostIl2CppStrip/<Target>`：IL2CPP 裁剪后的 AOT DLL，用于补充元数据；
- MethodBridge、AOTGenericReferences、link.xml 等生成代码和裁剪配置。

这三层有各自的版本，任何一层仍是旧产物，都可能出现“Editor 正常、Player 失败”。

## 三、本次遇到的问题与根因

### 问题 1：UnityLinker 无法解析 HotFix.Gameplay

典型错误：

```text
Mono.Cecil.AssemblyResolutionException:
Failed to resolve assembly: 'HotFix.Gameplay'
```

根因不是 Addressables 下载失败，而是 AOT/Player 构建阶段仍有程序集要求 UnityLinker 解析 HotFix.Gameplay，但 HotFix.Gameplay 已被 HybridCLR 从 AOT Player 中排除。

本项目中的直接来源是 Runtime 测试程序集被当成普通 Player 程序集参与构建。处理方式：

- 测试 asmdef 设置 `autoReferenced: false`；
- 添加 `UNITY_INCLUDE_TESTS` define constraint；
- 使用 TestAssemblies 可选引用；
- 不让测试程序集进入普通 Player 依赖图。

面试要点：Linker 阶段的“找不到程序集”和运行时 `Assembly.Load` 失败不是同一阶段，排查入口不同。

### 问题 2：HotUpdateAssemblies、Addressables DLL 列表与 asmdef 依赖不一致

需要同时核对：

- HybridCLRSettings 的 `hotUpdateAssemblyDefinitions`；
- 每个 asmdef 的 References；
- `Assets/_HotUpdate/DLLS` 中实际同步的 bytes；
- Addressables `Hotfix_DLL` 标签中的实际条目；
- Catalog 中实际记录的 bundle。

只在 HybridCLR 中加入程序集但未同步到 Addressables，会在运行时解析依赖时失败；只把 DLL 放入 Addressables 但未加入 HybridCLR 热更列表，则可能被错误当作 AOT Assembly。

程序集改名或拆分后，必须清理旧 asmdef 引用、旧 bytes 条目和旧 Addressables Entry。不能只删除源码目录。

### 问题 3：HTTP 被 PlayerSettings 禁止

典型错误：

```text
Insecure connection not allowed
```

原因是局域网 Hosting 使用 HTTP，而 Player 的 Insecure HTTP 策略为 NotAllowed。

开发环境处理：

- `PlayerSettings.insecureHttpOption = DevelopmentOnly`；
- Player 必须使用 Development Build。

正式环境处理：使用 HTTPS，并把策略恢复为 NotAllowed。不要为了方便把正式包永久设置为 AlwaysAllowed。

### 问题 4：Remote.LoadPath 已修改，但旧 Player 仍访问旧 IP

Remote.LoadPath 会影响两类持久产物：

- Player StreamingAssets 中的 Addressables 初始化 settings；
- Catalog 内各远端 bundle 的完整 URL。

因此 IP 变化后只修改 Profile、不重新构建 Addressables 和 Player是不够的。

本项目提供菜单：

```text
Tools > Addressables > LAN Remote Address
```

修改 IP 后至少需要重新构建 Addressables；如果远端 Catalog 初始 URL 也发生变化，还必须重新构建 Player。

### 问题 5：PowerShell 能访问，但 UnityWebRequest 访问本机局域网 IP 超时

本次实测结果：

```text
PowerShell -> http://10.29.99.205:64482     HTTP 200
UnityWebRequest -> http://10.29.99.205:64482 超时
UnityWebRequest -> http://127.0.0.1:64482    HTTP 200
```

说明“浏览器能打开”只能证明操作系统普通 HTTP 客户端可达，不能完全替代 UnityWebRequest 测试。

当前 BootstrapRunner 会判断 Catalog URL 的主机是否等于本机 IPv4：

- 是本机：仅本机运行时改写为 `127.0.0.1`；
- 不是本机：保持局域网 IP，供第二台电脑访问。

同时通过 Addressables WebRequestOverride 设置 15 秒超时，避免无限停在“初始化 Addressables”。

面试要点：Addressables 最终仍通过 UnityWebRequest 下载，可使用 InternalIdTransformFunc 做运行时 URL 重写，使用 WebRequestOverride 设置请求级参数。

### 问题 6：502 Bad Gateway 或 InitializeAsync 长时间无响应

Unity Addressables Hosting 的简单 HttpHostingService 正常情况下主要返回 200/404。出现 502 时需要区分：

- Catalog URL 是否正确；
- Hosting 是否真的监听目标端口；
- 请求是否经过代理、安全软件或网络过滤；
- UnityWebRequest 与浏览器行为是否一致；
- Catalog 请求超时是否为 0。

本项目把 `CatalogRequestsTimeout` 从 0 调整为 15 秒，并在启动 UI 中展示失败阶段。

### 问题 7：对 InitializeAsync 句柄先轮询、完成后再 ToUniTask

典型错误：

```text
Attempting to use an invalid operation handle
```

部分 Addressables 初始化句柄完成后可能自动释放。错误写法是先等待 `handle.IsDone`，完成后才调用 `ToUniTask()`；此时句柄可能已经失效。

正确原则：

- 创建句柄后立即注册 await/Completed；
- 或使用不会在完成后再次访问已释放句柄的等待方式；
- 释放前检查 `IsValid()`；
- 明确每个 API 的 autoReleaseHandle 行为。

### 问题 8：BootStrapScene 在 HotFix DLL 加载前包含 HotFix MonoBehaviour

本次 BootStrapScene 中存在 `PlayerRuntimeRoot` Prefab 实例。Player 反序列化启动场景时，HotFix DLL 尚未加载，因此出现 Missing Script 和序列化布局不一致。

处理方式：只移除 BootStrapScene 中的实例，保留源 Prefab 资产。

启动场景原则：

- 只放 AOT Assembly 中的启动组件；
- 不直接摆放 HotFix MonoBehaviour；
- HotFix 对象必须在 DLL 加载后，通过 Addressables 场景、Prefab 或代码实例化。

### 问题 9：HotFix 程序集装载顺序导致 TypeLoadException

典型错误：

```text
Could not load type GameplayNetworkRuntime from HotFix.Gameplay.Network
Could not load type LobbyPlayerState from HotFix.Lobby.Network
```

Editor 使用已编译、已注册的全部程序集，通常看不出加载顺序问题；IL2CPP + HybridCLR Player 会在解析跨程序集字段和方法签名时更早暴露依赖尚未注册的问题。

本次先加载 `HotFix.Gameplay`、后加载 `HotFix.Gameplay.Network` 会失败；先加载 `HotFix.Lobby.World`、后加载 `HotFix.Lobby.Network` 也会失败。

处理方式：基础程序集先加载，消费程序集后加载，总入口 HotFix.Entry 最后加载。

面试要点：asmdef References 是编译期依赖图；运行时 `Assembly.Load(byte[])` 仍需要按依赖拓扑注册程序集。二者不能混为一谈。

### 问题 10：Player 成功进入 LobbyScene，但场景内大量 Missing Script

原因是 LobbyScene 原本作为 Build Settings 内置场景打入 Player，而场景中挂载了大量 HotFix MonoBehaviour。IL2CPP 构建场景时没有可用的热更 MonoScript 绑定。

处理方式：

- LobbyScene 加入 Addressables `Scenes` Group；
- 地址为完整场景路径；
- 标签为 `Scene`；
- 在 Player Build Settings 中禁用 LobbyScene；
- HotFix DLL 和配表加载完成后，再调用 Addressables.LoadSceneAsync。

调整后 Player 日志中的 HotFix Missing Script 已消失。

### 问题 11：AOT generic method not instantiated

典型错误：

```text
AOT generic method not instantiated:
UniTask<SceneInstance>
NetworkVariable<float>
NetworkVariable<bool>
```

原因是热更代码调用了 AOT 程序集中的泛型方法，但 IL2CPP 没有生成该泛型实例，且对应 AOT 程序集元数据没有补充到 HybridCLR。

处理方式：

- 避免无必要的泛型桥接，例如场景句柄可直接轮询而不调用 `ToUniTask<SceneInstance>`；
- 把 `Unity.Netcode.Runtime.dll` 和 `UniTask.Addressables.dll` 的裁剪后 DLL 加入 AOT_DLL；
- 启动时调用 `RuntimeApi.LoadMetadataForAOTAssembly`；
- AOT API、泛型调用或 Unity/包版本变化后重新生成 HybridCLR 产物。

本项目的 `Tools > HotUpdate > Build And Sync DLLs` 已扩展为同时同步 HotFix DLL 和所需 AOT 补充元数据。

### 问题 12：旧 AOT.dll 元数据条目长期残留

旧 `AOT.dll.bytes` 被标记为 AOT_DLL，但当前 Player 中没有名为 AOT 的程序集，因此返回：

```text
AOT_ASSEMBLY_NOT_FIND
```

这类警告说明标签集合中存在历史遗留物。当前工具会按受管列表同步 AOT 条目，并从 Addressables Group 移除不再需要的旧标签项。

### 问题 13：防火墙和两条网络链路容易混淆

两台电脑联调至少需要：

| 用途 | 协议 | 端口 |
| --- | --- | ---: |
| Addressables Hosting | TCP | 64482 |
| NGO / Unity Transport | UDP | 7777 |

Addressables 能下载不代表游戏 UDP 能连接；UDP 能建立 Host 也不代表 Catalog 能下载。

Windows 当前网络若为 Public，只有 Domain Profile 的 Unity 防火墙规则不会生效。还要注意 AP Isolation、校园网/公司网客户端隔离和 VMware 虚拟网卡 IP。

## 四、推荐的正常导出链路

### 第 0 步：确认改动类型

先判断本次改动属于：

- 纯远端资源；
- HotFix 代码；
- AOT 代码或 AOT 泛型调用；
- Addressables Profile/远端地址；
- Build Settings/PlayerSettings；
- 包版本或 Unity 版本。

不同改动需要重建的层级不同。

### 第 1 步：检查程序集依赖

1. 检查所有 asmdef/asmref。
2. 确认 AOT Assembly 不引用 HotFix Assembly。
3. 确认所有 HotFix 依赖都能在 Player 中找到：要么同为 HotUpdateAssemblies，要么明确为 AOT Assembly。
4. 清理改名/拆分后的旧程序集名称和旧 DLL bytes。
5. 测试 asmdef 必须是 test-only。

### 第 2 步：生成 HybridCLR 产物

以下情况应执行 `HybridCLR > Generate > All`：

- 修改 AOT 代码；
- 新增或改变 AOT 泛型调用；
- 改 HotUpdateAssemblies；
- 升级 Unity、HybridCLR 或关键包；
- Development/Release 模式切换导致 MethodBridge 标志不一致。

### 第 3 步：编译并同步 DLL

执行：

```text
Tools > HotUpdate > Build And Sync DLLs
```

该步骤现在会：

- 编译 HybridCLR HotFix DLL；
- 复制为 `.dll.bytes`；
- 同步 `Hotfix_DLL` Addressables 条目；
- 从最新 AssembliesPostIl2CppStrip 复制 AOT 元数据；
- 同步 `AOT_DLL` 条目；
- 清理受管标签中的旧条目。

如果 AssembliesPostIl2CppStrip 不存在或过旧，先执行 HybridCLR Generate/All 或完成一次对应目标的 Player 构建。

### 第 4 步：设置远端地址

执行：

```text
Tools > Addressables > LAN Remote Address
```

选择真实 Wi-Fi/以太网 IPv4，不要选择：

- `127.0.0.1`（只能本机访问）；
- VMware/Hyper-V/VBox 虚拟网卡；
- 已断开的旧 IP。

开发 HTTP 测试同时确认：

- Insecure HTTP 为 DevelopmentOnly；
- Development Build 已开启；
- Catalog 请求超时不为 0。

### 第 5 步：构建 Addressables

构建后确认：

- `ServerData/StandaloneWindows64/catalog*.json` 存在；
- `catalog*.hash` 存在；
- Catalog 内 URL 是当前 IP；
- 最新 HotFix、AOT、Config、Scene bundle 均存在；
- LobbyScene 位于 Scenes bundle，而不是 Player 内置场景。

### 第 6 步：启动或部署远端服务

局域网测试：开启 Addressables Hosting，并保持 Unity Editor 运行。

正式/稳定测试：把完整 `ServerData/StandaloneWindows64` 部署到 HTTP/HTTPS 服务根目录。

从第二台电脑验证：

```text
http://<资源主机IP>:64482/catalog_0.1.0.hash
```

### 第 7 步：决定是否重建 Player

| 改动 | Addressables | Player |
| --- | --- | --- |
| 仅远端 Prefab/Config/HotFix DLL，URL 不变 | 必须重建/更新 | 通常不需要 |
| Remote.LoadPath/IP 改变 | 必须 | 必须 |
| Bootstrap/AOT 代码改变 | 视资源变化而定 | 必须 |
| Build Settings 场景改变 | 必须（若涉及 Addressable Scene） | 必须 |
| PlayerSettings/HTTP 策略改变 | 不一定 | 必须 |
| AOT 泛型或补充元数据列表改变 | 必须 | 建议重新生成并构建 |

### 第 8 步：Player 冒烟测试

不要只看 Unity Editor。至少检查 Player.log 中以下阶段：

```text
初始化 Addressables
检查/更新 Catalog
加载 AOT 补充元数据
加载全部 HotFix 程序集
加载配表
通过 Addressables 加载 LobbyScene
成功进入联机大厅
```

同时搜索：

```text
Missing Script
TypeLoadException
MissingMethodException
AssemblyResolutionException
RemoteProviderException
502
Request timeout
```

## 五、面试高频问题速答

### Addressables 的 BuildPath 和 LoadPath 有什么区别？

BuildPath 是构建产物输出到哪里；LoadPath 是运行时从哪里加载。远端 Group 常构建到 ServerData，再通过 HTTP/HTTPS LoadPath 下载。

### Catalog 和 hash 的作用是什么？

Catalog 记录 Address 到实际资源位置、依赖和 Provider；hash 用于判断远端 Catalog 是否变化。客户端先比较 hash，再决定是否更新 Catalog。

### 为什么修改 Remote.LoadPath 后旧包仍访问旧地址？

初始 Catalog URL 被写入 Player StreamingAssets，bundle URL 被写入 Catalog。只修改 Profile 不会回写已生成的 Player 或 Catalog。

### 为什么 Editor 正常，IL2CPP Player 失败？

Editor 使用 Mono、完整程序集和 AssetDatabase；Player 经历 IL2CPP、裁剪、AOT 泛型、远端 Catalog、缓存和动态 Assembly.Load，运行环境完全不同。

### HybridCLR 的 AOT 补充元数据解决什么？

它让解释执行的热更代码能够使用被 IL2CPP 裁剪或未生成具体实例的 AOT 泛型元数据。它不等于把 AOT 程序集变成热更程序集。

### 为什么 HotFix DLL 都在，仍会 TypeLoadException？

DLL 存在只代表二进制可下载；跨程序集字段、基类、接口和方法签名在解析时仍要求依赖程序集已注册。动态加载时要遵守依赖拓扑。

### 为什么热更 MonoBehaviour 场景应该 Addressable？

内置场景在 Player 构建时就被序列化，而热更程序集不在 AOT Player 中。Addressable Scene 可在热更 DLL 加载后再反序列化，从而恢复脚本类型绑定。

### 如何排查 Addressables 下载失败？

按顺序检查：最终 URL、Catalog/hash、Hosting 监听、UnityWebRequest、HTTP 策略、超时、防火墙、网络类型、客户端隔离、缓存和 bundle 是否存在。

### Addressables 缓存为什么会干扰排查？

旧 Catalog 和 bundle 可能仍在 persistentDataPath/Unity Cache。应通过版本/hash 正确更新；诊断时可使用新的版本号或清理测试客户端缓存，但不要把“清缓存”当成正式更新方案。

## 六、当前项目可用工具

### LAN Remote Address

```text
Tools > Addressables > LAN Remote Address
```

用于选择物理网卡 IP、修改 Remote.LoadPath、设置开发 HTTP 策略、设置 Catalog 超时、测试本机 Hosting、构建 Addressables 和打开操作指南。

### Build And Sync DLLs

```text
Tools > HotUpdate > Build And Sync DLLs
```

用于同步 HotFix DLL 和 AOT 补充元数据到 Addressables。

## 七、仍需区分的非阻塞日志

- Unity Analytics/Cloud 域名连接失败：通常不影响 Addressables 本地 Hosting 和 Lobby 进入。
- `Cannot read BuildLayout header`：本次来自 Addressables Build Report/Profiler 对未打开 BuildLayout 的读取；当 BuildPlayerContent 明确返回 Success、Catalog/hash/bundle 均已生成时，它不是内容构建失败。若持续出现，可关闭并重新打开 Addressables Report/Profiler 窗口再构建。
- 本次自动化隐藏 Player 中出现过 PlayerPrefs 写入失败，属于测试进程/系统存储权限问题，不是 Addressables、HybridCLR 或 Lobby 场景加载失败；正式双机测试时应单独确认 PlayerPrefs 可写。

局域网端口、防火墙和双机操作细节另见：`Assets/局域网联机与Addressables操作指南.md`。

## 八、本次 AOT 泛型缺失与元数据升级复盘

### 现象

Lobby 能加载，但点击开始游戏或创建房间后出现：

```text
MissingMethodException: AOT generic method not instantiated in aot.
assembly:AOT_Core.dll
method:LocalEvents.Publish<ShowOverviewMessageEvent>
```

### 根因

HybridCLR 生成的 `AOTGenericReferences.PatchedAOTAssemblyList` 中已经包含 `AOT_Core.dll`，但项目旧同步工具又硬编码了一份较短的补充元数据名单。生成分析结果没有真正同步到 Addressables，Player 日志中也没有加载 `AOT_Core.dll` 元数据。

这说明排查时必须区分三层状态：

1. 生成器是否分析出该 AOT 程序集。
2. 裁剪后 DLL 是否被复制并标记为 `AOT_DLL`。
3. Player 是否实际下载并成功调用 `LoadMetadataForAOTAssembly`。

只满足第 1 层不能证明补充元数据已经生效。

### 修复

- 事件总线迁移到独立热更程序集 `HotFix.Events`，业务事件泛型实例不再跨越 AOT/HotFix 边界。
- `HotUpdateBuilderTool` 改为直接使用生成的 `PatchedAOTAssemblyList`，删除第二份人工名单。
- `HotFix.Events` 与直接依赖它的 `HotFix.SceneFlow` 加入 HybridCLR 热更程序集配置和 Bootstrap 依赖顺序。
- 重新执行 Generate/All、Player 构建、DLL 同步和 Addressables 构建。

### 验证

- 当前共有 16 个 AOT 补充元数据程序集，`AOT_Core.dll` 加载结果为 `OK`。
- 当前共有 11 个热更程序集，按依赖顺序加载并全部激活。
- Windows Player 已经成功进入 LobbyScene。
- Editor 运行态实际触发“创建房间”，Host/Server/Client 状态均为 true，连接审批和玩家加入成功。
- 关闭 Host 后实际触发“开始游戏”，成功进入 GameRunTimeScene。
- 上述过程控制台没有 `MissingMethodException`、`TypeLoadException` 或匹配策略异常。

完整升级方案见：`Assets/HybridCLR元数据补充升级方案.md`。
