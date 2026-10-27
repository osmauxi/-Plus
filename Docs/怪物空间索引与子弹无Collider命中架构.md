# 怪物空间索引与子弹无 Collider 命中架构

## 1. 目标与边界

本次修改把弹丸命中怪物的宽相检测从“每颗子弹遍历房间全部 Monster Slot”改为房间级自适应四叉树查询，并彻底移除标准怪物受击 `BoxCollider`。

本阶段保持以下行为不变：

- 怪物权威位置仍由 `MonsterWorld` 的 SoA 数据持有；
- 精确命中仍是 XZ 平面的“弹丸扫掠圆 vs 怪物圆”；
- Unity Physics 只处理墙壁等 `WorldMask`；
- 穿透、反弹、已命中目标过滤与命中特效结算仍由 `ProjectileSimulation` 负责；
- 不在本阶段改网络协议、Projectile SoA 或 AOT 边界。

## 2. 整体架构

```text
NetworkSimulationClock.AdvanceOneTick
  -> WorldSimulation
     -> MonsterRuntimeService.TickAuthoritySimulation
     -> MonsterRoomRuntime.Tick
         -> MonsterSimulation.Tick（更新 MonsterWorld.Motion，记录近战请求）
        -> MonsterSpatialIndex.Invalidate（只标脏）
  -> PhysicsSync
     -> WeaponRuntimeService.SyncPhysicsTransforms（提交动态 WorldMask 碰撞体）
  -> MeleeCombat
     -> MonsterRuntimeService.ResolveAuthorityCombat
        -> MonsterRoomRuntime.ResolveCombat
           -> MonsterMeleePhysicsResolver
  -> CombatSimulation
     -> WeaponRuntimeService.TickCombat
        -> ProjectileSimulation.Tick
           -> TryInitialOverlap / TryCast
              -> Physics.*NonAlloc(WorldMask)                 [世界碰撞]
              -> MonsterRoomRuntime.QueryMonsterCandidates   [怪物宽相]
                 -> MonsterSpatialIndex.QuerySweptCircle
                    -> 首次查询时 EnsureBuilt
              -> ProjectileSimulation 精确线段-圆检测         [怪物窄相]
           -> ResolveContact
              -> MonsterRuntimeService.DamageMonster
                 -> MonsterRoomRuntime.DamageMonster
                    -> MonsterWorld.Damage
                    -> 死亡时 MonsterSpatialIndex.Remove
  -> Replication
     -> MonsterRuntimeService.PublishAuthorityState
     -> WeaponPresentationService.FlushServerFrame
```

`MonsterRoomRuntime` 是索引所有者。索引只依赖房间世界边界和稳定 Monster Slot，不依赖 GameObject、View、Collider 或导航 Cell 数据结构。房间边界仍取自导航房间的 `RoomSpatialData.HalfSize` 与 `roomRoot`，但四叉树粒度独立配置。

## 3. 四叉树布局

实现文件：`Assets/_HotUpdate/Scripts/GameProcess/Monster/Spatial/MonsterSpatialIndex.cs`。

当前参数：

| 参数 | 值 | 含义 |
|---|---:|---|
| `BaseDepth` | 4 | 固定建立 16 × 16 基础叶节点；70m 房间约 4.375m/格 |
| `SplitThreshold` | 16 | 单叶节点超过 16 只怪时继续四分 |
| `MaxDepth` | 6 | 最细为 64 × 64；70m 房间约 1.094m/格 |

根节点使用房间四角变换到世界空间后的 XZ 包围正方形。被击退到根范围外的怪物进入独立 overflow 链表，查询时始终加入候选，避免空间边界外漏判。

数据结构全部为数组：

- `_nodes`：连续四叉树节点；四个子节点连续存放；
- `_nextSlot` / `_previousSlot`：叶节点内双向 Slot 链表；
- `_leafBySlot`：Slot 到当前叶节点的反向索引；
- `_queryStack`：无递归查询栈；
- 查询结果直接写入 `ProjectileSimulation` 复用的候选数组。

基础树和最大深度容量一次预留。Monster Slot 数组只在房间规模超过现有容量时倍增，正常 Tick 查询不创建 `List`、LINQ、委托或临时集合。

## 4. 更新与一致性

索引采用“Tick 后标脏 + 首次弹丸查询重建 + Tick 内增量维护”：

1. `MonsterSimulation.Tick` 完成批量移动后，`MonsterRoomRuntime` 调用 `Invalidate`，不在没有弹丸查询的 Tick 支付重建成本。
2. 本 Tick 第一颗需要怪物检测的弹丸触发 `EnsureBuilt`，从当前 `MonsterWorld` 一次重建。
3. 同一 Tick 后续弹丸复用已构建索引。
4. 索引已构建时，出生调用 `Add`，死亡调用 `Remove`，击退调用 `Relocate`。
5. 索引尚未构建或已经标脏时，增量接口不做重复工作；下一次查询直接从最新 SoA 状态重建。

这保证击退类 Effect 在同一 Projectile Tick 内改变位置后，后续弹丸不会读取旧叶节点。

## 5. 弹丸查询与命中顺序

每次检测先用弹丸本段起点、终点和“弹丸半径 + 怪物受击半径”查询与扫掠胶囊包围范围相交的叶节点。四叉树只返回候选 Slot；`ProjectileSimulation` 再复用原有精确线段-圆公式选出最近命中。

命中顺序规则：

- `TryCast` 分别得到最近世界碰撞和最近怪物碰撞，距离更小者胜出；完全同距时世界碰撞优先；
- `TryInitialOverlap` 先处理 `WorldMask` 重叠；没有世界重叠时再查询怪物圆；
- 怪物候选仍过滤 Owner、死亡 Slot 和同一弹丸已经命中过的实体；
- `ContactKind` 明确区分 `World` 与 `Monster`，不再通过 Collider 是否为空推断命中类型。

怪物受击半径现在由 `MonsterRuntimeService._monsterHitRadius` 统一配置，当前值为 `0.73m`。后续若怪物体型差异成为实际需求，可把半径迁入 `MonsterRuntimeConfig` 的 SoA 配置；本阶段不预建多形状框架。

## 6. Gizmos 与 Profiler

菜单 `Tools/Navigation/可视化设置` 的 `怪物空间索引` 区域提供：

- `四叉树叶节点`：绘制实际叶节点，颜色同时表达深度和密度；
- `叶节点怪物数量`：显示非空叶节点的怪物数与深度；
- `受击半径`：绘制每只活跃怪物的 XZ 受击圆；
- `最近一次弹丸查询`：绘制最近一次扫掠起终点、查询半径、候选数和访问节点数。

索引按需构建，因此房间开始后尚未发生怪物查询时不会显示叶节点。Profiler 新增：

- `Monster.SpatialIndex.Build`
- `Monster.SpatialIndex.Query`
- `Weapon.Projectile.MonsterNarrowPhase`

这些 Marker 分别用于观察每 Tick 首次重建、每颗弹丸宽相查询与候选精确检测。任何性能结论必须通过 Player 同构场景重新采样，本次文档不预填未经测量的数据。

## 7. 本次修改清单

- 新增 `MonsterSpatialIndex` 和 Unity `.meta` 文件；
- `MonsterRoomRuntime` 接管空间索引生命周期、查询、死亡和击退一致性；
- `MonsterRuntimeService` 增加统一受击半径并通过房间包装 Damage/Push；
- `ProjectileSimulation` 改用四叉树候选，补齐无 Collider 初始重叠检测，简化 `ResolveContact`；
- `ProjectileSimulationConfig` 与 `WeaponRuntimeService` 移除不再使用的 `TargetMask`；
- `GameRoot.prefab` 移除 TargetMask 序列化项并记录怪物受击半径；
- `MonsterViewHandle` 移除受击 Collider 引用和池化启停逻辑；
- `Zombie_standard.prefab` 移除标准怪物 `DamageHitbox` GameObject 与 `BoxCollider`；
- 导航 Gizmo 设置和绘制器增加四叉树、密度、受击圆与最近查询可视化；
- 更新 Monster 架构与性能基线文档。

## 8. 后续方向的预留

本次只保留能直接服务已知后续需求的数据边界：

1. 怪物和弹丸已经由同一个 `NetworkSimulationClock` 按 `WorldSimulation -> PhysicsSync -> MeleeCombat -> CombatSimulation -> Replication` 固定顺序执行；完整阶段架构见 [Gameplay统一Tick阶段架构.md](Gameplay统一Tick阶段架构.md)。
2. 四叉树使用纯值类型数组和稳定 Slot，适合随 Projectile 数学内核一并迁入 AOT；迁移前先采样 Build/Query/NarrowPhase 占比。
3. Projectile 后续 SoA 化可直接复用“查询结果写入调用方数组”的接口。
4. 命中已不依赖怪物 Collider；后续消灭怪物 GameObject 时只需替换表现生命周期，不影响权威碰撞。
5. 空间索引不依赖导航拓扑重建，因此不会扩大后续导航重建优化的修改面。
6. 节点带 Morton 路径和深度，可作为网络内/中/外层或群组划分的输入；节点数组下标会随重建变化，不能作为网络稳定 ID。

范围伤害和雷电链已经通过 `MonsterRuntimeService.QueryMonsterCircleCandidates` 进入房间空间索引入口；索引返回的是宽相候选，Weapon 仍保留距离精确判断和已访问过滤。后续可将最近邻、扇形和导航群组查询继续收敛到同一服务，避免业务层重新遍历全部 Slot。

## 9. Editor 可视化修复

2026-10-25 补充两项调试修复：

- `导航可视化` 设置窗口改为纵向滚动并完成中文化，低分辨率或高 UI 缩放下也能访问 `怪物空间索引` 的全部开关；
- VAT 动画状态仍在 `LateUpdate` 中每帧推进，`Graphics.DrawMeshInstanced` 提交改由 `RenderPipelineManager.beginContextRendering` 驱动。Editor Pause 停止动画和 Gameplay Tick 后，Scene/Game 重绘仍会提交冻结姿态，不再让全部怪物消失。

正常 URP Player 的单渲染上下文中，怪物状态遍历、属性数组上传和批量 Draw 次数与修改前一致，只增加一次渲染回调派发。Editor 的额外 Scene/Game 重绘会产生对应的额外提交，这是调试可见性所需成本；后续性能复测仍应以独立 Player 构建为准。

## 10. 2026-10-27 房间空间查询与阶段对齐修正

### 10.1 Unity Physics 与怪物命中职责分离

怪物命中不再依赖 `MonsterViewHandle` 的 Transform 或 Collider。`MonsterViewHandle` 现在只提供对象池实例和 VAT 资源配置；Host/Client 的姿态提交直接进入 `MonsterVATRenderWorld`。

`Physics.SyncTransforms()` 被提升到 `GameplayTickPhase.PhysicsSync`。它当前仍由 Weapon 服务注册，因为该服务是现有 WorldMask Physics 查询的拥有者，但不再隐藏在弹丸 `TickCombat` 内。动态墙在 `PreSimulation` 移动后，会在所有 Actor/World 更新完成时统一提交 Physics 场景。

怪物近战从 `WorldSimulation` 改成两段式：World 阶段只记录 `MonsterMeleeAttackRequest`，`MeleeCombat` 阶段在 PhysicsSync 之后调用 `MonsterMeleePhysicsResolver`。这样近战的 Physics 读取与弹丸的 WorldMask 读取使用同一个明确的同步边界。

### 10.2 房间空间查询服务的第一步

`MonsterRoomRuntime` 继续作为索引所有者，并新增圆形候选查询。范围伤害、风暴云和连锁闪电不再在 Weapon 内直接遍历 `MonsterSlotCount`，而是：

```text
Weapon effect
  -> MonsterRuntimeService.QueryMonsterCircleCandidates
     -> MonsterRoomRuntime
        -> MonsterSpatialIndex.QuerySweptCircle(start=end)
  -> Weapon 自己做距离、死亡和已访问精确过滤
```

这里没有改变命中语义：四叉树只负责宽相，Weapon 仍负责精确距离、最近目标和连锁访问集合。候选数组在 Weapon 服务中复用，首次超过房间 Slot 容量时才扩容。

这一步的意义是收拢所有空间查询的入口，而不是宣称四叉树已经成为最终统一空间结构。当前四叉树仍在怪物移动后按需重建；后续可以在不改 Weapon 语义的前提下，把底层替换成固定网格或统一快照。

按 `20261026-162947-530d99d1` 的 Host 数据，5012 档每次 `Build` 约 8.56--8.75 ms，而同一档的 `Query` 约 0.010--0.014 ms；三次重复的 Build 频率为 15.65--24.23 Hz，低于 Query 的 50.95--115.47 Hz，按逻辑 Tick 摊销约 6.34--6.91 ms/Tick。这说明子弹频繁查询不会按“每颗子弹”重复重建：一次空间索引失效后，当前 Tick 的第一次查询触发一次 Build，后续同 Tick 查询复用结果。Build 仍是明显的 CPU 成本，多个逻辑 Tick 在同一渲染帧补发时会把多个 Build 叠在同一帧，因此它需要和 EntityTick 一起优化，但不能单独解释 5012 Host 的全部帧率坍塌。

### 10.3 后续统一 Grid/QuadTree 的设计边界

关于 AOT 对照、统一 Grid 快照的运行时序和分阶段迁移路径，见 [怪物空间查询AOT与统一Grid快照设计.md](怪物空间查询AOT与统一Grid快照设计.md)。

当前所有导航 Grid、Density、Separation、四叉树和 Flow Field 模块的横向盘点见 [怪物空间网格模块横向盘点与统一快照映射.md](怪物空间网格模块横向盘点与统一快照映射.md)。

当前 Separation、Density、Projectile 和范围 Effect 曾各自维护或遍历自己的空间数据。后续应由房间级空间服务拥有一次“本 Tick 怪物空间快照”，至少提供：

- `QuerySweptCircle`：弹丸宽相；
- `QueryCircle`：范围伤害和风暴云；
- `QueryNearest`：连锁闪电；
- Cell/Bucket 计数：Density；
- 邻接 Bucket：Separation。

实现上优先评估固定网格/空间哈希，而不是把现有四叉树直接扩展成更多业务特例。房间尺寸固定、怪物持续移动、查询类型以圆形和邻近为主，这些条件更适合一次线性建桶后复用。若保留四叉树，也应把它封装在同一 `MonsterRoomRuntime` 查询接口之后，业务层不再知道底层是 QuadTree 还是 Grid。

快照的提交点需要放在本 Tick 的位置、死亡和击退变更都完成之后；如果为了降低 Build 延迟而在更早阶段建桶，就必须同时维护死亡、迁移和新增 Slot 的增量覆盖层。这样才能保证 Density、Separation、范围效果和弹丸在同一 Tick 读取同一份空间事实。

本阶段未移除弹丸自身的 Collider/GameObject；那是表现生命周期的后续独立改造，不应与本次怪物命中和空间查询语义混在一起。
