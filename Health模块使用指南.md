# Health 模块使用指南

## 1. 当前交付范围

玩家、怪物和可破坏物使用同一个集中式 Health 模块。每个实体只有一条 Health 数据记录，整个会话由一个 `HealthRuntime` 管理，生成器不需要给实体挂载 Health MonoBehaviour 或 NetworkBehaviour。

本轮完成模块及脱离场景的 EditMode 测试，没有修改 GameRoot、玩家/怪物预制体、Weapon 命中流程、场景流转或旧版 Health。模块不会自动运行：装配层需要显式创建实例、注册实体、提交命令并调用 `Step`。

源码目录：`Assets/_HotUpdate/Scripts/GameProcess/Combat/Health`；命名空间：`ProjectGame.HotFix.Gameplay.Combat.Health`。沿用 `HotFix.Gameplay` 程序集，没有新增需要登记到热更新启动链路的 DLL。模块不引用 ConfigManager、NGO、玩家控制器、怪物 AI、UI、对象池、Animator 或 Rigidbody；仅命中上下文使用 Unity 的 `Vector3` 值类型。

| 文件 | 职责 |
| --- | --- |
| HealthModels.cs | 实体句柄、不可变参数目录、只读状态、UI 查询接口和通知辅助逻辑 |
| HealthCommands.cs | 伤害/治疗/护盾/复活等命令、结算结果、受击反应上下文 |
| HealthWorld.cs | 集中保存状态、实体代次、事件去重及伤害频率门控记录；内部类型 |
| HealthSystem.cs | 防御、护盾、扣血、治疗、死亡与复活规则；内部类型 |
| HealthRuntime.cs | 权威命令队列、显式 Tick、注册/注销、通知和同步数据捕获 |
| HealthReplication.cs | 全量/增量数据帧、版本检查和客户端只读镜像；不包含网络传输 |
| HealthEntityBindings.cs | 集中的命中对象→实体映射，可直接使用 `HealthEntityBindings<Collider>` |

这是当前项目的类 ECS 组织方式：Entity 标识实体，Snapshot 保存状态，World 集中存储，System 统一结算。没有引入 Unity Entities/DOTS、Job 或 Burst；当前存储采用字典，也没有宣称实现跨平台浮点锁步。

## 2. 独立使用示例

以下只是模块调用示例，参数值不是已确定的玩家/怪物正式配置；不要将示例复制进每个怪物的 Update。

```csharp
using ProjectGame.HotFix.Gameplay.Combat.Health;

var catalog = new HealthCatalog(new[]
{
    new HealthDefinition(profileId: 0, maxHealth: 100, maxShield: 50, defense: 20),
    new HealthDefinition(profileId: 1, maxHealth: 200, defense: 10),
});
using var health = new HealthRuntime(catalog);

// entityId 后续可使用 NetworkObjectId，不是 OwnerClientId。
// 玩家和怪物共享 Register 和后续全部命令，仅 profileId 不同。
HealthEntity player = health.Register(entityId: 0, profileId: 0);
HealthEntity monster = health.Register(entityId: 100, profileId: 1);

health.CommandResolved += result =>
{
    // 伤害数字/统计读取实际扣除量，拒绝原因读取 result.Code。
    float actualDamage = result.HealthLost + result.ShieldLost;
};

bool queued = health.TryEnqueue(HealthCommand.Damage(
    eventId: 1, tick: 1, source: player, target: monster, amount: 30));
if (!queued)
    throw new System.InvalidOperationException("Health 命令队列已满，需要处理背压。");

health.Step(1); // 由唯一的战斗协调器每 Tick 调用一次。
health.TryGetHealth(monster, out HealthSnapshot state);

health.Unregister(monster); // 对象池回收/网络 Despawn 时调用。
```

服务端应持有 `HealthRuntime`；UI 只拿 `IHealthStateSource`；伤害生产者只拿 `IHealthCommandSink`；反应/统计系统只订阅 `IHealthEventSource`。网络客户端持有 `HealthReplica`，它没有伤害或治疗入口。

`HealthRuntime` 本身不知道当前进程是否为服务器。未来的 GameRuntime 服务/网络适配器必须负责权威校验，不能将客户端上传的伤害数值、治疗量、护盾上限或绕过标记直接转交命令入口。

## 3. 数据和结算规则

`HealthDefinition` 保存 `ProfileId / MaxHealth / MaxShield / Defense / DamageGateTicks`。构造时校验数值，目录禁止重复 ID；不存在的 ID 直接报错。注册默认满血、零护盾、无临时无敌，也可显式指定不超过上限的初始护盾。

`HealthSnapshot` 是只读值副本，定义也不可变。HP 为 0 就是死亡，不另存一份可写的 `isDead`。玩家和怪物的参数差异由定义提供，模块内没有类型、Tag 或角色分支。

| 命令 | 规则 |
| --- | --- |
| Damage / Physical | `结算伤害 = Amount × 100 / (100 + Defense)`，先消耗护盾，再扣 HP |
| Damage / True | 绕过防御，仍先消耗护盾；绕过护盾需要独立的 `BypassShield` 标记 |
| Heal | 只治疗存活实体，上限为 MaxHealth；不会复活 |
| AddShield | 只作用于存活实体，上限为 MaxShield；上限来自可信定义 |
| Revive | 仅死亡实体可用，恢复指定 HP 并裁剪上限，清空护盾、临时无敌和受击门控 |
| SetInvulnerable | 显式切换临时无敌；不阻止治疗或参数调整 |
| Reconfigure | 替换参数快照，将现有 HP/护盾裁剪到新上限，清空旧门控；提高上限不补血，也不复活 |

伤害、治疗和加盾的 Amount 必须是有限正数；NaN、无穷、负数和零会被拒绝。没有旧 Health 的“至少扣 1 HP”规则。中间计算使用 double 防止有限极大值溢出，最终仍保存 float；小到无法改变 float 状态的伤害返回 `NoChange`。

`MitigatedDamage` 是防御处理后的伤害。`HealthLost`、`ShieldLost` 是实际扣除量，不包含过量伤害。`Died` 只在存活→死亡时发出一次；对尸体的后续伤害不会重复触发死亡。显式复活后可以再次发生死亡。

`Critical` 是上游已经确定的暴击标记，模块不再乘一次暴击倍率。来源实体可以已经注销，例如射击者死亡后仍在飞行的弹丸；目标必须是当前有效代次。

## 4. Tick、去重和对象池

1. 命令按权威端入队顺序结算。`Step` 不按客户端时间重排攻击，也不进行回滚。未来网络命中验证必须在提交前完成。
2. `Step(uint tick)` 必须前进，相邻调用跨度小于 `2^31`，支持 uint 回绕。重复 Tick 或倒退会报错。
3. 命令 Tick 是权威生成 Tick。默认接受最多 120 Tick 的历史请求；未来请求返回 `FutureTick`，不会自动保留到将来。`AppliedTick` 记录实际结算 Tick，频率门控也使用该 Tick。
4. `EventId` 必须非零，并在 `(Source, Target)` 范围内跨命令种类唯一；推荐由服务器统一分配单调递增事件 ID。重试必须保留原 ID、原 Tick 和原内容。不要给重试换新 Tick，也不要直接让一发霰弹的所有弹丸共用一个伤害事件 ID。
5. 无敌、门控、死亡等业务拒绝也会消费事件，防止同次攻击在状态改变后重试成功。过期记录会清理，原请求同时因过期被拒绝；这不是允许调用端复用近期 ID 的理由。
6. 目标通过 `HealthEntity(EntityId, Generation)` 标识。EntityId 可以为 0，Generation=0 才表示无实体。每次 Register 分配新代次；禁止只持有 EntityId 的旧请求在命中时重新查找代次后扣血。
7. 同一会话禁止重复注册活跃 EntityId；必须先 Unregister。回收时已排队的旧命令保留并返回 `UnknownTarget`，不会伤害重生实体。复活保留原代次，去重记录也保留；重新注册则创建新生命周期。
8. 句柄仅在对应 HealthRuntime 会话中有效。新关卡/会话使用新 Runtime 和新 SessionId，装配层必须隔离跨会话请求。

旧 Health 的全局 0.2 秒无敌窗口会吞掉高射速步枪伤害，因此这里的频率门控需要显式启用：`GateChannel=0` 完全关闭门控；非零通道按 `(Source, Target, GateChannel)` 独立计时，持续时间来自目标定义。普通步枪和独立霰弹丸使用 0；持续接触伤害可设置非零通道。`BypassInvulnerability` 与 `BypassDamageGate` 是两个独立标记。

默认队列容量 4096，可在创建时调整。`TryEnqueue=false` 表示没有接收该命令，调用者必须处理；返回 true 只表示入队，不等于命中或结算成功。模块是单线程模型，网络线程产生的输入应先切换到战斗线程。

## 5. 通知、UI、VFX、顿帧和击退

整个当前批次结算完成后，模块才按结算顺序发出通知。因此 `HealthResult.Before/After` 表示该次命令的变化，而回调中重新查询 World 得到的是整批结束后的最终状态。UI 可以读取结果快照逐次展示，也可以只渲染最终状态。

| 接口/事件 | 消费方与约定 |
| --- | --- |
| IHealthStateSource.TryGetEntity / TryGetHealth | UI、AI 查询当前状态。先订阅 StateChanged，再读取初值；解绑时取消订阅 |
| StateChanged | 注册、状态变化、移除。HP、护盾、上限和无敌状态都由只读快照提供 |
| CommandResolved | 每条入队请求的结果，包括拒绝原因；适合统计和调试 |
| Died / Revived | 权威生命周期事件；死亡奖励、掉落、状态投影由外层统一订阅处理 |
| ReactionRequested | 有实际扣血/扣盾的 Damage 才发出；SuppressReaction 可关闭 |
| HealthImpact | 命中点、方向、独立力度、ReactionProfileId、DamageOriginType；作为后续反应和按伤害来源区分死亡表现的输入 |

回调可通过 TryEnqueue 追加命令，这些命令留到下一次 Step。回调期间禁止注册/注销、递归 Step、捕获同步帧或 Dispose，避免半批次状态泄漏；死亡处理器应把回收操作排到战斗阶段边界。

单个订阅者抛异常不会跳过其他订阅者，也不会阻止其余命令结算。模块在全部通知后抛出 AggregateException，状态与 Tick 已经提交，调用者应记录/处理异常，不能重放当前 Tick。Register/Unregister 的通知也遵守“先提交后通知”；注册通知失败时可用 TryGetEntity 取回已注册句柄。

后续反应系统建议分别消费以下数据：

- 顿帧：由客户端表现层冻结对应动画或局部表现，不直接改全局 Time.timeScale。
- 硬直：由服务器确定控制 Tick，交给角色控制状态处理。
- 击退：把 Impact 转为运动命令，交给玩家/怪物 Motor；玩家的运动状态需要进入现有预测与纠正流程。
- VFX、音效、飘字、仇恨：读取明确的结算结果；Health 不直接查找或操作这些组件。

本轮只完成上下文及事件接口，没有实现反应数值曲线、霸体/韧性、顿帧执行器、击退执行器或实际 UI/VFX。

## 6. 集中命中对象映射

未来生成器可创建一次 `HealthEntityBindings<Collider>`，在玩家/怪物生成时集中绑定整组碰撞体：

```csharp
using var bindings = new HealthEntityBindings<UnityEngine.Collider>(health);
bindings.Bind(monster, monsterRoot.GetComponentsInChildren<UnityEngine.Collider>(true));

if (bindings.TryResolve(hitCollider, out HealthEntity target))
{
    // 向 IHealthCommandSink 提交命令；不在 Collider 上寻找 Health 组件。
}
```

Bind 会原子替换该实体的全部映射，重复碰撞体会合并，与其他实体冲突时拒绝且保留原映射。Unregister 会自动删除该实体的映射；动态更换模型、碰撞体或启用弱点时需要重新 Bind，模块不扫描场景。销毁/回收单个碰撞体也需要更新映射，不能只依赖 Unity 对象销毁状态。

TryResolve 只负责身份，不保证实体存活；尸体是否阻挡子弹由 Weapon 决定，扣血端会拒绝尸体伤害。整套绑定容器应先于 HealthRuntime/HealthReplica Dispose，避免场景对象引用残留。

当前映射不包含头部倍率、弱点和阵营规则；这些由命中/伤害生产层决定，后续可扩展命中元数据，仍无需逐实体增加 Health 行为脚本。

## 7. 同步模块的使用与限制

服务器通过 `CaptureFullSnapshot()` 获取无副作用的全量数据，通过 `CaptureDelta()` 获取并消费当前的全局增量批次。增量仅保留每个 EntityId 的最新状态或移除记录。

一个网络发送器每次调用一次 CaptureDelta，然后将同一个 HealthFrame 广播给所有目标客户端。不能对每个客户端分别调用，否则后面的客户端拿不到同一批变化。客户端使用权威 SessionId 创建 HealthReplica，先 Apply 全量，之后 Apply 增量。

```csharp
using var replica = new HealthReplica(health.SessionId);
replica.Apply(health.CaptureFullSnapshot());
// 网络适配器可在这里序列化/传输帧；本轮只完成帧模型和镜像合并。
HealthFrame delta = health.CaptureDelta();
HealthFrameResult result = replica.Apply(delta);
```

版本检查同时覆盖会话、全局版本和实体代次。旧帧返回 Stale，跨会话帧返回 WrongSession；首次收到增量返回 NeedsFullSnapshot，缺少增量基线返回 MissingBaseline。后两种情况需要请求全量恢复。晚加入全量可以位于已有增量区间中间，镜像只应用其版本之后的变化。

全量会删除本地已不存在的实体。实例复用时，增量会移除旧代次再注册新代次；同一发送周期内多次注册/删除造成的压缩移除记录也会清理客户端旧实例。

HealthFrame 和内部快照对外只读，不会随服务器后续结算而变化。镜像在整帧提交后通知 UI，不会发出权威 Died/Revived/ReactionRequested 事件，避免晚加入和补全量触发奖励或受击特效。

同步帧是状态通道，同一发送周期内“死亡再复活”可合并为最终存活状态。若客户端必须表现中间过程，后续需要单独传输带事件 ID 的战斗表现事件流；本轮未实现 NGO 消息、字节序列化、分包、可靠传输和表现事件广播。接入时先使用可靠有序传输，分包全部到齐后再构造并应用一帧，缺帧时使用现有全量恢复接口。

## 8. ConfigManager 与 GamePlay 的后续接入顺序

1. 在 Excel 注册正式 HealthProfile 表，确定玩家/怪物的 ProfileId 引用位置。建议字段为 `HealthProfileID / MaxHealth / MaxShield / Defense / DamageGateSeconds`；装配时按实际战斗 TickRate 向上取整为 DamageGateTicks。表名及字段尚未生成，本轮没有修改 Excel 或 Addressables。
2. 在配置导出工具的显式 ConfigSources 中登记工作簿和工作表，生成类型/注册表/MessagePack 解析器并导出 bytes，确认 Configs 分组和 Config 标签齐全。不要只新增 Excel 工作表而漏掉显式登记。
3. ConfigManager 完成加载后，由装配层将表行转换为 HealthDefinition/HealthCatalog，校验引用和数值。对局内属性增益使用 Reconfigure 生成新快照，不修改已发布定义。
4. 在 GameRoot 添加单个生命周期适配服务，创建 HealthRuntime、命中映射及发送器。将其接入显式战斗阶段：命中收集→Health.Step→消费反应/生命状态投影→延迟回收→同步捕获。当前没有添加这个服务。
5. 玩家和怪物生成器用相同的 Register/Bind/Unregister 链路。Player 的 Alive/Dead 投影到现有控制状态时，应确定唯一权威写入口，并处理该 Tick 的网络快照顺序；不额外维护一套玩家专用扣血逻辑。
6. 修改 Weapon 命中适配层，将 `ProjectileDamageContext` 转换为 HealthCommand；移除对具体目标组件扣血方法的依赖。当前 Weapon 仍使用现有命中接口，没有在本轮接通 Health。
7. 接入网络传输、UI、死亡回收和各类反应执行器，再进行 Host/Client、晚加入、重连和实际对象池场景验证。旧 Health 的迁移按调用方逐步进行。

## 9. 验证和完成度

测试位于 `Assets/_HotUpdate/Tests/EditMode/HealthSystemTests.cs` 与 `HealthReplicationTests.cs`，覆盖统一玩家/怪物结算、防御护盾、死亡复活、无敌/频率门控、去重过期、代次复用、异常输入、Tick 回绕、队列背压、回调隔离、集中映射、全量增量、晚加入、缺帧恢复及快照不可变性。

2026-09-07：Unity EditMode 运行这两个测试类，实际执行 35 项，35 项通过、0 失败、0 跳过。测试任务 ID：`6995502ceaf4`。测试工具的 `total=775` 是全项目测试树统计，本次通过数只代表实际筛选执行的 35 项。

当前定位是可独立运行和验证的 Health 模块。实际 GamePlay 生命周期、正式 Excel 数据、Weapon 扣血接入、NGO 传输、客户端表现事件流、UI/VFX 和顿帧/击退执行器仍在上述后续步骤中。没有在实际联网场景中宣称验证这些尚未接入的部分。

性能方面采用集中字典和有时间边界的去重记录，事件发布及同步捕获仍有集合/委托分配；未做大规模怪物压力测试。后续以实际负载决定是否将状态改为稠密数组、将过期记录改为时间桶、减少通知分配，不影响现有外部接口。
