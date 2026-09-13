# Monster 第一阶段逻辑实现计划

## 目标与边界

本轮以房间为生命周期，把服务器 Monster 逻辑迁移到 Slot + Component SoA + 批处理 System。先建立可测试的数据与协议核心，再替换旧 `BattleManager/MonsterEntity/MonsterBrain` 链路；旧链路在新组合根完成前保持可运行，不做半接线式删除。

第一轮不引入 DOTS、Job、Burst、Archetype。房间生成采用局部采样区域随机取点，并从上向下检查最近 Collider 是否属于 GroundMask；不依赖 GameObject Static 标记。Monster 高频状态不进入普通 NetEventBus；NetEventBus 只保留低频 Spawn/DeathReaction 等事件候选。

## 仓库现状

- 新 Monster 数据、模拟、生命、生成、协议和服务组合根已落在 `GameProcess/Monster`；生产 `GameRoot` 已注册 `MonsterRuntimeService` 与 `MonsterBattleManager`，后者位于 `GameLevelFlowController` 之前。
- 旧怪物仍位于 `GameProcess/OLD`，生成依赖 `SyncObjectPool + NetworkObject`，AI/移动/攻击由逐对象组件执行。新服务尚未调用 `BeginRoom`，因此不会与旧 `BattleManager` 双重刷怪。
- `Game_Config.xlsx` 已增加 Health、MonsterAttack、MonsterRuntime、MonsterSpawn、MonsterView 五张正式表；生成代码、Addressables 二进制和不可变 Catalog 装配器已贯通。

## 实施阶段

### A. 逻辑内核（已完成）

- `MonsterWorld`：Meta/Motion/Target/Move/Attack/Presentation 六组 SoA；Slot 仅追加，死亡不复用，房间 Clear 后从 0 开始。
- `MonsterSimulation`：固定执行 `Target → Attack → Move → Motor`。
- `MonsterRoomRuntime`：组合 BeginRoom、整批 Spawn、Tick、Health Step、CaptureFrame 与 EndRoom，固定旧 Health Binding 先于 Slot 复用清理。
- 基础模块：NearestTarget、DirectChase、MeleeAttack 状态机。
- 玩家目标：每 Tick 最多捕获一次四名玩家的 ClientId 与二维位置。
- Attack 状态机只输出 `MonsterAttackRequest`，不直接访问 Physics。

### B. Health 与 Physics 边界（运行时接线完成，View 调用待接）

- `MonsterHealthBinding` 维护 `Slot ↔ HealthEntity`，监听 Health 死亡并关闭 Slot。
- `MonsterMeleePhysicsResolver` 使用 `SphereCastNonAlloc` 和集中 `Collider → HealthEntity` 映射提交伤害，不调用 `GetComponent`。
- `ProjectileSimulation` 已抽出 `IProjectileTargetResolver`；Monster 使用 `MonsterProjectileHealthResolver`，旧对象使用兼容 Resolver，二者可通过 Composite Resolver 并存迁移。
- `PlayerHealthRuntimeService` 已集中维护玩家子 Collider 映射；`MonsterRuntimeService` 在 Weapon 初始化前注入新旧兼容的 Projectile Resolver。
- `HealthImpact` 增加 `DamageOriginType`；Projectile 致死时输出命中点和反向喷射方向，其他来源不请求该血液死亡表现。
- 待接：Monster View 创建/回收时实际调用 `BindViewColliders/UnbindViewColliders`，以及 DeathReaction 的客户端 VFX 适配。

### C. 生成与配置（已完成）

- 三层 Catalog：Runtime、Spawn、View；AttackProfile 独立目录。
- 正式配置增加 Health、MonsterAttack、MonsterRuntime、MonsterSpawn、MonsterView 五张表；现有四种旧 Monster 数据已迁入。
- `MonsterConfigAssembler` 对配置排序后建立稳定 `ushort` 索引，把秒数按上取整规则转换为 Tick，并校验主键、外键、模块名和 Spawn/View 覆盖完整性。
- Excel 生成器已注册五张表，生成 `Config_*.cs`、MessagePack `.bytes` 和 Addressables 条目；二进制已在 Unity 内完成真实反序列化与 Catalog 装配验证。
- `MonsterSpawnSelector` 支持预算、权重、难度范围和单波上限。
- `MonsterRandomGroundSpawnPlanner` 为当前波次一次性随机采样全部种类/位置；完整成功后才允许创建 SpawnBatch，失败不推进待生成游标。
- `RoomView` 已提供可旋转、缩放的局部 SpawnRegion、ProbeMask 与 GroundMask；共享 `RoomGroundPointSampler` 只接受向下 Raycast 的最近 Ground 命中。
- 起始房间玩家出生已改为在房间中心区域批量随机采样完整三维 Pose，不再依赖固定 PlayerSpawnPoint。
- `MonsterSpawnPlan/MonsterSpawnBatch` 使用连续 StartSlot + 数组顺序隐式标识 Slot。
- RuntimeConfig 额外包含 `AttackDamage`：这是向 HealthCommand 提交实际伤害所必需、原设计字段清单中缺失的权威参数。

### D. 集中表现状态（协议与运行时完成，Prefab 适配待接）

- `MonsterPoseQuantizer`：房间局部 X/Z 各 12 bit、Yaw 8 bit，共 32 bit。
- `MonsterPresentationFrame`：ServerTick + SlotCount + ActiveMask + 存活 Slot 的 5B State。
- `MonsterNetworkCodec`：显式小端字节布局，Spawn 每项 6B、Presentation 每个 Active Slot 5B，并严格拒绝截断或长度不一致的数据。
- `MonsterNetworkTransport`：RoomBegin/SpawnBatch/RoomEnd 使用可靠消息，PresentationFrame 使用不可靠消息；RoomBegin 同步量化范围、统一 GroundY 与最低接受 Tick。
- Host 会把 Server 产生的同一 SpawnBatch/PresentationFrame 回环到本地 Replica，Host 与远端客户端复用同一表现输入。
- `MonsterReplica`：要求可靠 SpawnBatch 连续先到；拒绝旧房 Tick、旧帧、缺 SpawnBatch 和 SlotCount 不匹配帧；统一保存 A/B 插值历史。
- `MonsterViewWorld`：维护 Slot → View 映射，原子创建 SpawnBatch、插值姿态、AttackSequence 单次触发、死亡回收与整房清理。
- 待接：四个旧 Monster Prefab 的本地池配置、`IMonsterViewPool/IMonsterViewHandle` Unity 适配、集中 Animator 驱动；超过单消息上限后的可选分包策略后置。

### E. 房间统筹与旧链替换（Manager 已完成，触发接线待推进）

1. `MonsterBattleManager` 已沿用旧 Manager 的单房间、预算采购、同屏上限、半数补波与清空结算思想；每个波次独立选择种类并一次性规划全部位置，只执行一次原子 `Spawn(plans)` 和一次可靠广播。
2. 为新版房间检测留下 `IRoomBattleController.TryStartRoomBattle` 接口；下一步由房间入口负责锁门、构造 Request，并监听 Cleared/Failed 完成开门与奖励。
3. 为四个 Monster Prefab 制作纯本地 View 版本，注册 LocalObjectPool，并接入 `MonsterViewWorld` 与 Collider Binding。
4. 补齐迟加入 Spawn 快照和断线恢复；再接 DeathReaction/VFX。
5. 完成 Host/Client 功能测试及 100/300/500 Monster 压力测试后，删除旧 `BattleManager` 与旧怪物组件。

## 生命周期顺序

进入房间时：清理旧 Binding/Replica/View/FrameBuffer，`MonsterWorld.Clear()`，记录新房最小接受 ServerTick，再允许第一批 Spawn。

离开房间时建议顺序：停止 Tick/网络接收 → 清理 View → 清理 Replica/FrameBuffer → `MonsterHealthBinding.Clear()` → `MonsterWorld.Clear()`。这样不会让 Health 的延迟回调命中已经复用的新 Slot。

## 验证

- Unity 2022.3.44f1c1 脚本编译：0 error。
- Unity EditMode 完整测试集：876 total，871 passed，0 failed，5 skipped。
- 新增测试覆盖：Slot 稳定性/清房覆盖、System 顺序、Windup/Recovery 时序、Health 死亡链、Projectile DeathReaction、预算选择、量化误差、ActiveMask、Replica 插值及迟到帧拒绝、正式配表装配校验、ViewWorld 出生/插值/攻击/死亡/失败回滚。
- 生产 `GameRoot` 已验证 Monster Runtime/Manager 唯一、引用有效、服务顺序正确；三个 Grid Room Prefab 均已配置 SpawnRegion/ProbeMask/GroundMask；正式 `.bytes` 五表均成功反序列化并装配为运行时 Catalog。
