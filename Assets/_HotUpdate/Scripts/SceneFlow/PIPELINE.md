# Addressables SceneFlow 管线

执行顺序：物理场景 Load → Prefab Prepare → Server Spawn → RootReady → Bind / Initialize → Commit → 旧 Scope Cleanup → 旧物理场景 Unload → Activate。

- RootReady 确认本机需要的 Root 已 Spawn、登记并完成接口扫描。
- `IScopeBindable`、`IScopeInitializable`、`IScopeActivatable` 位于 `HotFix.Network.Runtime`。按需实现，没有默认空方法。扫描包含未激活子节点，排除另一个 NetworkObject 的子树。
- 扫描只发生在本机 Spawn 登记时。阶段按 Catalog SpawnOrder、PrefabId、组件层级顺序执行；Despawn 清除缓存。
- 每个 Root 的生命周期每次 Spawn 执行一次。已激活且复用的 Root 保持原运行态，不重新初始化。需要随场景变化的业务绑定应在后续接入时明确设计，不在本次框架补全中隐式重启持久服务。
- 本机全部 Bind 完成后执行全部 Initialize，两者合并一个 RuntimeReady ACK。这里没有跨端 Bind 单独屏障。
- Bind/Initialize 只能准备新 Root 的引用和资源，必须响应 CancellationToken；不能修改旧 Root 的运行态、提前生成玩家或开始 Gameplay。取消/Despawn 后由组件自身释放初始化资源。
- Commit 要求 RuntimeReady，保留 ACK；Cleanup 保留上下文直到 Activate。Activate 同步触发业务入口，不收成功 ACK，不保证各端同帧开始。激活异常会上报 Server 并返回大厅。
- 实现 `IScopeShutdown` 的 Root 在 Cleanup 内先全端关闭业务，再由 Server Despawn；关闭和资源释放分别确认。Bootstrap 的异步启动错误通过 `ReportRuntimeFailure` 进入同一个返回大厅入口。
- Persistent Root 置于 DontDestroyOnLoad；SceneScoped Root 配置 OwnerSceneName，并在各端 Spawn 登记时迁入已加载的物理场景。玩家与 GameRoot 内对象池实例跟随 GameRunTimeScene。

## 失败与资源所有权

ACK 校验 Revision、Phase 和参与 ClientId，重复或迟到的结果不能覆盖首次结果。收到失败 ACK 后仍等待其他参与端完成或到达本轮截止时间；断线客户端沿用原有移出等待集合的规则。Dedicated Server 自身执行有本地超时，零客户端时不等待自身 ACK。

超时会取消本地操作；回滚先等待旧操作真正退出，再 Despawn、Release。无法退出的操作不会与下一项资源写入并发运行，会转入恢复流程。所有超时按真实时间计算，不受 timeScale 影响。

物理场景加载一开始就登记 Addressables Handle。取消等待不会遗失仍在加载的场景，后续 Unload 能等待并清理它。已开始的卸载持有 Handle 到实际完成，取消或重复调用只停止/复用等待，不重复卸载或提前释放 Handle。

Commit 前回滚本轮新增 Root、Prefab 和目标场景，保留旧 Scope。Commit 已开始后，不恢复业务快照：销毁 SceneScoped Root，保留 Persistent 会话 Root，从各端实际状态重新准备大厅并卸载游戏场景。大厅恢复失败时终止网络会话并尝试本地返回大厅，不递归重试。大厅资源本身不可用等不可恢复错误会输出日志。

`NetworkSceneMask` 使用可被 Unity 序列化的 int 枚举；标志位仍为 1、2、4，场景 RPC 继续使用 ulong 数值。

## 本次范围与验证

生产 GameRoot、Persistent 会话 Root 和 LobbyNetworkManager 已接入。`GameRuntimeBootstrap.RunRuntimeAsync` 由 Activate 触发，保留原有 IGameRuntimeService 启动顺序。管线 RuntimeReady 与 Gameplay RuntimeReady / PlayerRuntimeReady 各自负责不同的就绪条件，详见同目录 [网络组件接入说明](网络组件接入Addressable场景加载说明.md)。

Unity 菜单：`Tools/ProjectGame/Run SceneFlow Pipeline Tests`。

测试使用 `Assets/_HotUpdate/Tests/SceneFlow/Fixtures` 中独立的空场景、Prefab 和 Catalog，以运行期 ResourceLocator 接入 Addressables，不修改生产 Addressables 分组或场景。覆盖 Host 自身 RPC 与无客户端 Dedicated Server、本地阶段顺序、回滚/重试、Activate 失败恢复、超时、ACK 过滤及取消中的场景清理。

结果：`Temp/SceneFlowPipeline/playmode-results.xml`。当前还包含生产 GameRoot 的 Host 往返验证；独立进程联机测试和记录见接入说明。

前一轮管线独立验证：Unity 2022.3.44f1c1，22 / 22 PlayMode 测试通过；当时尚未迁移生产组件。本轮增加 Bootstrap 接入测试并迁移生产资源，最新结果以接入说明为准。
