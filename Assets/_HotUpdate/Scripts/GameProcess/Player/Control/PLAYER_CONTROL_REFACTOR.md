# 玩家控制与同步链路收束说明

更新时间：2026-08-13

## 目标与结果

本次修改将 `Control` 下的本地控制与网络同步代码按职责重新分层，并补齐正式玩家预制体上的运行链路。运行时代码由原 27 个脚本收束为 13 个脚本，另保留 1 个独立测试场景驱动脚本，运行时代码文件减少约 52%。

所有配置项 `Header`、Inspector 显示名与功能提示均已改为中文；数值提示明确说明了增大/减小时的实际效用。历史 GBK/ANSI 同步脚本在合并时统一为 UTF-8。

## 当前目录结构

```text
Control/
├─ Local/
│  ├─ Upper/
│  │  └─ LocalPlayerLocomotionDriver.cs
│  ├─ Middle/
│  │  └─ PlayerLocomotionController.cs
│  ├─ Lower/
│  │  └─ PlayerMotor.cs
│  ├─ Data/
│  │  └─ PlayerControlData.cs
│  └─ Config/
│     └─ PlayerControlConfig.cs
├─ Sync/
│  ├─ Upper/
│  │  └─ PlayerSyncController.cs
│  ├─ Middle/
│  │  ├─ PlayerSimulation.cs
│  │  ├─ PlayerPrediction.cs
│  │  ├─ PlayerServerAuthority.cs
│  │  └─ PlayerRemoteInterpolation.cs
│  ├─ Lower/
│  │  └─ PlayerSyncTransport.cs
│  ├─ Data/
│  │  └─ PlayerSyncData.cs
│  └─ Config/
│     └─ PlayerSyncConfig.cs
├─ Tests/
│  └─ PlayerLocomotionTestDriver.cs
└─ PLAYER_CONTROL_REFACTOR.md
```

分层定义：

- Upper：输入采集、网络身份判断、生命周期与链路编排。
- Middle：移动状态规则、预测/回滚、服务器权威模拟和远端插值策略。
- Lower：`CharacterController` 运动执行和 NGO 消息传输。
- Data：枚举、输入命令、运行时状态、网络快照与 Tick 缓存。
- Config：只保存可序列化参数和参数合法性校验。

## 正式运行链路

### Remote Client Owner

```text
InputManager + Camera
  -> LocalPlayerLocomotionDriver
  -> PlayerInputCommand
  -> PlayerSyncController（NGO LocalTime Tick）
  -> PlayerPrediction
  -> PlayerSimulation
  -> PlayerLocomotionController
  -> PlayerMotor
  -> PlayerSyncTransport.SendInputBatch
  -> Server Snapshot
  -> Reconcile / Rollback / Replay
```

### Server Authority

```text
PlayerSyncTransport.ReceiveInput
  -> PlayerServerAuthority 输入缓冲
  -> NGO ServerTime Tick
  -> PlayerSimulation
  -> PlayerLocomotionController
  -> PlayerMotor
  -> PlayerSimulationState Snapshot
  -> PlayerSyncTransport.SendSnapshot
```

### Remote Observer

```text
Server Snapshot
  -> PlayerRemoteInterpolation 环形缓冲
  -> 延迟插值
  -> PlayerSimulation.RestoreState
  -> Transform / Motor / Gameplay 状态保持一致
```

Host Owner 直接走服务器权威路径，不在同一对象上重复执行客户端预测。

## 脚本合并明细

- `PlayerControlData.cs` 合并：`PlayerLocomotionInput`、`PlayerMotionState`、`PlayerControlState`、`PlayerStaminaState`、`PlayerMotorRuntimeState` 及关联枚举/命令。
- `PlayerControlConfig.cs` 合并：`PlayerMovementProfile`、`PlayerMovementConfig`、`PlayerStaminaConfig`。
- `PlayerLocomotionController.cs` 吸收：`PlayerStateMachine` 与 `PlayerStaminaLogic`，状态和体力只在中层更新一次。
- `PlayerSyncData.cs` 合并：`PlayerInputCommand`、`PlayerSimulationState`、`TickMath`、`TickRingBuffer`。
- `PlayerPrediction.cs` 合并：预测历史、校正/回滚与输入发送调度。
- `PlayerServerAuthority.cs` 合并：服务器输入缓冲与权威模拟。
- `PlayerRemoteInterpolation.cs` 合并：远端快照缓存与插值采样。
- `PlayerSyncTransport.cs` 吸收：`IPlayerSyncEndpoint`。
- 删除未接入实际动作链的 `PlayerActionCommandHeader` 与 `LastProcessedActionSequence` 脚手架。

## 完善与缺陷修复

1. **正式输入链闭环**：`LocalPlayerLocomotionDriver` 不再在 `Update` 中直接调用 `Simulate(Time.deltaTime)`，只提交 `PlayerInputCommand`，避免正式玩家同时按可变帧率与固定 Tick 被重复模拟。
2. **统一网络时钟**：Owner 预测与 Server Authority 改由 NGO `NetworkTickSystem.Tick` 驱动。Owner 从 `LocalTime.Tick` 起步，服务器从 `ServerTime.Tick` 起步；服务器输入缓冲因此能正常接收处于未来窗口内的客户端输入。
3. **TickRate 对齐**：同步配置默认 TickRate 改为项目 NGO 的 30；运行时若二者不一致会打印警告。
4. **预测硬同步**：服务器 Tick 超前或预测历史已不可恢复时执行 Hard Resync；回放遇到输入历史缺口时不会留下“状态已回退、CurrentTick 仍领先”的不一致状态。
5. **起始 Tick 容错**：客户端 LocalTime 领先时，早于本地预测起点的服务器快照只推进确认 Tick，不会把客户端倒退到旧 Tick。
6. **瞄准即时发送**：`AimDirection` 变化纳入立即发送判定，解决持续按住瞄准时方向改变未发送的问题。
7. **uint Tick 溢出**：远端插值统一使用相对最新 Tick 的无符号距离，不再直接比较/相减绝对 uint Tick。
8. **远端传送断点**：相邻快照位移超过配置阈值时清空旧插值历史，避免换层/传送被缓慢插值穿过场景。
9. **传送状态复位**：`PlayerSpawnController.RepositionPlayers` 优先调用 `PlayerSyncController.ResetAfterWarp`，统一清空 Motor 惯性、预测历史、权威输入缓冲和插值缓冲。
10. **远端组件状态一致**：观察者采样通过 `PlayerSimulation.RestoreState` 落地，使 Transform、Motor 速度、控制状态与体力状态一致。

## PlayerRuntimeRoot 预制体

`Assets/_HotUpdate/Prefabs/Character/PlayerRuntimeRoot.prefab` 已补齐以下组件：

1. `CharacterController`
2. `PlayerMotor`
3. `PlayerLocomotionController`
4. `PlayerSimulation`
5. `PlayerSyncController`
6. `LocalPlayerLocomotionDriver`

`PlayerSyncController._simulation` 已显式绑定。预制体扫描结果为 0 个 Missing Script。

## 配置与 Inspector

配置集中在：

- `Local/Config/PlayerControlConfig.cs`
- `Sync/Config/PlayerSyncConfig.cs`

全部 `Header` 为中文，所有序列化可配置字段均带中文 `Tooltip` 与 `InspectorName`。提示覆盖速度、加减速、急转、旋转、背向惩罚、体力、历史容量、容错窗口、误差阈值、网络发送和插值等参数，并注明数值增大/减小的影响。

## 测试与验证

新增 `Assets/_HotUpdate/Tests/EditMode/PlayerSyncPipelineTests.cs`，覆盖：

- 持续瞄准时改变 `AimDirection` 会立即发送。
- 服务器接受下一 Tick 输入并拒绝已处理 Tick。
- 服务器快照领先预测时执行 Hard Resync。
- 远端插值跨 `uint.MaxValue -> 0` Tick 溢出仍能采样。

最终验证：

- Unity 2022.3.44f1c1 编译：0 error。
- EditMode 测试：8/8 通过，0 failed。
- `PlayerRuntimeRoot`：完整组件栈存在，0 Missing Script，`_simulation` 引用已绑定。
- 全场景/资源 Missing Reference 扫描：0。
- `PlayerLocomotionTest` PlayMode 冒烟运行：进入/退出正常，Console 0 error。
- 原测试场景 `Assets/_HotUpdate/Scenes/Tests/PlayerLocomotionTest.unity` 保持可用，仍使用无 Animator 白模验证本地移动手感。

## 兼容性说明与下一步验证

- 核心 MonoBehaviour（`PlayerMotor`、`PlayerLocomotionController`、`LocalPlayerLocomotionDriver`、`PlayerSimulation`、`PlayerSyncController`、测试 Driver）通过 Unity `AssetDatabase.MoveAsset` 移动，保留原 `.meta` GUID，避免已存在场景/预制体引用失效。
- 数据类与纯逻辑类没有独立 Unity 序列化引用，合并后由 Unity 自动生成新 `.meta`。
- 本次已完成编译、EditMode 自动测试与单预制体结构验证。正式网络体验仍建议使用 Host + 独立 Client 进行一次多实例验收，并在网络模拟器中覆盖 100~200ms 延迟、5%~10% 丢包、玩家换层和长时间运行。
