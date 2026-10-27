# Gameplay 统一 Tick 阶段架构

## 1. 修改目的

怪物、玩家、弹丸、房间流程和网络表现继续使用同一个 `NetworkSimulationClock`，不增加怪物—武器专用桥接层。时钟在一个 Tick 内按固定阶段派发，使业务执行顺序由阶段语义决定，不再隐式依赖 `GameRoot` 中服务的初始化先后。

服务初始化顺序仍用于保证依赖已经创建，例如 Weapon 必须在 Monster 后初始化；它不再承担运行时 Tick 排序职责。

## 2. 固定阶段

`GameplayTickPhase` 的顺序就是执行顺序：

| 阶段 | 当前职责 |
|---|---|
| `PreSimulation` | 在权威模拟前更新动态地图状态；当前为巡逻墙 |
| `ActorSimulation` | 玩家权威模拟、Owner 预测，以及由玩家射击确认产生的弹丸 |
| `WorldSimulation` | 怪物 AI、寻路与移动，并把怪物空间索引标脏 |
| `PhysicsSync` | 所有 Actor/World Transform 更新完成后，统一提交 Unity Physics 场景 |
| `MeleeCombat` | 结算 WorldSimulation 产生的怪物近战请求 |
| `CombatSimulation` | 弹丸移动、空间查询、伤害、持续伤害区和武器特殊效果 |
| `PostSimulation` | 房间归属、集合与战斗流程等结算 |
| `Replication` | 捕获并发送本 Tick 最终状态；Host 表现也在这里刷新 |
| `Completed` | 兼容旧的 Tick 尾部订阅；新系统不应默认挂到这里 |

`NetworkSimulationClock.AdvanceOneTick` 每次完整执行上述全部阶段。`GameplayNetworkTickDriver` 在掉帧补跑多个 Tick 时，也是逐 Tick 完整走完一条阶段链，不会先批量执行某个阶段再补另一个阶段。

玩家输入、预测校正所用快照仍与 `PlayerSyncController` 的玩家时间线一起留在 `ActorSimulation`；本次没有为了形式统一而拆开已经内聚的玩家协议。`Replication` 当前负责怪物最终状态和武器表现事件，后续只有在玩家协议确实需要消费战斗后状态时再迁移。

## 3. 当前调用链

```text
GameplayNetworkTickDriver.Update
  -> NetworkSimulationClock.AdvanceOneTick
     -> PreSimulation
        -> RoomPatrolWall.HandleTick
     -> ActorSimulation
        -> PlayerSyncController.HandleGameplayTick
           -> PlayerServerAuthority.SimulateNextTick
           -> PlayerWeaponController.CommitAuthoritativeShot
              -> WeaponRuntimeService.CommitShot
     -> WorldSimulation
        -> MonsterRuntimeService.TickAuthoritySimulation
           -> MonsterRoomRuntime.Tick
              -> MonsterSimulation.Tick
                  -> 记录 MonsterMeleeAttackRequest
              -> MonsterSpatialIndex.Invalidate
     -> PhysicsSync
        -> WeaponRuntimeService.SyncPhysicsTransforms
     -> MeleeCombat
        -> MonsterRuntimeService.ResolveAuthorityCombat
           -> MonsterRoomRuntime.ResolveCombat
              -> MonsterMeleePhysicsResolver
     -> CombatSimulation
        -> WeaponRuntimeService.TickCombat
           -> ProjectileSimulation.Tick
              -> MonsterSpatialIndex.QuerySweptCircle
              -> MonsterRuntimeService.DamageMonster
           -> TickStormClouds / TickDamageAreas
     -> PostSimulation
        -> RoomFlowController.HandleTickCompleted
     -> Replication
        -> MonsterRuntimeService.PublishAuthorityState
            -> CaptureFrame / SendPresentation（按发送间隔）
            -> Host 按 PresentationFrame 应用怪物表现
        -> WeaponPresentationService.FlushServerFrame
     -> Completed
        -> 仅兼容旧 TickCompleted 订阅
```

关键保证是 `Actor -> World -> PhysicsSync -> MeleeCombat -> Combat -> Replication`。同 Tick 确认的射击可以在 Combat 阶段推进；弹丸造成的死亡、击退与状态变化会在随后 Replication 阶段进入怪物最终状态和表现帧。怪物近战不再在 WorldSimulation 中直接访问 Unity Physics 或修改玩家生命。

## 4. 订阅规则

- 新系统使用 `Subscribe(GameplayTickPhase, Action<uint>)` 和对应的 `Unsubscribe`。
- 同一阶段内不承诺跨系统先后顺序；存在先后依赖时应选择不同阶段，而不是依赖初始化顺序。
- Tick 回调同步执行。异常继续向上暴露，不在时钟层吞掉或跳过某个系统。
- 热路径使用预分配的阶段委托数组；推进 Tick 不创建列表、排序器或任务对象。
- `TickAdvanced` 暂时映射到 `ActorSimulation`，`TickCompleted` 暂时映射到 `Completed`，只为兼容尚未迁移的外部调用者。

## 5. Profiler Marker

每个阶段都有独立 Marker：

- `Gameplay.TickClock.PreSimulation`
- `Gameplay.TickClock.ActorSimulation`
- `Gameplay.TickClock.WorldSimulation`
- `Gameplay.TickClock.PhysicsSync`
- `Gameplay.TickClock.MeleeCombat`
- `Gameplay.TickClock.CombatSimulation`
- `Gameplay.TickClock.PostSimulation`
- `Gameplay.TickClock.Replication`
- `Gameplay.TickClock.Completed`

旧采样中的 `Gameplay.TickClock.TickAdvanced` 和 `Gameplay.TickClock.TickCompleted` 是历史两段式包络，不能与新阶段 Marker 直接逐项比较。复测时仍以 `Gameplay.TickDriver.Dispatch` 为同一 Tick 的总包络，再观察各阶段占比。

## 6. 本次修改清单

- `NetworkSimulationClock` 增加固定阶段枚举、订阅 API 和逐阶段 Marker；
- 玩家同步显式迁入 `ActorSimulation`；
- 巡逻墙迁入 `PreSimulation`；
- 怪物权威模拟与状态发布拆成 `WorldSimulation` / `Replication`；
- 武器与弹丸迁入 `CombatSimulation`；
- 房间流程迁入 `PostSimulation`；
- 武器表现帧发送迁入 `Replication`；
- 保留旧事件映射，降低未检索到的外部扩展代码升级风险。

## 7. 后续优化边界

该调度层只定义时间尺度和阶段，不持有 Monster、Weapon 或 Player 引用。后续工作可以直接落入已有阶段：

1. Projectile AOT 迁移和 SoA 化只替换 `CombatSimulation` 内核，不改变上层时间轴。
2. 移除怪物 GameObject 只替换 `Replication` 后的表现生命周期，不改变权威模拟顺序。
3. 怪物网络内/中/外层同步在 `Replication` 中决定接收者、频率与群组编码；四叉树可提供空间候选和群组输入，但同步层级不能只由叶节点深度决定。
4. 怪物寻路重建继续属于 `WorldSimulation`，可独立量化并优化，不向时钟层增加专用分支。

## 8. 2026-10-27 架构对齐修正

- 新增 `PhysicsSync` 与 `MeleeCombat` 阶段。`Physics.SyncTransforms()` 从 Weapon 的 Combat 热路径移到明确的阶段订阅中，当前仍由 Weapon 服务注册，但其职责已限定为提交动态 WorldMask 碰撞体。
- `MonsterSimulation` 只在 `WorldSimulation` 记录近战请求；`MonsterMeleePhysicsResolver` 在 `MeleeCombat`、Physics 同步之后执行，避免 WorldSimulation 使用未同步的 Unity Physics 场景。
- `MonsterViewHandle` 不再暴露或写入运行时 Pose。怪物命中已经走 `MonsterWorld + MonsterSpatialIndex`，表现层只更新 VAT 状态；后续可继续移除每怪 GameObject。
- Weapon 的范围伤害与连锁闪电已通过 `MonsterRuntimeService.QueryMonsterCircleCandidates` 进入房间空间索引入口，先消除业务层直接遍历全部 Slot 的分叉路径。四叉树实现暂不替换，固定网格统一快照作为后续评估方向。

