# Health 使用指南

Health 不再是跨玩家、怪物和武器的通用框架。当前项目只有两条直接链路。

## 玩家

`PlayerHealthNetworkState` 挂在 `PlayerRuntimeRoot`：

- 服务器直接调用 `Damage`、`Heal`、`AddShield`、`Revive`。
- 组件只保存当前生命、当前护盾和两项上限。
- 一个 `NetworkVariable<PlayerHealthState>` 负责初始状态、晚加入和后续变化。
- 死亡时直接调用 `PlayerSyncController.SetLifeState(Dead)`；受击时直接调用 `ApplyHit()`。
- HUD 直接读取玩家组件，不经过全局 Health 服务。

## 怪物

怪物生命是 `MonsterWorld` 内的服务器数据：

- `MonsterHealthData` 与 Slot 一一对应，不挂 MonoBehaviour，也不走网络同步。
- `MonsterWorld.Damage(slot, amount)` 先扣护盾，再扣生命；归零后关闭 Slot。
- 客户端只接收现有表现帧中的 Active 状态，因此只知道怪物是否死亡。
- 若增加半血减速等规则，直接在服务器 `MonsterWorld`/`MonsterSimulation` 中读取生命并调整速度，不新增生命同步字段。

## 武器命中

`ProjectileSimulation` 只在服务器推进。命中后直接从 Collider 父节点识别：

- `MonsterViewHandle`：用其 Slot 调用 `MonsterRuntimeService.DamageMonster`。
- `PlayerHealthNetworkState`：直接调用 `Damage`。

不再使用 `HealthRuntime`、`HealthEntity`、命令队列、复制层、Collider Binding、Projectile Target Resolver 或 HitTarget Adapter。新增普通敌人和玩家伤害时沿用上述两条直接路径即可。
