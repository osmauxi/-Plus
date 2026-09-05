# 局域网联机与 Addressables 操作指南

本文用于两台 Windows 电脑在同一局域网内进行 `_plus` 游戏联调。

## 一、需要打通的两条网络链路

客户端电脑需要分别访问：

| 用途 | 协议 | 默认端口 | 目标 |
| --- | --- | ---: | --- |
| Addressables Catalog、热更 DLL、配表和资源包 | TCP/HTTP | 64482 | 运行 Unity Addressables Hosting 的电脑 |
| NGO / Unity Transport 游戏联机 | UDP | 7777 | 点击“创建房间”的游戏 Host 电脑 |

如果 Unity Editor、Addressables Hosting 和游戏 Host 都运行在同一台电脑上，两条链路使用同一个局域网 IP。

## 二、当前推荐配置

当前资源主机的物理 Wi-Fi IPv4 是 `10.29.99.205`，但 DHCP 重新连接后可能变化。截图中的 `192.168.47.1` 和 `192.168.121.1` 是 VMware 虚拟网卡，不应作为局域网客户端地址；`127.0.0.1` 只能访问本机。

局域网开发测试建议：

- Remote.LoadPath：`http://<资源主机局域网IP>:64482`
- Player HTTP 策略：`Development Builds Only`
- Player 构建：勾选 `Development Build`
- 游戏联机端口：UDP `7777`

正式发布不应依赖 Unity Editor Hosting。应部署到稳定 HTTP/HTTPS 服务，优先使用 HTTPS，并把 Player HTTP 策略恢复为 `NotAllowed`。

## 三、使用项目内地址工具

Unity 菜单：

`Tools > Addressables > LAN Remote Address`

工具提供以下能力：

1. 显示当前 Active Profile、Remote.LoadPath、HTTP 策略、Development Build 和 Catalog 超时状态。
2. 自动列出已启用的物理以太网/Wi-Fi IPv4，过滤 VMware、Hyper-V、Docker、WSL 等虚拟网卡。
3. 支持手动输入 IP、可解析主机名和 Hosting 端口。
4. “应用远端地址”只修改当前 Addressables Profile。
5. “应用开发测试 HTTP 策略”会设置 `DevelopmentOnly` 并开启 Development Build。
6. “测试本机 Hosting 连接”使用 UnityWebRequest 读取当前构建的 Catalog hash；资源服务器就是本机时会自动使用 `127.0.0.1` 测试。
7. “应用地址并构建 Addressables”会保存地址、设置有限的 Catalog 请求超时，并执行 Addressables Player Content 构建。
8. “打开局域网联机操作指南”可直接打开本文档。

IP 变化后的标准操作：

1. 打开 `Tools > Addressables > LAN Remote Address`。
2. 点击“重新检测”，在真实 Wi-Fi/以太网地址旁点击“使用”；也可手动输入地址。
3. 确认端口是 `64482`。
4. 点击“应用地址并构建 Addressables”。
5. 打开 Addressables Hosting 窗口并启动 Local Hosting Service。
6. 回到工具点击“测试本机 Hosting 连接”，确认显示 HTTP 200。
7. 重新构建完整 Player，再把完整构建目录复制到另一台电脑。

Addressables 构建和 Player 构建都必须在地址变化后重新执行。Player 的 StreamingAssets 中保存了初始 Catalog/远端 Catalog 地址，仅重新生成 `ServerData` 不能让旧 Player 自动知道新服务器地址。

## 四、Addressables 正确构建与托管顺序

1. 需要更新热更代码时，先执行项目的 HybridCLR 热更 DLL 编译/同步工具。
2. 使用 LAN Remote Address 工具应用正确 IP。
3. 构建 Addressables Player Content。
4. 确认 `ServerData/StandaloneWindows64` 中存在最新 Catalog、hash 和 bundle。
5. 启动 Addressables Hosting，确认服务监听端口 `64482`。
6. 最后重新构建 Windows Player。

`ServerData/StandaloneWindows64` 是远端资源服务器目录，不需要复制进客户端游戏目录。测试期间 Unity Editor 必须保持运行且 Hosting Service 必须保持 Enabled；关闭 Unity Editor 后，本地 Hosting 会停止。

Windows 上代理/VPN 可能拦截 UnityWebRequest，甚至把 `127.0.0.1` 请求转成 502。项目启动器会检测 Catalog URL 的主机是否为本机 IPv4；若资源服务器与 Player 在同一台电脑，并且能在项目旁找到 `ServerData/<平台>`，会把该电脑上的 Addressables 请求改写为本地 `file://` 文件读取，完全绕过 HTTP 代理。复制到第二台电脑后，服务器 IP 不属于客户端本机，仍会使用 Profile 中的局域网 HTTP 地址，不影响双机测试。

因此本机开关代理仍能进入 Lobby，并不代表第二台电脑已经连通资源服务器。双机测试前仍需在第二台电脑浏览器访问 Catalog hash；正式发布也不能依赖本地 `ServerData` 回退，必须部署完整远端目录。

## 五、Windows 防火墙

资源/Host 电脑至少需要放行：

- 入站 TCP 64482：Addressables Hosting。
- 入站 UDP 7777：Unity Transport。

当前 Wi-Fi 可能被 Windows 标记为 Public，而 Unity 自动创建的规则可能只允许 Domain，因此同一 Wi-Fi 的另一台电脑仍可能访问失败。

推荐在“高级安全 Windows Defender 防火墙”中新建两条入站规则，并把远端地址范围限制为“本地子网”。如使用管理员 PowerShell，可执行：

```powershell
New-NetFirewallRule -DisplayName "Plus Addressables LAN TCP 64482" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 64482 -Profile Private,Public -RemoteAddress LocalSubnet
New-NetFirewallRule -DisplayName "Plus Game LAN UDP 7777" -Direction Inbound -Action Allow -Protocol UDP -LocalPort 7777 -Profile Private,Public -RemoteAddress LocalSubnet
```

以上命令需要管理员权限。只应在可信任的局域网中放行；测试完成后可禁用或删除对应规则。

## 六、第二台电脑的连通性检查

在客户端电脑浏览器打开：

`http://<资源主机IP>:64482/catalog_0.1.0.hash`

能看到 hash 内容后，才继续启动游戏。也可以在客户端 PowerShell 执行：

```powershell
Test-NetConnection <资源主机IP> -Port 64482
```

如果失败，依次检查：

1. 两台电脑是否连接同一局域网。
2. 客户端是否使用了资源主机当前真实 IP。
3. Addressables Hosting 是否处于 Enabled。
4. TCP 64482 防火墙规则是否覆盖当前 Public/Private 网络类型。
5. 路由器或 Wi-Fi 是否启用了 AP Isolation/客户端隔离。
6. 公司、校园或公共 Wi-Fi 是否禁止设备间直接访问。

## 七、游戏 Host / Client 操作

1. 两台电脑运行同一版本的完整游戏构建。
2. 电脑 A 进入 Lobby 后点击“创建房间”。项目会在 `0.0.0.0:7777` 启动 Host。
3. 电脑 B 点击“加入房间”，输入电脑 A 的局域网 IP，例如 `10.29.99.205`；不要输入 `127.0.0.1`，也不需要附加端口。
4. 客户端代码会使用输入地址连接 UDP 7777。
5. 如果 Addressables 正常但加入房间超时，检查 UDP 7777、防火墙和 Host 是否已成功创建。

## 八、减少 IP 频繁变化的方法

优先在路由器 DHCP 设置中为资源主机网卡 MAC 地址做“地址保留/静态租约”。这样 Remote.LoadPath 可长期使用固定局域网 IP，无需每次重新构建 Player。

也可以使用局域网 DNS 主机名，但必须先在第二台电脑确认该主机名能够稳定解析。Windows 计算机名或 `.local` 名称并不保证在所有路由器和网络策略下可用。

## 九、常见错误含义

- `Insecure connection not allowed`：Player 禁止 HTTP；使用 Development Build + DevelopmentOnly，或改为 HTTPS。
- `Unable to load asset bundle`：远端 IP、端口、Hosting、Catalog、服务器文件或防火墙存在问题。
- `502 Bad Gateway` 或长时间停在 `初始化 Addressables`：先用工具测试本机 Hosting，再到第二台电脑测试 Catalog hash；项目已设置请求超时，失败后会在启动诊断 UI 显示原因，不再无限等待。
- 一直停在 `下载资源：Hotfix_DLL`：优先从客户端浏览器测试对应 Catalog/hash URL。
- 能进 Lobby 但无法加入房间：Addressables 已经正常，继续排查 UDP 7777 和 NGO Host。
- `Missing Script` 出现在 BootStrapScene：启动场景包含热更程序集 MonoBehaviour；应把热更对象移到热更场景或在 DLL 加载后实例化。BootStrapScene 只保留 AOT 启动组件，不能直接摆放 `HotFix.*` 脚本组件。
