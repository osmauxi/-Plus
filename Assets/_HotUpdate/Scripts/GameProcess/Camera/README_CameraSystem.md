# Gameplay 摄像机系统

本文说明 `GameRunTimeScene` 当前使用的 Gameplay 摄像机结构、数据流、效果配置和扩展约定。脚本命名空间统一为 `ProjectGame.HotFix.Gameplay.CameraSystem`。

## 场景挂载

```text
Main Camera
└─ Camera + CinemachineBrain

GameRoot
├─ PlayerCameraRig
│  ├─ PlayerCameraController
│  ├─ FollowPivot
│  ├─ LookAtPivot
│  └─ PlayerVirtualCamera
│     ├─ CinemachineVirtualCamera
│     ├─ CinemachineImpulseListener
│     └─ cm
│        ├─ CinemachineTransposer
│        └─ CinemachineComposer
└─ CameraEffectManager
   ├─ CameraEffectManager
   └─ CinemachineImpulseSource
```

`PlayerCameraController` 的 World Camera、Virtual Camera、Follow Pivot、LookAt Pivot 已在场景中显式绑定。`CameraEffectManager` 也已绑定 Controller 和 Impulse Source。当前场景扫描结果为 0 个 Missing Reference。

`GameRuntimeBootstrap` 按序先初始化 `PlayerCameraController`（服务序号 6），再初始化 `CameraEffectManager`（服务序号 7）；关闭时顺序相反。这保证效果管理器发出命令前，控制器和 Cinemachine 管线已经就绪。

## 脚本职责

| 脚本 | 职责 |
| --- | --- |
| `PlayerCameraController.cs` | Unity 生命周期、目标仲裁、输入命令翻译、Cinemachine 引用和最终应用 |
| `CameraMotionModel.cs` | 纯 C# 计算 Yaw、基础缩放、持续 Zoom/FOV Modifier、瞬时 Kick |
| `CameraCompositionModel.cs` | 纯 C# 计算 Aim LookAhead 与 Movement LookAhead |
| `CameraEffectManager.cs` | 将语义效果映射为 Impulse、Zoom、FOV 和 Aim 构图，并桥接本地玩家同步事件 |
| `CameraEffects.cs` | Gameplay 侧统一静态入口，只发布本地事件 |
| `CameraEffectEvents.cs` | 瞬时效果、持续效果和 Aim 世界点的事件数据 |
| `CameraEffectId.cs` | 稳定的语义效果 ID；枚举值已显式固定，避免场景序列化错位 |

Controller 只负责协调，不在自身字段中重复实现平滑算法。两个 Model 不持有 `Transform`、Cinemachine、输入或玩家引用，因此可以独立测试。

## 每帧数据流

```text
InputManager
├─ RotateStep / Zoom ───────────────> CameraMotionModel
└─ PointerPosition + AimHeld
   └─ LocalPlayerLocomotionDriver
      └─ CameraEffects.UpdateAimTarget(worldPosition)
         └─ CameraAimTargetUpdatedEvent

PlayerManager / 临时镜头系统
└─ GameplayCameraTargetRequestedEvent
   └─ PlayerCameraController 目标仲裁

PlayerCameraController.LateUpdate
├─ 计算原始 Follow / LookAt 位置
├─ 检测 Snap / Teleport
├─ CameraCompositionModel.Tick
├─ CameraMotionModel.Tick
└─ 应用 Pivot、Transposer FollowOffset 与 VirtualCamera FOV
```

执行顺序中，`PlayerPresentationDriver` 为 `-300`，`PlayerCameraController` 为 `-200`。摄像机读取统一的 Render Pose，不另建第二条插值时间线。

## 目标请求与切换

目标通过 `GameplayCameraTargetRequestedEvent` 发布：

- `Requester` 是请求身份；同一请求方再次发布会更新旧请求。
- `Priority` 越高越优先；同优先级使用最后发布的请求。
- 临时镜头释放后会自动恢复下一条有效请求。
- `Snap=true`、首次绑定、显式 Snap 事件或单帧位移超过 8 米时，会清除 Cinemachine 历史并立即对齐。
- `Snap=false` 的目标切换会保留 Cinemachine 软过渡，但重置 Movement LookAhead 采样，避免把新旧目标的位置差误判为高速移动。
- 普通帧会持续更新原始目标采样；传送判定只比较相邻帧，不累计移动距离。

## Motion 分层

观察高度和 FOV 均使用三层叠加：

```text
最终观察高度 = Clamp(玩家基础高度 + Σ持续 Zoom Modifier + Zoom Kick)
最终 FOV      = Clamp(基础 FOV      + Σ持续 FOV Modifier  + FOV Kick)
```

持续效果以 `CameraEffectId` 为键，可以并存并独立退出；瞬时 Kick 独立维护建立、短暂保持和释放过程。运行时按 K 刷新 Inspector 参数时会保留持续 Modifier 和 Aim 激活状态，只清除瞬时 Kick 与平滑历史。

当前基础配置：

- ViewHeight 20，ViewDistance 13，缩放范围 18–26。
- 初始 Yaw 0°，单次旋转 90°，旋转平滑 0.15 秒。
- 基础 FOV 60°，范围 35°–80°。
- Follow 水平/竖直阻尼为 0.6/0.4，Aim 阻尼为 0.5。

## Composition 分层

`CameraCompositionModel` 将两条世界空间前视偏移相加：

- Aim LookAhead 表达“玩家想看哪里”。距离角色 1 米内为死区，8 米达到最大 3 米偏移；进入平滑 0.12 秒，退出回中 0.18 秒。
- Movement LookAhead 表达“玩家正在往哪里移动”。视觉速度 0.5 以下为死区，7 达到最大 1.5 米偏移；建立 0.18 秒，回中 0.25 秒。
- Aim 激活时 Movement 权重当前为 0，因此瞄准方向完全接管构图，避免两条方向互相争夺。
- FollowPivot 与 LookAtPivot 使用相同构图偏移，因此是整套构图平移，不改变原有俯视角。

瞄准世界点由 `LocalPlayerLocomotionDriver` 的屏幕射线与角色水平面求交得到。射线无有效交点或交点过近时，使用角色前方 10 米作为稳定回退点。

## 效果预设与事件接线

| Effect ID | 触发来源 | Shake | Zoom Offset | FOV Offset | 进入/退出时间 |
| --- | --- | ---: | ---: | ---: | --- |
| `RifleFire` | 本地 `ShotSequence` 递增 | 0.35 | — | +1.5 | FOV 0.02 / 0.10 秒 |
| `PlayerHit` | 本地 `HitSequence` 递增 | 0.80 | +0.5 | +2.5 | 0.03 / 0.14 秒 |
| `PlayerDeath` | 本地 `IsDead` 上升沿 | 1.00 | +2.5 | +5.0 | 0.10 / 0.35 秒 |
| `Explosion` | Gameplay 主动调用 | 1.60 | +1.2 | +4.0 | 0.05 / 0.28 秒 |
| `Aim` | 本地 `IsAiming` 状态边沿 | — | -2.0 | -8.0 | 0.12 / 0.16 秒 |

正 Zoom Offset 增加观察高度（拉远），负值降低观察高度（拉近）；正 FOV Offset 扩大视野，负值收窄视野。

Gameplay 调用示例：

```csharp
CameraEffects.Play(CameraEffectId.Explosion, intensity: 1.25f);
CameraEffects.Set(CameraEffectId.Aim, active: true);
CameraEffects.UpdateAimTarget(aimWorldPosition);
```

`CameraEffectManager` 观察本地 `PlayerSyncController`：Shot/Hit 使用支持 `uint` 回绕的序号比较，Aim/Dead 使用状态边沿。更换本地玩家、服务关闭或对象销毁时会释放 Aim 和全部持续 Modifier，避免对象池复用后残留效果。

Aim 构图开关不依赖 Inspector 中是否存在 Aim 预设；即使效果表临时漏配，构图状态仍会正常切换。

## 扩展约定

新增效果时建议按以下顺序进行：

1. 在 `CameraEffectId` 末尾追加显式数值，不要改动已有值。
2. 在 `GameRunTimeScene/CameraEffectManager` 的 Effects 列表增加唯一配置。
3. Gameplay 只调用 `CameraEffects.Play` 或 `CameraEffects.Set`，不要直接取得 Controller/Manager。
4. 新的持续效果必须保证关闭、换本地玩家和 Shutdown 路径都能清理。
5. 新的纯计算规则优先放进 Motion/Composition Model，并补 EditMode 测试。

效果列表中重复的 ID 会输出警告，并使用最后一项配置；`None` 和空项会被忽略。

## 验证记录

2026-08-28 完成以下验证：

- Unity 脚本重新导入和编译：0 个编译错误。
- `HotFix.Gameplay.EditModeTests`：31/31 通过。
- 新增 `CameraModelsTests`：7/7 通过，覆盖持续效果叠加/移除、瞬时 Kick、刷新时保留 Modifier、Aim 死区/最大偏移、Movement 相邻采样、目标切换采样重置、Aim 独占构图。
- PlayMode 冒烟：Controller/Manager 初始化和关闭成功；Aim 事件开关成功；RifleFire 事件使 FOV Kick 进入生效；Console 0 个错误。
- `GameRunTimeScene` Missing Reference 扫描：0。

测试源码位于 `Assets/_HotUpdate/Tests/EditMode/CameraModelsTests.cs`。
