# 网络组件接入 Addressables 场景加载说明

本文对应当前生产资源与代码。网络阶段由 SceneFlow / Network.Runtime 编排，Gameplay 内部启动仍由 GameRuntimeBootstrap 和 IGameRuntimeService 负责。

## 1. 当前接入情况

| 对象 | 资源 / 身份 | 生命周期 | 接入方式 |
| --- | --- | --- | --- |
| NetworkManager、UnityTransport、NetworkRuntimeBootstrap、NetworkSessionBootstrap、LobbyConnectionGate | Prefabs/Network/NetworkBootstrap.prefab | 本地 DDOL 引导对象，无 NetworkObject | 配置 Backend、Catalog、会话种子；联网前负责连接审批 |
| GameSceneFlowController、AddressableSceneBarrier、NetworkScopeBarrier | Prefabs/Network/NetworkSessionRoot.prefab；NetworkSessionRoot | Persistent，SceneMask 覆盖 Lobby / GameRuntime / GameUI | 各端连接前预注册并创建本地种子；Server 联网后显式 Spawn |
| LobbyNetworkManager | Prefabs/Network/LobbyNetworkRoot.prefab；LobbyNetworkRoot | SceneScoped，仅 Lobby；OwnerSceneName=LobbyScene | IScopeBindable 获取会话依赖；大厅状态随 Lobby Scope 重建 |
| GameRuntimeBootstrap | Prefabs/Network/GameRoot.prefab 的子组件；Root 身份为 GameRuntimeNetworkRoot | SceneScoped，GameRuntime | Bind / Initialize / Activate / ShutdownScopeAsync |
| GameStateController | GameRoot 子组件，共用根 NetworkObject | 随 GameRoot | 保留 OnNetworkSpawn 的单例和 NetworkVariable 监听 |
| MapGenerationController | GameRoot 子组件，共用根 NetworkObject | 随 GameRoot | 保留 IGameRuntimeService，由 Bootstrap 顺序初始化 |
| 其余 12 个 GameRoot 运行时服务 | GameRoot 子组件 | 随 GameRoot | 保留 Bootstrap 的 Inspector 服务列表及逆序关闭 |
| PlayerRuntime、PlayerRuntimeInitializer、PlayerSyncController 等玩家组件 | 原 PlayerRuntimeRoot Prefab | 动态 Gameplay 对象 | 继续走 SyncObjectPool 和原 OnNetworkSpawn / 玩家初始化链，不加入 Scope Catalog |

上表资源路径以 Assets/_HotUpdate 为起点。生产 Catalog 位于 Prefabs/Network/NetworkPrefabCatalog.asset。

GameRoot 已从 GameRunTimeScene 提取；LobbyScene 中原 NetworkManager、GameSceneFlowManager、LobbyNetworkManager 已迁入对应 Prefab。场景不再保留这些对象的重复实例。GameRoot 的层级、单个根 NetworkObject、原有内部序列化引用和服务顺序保留。

## 2. 入口与完整时序

正常游戏从 BootStrapScene → HotFixEntry 进入：

1. 加载配置表。
2. NetworkSessionBootstrap.EnsureAvailableAsync 加载本地 NetworkBootstrap，初始化 NetworkRuntimeBootstrap。
3. 各端创建 NetworkSessionRoot 这一个 Persistent 种子，并提前注册 LobbyNetworkRoot Prefab。LobbyConnectionGate 在本地 NetworkBootstrap 上提供连接前审批。
4. 通过 Addressables 加载 LobbyScene。大厅 UI 先使用离线数据，LobbyNetworkRoot Spawn 后再绑定 LobbyNetworkManager.Instance。
5. 每次 StartHost / StartClient 前调用 PrepareConnectionAsync。Host / Server 的启动回调只 Spawn 会话种子；初始 Lobby Scope 由 Server 正常 Spawn LobbyNetworkRoot。
6. Host 先完成初始 Lobby Scope，再允许开局。LobbyNetworkManager 准备会话数据后调用原有切场入口。

不要直接打开已经剥离引导对象的 LobbyScene 或 GameRunTimeScene 来替代正式启动入口。编辑器测试应使用测试菜单，或者从 BootStrapScene 启动。

切场外层顺序：

物理场景 Load → Prefab Prepare → Server Spawn → RootReady → Bind / Initialize → Commit → Cleanup → 旧物理场景 Unload → Activate。

- Prepare：每台机器加载并注册目标 Root Prefab，必须早于 Server Spawn。
- RootReady：确认本机 Root 已 Spawn、登记、完成接口扫描。
- Bind / Initialize：本机全部 Bind 完成后再执行全部 Initialize，合并一个准备 ACK。
- Commit：越过回滚边界。
- Cleanup：先等待各端业务关闭，再让 Server Despawn；各端确认 Despawn 后释放 Prefab。关闭与释放是 Cleanup 内部的两次确认，不改变外层阶段顺序。
- Activate：触发业务入口，不收成功 ACK，不保证各端在同一帧执行。

## 3. GameRuntimeBootstrap 与子服务

Bootstrap 的阶段职责：

| 阶段 | 执行内容 |
| --- | --- |
| OnNetworkSpawn | 建立本轮取消源与状态，不调用 RunRuntimeAsync |
| Bind | 获取 NetworkSessionRoot / NetworkScopeBarrier，获取同 Root 的 GameStateController，记录 Revision |
| Initialize | 校验会话数据、服务列表、必要场景和依赖；在 Server 建立两轮业务 Ready 的参与者集合 |
| Activate | 防重复启动，调用 RunRuntimeAsync |
| ShutdownScopeAsync | 取消并等待运行任务退出，按逆序关闭已开始初始化的服务；管线等待完成后才 Despawn |

RunRuntimeAsync 内部顺序保持：

GameStateController 就绪 → GameLoading → 顺序初始化服务 → 必要 UI 场景就绪 → Gameplay RuntimeReady → 启动玩家初始化观察任务 → Server 等待所有参与者 RuntimeReady → StartInitialLevelAsync → 地图生成 → 玩家生成 → PlayerRuntimeReady → GamePlaying。

玩家观察任务必须先启动但不能阻塞 Server 生成玩家，否则双方会互相等待。Activate 代表允许启动 Gameplay，GamePlaying 才表示地图、玩家和表现准备完成并可以开放输入。

服务顺序：

1. GameNetworkRuntime
2. InputManager
3. LocalObjectPool
4. SyncObjectPool
5. LocalVFXPool
6. PlayerManager
7. PlayerCameraController
8. CameraEffectManager
9. RoomTemplateCatalog
10. MapVisualBuilder
11. MapGenerationController
12. PlayerSpawnController
13. GameLevelFlowController

这里存在两种不同的 Initialize：

- IScopeInitializable.InitializeAsync：Commit 前的准备，不启动 Gameplay。
- IGameRuntimeService.InitializeAsync：Activate 后由 RunRuntimeAsync 调用的业务初始化。

同一个服务不能同时由 Bootstrap 和 Scope Initialize / Activate 驱动。Bootstrap 会拒绝重复注册、空服务、其他 NetworkObject 下的服务，以及会造成双重启动的接口组合。需要提前校验的内容使用无业务副作用的准备方法，不把整段业务初始化移到 Commit 前。

初始化失败的当前服务也会收到 ShutdownAsync，因此服务关闭应能处理“只初始化了一部分”和重复关闭。接口接入本身不能替业务资源自动释放 Addressables Handle、事件订阅或对象池。

## 4. 单 Scene 网络组件如何接入

独立场景网络组件也需要一个受管 Root：

1. 做成 Addressable Prefab，根节点放 NetworkObject 与 NetworkScopeMember。
2. 分配稳定且唯一的 NetworkPrefabId，写入 Catalog。
3. Lifetime 设置 SceneScoped，SceneMask 设置实际需要它的 Scope，并配置唯一的 OwnerSceneName。RootReady 前该场景必须已经加载。
4. 按需实现 IScopeBindable、IScopeInitializable、IScopeActivatable。
5. 有异步清理工作时实现 IScopeShutdown。
6. 保留 NGO 登记、NetworkVariable 监听等必须在 OnNetworkSpawn 发生的工作；把依赖其他 Root 或提前开始 Gameplay 的部分迁入对应阶段。

接口定义位于 Network.Runtime/Scope/NetworkScopeLifecycle.cs。没有默认空方法，只实现需要的接口；管线按接口能力筛选，不分析方法体是否为空。

Bind 通过 context.TryGetRoot(id, out root) 获取其他 Root。Initialize 校验和准备当前 Root 的资源，不修改仍在运行的旧 Root，不提前生成玩家。Activate 只触发业务入口；异步任务需要自己捕获异常并调用 NetworkScopeBarrier.ReportRuntimeFailure(revision, error)，普通 Forget() 不会自动通知管线恢复。

扫描在本机 Spawn 登记时发生，包含 inactive 子节点，排除属于另一个 NetworkObject 的子树。Spawn 后动态新增阶段组件不会自动加入本轮缓存。Root 内部的阶段依赖不要依赖“另一个 Root 已经执行完 Activate”；跨端业务就绪使用明确的业务握手。

不要给 GameRoot 子服务逐个加 NetworkObject。嵌套 NetworkObject 不属于当前 Prefab 接入方案。玩家、怪物、投射物等多个实例的动态对象继续交给其 Spawn / Pool 管理者。

## 5. DDOL / Persistent 网络组件如何接入

Persistent 仍需要 Catalog、Prefab 注册和 NetworkScopeMember，只是跨 Scope 保留实例。

- SceneMask 应覆盖使用该服务的 Scope。
- 生命周期每次 Spawn 执行一次。复用的 Persistent Root 不会随每次切场重新 Bind、Initialize、Activate。
- 随场景变化的常驻业务通过 ScopeManager.ScopeActivated 显式处理；LobbyNetworkManager 已是 SceneScoped，会随每次 Lobby Scope 重建。
- Persistent 不能长期保留某个已卸载物理场景的引用；场景重新加载后应重新绑定，并取消旧订阅。
- 会话种子是特殊引导对象：它们必须先于屏障可用，不能用自己的未 Spawn 屏障创建自己。Registry 的种子 Handler 在断开时保留本地对象，下一会话重新 Spawn。
- NetworkBootstrap 是本地组合根，不能作为网络 Root 递归加入自身 Catalog。

当前只有 Persistent Root 会移动到 DDOL。SceneScoped Root 在每个 Peer 的 Spawn 登记阶段移动到 OwnerSceneName 对应的已加载场景；GameRoot 和 Player 属于 GameRunTimeScene，LobbyNetworkRoot 属于 LobbyScene。

当前流程以大厅内连接、全员准备后开局为验证范围。游戏进行中的新加入 / 重连还需要额外的目标场景、Root、会话与地图状态同步；原 LobbyNetworkManager 中相关 TODO 不等于已具备此能力。

## 6. Ready、失败与回滚

| 就绪信号 | 含义 | 用途 |
| --- | --- | --- |
| 管线 RuntimeReady | 本轮 Root 的 Bind / Initialize 已完成 | 允许 Commit |
| Gameplay RuntimeReady | 该端 GameRoot 子服务初始化完成 | 允许 Server 启动初始关卡 |
| PlayerRuntimeReady | 该端会话玩家、角色、武器、Animator 初始化完成 | 允许 GamePlaying |

Gameplay 两轮 Ready 都携带 Revision、发送方身份、成功状态和失败原因。Server 使用准备时的参与者快照，忽略过期、重复和无效发送方结果；失败结果不能被后续成功覆盖。开局参与者断线会使本局准备失败，避免用已经过期的 GameSessionContext 继续启动。

本机服务初始化有独立真实时间看门狗；Server 的业务 Ready 等待和玩家 Ready 等待也有超时，不依赖 timeScale。Dedicated Server 不等待自己的客户端 ACK，但执行并检查本机业务准备。

Commit 前失败：清理本轮新增资源，保留旧 Scope。Commit 后，包括 Activate 启动的异步任务失败：统一返回 Lobby，不恢复复杂业务快照。联机恢复失败时结束网络会话并尝试本地回大厅。

所有异步服务必须响应取消。ShutdownScopeAsync 的等待被取消，不代表业务关闭已经结束；管线不会在仍有写入任务时按正常路径提前释放 Root。无法在期限内关闭时进入终止会话的兜底路径。

本次接入同时修复了真实往返时暴露的问题：

- SyncObjectPool 在未 Spawn 的本地对象调整父节点时临时关闭 NGO 父节点同步；客户端关闭池前等待 Server Despawn。
- StandView 从自身 LobbyScene 获取相机，避免 Additive 返回大厅时缓存即将卸载的游戏相机。
- AvatarResManager 按展位 Revision 丢弃卸载时的过期加载结果，避免访问已由场景清理释放的实例句柄。
- Activate 后的异步失败若在切场 `finally` 前到达，Server 会先取消当前流程，再排队等待它退出并执行一次回大厅恢复，避免只取消末尾已无 `await` 的流程而停留在 Game Scope。

## 7. 验证与复现

编辑器菜单：Tools/ProjectGame/Run SceneFlow Pipeline Tests。

测试包含管线独立夹具、真实 Bootstrap 的阶段 / 失败 / 超时 / 逆序关闭、生产 Host 两次往返、Root 与 Player 物理场景归属，以及关闭 NGO 后只复用会话种子再次开房。XML 结果输出到 Temp/SceneFlowPipeline/playmode-results.xml。

独立进程测试场景：Tests/SceneFlow/Fixtures/NetworkSmoke.unity。
测试脚本：Tests/SceneFlow/SceneFlowNetworkSmoke.cs。
开发测试包使用 IncludeTestAssemblies；直接编译 Gameplay 程序集的 Mono 开发包用于验证 NGO 与 Addressables 联机链，不替代 HybridCLR / IL2CPP 发布包验证。

命令行参数：

- --scene-flow-role=client
- --scene-flow-output=本轮结果目录的绝对路径
- --scene-flow-content=ServerData/StandaloneWindows64 的绝对路径
- --scene-flow-failure=initialize（可选；客户端清空本机测试配置，验证初始化失败上报）

使用独立结果目录避免上一轮 flag 干扰。Host 先运行，Client 使用同一结果目录；联机端口为 17879。测试仅将开发 HTTP 资源映射到本机已构建内容，生产 Addressables 地址保持不变。

失败用例给 Host 与 Client 都传入 --scene-flow-failure=initialize，使两端按预期失败用例过滤日志并缩短为一轮；脚本只会在 Client 角色清空自身 RoomTemplate 表，因此 Host 配置保持正常。

独立进程结果：

2026-09-05 最终 PlayMode 回归：29 / 29 通过，其中包括原管线用例、Bootstrap 接入用例及生产 Host 往返 / 会话重开验证。

| 用例 | Host | 独立 Client | 记录 |
| --- | --- | --- | --- |
| 两次正常开局并返回大厅，每端两个玩家完成初始化 | 通过，2 局，0 错误 | 通过，2 局，0 错误 | Temp/SceneFlowSmoke/normal/host.json、client.json |
| Client 子服务初始化失败后双方恢复 Lobby | 通过，确认预期失败上报 | 通过，确认预期失败上报 | Temp/SceneFlowSmoke/client-initialize-failure/host.json、client.json |

环境：Unity 2022.3.44f1c1 / NGO 1.10.0。独立 Client 使用 Mono 开发测试包和本机新构建 Addressables 内容。测试过程中的 ScriptingBackend、HybridCLR 开关及生产构建配置已恢复。
