# Gameplay UI 框架

入口为 `ProjectGame.HotFix.UI.Gameplay.GameplayUIManager`，场景为
`Assets/_HotUpdate/Scenes/UIGameUIScene.unity`。当前只提供框架、三个 Canvas 层及专属
EventSystem，Presenter 注册列表为空；没有 HUD、Roll 卡片、玩家状态页和设置页的具体实现。

## 外部调用

UI 上层可直接调用 Manager，并检查 bool 返回值：

```csharp
using ProjectGame.HotFix.UI.Gameplay;

GameplayUIManager ui = GameplayUIManager.Instance;
bool accepted = ui != null && ui.Show(GameplayUIId.EffectRoll);
ui?.Hide(GameplayUIId.EffectRoll);       // 业务完成后强制关闭
ui?.Toggle(GameplayUIId.PlayerStatus);  // 用户打开/关闭
ui?.Show(GameplayUIId.Settings);        // 在当前 Screen 上覆盖设置
ui?.TryNavigateBack();
ui?.CloseAllScreens();                 // 关闭 Screen 和 Modal，保留 HUD 选择
ui?.ResetToGameplay();                 // 关闭功能页并恢复 HUD
```

`Show/Hide` 对已打开/已关闭的已注册页面幂等。`Show` 不会把已被覆盖的页面重新提到栈顶。
`IsOpen` 查询是否仍在打开会话中，`IsVisible` 查询当前是否实际显示。未注册 ID 返回 false。
生命周期回调里的导航请求延后到当前切换结束执行，bool 表示请求已接受，不能视为业务操作成功。

Gameplay 程序集通过 Events 内的门面请求，避免 Gameplay 与 UI 程序集循环引用：

```csharp
GameplayUIRequests.Show(GameplayUIId.EffectRoll);
GameplayUIRequests.Hide(GameplayUIId.EffectRoll);
```

这些是本机主线程事件，不执行 RPC。请求不跨场景缓存：管理器未初始化、模块未注册，或当前
不是 `GameState.GamePlaying` 时，打开请求不会执行；业务层应在 UI 就绪后发起请求。
UI 只展示 Model/服务的状态，选择结果由模块 Presenter 请求业务层处理和校验。

## 分层与默认规则

| 模块 | 层 | 占用输入 | ESC 可关闭 | 隐藏 HUD |
| --- | --- | --- | --- | --- |
| GameplayHUD | Hud | 否 | 否 | 否 |
| EffectRoll | Screen | 是 | 否 | 是 |
| PlayerStatus | Screen | 是 | 是 | 否 |
| Settings | Modal | 是 | 是 | 否 |

Hud 可共存并始终不接收交互。Screen 和 Modal 各维护一个无重复的栈，同层只有栈顶可见。
被其他 Screen 覆盖的页面保持 IsOpen，隐藏 View；Modal 下的 Screen 保持可见但停止交互。
关闭页面会恢复下层的同一个 Presenter 实例。关闭被覆盖的页面会把它移出栈，不会随后复活。
HUD 的主动隐藏与被策略遮挡分开保存，功能页关闭不会把用户主动隐藏的 HUD 显示出来。

返回先交给顶层 Presenter 的 `TryHandleBackRequest()`，处理内部弹窗、页签或改键取消。
未消费时，Manager 关闭允许返回的顶层页。必选 EffectRoll 不被返回关闭，可在其上打开
Settings，关闭 Settings 后仍回到 Roll。`Hide` 和 `CloseAllScreens` 是业务强制关闭入口。
`Toggle` 与 `CloseTop` 遵守 `CanCloseOnBack`。

`GameplayUIPolicy` 可在具体 Presenter 的 Inspector 中勾选 Override Policy 后覆盖。
策略在注册时复制，修改后需重新初始化。新增 ID 时应补充默认策略或显式覆盖策略。

## 接入 MVP 模块

1. 创建一个继承 `BaseGameplayUIView` 的 View，挂在所属层的面板根上；基类要求 CanvasGroup。
   View 只显示控件、抛出交互事件，可配置默认选中的 Selectable。
2. 创建继承 `BaseGameplayPresenter<具体View>` 的 Presenter，重写 `Id` 与 `RenderView()`，
   在 Inspector 绑定 View。Model/业务服务通过该模块自己的 Bind 方法或服务接口注入。
3. 将 Presenter 拖入 Manager 的 Presenters 数组；其 View 必须位于对应 Hud/Screen/Modal Root 下。
4. 在 `OnInitialize` 绑定 View 事件，在 `OnShutdown` 解绑；业务订阅按模块需要在
   `OnOpened/OnClosed` 或 `OnShown/OnHidden` 成对管理。隐藏后基类的 `Refresh()` 不执行渲染。
5. Presenter 内通过受保护的 `Navigator.Show/Hide/Toggle/TryNavigateBack` 请求导航。
   状态页内部的多个子面板/页签由自身 Presenter 编排，Manager 只管理整个模块。

基类生命周期：

```text
Initialize（每次管理器生命周期一次，绑定 View）
  Opened（开始打开会话）
    Shown / RenderView（显示，创建 VisibleToken）
    InteractionChanged（模态覆盖时可见但不交互）
    Hidden（被同层覆盖，取消 VisibleToken）
    Shown / RenderView（恢复并读取最新 Model）
  Closed（结束打开会话）
Shutdown（清理全部订阅与资源，幂等）
```

`VisibleToken` 只对应当前可见区间，不用于必须在 Modal 覆盖后继续完成的业务事务。
显示状态由 CanvasGroup 的 alpha/interactable/blocksRaycasts 控制，根 GameObject 保持激活。
不要从业务代码直接改 Presenter View 的显隐或调用生命周期接口。
`BaseGameplayUIView.OnPresentationChanged` 留有视觉扩展钩子；动画需自行取消上次过渡，并使用
不受 timeScale 影响的时间。框架当前同步切换，不包含异步页面加载和过渡动画调度。

## 输入与场景生命周期

`InputManager` 的 Menu Map 在 Gameplay/UI 上下文都启用，在 Disabled 或非 Gameplay 基础
上下文关闭。默认 Escape / Gamepad Start、B 为返回，Tab / Gamepad Select 为玩家状态页。
状态页快捷键在 Modal 或不可返回 Screen 顶层时不执行。

UI Map 包含 Point、Click、ScrollWheel、Navigate、Submit。EventSystem 在运行时绑定
InputManager 克隆的同一份 InputActionAsset；Cancel 不交给 EventSystem 二次派发。
Manager 使用一个 UI 输入租约，任一阻塞页保留时不会释放；关闭最后一个阻塞页时仅释放自己的
租约。外部 Disabled 上下文会暂时停用 UI 交互，释放后恢复。

场景中的专属 EventSystem 默认禁用，只在交互页启用期间工作，避免与切场期间的 Lobby
EventSystem 重叠。三层 Canvas 的排序为 100 / 200 / 300，参考分辨率为 1920×1080。
模块应使用所属层排序，避免自行 overrideSorting 越过 Modal。

Manager 监听 GameStateChangedEvent，并在初始化时读取现有 GameState，以支持晚加载。
进入 GamePlaying 显示已注册的 HUD；离开该状态立即关闭全部页面并释放自身输入占用。
OnDisable/OnDestroy 负责幂等清理；重新启用后会重新初始化。场景退出强制清理不发布正常
导航事件，业务不得依赖显隐事件完成结算。Manager 不修改 timeScale 或服务器游戏状态。

## 程序集与验证

ID、请求、通知契约位于 `HotFix.Events`。MVP 基类、Manager、输入适配位于
`HotFix.Gameplay.UI`，引用 Gameplay、SceneFlow、Events 以及 Unity UI/Input System。
新程序集已登记到 HybridCLRSettings，加载顺序排在 Gameplay 之后。
Player 发布时仍需执行项目已有的 HybridCLR 生成和 `Tools/HotUpdate/Build And Sync DLLs`
流程，使新 DLL 及输入/场景资源进入 Addressables；本次源码开发不会替换已有发布包。

Unity 菜单 `Tools/Tests/Run Gameplay UI Tests` 运行 UI EditMode 测试与现有 InputManager
回归测试。结果生成于 `Temp/GameplayUI/EditModeResults.xml`。
测试夹具是独立的测试程序集，不属于 HybridCLR 热更程序集，也不挂在正式场景中。
