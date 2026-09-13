# Weapon 模块

当前 Weapon 由固定 Tick 武器/弹丸模拟、玩家级 Effect 数值快照、Effect Roll 和特殊 Effect System 组成。完整枪械新增流程见仓库根目录的 `枪械新增指南.md`。

## Effect 配置

Effect 使用 `Assets/DesignData/Excels/EffectConfig.xlsx`，与其他 Excel 一起通过 `Tools/Config/导出并注册全部配置` 导出，并由 `ConfigManager` 一次性加载。

- `Effect`：EffectID、ModifierIDs、可重复/等级上限、普通或特殊类型、卡池、所属/冲突流派和权重。
- `Modifier`：一行只描述一个属性操作。`Add` 直接加基础值；`Multiply` 使用精确倍率，`1.2` 表示乘 1.2，`0.8` 表示乘 0.8。
- `EffectRoll`：名称、当前描述、升级描述和 Addressables 图标地址，只供客户端展示。

启用的 Effect 必须有启用的 EffectRoll 行；所有 Modifier 外键、枚举、重复 ID、等级和权重会在导出及运行时目录构建时校验。不要增加 Effect 专用导出菜单。

## 数值管线

`PlayerEffectLoadout.AcquisitionOrder` 是唯一顺序真相。计算时先按 Effect 实际获得顺序，再按每个 Effect 的 `ModifierIDs` 顺序执行，不把 Add/Multiply 重新排序。武器字段与 ShieldCapacity 已接入；玩家生命、移动速度和护甲不在本轮范围。

服务端每次 Effect 变化都会创建不可覆盖的 `EffectSet` 和 `WeaponStatSnapshot`，后续 Shot/Projectile 持有对应快照 ID。客户端只用相同规则做显示和预测，不注册权威快照，也不执行特殊玩法。

## Roll 与网络边界

普通/异变池共用 `EffectRollService`。普通模式检查冲突并叠加同流派权重；Chaos 关闭这两项，但仍检查不可重复和 MaxLevel。候选不足时可启用 Chaos 保底，Offer 会标记实际采用的 Chaos 状态。

玩法端只在服务端调用 `PlayerWeaponController.CreateEffectRollOfferOnServer`。需要让所有玩家同时进入 Roll 时，可在任意已 Spawn 的服务端玩家控制器上调用 `RequestEffectRollBroadcast(WeaponEffectRollPool)`；该入口遍历当前玩家，为每名玩家建立独立权威 Offer，并沿用 owner-targeted RPC 分发。组件保存一个当前 Offer，并只向所属客户端发送 OfferID、EffectID、当前等级和卡池信息；客户端使用本机 `EffectRoll` 表补齐名称、描述和图标地址。UI 通过 `IEffectRollOfferSource` 读取候选并调用 `RequestEffectSelection`，服务端再次校验 OfferID、候选成员和最新等级后，才把 Effect 写入 `NetworkList`。任意直接发奖或清空 Effect 都会使旧 Offer 失效。

`GameplayUIId.EffectRoll` 已有不可返回关闭的 Screen 策略；收到/完成 Offer 时会发送 Show/Hide 请求。MVP 页面位于 `UIGameUIScene/ScreenLayer/EffectRollPanel`，使用三个 `EffectRollCard` Prefab。Host/Server 可用 N、M 分别广播普通池、异变池 Roll；客户端按键不会越过服务端入口。卡片数据和 DOTween 动效由 GameplayUI 的 Presenter/View 分层负责，不进入 Weapon 规则层。

## 特殊 Effect

`WeaponSpecialEffectManager` 按 EffectID 注册纯 C# `WeaponSpecialEffectSystem`。System 接收当前等级，可在装备、开火前后、弹丸生成、命中、反弹和销毁时触发；它只修改传入的弹丸数据或提交命令，不直接访问网络、对象池、Health 或场景单例。

新增特殊 Effect 时：

1. 在 Effect 表把 EffectType 配为 Special；如为混合效果，也可以同时填写 ModifierIDs。
2. 新建一个继承 `WeaponSpecialEffectSystem` 的纯 C# 类，并把常量 ID 与配置对齐。
3. 在 `WeaponRuntimeService.InitializeAsync` 注册。启动校验会拒绝任何缺少脚本的启用 Special Effect。
4. 需要外部玩法时实现/组合 `IWeaponSpecialEffectCommandSink`。当前命令覆盖连锁闪电、跟随风暴云、护盾回复/容量、处决、控制、范围冲击和持续伤害区域；`HealthShieldEffectAdapter` 负责护盾容量聚合与回复，其余目标选择、状态和 VFX 由对应战斗模块实现。

当前已迁移的特殊 System 为 `Overload`、`StormCloud`、`StaticShield`、`PhotonMomentum`、`EnergySiphon`、`Executioner`、`KineticBoost`、`MultiSplit`、`NuclearFission` 和 `Shockwave`。`MultiSplit` 的子弹由 `WeaponRuntimeService` 内部生成，沿用父弹 Shot，并通过普通 `ProjectileSpawned` 事件进入既有全端表现同步；跟随玩家的常驻 Effect 会在效果集变化、玩家解绑和服务关闭时成对移除。

旧 VFX 已按原 Addressable ID 注册到 `Prefabs` 组，并由 `PoolConfig.xlsx/LocalVFXPool` 配置容量：`OnLightning`、`OnHit_Lightning`、`Shield_Lightning`、`RainCloud`、`Explo_Fire`、`OnRadiation`。

## 其他边界

- `WeaponSystem` 只更新传入状态；预测端和服务端共用规则。
- `ShotBuilder` 创建弹丸，`ProjectileSimulation` 处理移动、碰撞与生命周期。同一弹丸不会因目标的复合 Collider 对同一实体重复命中。
- `WeaponRuntimeService` 从 `ConfigManager` 构建局内冻结目录，并只在服务端推进权威弹丸。
- `ProjectileHitTargetAdapter` 对接伤害目标；远端 VFX 广播仍未实现。
