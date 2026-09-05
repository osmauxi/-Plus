# Addressable 场景加载管线说明

## 1. 结论与所有权边界

本项目的生产场景由 Addressables 管理，不再由 NGO 的 `NetworkSceneManager` 管理。

- Addressables 拥有 `Scene` 与场景内 `GameObject` 的加载、激活、卸载和最终销毁权。
- NGO 只拥有同一批对象上的 `NetworkObject` 网络身份，以及 Spawn、Despawn、RPC、NetworkVariable/NetworkList 同步权。
- `INetworkPrefabInstanceHandler` 只负责把 NGO 的 Spawn 消息映射到本机已经由 Addressables 创建的场景对象，不负责销毁该对象。
- Server 负责决定场景切换顺序；每个客户端独立下载和加载同一 Address，完成后向 Server 回执。所有客户端成功后，Server 才 Spawn 场景网络对象。

这条边界必须保持唯一。不能同时让 Addressables 和 NGO 都认为自己拥有场景对象的销毁权。

## 2. 为什么脱离 NGO 默认场景管理

NGO 1.10 默认场景管线以 Player Build Settings 中的场景和 Build Index 为基础。当前工程的发布约束是：

- Build Settings 只保留 `Assets/_HotUpdate/Scenes/BootStrapScene.unity`。
- Lobby、GameRuntime、UI 等生产场景全部作为 Addressables 远端内容发布。
- 场景和热更 DLL 可以更新，而不要求每次把所有业务场景重新内置进 Player。

因此，如果继续调用 `NetworkManager.SceneManager.LoadScene`，NGO 会验证一个并不存在于 Build Settings 的热更场景，加载协议与资源发布协议也会分裂。项目必须在 `StartHost/StartClient` 前设置：

```csharp
NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;
```

关闭的只是 NGO 的“物理场景加载器”，并没有关闭 NGO 的联网、RPC、NetworkObject 或状态同步。

## 3. 当前加载管线

### 3.1 启动到 Lobby

1. Player 只启动 `BootStrapScene`。
2. `BootstrapRunner` 加载 AOT 补充元数据和热更 DLL。
3. 每次 `Assembly.Load(bytes)` 后立即显式执行该热更程序集的 NGO 生成序列化注册函数。
4. `HotFixEntry` 先加载配表，再由 `NetworkSceneLoadService.Shared` 以 Addressables Single 模式加载 Lobby。
5. Lobby 中的 `NetworkManager` 完成 Awake 后，关闭 NGO 默认场景管理。
6. 注册 Lobby 场景以及已经迁移到 `DontDestroyOnLoad` 的 NetworkObject Handler。
7. 此后才允许 UI 调用 `StartHost` 或 `StartClient`。

### 3.2 远端客户端连接 Lobby

1. 客户端本地已经有 Addressables 创建的 Lobby 网络控制器。
2. NGO 处理 `ConnectionApprovedMessage` 时会先调用 `DestroySceneObjects()` 清理旧的场景网络身份。
3. 自定义 Handler 的 `Destroy` 现在是保留实例的 no-op：它不能调用 `Object.Destroy`。
4. NGO 收到 Server 的 Spawn 消息后，Handler 的 `Instantiate` 返回同一个本地场景对象。
5. NGO 在该对象上重新执行 Spawn 和 NetworkList/NetworkVariable 同步。

原来的错误正发生在第 3 步：Handler 真正销毁了 Lobby 对象，导致下一帧 `LobbyNetworkManager.OnDestroy()` 释放 `NetworkList` 并清空单例。随后点击人物、装备或按钮时才表现为 `NativeList.GetEnumerator`、空引用和“正确 IP 无反馈”。这不是 UI 判空可以修复的问题。

### 3.3 Lobby 进入游戏

1. `LobbyNetworkManager` 只在 Server/Host 侧发起 `TransitionToGameSceneAsync()`。
2. `GameSceneFlowController` 生成本轮 revision，记录所有仍在线 ClientId。
3. Server 通过 ClientRpc 下发完整 Addressables 场景地址和加载模式。
4. Host 与各远端 Client 使用同一 `NetworkSceneLoadService.Shared` 本地加载场景、注册 Handler，然后发送 ServerRpc 回执。
5. Server 等到所有客户端成功；任一客户端失败或超时则整轮失败，不进行半完成 Spawn。
6. Server 对该场景的 NetworkObject 调用 `Spawn(destroyWithScene: false)`。
7. 先以 Single 模式完成 `GameRunTimeScene`，再以 Additive 模式完成 `UIGameUIScene`。

场景 Address 是联机协议的一部分。Host 与 Client 的 catalog、hash、bundle 和热更 DLL 必须来自同一发布版本。

### 3.4 卸载的逆向顺序

底层服务已经提供 Despawn、注册注销和 Addressables 卸载能力。当前业务只有
“Lobby -> 游戏”的单向入口，尚未提供“游戏 -> Lobby”的按钮或 Server 编排 RPC；
以后增加返回 Lobby/下一关业务时，跨客户端控制器必须严格执行：

1. Server 对目标场景 NetworkObject 调用 `Despawn(destroy: false)`。
2. 等待 Despawn 消息到达客户端，并由新增的跨客户端流程进入同一个卸载 revision。
3. 各端注销该场景的 Prefab Handler 和静态注册记录。
4. 各端调用 `Addressables.UnloadSceneAsync`，由 Addressables 最终销毁场景和 GameObject。
5. `DontDestroyOnLoad` 的 Lobby/流程控制器在普通切场时不注销；只在完整退出联机运行时释放。

`NetworkSceneLoadService.UnloadAddressableSceneAsync` 会拒绝卸载仍然处于 Spawn 状态的对象，以便直接暴露流程错误，而不是留下悬空 NetworkObject。它是本机底层原语，不能替代 Server 的全端卸载编排。

## 4. 本次代码修改位置

### `Assets/_HotUpdate/Scripts/SceneFlow/NetworkSceneLoadService.cs`

- `Shared`：统一保存 Lobby、GameRuntime、UI 场景的 Addressables handle，避免不同入口各自维护不完整生命周期。
- `ConfigureNetworkManagerForAddressableScenes`：在网络启动前关闭 NGO 默认场景管理。
- `RegisterSceneNetworkObjects`：注册已有场景实例；首个 Lobby 可显式包含 DDOL 控制器；发现两个存活对象 Hash 冲突时直接失败。
- `LoadAddressableSceneAsync`：Addressables 本地加载、旧 Single 场景 handle 清理、NetworkObject Handler 注册。
- `SpawnSceneNetworkObjects`：Server Spawn 使用 `destroyWithScene: false`，禁止 NGO 获得物理实例销毁权。
- `ExistingSceneNetworkObjectHandler.Instantiate`：返回 Addressables 已创建的场景对象。
- `ExistingSceneNetworkObjectHandler.Destroy`：改为 no-op，只记录 NGO 已释放网络身份；不再调用 `Object.Destroy`。
- `DespawnSceneNetworkObjects`：卸载前只撤销网络身份。
- `UnloadAddressableSceneAsync`：检查已 Despawn、注销 Handler、最后调用 Addressables 卸载。
- `ReleasePersistentNetworkObjectRegistrations`：仅供完整退出联机运行时释放 DDOL Handler。

### `Assets/_HotUpdate/Scripts/SceneFlow/GameSceneFlowController.cs`

- `_networkSceneLoader`：改为使用 `NetworkSceneLoadService.Shared`。
- `TransitionToGameSceneAsync`：Server 权威执行 Runtime Single、UI Additive 两阶段加载。
- `LoadAddressableSceneForAllClientsAsync`：维护 ClientId 等待集合、revision、失败详情和超时。
- `LoadAddressableSceneClientRpc` / `ConfirmAddressableSceneLoadedServerRpc`：组成全端加载命令与确认闭环。

### `Assets/_HotUpdate/Scripts/Entry/HotFixEntry.cs`

- `EnterLobbySceneAsync`：Lobby 改由共享场景服务加载；先创建 Lobby，再关闭 NGO SceneManagement，最后注册场景和 DDOL NetworkObject。

### `Assets/_HotUpdate/Scripts/UI/LobbyUI/OverviewUI/OverviewPresenter.cs`

- `HandleJoinSubmit`：检查 `StartClient()` 返回值。如果上一轮 `Shutdown` 尚未完成，明确拒绝本次启动并恢复连接输入，不再启动一个永远等不到回调的超时流程。
- 此修改只处理 NGO 会话状态机结果，没有给 Lobby UI、单例或 `NetworkList` 添加防御性判空。

## 5. 既有相关位置

以下文件不是本次核心修复，但属于完整管线，后续修改场景加载时必须一并检查：

| 位置 | 作用 |
| --- | --- |
| `Assets/_AOT/Scripts/Bootstrap/BootstrapRunner.cs`：`LoadHotUpdateAssemblyAsync`、`InitializeNgoSerialization` | 加载热更 DLL，并补执行 NGO ILPP 生成的序列化注册 |
| `Assets/_HotUpdate/Scripts/Network_Lobby/LobbyNetworkManager.cs`：`StartGameFlow`、`StartGameFlowAsync` | 单机/多人进入游戏的 Server 侧业务入口 |
| `Assets/_HotUpdate/Scripts/SceneFlow/HotFix.SceneFlow.asmdef` | 场景管线热更程序集及依赖边界 |
| `Assets/_HotUpdate/Scripts/Entry/HotFix.Entry.asmdef` | 热更入口引用 SceneFlow，保证入口可以调用共享加载服务 |
| `Assets/AddressableAssetsData/AssetGroups/Scenes.asset` | 所有热更场景的 Address、GUID 与标签登记 |
| `Assets/AddressableAssetsData/AssetGroups/Schemas/Scenes_BundledAssetGroupSchema.asset` | Scenes 组的 BuildPath/LoadPath 和 bundle 规则 |
| `Assets/AddressableAssetsData/AddressableAssetSettings.asset` | Addressables catalog、Profile 与全局构建设置 |
| `ProjectSettings/EditorBuildSettings.asset` | 只允许 Boot 场景进入 Player Build Settings |
| `Assets/Editor/Tools/SceneAssemblyHandoverValidator.cs` | 检查 Boot-only、场景 Addressable、Missing Script、热更程序集登记和 NetworkObject Hash |
| `Assets/Editor/Tools/AddressablesLanRemoteWindow.cs` | 修改当前 Profile 的局域网 RemoteLoadPath，并展示 TCP/UDP 联调信息 |
| `Assets/_AOT/Scripts/Core/System/SceneManage/AsynchronousLoader.cs` | 旧 NGO Build Settings 加载器；不得用于生产热更场景 |

## 6. 禁止事项

- 禁止对热更生产场景调用 `SceneManager.LoadScene`。
- 禁止调用 `NetworkManager.Singleton.SceneManager.LoadScene` 加载 Addressable 场景。
- 禁止在场景实例 Handler 的 `Destroy` 中销毁 Addressables 场景对象。
- 禁止 Server 在客户端未完成加载确认前 Spawn 场景 NetworkObject。
- 禁止卸载仍处于 Spawn 状态的场景。
- 禁止用 UI 判空、捕获后忽略异常或自动重建单例来掩盖生命周期错误。
- 禁止 Host 与 Client 使用不同 catalog/content 版本。

## 7. 发布与验收

1. 执行 `Tools/Validation/Scene And Assembly Handover`。
2. 执行 HybridCLR 生成和 `Tools/HotUpdate/Build And Sync DLLs`。
3. 执行 Addressables New Build，完整更新 `ServerData/StandaloneWindows64`。
4. 确认客户端能访问 `catalog_*.hash`、catalog 和全部 bundle。
5. 清缓存测试 Boot -> Lobby。
6. 双机测试 Host 创建房间、Client 正确 IP 加入、人物/装备 UI、准备和进入游戏。
7. 断开后等待 NGO Shutdown 完成，再次加入同一 Host，确认能够第二次连接。
8. 日志不得出现已释放 `NetworkList`、重复 Hash、场景回执超时或 Addressables 404。

远端双机验证时，正确连接应看到“NGO 已释放场景对象网络身份，保留 Addressables 实例”的日志，随后仍能正常执行 `LobbyNetworkManager.OnNetworkSpawn()` 和 Lobby UI 操作。
