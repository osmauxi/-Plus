# Monster 当前结构

Monster 是服务器批量模拟模块，不再为当前仅有的一种小怪维护 Target/Move/Attack 接口与注册表。

主链路如下：

1. `MonsterRoomRuntime` 生成 Slot，并把配置中的生命、护盾直接写入 `MonsterWorld`。
2. `MonsterSimulation` 在一个循环中完成最近玩家索敌、近战前后摇、追踪和运动积分。
3. `MonsterMeleePhysicsResolver` 只负责服务器物理查询，命中玩家后直接调用 `PlayerHealthNetworkState.Damage`。
4. 玩家弹丸通过 `MonsterViewHandle.Slot` 调用 `MonsterWorld.Damage`。
5. 怪物死亡只把 Slot 设为非 Active；现有表现帧的 ActiveMask 负责通知客户端回收 View。

`MonsterHealthData`、减速和控制状态均只属于服务器 `MonsterWorld`。不要为它们新增 NetworkVariable、Health 服务、Collider 映射或跨模块命令队列。只有未来真的出现第二套怪物行为时，再根据重复代码提取分支，不预先恢复模块注册框架。
