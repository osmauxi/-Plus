# Addressables 与 HybridCLR 热更链路操作手册

本文说明 `_plus` 当前局域网热更管线的实际流转、各脚本职责、已知边界，以及如何创建 Baseline 和发布后续 Content Update。

当前目标是打通开发环境闭环：客户端连接运行 Unity Editor 的资源电脑，通过 Addressables Local Hosting 下载全部缺失资源和 HotFix DLL，然后使用相同版本进入局域网游戏。当前不建设正式资源服务器，不实现资源签名、回滚、灰度发布或分阶段下载。

## 1. 版本概念

管线使用三个相互独立的版本量：

| 名称 | 示例 | 用途 | 何时修改 |
| --- | --- | --- | --- |
| Baseline 版本 | `0.1.0` | 标识一个 Player 和它的初始 Addressables 基线 | 需要重新构建 Player 时修改 |
| 内容修订号 | `1`、`2`、`3` | 标识同一个 Baseline 下的连续热更 | 每次 Content Update 递增 |
| 联机协议版本 | `1`、`2` | 阻止网络协议不兼容的客户端进入同一房间 | NGO 消息、序列化或会话规则不兼容时递增 |

对外可把一次发布记成：

```text
BaselineVersion.ContentRevision

例如：
0.1.0.0  = 0.1.0 Player 对应的初始基线
0.1.0.1  = 0.1.0 Player 的第一次内容热更
0.1.0.2  = 0.1.0 Player 的第二次内容热更
```

内容修订号不会改写 Player 版本。Content Update 会继续使用所选 Baseline 状态文件中记录的 Catalog 版本。

## 2. 完整链路

### 2.1 Baseline 构建链路

```text
设置新的 Player/Baseline 版本
  → 扫描并登记所有 HotFix.* 程序集
  → HybridCLR Generate/All
  → 同步 HotFix DLL 和 AOT 补充元数据 DLL
  → 同步全部远端资源的 Preload 标签
  → 完整构建 Addressables
  → 保存 addressables_content_state.bin
  → 构建与该状态严格配套的 Player
  → 写入 release-manifest.json
```

Baseline 的核心含义不是“保存了一份 Bundle”，而是保存了当时 Player 所对应的 Addressables 内容状态。后续 Content Update 必须使用这份状态计算差异。

### 2.2 Content Update 链路

```text
选择目标 Baseline
  → 递增内容修订号
  → 扫描遗漏的 HotFix.* 程序集
  → 只编译 HotFix DLL
  → 同步 HotFix/AOT Addressables 条目
  → 同步全部远端资源的 Preload 标签
  → ContentUpdateScript.BuildContentUpdate
  → 输出更新后的 Catalog、hash 和变化 Bundle
  → 最后提交 release-manifest.json
```

Content Update 不重新构建 Player，也不覆盖所选 Baseline 的 content state。

### 2.3 客户端启动链路

```text
BootstrapScene
  → UDP 64481 广播寻找资源服务器
  → 获得服务器 IP、资源端口、游戏端口和发布版本
  → 注入 LanRuntimeContext
  → 把 Catalog 中的构建时地址改写为发现到的服务器地址
  → Addressables.InitializeAsync
  → CheckForCatalogUpdates
  → UpdateCatalogs
  → 计算 Preload 标签的缺失下载量
  → 下载全部缺失 Bundle
  → 加载 AOT 补充元数据
  → 加载所有 HotFix DLL
  → 调用 HotFixEntry.StartGame
  → 进入 LobbyScene
```

项目当前有意把所有远端资源都标记为 `Preload`。因此客户端会在启动阶段一次性下载当前 Catalog 下所有缺失资源，下载完成后才进入游戏。

### 2.4 联机准入链路

```text
客户端准备加入房间
  → ConnectionData 写入玩家 ID、Baseline、内容修订号、协议版本
  → Host 的 LobbyConnectionGate 解析载荷
  → 与 Host 的 LanRuntimeContext.Manifest 比较
  → 版本一致：允许连接
  → 版本不同：拒绝连接并要求客户端更新
```

资源更新发生在进入大厅前，联机准入发生在资源更新后。准入校验是最后一道保护，不能代替 Catalog 和 Bundle 下载。

## 3. 关键脚本职责

### 编辑器发布工具

| 脚本 | 职责 |
| --- | --- |
| `Assets/_AOT/Editor/AddressablesContentBuildTool.cs` | 发布中心主窗口；维护版本；选择 Baseline；构建完整基线或 Content Update；保存状态文件和发布清单；可构建 Player |
| `Assets/Editor/Tools/HotUpdateBuilderTool.cs` | 扫描遗漏的 `HotFix.*` asmdef；执行 HybridCLR Generate/All 或 CompileDll；复制 DLL；同步 Addressables 条目 |
| `Assets/Editor/Tools/AddressablesPreloadTool.cs` | 把所有远端 Addressables 条目同步到 `Preload` 标签 |
| `Assets/Editor/Tools/AddressablesLanRemoteWindow.cs` | 发布中心的局域网页；修改 Remote.LoadPath；配置开发 HTTP；测试 Hosting；启动自动发现服务 |
| `Assets/Editor/Tools/AddressablesLanDiscoveryService.cs` | 在 Unity Editor 中监听 UDP 64481；向客户端返回本机地址、端口和当前发布版本 |

### AOT 启动层

| 脚本 | 职责 |
| --- | --- |
| `Assets/_AOT/Scripts/Bootstrap/LanEndpoint.cs` | 表示资源服务器与游戏服务器端点 |
| `Assets/_AOT/Scripts/Bootstrap/LanEndpointResolver.cs` | 启动时执行 UDP 广播发现；失败时回退到 `192.168.137.1:64482/7777` |
| `Assets/_AOT/Scripts/Bootstrap/HotUpdateReleaseManifest.cs` | 定义发布清单、运行时上下文、发现响应和 NGO 连接版本载荷 |
| `Assets/_AOT/Scripts/Bootstrap/BootstrapRunner.cs` | 串联发现、URL 重写、Catalog 更新、全量 Preload 下载、AOT 元数据、HotFix DLL 和入口调用 |

这些脚本属于 AOT/Player 本体。修改它们后不能只发 Content Update，必须创建新的 Baseline 并重新构建 Player。

### HotFix 联机层

| 脚本 | 职责 |
| --- | --- |
| `LobbyOverviewCoordinator.cs` | 在启动 Host/Client 前生成带发布版本的 ConnectionData |
| `OverviewPresenter.cs` | 使用自动发现的游戏 IP/端口；在加入房间输入框中预填服务器地址 |
| `LobbyConnectionGate.cs` | 服务器连接审批；检查玩家 ID、房间状态、人数和版本兼容性 |
| `LobbyNetworkManager.cs` | 管理大厅玩家状态；Host 本地连接也从统一载荷中读取玩家身份 |

这些脚本都属于 HotFix 程序集。修改后可以通过 Content Update 发布。

## 4. 当前目录关系

Windows 开发环境下主要产物为：

```text
ServerData/StandaloneWindows64/
  catalog_<Baseline>.json
  catalog_<Baseline>.hash
  *.bundle
  release-manifest.json

Assets/AddressableAssetsData/Windows/
  addressables_content_state.bin       # 最近一次完整构建产生的工作状态

AddressablesBaselines/Windows/<Baseline>/
  addressables_content_state.bin       # 被保存并锁定的历史基线

AddressablesReleases/Windows/<Baseline>/
  content-0000.json
  content-0001.json
  ...                                  # 发布清单历史记录

Builds/Windows/<Baseline>/
  <ProductName>.exe
  <ProductName>_Data/
```

`Windows` 是 Addressables content state 使用的平台标识；`StandaloneWindows64` 是当前 Profile 的实际远端构建目录。发布工具会直接读取 `RemoteCatalogBuildPath`，不要再手工假定两者相同。

## 5. 什么时候创建新 Baseline

出现以下任一情况时，应创建新 Baseline，并重新分发完整 Player：

- 第一次建立热更版本。
- 修改 `BootstrapRunner`、`LanEndpointResolver`、发布清单类型等 AOT Bootstrap 代码。
- 修改非 HotFix 的运行时代码。
- 修改 IL2CPP、HybridCLR、Unity 版本、包版本或 Player Settings。
- 修改 AOT 泛型结构，原有补充元数据不再适合旧 Player。
- 修改启动场景或 Player 必须内置的内容。
- 需要改变 Player 大版本，例如从 `0.1.0` 升到 `0.2.0`。
- Addressables Profile、分组结构或加载策略发生无法确认兼容性的重大变化。

不要为了普通资源、配置表或 `HotFix.*` 业务代码变化创建新 Baseline。

## 6. 如何创建 Baseline

当前 `0.1.0` Player、Baseline、Catalog、ServerData 和发布清单已于同一次完整构建中重新生成，可作为后续 `0.1.0.x` Content Update 的起点。以后如果这些产物失配，应先清理旧产物，再以同一个目标版本重新创建完整基线。

### 6.1 构建前准备

1. 切换到目标平台：`StandaloneWindows64`。
2. 打开 `Tools > Addressables > 发布中心`。
3. 在“局域网”页选择资源电脑真实的 Wi-Fi 或以太网 IPv4。
4. 资源端口保持 `64482`，游戏端口保持 `7777`。
5. 点击“应用远端地址”。
6. HTTP 联调时点击“应用开发测试 HTTP 策略”。
7. 返回“版本与构建”页。

### 6.2 创建基线

1. 在“本次构建版本”中输入新版本，例如当前使用的 `0.1.0`。
2. 点击“应用版本”。
3. 联机协议版本首次使用 `1`。
4. 保持“基线同时构建 Player”开启。
5. 点击“应用版本并创建完整基线包”。
6. 等待 HybridCLR、Addressables 和 Player 全部完成。
7. 不要在成功后修改或覆盖保存下来的 Baseline 状态文件。

### 6.3 基线验收

构建完成后确认：

- `AddressablesBaselines/Windows/0.1.0/addressables_content_state.bin` 存在。
- `ServerData/StandaloneWindows64/catalog_0.1.0.json` 和 `.hash` 存在。
- `ServerData/StandaloneWindows64/release-manifest.json` 中修订号为 `0`。
- `AddressablesReleases/Windows/0.1.0/content-0000.json` 存在。
- `Builds/Windows/0.1.0/` 下存在完整 Player。
- 发布中心显示当前发布为 `0.1.0.0`。

Baseline、Player、Catalog 和清单必须来自同一次成功操作。只有 Addressables 成功、但 Player 构建失败时，不要把该次状态当成可发布基线。

## 7. 什么时候走 Content Update

以下修改通常走热更，不需要重新构建 Player：

- 修改 `HotFix.*` 程序集中的业务代码。
- 修改配置表并重新生成 `.bytes`。
- 修改远端 Prefab、场景、贴图、材质、UI 或其他 Addressables 资源。
- 新增远端 Addressables 资源。
- 同时修改 HotFix 代码和远端资源。
- 修复 NGO 业务逻辑，但没有改变 AOT Bootstrap。

如果网络消息或序列化格式与旧版本不兼容，在走 Content Update 的同时递增“联机协议版本”。这样旧修订客户端会被 Host 拒绝。

## 8. 如何发布 Content Update

假设当前正式基线是 `0.1.0`，当前发布是 `0.1.0.0`。

1. 完成 HotFix 代码或远端资源修改。
2. 等待 Unity 编译无错误。
3. 打开 `Tools > Addressables > 发布中心`。
4. 在“用于内容更新的基线”中选择 `0.1.0`。
5. 内容修订号填写 `1`。
6. 没有网络兼容性变化时，协议版本保持原值。
7. 点击“按已选基线构建 Content Update”。
8. 等待 HotFix DLL 编译和 Addressables 差异构建完成。
9. 确认发布中心显示当前发布为 `0.1.0.1`。
10. 后续更新依次使用修订号 `2`、`3`，始终选择原始 `0.1.0` Baseline。

不要把上一次 Content Update 生成的工作状态当成新 Baseline。整个 `0.1.0.x` 系列始终以 `AddressablesBaselines/Windows/0.1.0/addressables_content_state.bin` 为基础。

## 9. 启动本地资源服务与联机

在资源电脑上：

1. 必须先完成 Baseline 或 Content Update 构建。
2. 打开 `Window > Asset Management > Addressables > Hosting`。
3. 启动 Local Hosting Service，端口使用 `64482`。
4. 打开 `Tools > Addressables > 发布中心 > 局域网`。
5. 点击“测试本机 Hosting 连接”，确认 Catalog hash 返回成功。
6. 点击“启动自动发现服务”。
7. 启动同版本游戏并创建房间。

客户端启动后会广播寻找资源电脑。成功发现后，启动诊断中应依次看到：

```text
解析局域网资源服务器
初始化 Addressables
检查远端 Catalog
更新 Catalog（有更新时）
检查完整资源更新
下载完整资源更新（有缺失时）
加载 AOT 补充元数据
加载 HotFix 程序集
调用热更入口
进入 LobbyScene
```

资源电脑需要放行：

| 协议 | 端口 | 用途 |
| --- | ---: | --- |
| UDP | 64481 | 客户端自动发现 |
| TCP | 64482 | Addressables Catalog 和 Bundle |
| UDP | 7777 | NGO / Unity Transport 游戏联机 |

自动发现服务和 Addressables Local Hosting 都依赖 Unity Editor。关闭 Editor 后，两项本地服务都会停止。

## 10. 如何验证确实发生了热更

第一次验证建议选择一个容易观察的资源变化和一条容易观察的 HotFix 日志变化。

1. 使用 `0.1.0.0` Player 成功进入大厅一次，让资源进入缓存。
2. 不清理客户端 Addressables 缓存。
3. 修改一个 HotFix 行为和一个远端资源。
4. 发布 `0.1.0.1` Content Update。
5. 保持同一个旧 Player，再次启动。
6. 日志应显示发现待更新 Catalog 和非零缺失下载量。
7. 进入游戏后确认新资源和新 HotFix 行为同时生效。
8. 使用仍停留在旧发布版本的客户端连接新 Host，确认被版本准入拒绝。

如果测试前清除了整个缓存，只能证明全量下载可用，不能证明 Content Update 的增量复用有效。

## 11. 当前已知边界

这些边界是当前阶段有意保留的，不阻塞开发环境闭环：

- 所有远端资源都属于 `Preload`，启动时全量补齐，不做大厅/战斗分阶段下载。
- 只使用 Unity Addressables Local Hosting，不提供独立正式服务器程序。
- 使用开发环境 HTTP，不实现 HTTPS、DLL 签名或发布清单签名。
- 不实现回滚、灰度和多版本并行服务。
- Bundle、Catalog 直接写入当前服务目录，没有完整目录级原子切换。
- 多次构建可能在 `ServerData` 中留下旧 Bundle；Catalog 不引用它们，但会占用磁盘。
- Addressables Group 的内建重试次数目前为 `0`；启动器提供请求超时和下载停滞检测，但热点瞬时断网仍可能导致本次启动失败。
- 发现响应中的版本用于联机准入，当前没有再用发布清单中的 Catalog hash 做二次强校验。

为避免直接写目录带来的构建中间态，开发期间遵守一条规则即可：先停止客户端连接，完成构建并看到成功提示，再启动 Hosting/自动发现并让客户端更新。

## 12. 常见问题

### 客户端仍访问旧 IP

确认客户端使用的是包含 `LanEndpointResolver` 和 URL Rewrite 的新 Baseline Player。旧 Player 无法通过 Content Update 获得 AOT Bootstrap 新功能。

### 找不到服务器并回退到 192.168.137.1

确认资源电脑已经点击“启动自动发现服务”，并放行 UDP 64481。某些热点环境下 `192.168.137.1` 正好是 Windows 热点主机地址，但不应依赖这个巧合。

### 自动发现成功，但 Catalog 下载失败

确认 Addressables Hosting 已启动、TCP 64482 已放行，并在资源电脑本机先执行“测试本机 Hosting 连接”。

### Catalog 更新成功，但 HotFix 没变化

检查 Content Update 日志是否执行了“编译并同步 HotFix DLL”，并确认 `HotfixDLLs` 组里的 DLL `.bytes` 时间已更新。

### 出现版本不一致，无法加入房间

双方必须完成相同 Baseline、内容修订号和协议版本的启动更新。不要只修改输入框 IP 来绕过版本校验。

### 修改后应该点哪个按钮

- 改 AOT/Bootstrap/Player 环境：修改 Baseline 版本，点“创建完整基线包”。
- 只改 HotFix 或远端资源：保持 Baseline 版本，递增内容修订号，点“构建 Content Update”。
- 只换热点 IP，端口不变：不需要重新构建 Player；启动 Hosting 和自动发现服务即可。
