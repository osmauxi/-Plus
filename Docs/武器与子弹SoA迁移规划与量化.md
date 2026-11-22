# 武器与子弹 SoA 迁移规划与量化

## 1. 当前链路

```text
玩家输入
  -> PlayerStateMachine
  -> WeaponSystem.Simulate
  -> PlayerWeaponController.CommitAuthoritativeShot
  -> WeaponRuntimeService.CommitShot
  -> ShotBuilder.Build
  -> ProjectileWorld
  -> ProjectileSimulation.Tick
     -> 四叉树候选查询
     -> 窄相检测
     -> 命中、穿透、反弹
     -> Special Effect
  -> WeaponPresentationService
  -> WeaponPresentationTransport
```

`ShotContext` 保存一枪共享的数据，例如拥有者、射击 Tick、随机种子、武器快照 ID 和 EffectSet ID。`ProjectileState` 保存每一颗实际弹丸的运行状态。当前 SoA 迁移围绕 `ProjectileWorld` 展开，先降低活动弹丸容器和主循环的数据搬运成本，再处理碰撞和 Effect 的字段访问。

## 2. Benchmark 压力武器

压力武器只在 Benchmark 入口创建运行时快照，不修改正式 `Config_Weapon`。

- 稳定基线使用 5 发/秒、8 发/枪、2 秒寿命；
- 穿透 8、反弹 8、扩散角 8°；
- 伤害 0.001，并沿用 Benchmark 的伤害抑制；
- 固定挂载 `MultiSplit` 1 级和 `Shockwave` 1 级；
- Benchmark 入口补满弹药，避免采样窗口被换弹逻辑截断；
- 10 发/秒、16 发/枪是中压档位；20 发/秒、32 发/枪和 MultiSplit 2 级是传输上限故障档位。

压力武器的作用是稳定制造 Shot、Spawn、Impact、Removed 和 Effect 事件，让子弹系统进入足够高的数量级。

## 3. Marker 与验收指标

当前子弹链路使用以下 marker。

```text
Weapon.Projectile.Tick
Weapon.Projectile.ActiveLoop
Weapon.Projectile.MonsterQuery
Weapon.Projectile.MonsterNarrowPhase
Weapon.Projectile.ResolveContact
Weapon.Projectile.EffectDispatch
Weapon.Projectile.Remove
```

每次迁移记录以下数据。

1. 射击次数、生成弹丸数、活动弹丸平均值和最大值；
2. 子弹 marker 的平均、P95 和最大单帧耗时；
3. Host 发送消息数、载荷和 GC，Client 接收消息数、载荷和 GC；
4. 穿透、反弹、分裂和特殊 Effect 的事件数量；
5. Host/Client 怪物数量、View 槽位和表现对象池完整性。

## 4. 表现网络边界记录

首轮压力测试 `20261121-073546-8dcb2401` 发现，一个 server Tick 的全部表现事件被写入单个 64 KiB `FastBufferWriter`，在 `WriteImpact/WriteProjectile` 处发生 `OverflowException`。

第一次拆包后，`20261121-075138-16dabfa3` 仍在 `NetworkMessageTransport.SendToClient` 处溢出。当前 `UnreliableSequenced` 的真实传输上限小于本地 Writer 上限。

随后尝试每条消息 8 个事件。包大小安全了，但消息数量暴增触发 `CompleteSend failed (-5)`，固定 Tick 开始积压。

最终实现为每条消息最多 128 个事件，并增加 `ReliableFragmentedEvent`，映射到 NGO `ReliableFragmentedSequenced`。同一个 serverTick 的事件保持 `Shot → Spawn → Impact → Removed → SpecialEffect` 顺序，剩余事件继续发下一条消息。

压力档位因此分成两类。极端档位用于验证传输上限，稳定档位用于 SoA 前后量化。

## 5. AoS 基线

稳定压力档位的有效测试为 `20261121-091601-b697f392`。

| 指标 | Host | Client |
|---|---:|---:|
| 平均帧时 | 34.271 ms | 21.182 ms |
| P95 帧时 | 103.107 ms | 42.973 ms |
| GC 平均 | 61,626 B/帧 | 36,149 B/帧 |
| 射击/生成弹丸 | 100/6,350 | 0/0 |
| 活动弹丸平均/最大 | 84.09/403 | - |
| 发送/接收消息每秒 | 1215/30 | 30/1236 |
| 发送/接收载荷 | 762.73/2.51 KiB/s | 2.29/746.85 KiB/s |

Host 子弹 marker 如下。

| Marker | 平均每帧 |
|---|---:|
| `Weapon.Projectile.Tick` | 3.033 ms |
| `Weapon.Projectile.ActiveLoop` | 3.033 ms |
| `Weapon.Projectile.MonsterQuery` | 1.197 ms |
| `Weapon.Projectile.MonsterNarrowPhase` | 1.163 ms |
| `Weapon.Projectile.ResolveContact` | 0.243 ms |
| `Weapon.Projectile.EffectDispatch` | 0.269 ms |
| `Weapon.Projectile.Remove` | 0.136 ms |

## 6. 第一层容器 SoA

`ProjectileWorld` 已经从 `List<ProjectileState>` 改为并行数组，数组保存 ProjectileId、ShotId、位置、速度、生命周期、倍率、穿透/反弹、代数、命中数和 Flags。swap-back 删除保持原有行为，数组只在容量不足时扩容。

当前模拟边界仍使用 `Read/Write` 组装完整 `ProjectileState`，所以这一步主要验证容器布局和扩容成本。

测试 `20261121-094145-4036b830` 与 AoS 基线的单次对比如下。

| 指标 | AoS | 容器 SoA | 变化 |
|---|---:|---:|---:|
| Host 平均帧时 | 34.271 ms | 33.188 ms | -3.2% |
| Host GC 平均 | 61,626 B/帧 | 57,044 B/帧 | -7.4% |
| `Projectile.Tick` | 3.033 ms/帧 | 2.928 ms/帧 | -3.5% |
| `MonsterQuery` | 1.197 ms/帧 | 1.133 ms/帧 | -5.4% |
| `MonsterNarrowPhase` | 1.163 ms/帧 | 1.089 ms/帧 | -6.4% |
| `ResolveContact` | 0.243 ms/帧 | 0.245 ms/帧 | 持平 |
| `EffectDispatch` | 0.269 ms/帧 | 0.255 ms/帧 | -5.2% |

这只是单次重复，射击数量存在波动。当前可以确认容器 SoA 没有改变事件数量、网络载荷或客户端表现，收益需要多次重复后再确认。

## 7. 主循环直接数组访问首轮结果

第二层改动让无碰撞弹丸直接读取并行数组中的位置、速度、生命周期和 Flags。只有初始重叠或真实碰撞的弹丸才组装完整 `ProjectileState`，进入命中、穿透、反弹和 Effect 路径。

测试 `20261121-103235-75fc741c` 有效，Host 生成 6,550 个弹丸，活动弹丸平均 142.12，容器 SoA 基线生成 6,166 个弹丸，活动弹丸平均 85.32。两轮绝对帧时不能直接比较。

| 指标 | 容器 SoA | 直接数组访问 | 说明 |
|---|---:|---:|---|
| `Projectile.Tick` | 2.928 ms/帧 | 3.344 ms/帧 | 当前弹丸数量不同 |
| `MonsterQuery` | 1.133 ms/帧 | 1.315 ms/帧 | Query 次数随弹丸数量变化 |
| Host 平均 GC | 57,044 B/帧 | 54,538 B/帧 | 方向下降 |
| 活动弹丸平均 | 85.32 | 142.12 | 负载不同 |

按活动弹丸数量归一化，直接数组访问版本的单位 Tick 成本低于前一轮。需要三次重复确认后，才能决定继续拆碰撞字段还是回退当前边界。

三次重复测试 `20261121-103557-1a8b8ac5` 的 Host 结果如下。

| 重复 | 有效性 | 平均帧时 | `Projectile.Tick` | 活动弹丸平均 | GC 平均 |
|---:|---|---:|---:|---:|---:|
| 1 | 有效 | 46.359 ms | 5.140 ms/帧 | 151.70 | 68,187 B/帧 |
| 2 | 前台对齐无效 | 45.401 ms | 4.390 ms/帧 | 96.03 | 64,501 B/帧 |
| 3 | 有效 | 37.628 ms | 2.910 ms/帧 | 22.43 | 54,465 B/帧 |

三次的活动弹丸数量差异过大，直接数组访问的绝对耗时仍然不能作为稳定收益结论。当前代码通过了完整链路验证，下一步应先把压力武器的射击时序固定到更稳定的活动弹丸区间，再决定是否继续把碰撞字段拆成数组字段。

曾尝试在压力模式下持续提交开火意图，让三次重复的射击数量完全一致。该模式把发送和模拟压力推高到无法完成 Benchmark 前置移动校验，测试停在 `driver-configured`，随后已回退。当前保留能完成完整 Host/Client 测试的脉冲开火入口，并把射击数量波动作为量化报告中的有效性条件。

## 8. Shot 元数据缓存 SoA 首轮结果

最新版本把 `StatSnapshotId` 和 `OwnerEntityId` 缓存在 `ProjectileWorld` 并行数组中。无碰撞弹丸可以直接读取武器快照和拥有者，只有发生实际碰撞时才回查完整 `ShotContext`。

有效测试为 `20261121-161849-ca31e6ab`。

| 指标 | 当前结果 |
|---|---:|
| Host 平均帧时 | 41.897 ms |
| Host GC 平均 | 70,956 B/帧 |
| 射击/生成弹丸 | 79/5,764 |
| 活动弹丸平均/最大 | 67.78/395 |
| `Projectile.Tick` | 3.964 ms/帧 |
| `MonsterQuery` | 1.620 ms/帧 |
| `MonsterNarrowPhase` | 1.278 ms/帧 |
| Host 发送消息/载荷 | 1352.57/s，803.89 KiB/s |

这一轮的活动弹丸数量低于前一轮，绝对耗时没有出现可确认的下降。当前先保留字段缓存结构，下一轮需要固定更稳定的压力负载后再判断字典查询是否值得继续优化。

## 9. 网络发送 GC 观测计划

最新有效 Host 样本 `20261121-161849-ca31e6ab` 在 30 秒内发送 39,564 条应用层消息，累计 24.15 MB，平均发送速率 785.78 KiB/s。武器表现层采用每条消息最多 128 个事件的批次，发送次数低于怪物群组，但每条消息包含的弹丸和特殊 Effect 数据更多。

## 10. Weapon 表现改用 Unnamed Message

Weapon 表现帧已从 Named Message 切换到 Unnamed Message。发送帧首字节使用专用路由值 `0x80`，`NetworkMessageTransport` 在 Unnamed 入口复制 Reader 进行路由探测，Monster 继续使用原有 Unnamed 处理器和原有包体。Weapon 接收器消费路由字节后再读取原有表现帧内容。

这次切换只移除了 Named Message 名称路由，未改变 Weapon 的事件顺序、128 事件分块策略和 `ReliableFragmentedEvent` 投递语义。Weapon 发送范围使用独立 Marker `Weapon.Network.Presentation.SendUnnamedMessage`，不会再把通用 Named Message 数量当成 Weapon 发包数量。

切换后的第一次 Player 重测曾被现有 `ControlFeelTestController` 编译错误阻塞。该脚本已恢复到可编译状态后重新生成了 HotFix DLL、Addressables 和 Player。此前 `20261122-020847-787567a8` 与 `20261122-025008-ea78de52` 的失败轮分别包含旧 DLL、路由读写错位和 Reader 游标共享问题，均不作为新协议结果。

### 10.1 Unnamed 版本实测

有效测试为 `20261122-030828-be26a3d3`，Host 与 Client 均有效。Host 的 Weapon 专属 Unnamed Marker 执行 933 次，总耗时 17.10 ms，平均 0.016 ms/帧。Host GC 平均 51.2 KB/帧，P95 为 310.5 KB/帧，最大单帧为 1.49 MB。

上一轮有效 Named 结果 `20261121-173043-033c426f` 中，Weapon 专属 Named Marker 执行 764 次，总耗时 16.39 ms，平均 0.015 ms/帧，Host GC 平均 49.3 KB/帧，P95 为 373.8 KB/帧，最大单帧为 1.50 MB。

两轮射击和弹丸数量不同，不能据此宣布 GC 已经下降。当前能确认 Unnamed 路由已正确工作，Weapon 发送范围的 CPU 耗时与 Named 版本处于同一量级。是否能降低 NGO 内部托管分配，需要在相同射击事件数量下重复测试，或使用完整 Profiler 直接查看 `CustomMessagingManager.SendUnnamedMessage` 的 GC Alloc。

当前 GC 的第一嫌疑是 NGO 的 `CustomMessagingManager.SendNamedMessage` 和 `SendUnnamedMessage` 内部发送路径。`FastBufferWriter` 使用临时原生分配器，序列化字段写入不是当前最需要怀疑的托管分配来源。网络统计字典在消息名称稳定后不会持续新增键，优先级低于发送调用本身。

弹丸侧仍有一处明确的托管分配路径。`ProjectileSimulation.RememberHit` 会在命中集合池为空时创建 `HashSet<ulong>`，集合容量增长也可能产生分配。该路径会在穿透压力下被放大，因此本轮加入 `Weapon.Projectile.HitSet` Marker。

本轮加入的观测 Marker 如下：

| Marker | 观测范围 |
|---|---|
| `Weapon.Network.Presentation.SendChunk` | 单个表现网络分块的 Writer、序列化和发送。 |
| `Weapon.Network.Presentation.SendUnnamedMessage` | 仅 Weapon 表现帧进入 Unnamed Transport 的发送范围。 |
| `Monster.Network.SendNamedMessage` | 通用 Transport 的单目标 Named Message 发送。 |
| `Monster.Network.RecordSent` | 发送统计字典更新。 |
| `Weapon.Projectile.HitSet` | 弹丸命中集合的查找、创建和写入。 |

下一次测试保持 10024 怪和压力武器配置不变，重点对齐 Weapon 专属发送 Marker 的 GC Alloc 列、逐帧 GC、长帧和消息数。通用 `Monster.Network.SendNamedMessage` 不再作为 Weapon 发包次数的依据，因为它还覆盖其他 Named Message。

### 9.1 单次 Marker 实测

测试 `20261121-170353-0b16e17f` 的 Host 与 Client 均有效。Host 记录 5513 个弹丸，平均活动弹丸 68.30，累计 GC Alloc 49.75 MB，平均 53.6 KB/帧。Host 发送 38,468 条消息，累计 23.95 MB，平均 779.55 KiB/s。

| Marker | Host 调用次数 | Host 总耗时(ms) | 平均每帧(ms) |
|---|---:|---:|---:|
| `Monster.Network.SendUnnamedMessage` | 36,461 | 138.29 | 0.149 |
| `Monster.Network.SendNamedMessage` | 1,956 | 32.38 | 0.035 |
| `Monster.Network.RecordSent` | 38,417 | 39.58 | 0.043 |
| `Weapon.Network.Presentation.SendChunk` | 822 | 62.97 | 0.068 |
| `Weapon.Network.Presentation.SendUnnamedMessage` | 764 | 16.39 | 0.015 |
| `Weapon.Projectile.HitSet` | 52,350 | 10.43 | 0.011 |

本轮专属测试为 `20261121-173043-033c426f`。Host 样本有效，Client 因双端采样停止偏差超过 100 ms 被判为无效。Host 的 Weapon 表现分块发送只有 764 次，约 25.5 次/秒。通用 `Monster.Network.SendNamedMessage` 为 1,902 次，不能用来代表 Weapon 发包次数。

`Weapon.Network.Presentation.SendNamedMessage` 嵌套在 `Weapon.Network.Presentation.SendChunk` 内。两者平均每帧分别为 0.015 ms 和 0.052 ms，差值主要覆盖 Writer 创建、事件序列化和发送前的业务范围。`Weapon.Projectile.HitSet` 平均每帧 0.009 ms，当前 CPU 成本很低。

Host 本轮 GC Alloc 平均 49.3 KB/帧，P95 为 373.8 KB/帧，最大单帧 1.50 MB。轻量采样仍然没有按 Marker 输出 GC Alloc，因此这组数据可以确认 Weapon 发送范围和命中集合的调用规模，不能单独证明两者各自产生了多少托管字节。下一轮完整 Profiler 只需要围绕 Weapon 专属发送 Marker 展开，不再把怪物群组发送作为 Weapon GC 归因对象。

轻量采样确认了武器表现发送范围只执行 822 次，命中集合范围执行 52,350 次，但当前两者 CPU 耗时都很低。它们是否产生了托管分配，仍不能由 Marker CSV 单独判断。下一次需要保存一轮完整 `profiler.raw`，在 Hierarchy 中展开 `GC.Alloc`，查看 `SendNamedMessage`、`SendUnnamedMessage`、`SendChunk` 和 `HitSet` 的子调用。

### 10.2 完整 Profiler GC 归因

完整 Profiler 测试为 `20261122-031527-d51e837d`。Host 原始 Profiler 已加载分析。Weapon Unnamed 发送范围在 300 个正式采样帧内累计约 22.7 KB GC Alloc，发送路径本身只占很小部分。

主要 GC 来源如下：

| 调用范围 | 累计 GC Alloc |
|---|---:|
| `Gameplay.TickClock.Replication` 总范围 | 5.87 MB |
| `Instantiate.Produce` | 1.80 MB |
| `VFXImpactScaler.Awake` | 1.70 MB |
| `VFXAutoReturn.Awake` | 1.17 MB |
| `Monster.Network.GroupSchedule.Membership` 发送链路 | 0.74 MB |
| `Monster.Network.GroupSnapshot.Send` 发送链路 | 0.71 MB |
| `Weapon.Network.Presentation.SendUnnamedMessage` | 22.7 KB |
| `Weapon.Projectile.Remove` | 8.2 KB |
| `Weapon.Projectile.EffectDispatch` | 7.2 KB |

`WeaponPresentationService.ReceiveProjectileImpact` 每次命中都会调用 `LocalVFXPool.Play`。当前 `VFX_OnHit` 和 `HitBlood` 的对象池初始容量都是 20，最大容量都是 64。压力武器的同时命中特效超过预热容量时，`LocalVFXPool.CreateInstance` 会在正式 Tick 中实例化新对象，进而触发 `VFXImpactScaler.Awake`、`VFXAutoReturn.Awake` 以及 Unity Instantiate 分配。

这说明当前 GC 增量主要来自表现层 VFX 池扩容，Weapon 网络发送只贡献约 22.7 KB。下一步应优先优化 VFX 池预热和扩容行为，Weapon 网络 GC 继续保持当前 Unnamed 版本进行对比。

### 10.3 Weapon GC 优化计划

#### 第一阶段：消除 VFX 热路径实例化

先在 `LocalVFXPool` 记录 `VFX_OnHit` 和 `HitBlood` 的运行中 `PlayingCount` 峰值。当前两个池的初始容量都是 20，最大容量都是 64。下一轮先用压测峰值设置预热容量，确保正式采样窗口不再进入 `CreateInstance`。

验证目标如下：

- 正式采样期间 `Instantiate.Produce` 为 0；
- `VFXImpactScaler.Awake` 和 `VFXAutoReturn.Awake` 不再出现在正式采样帧；
- `VFX_OnHit`、`HitBlood` 的 PlayingCount 不超过预热容量；
- VFX 表现事件数量和客户端表现完整性保持不变。

如果峰值超过 64，再同步提高 `MaxSize`。容量以压测实测峰值为依据，避免把对象池无限放大。

#### 第二阶段：去除 VFX 实例初始化重复扫描

当前一个 VFX 实例会同时经过 `VFXAutoReturn.Awake` 和 `LocalVFXPool.VFXRuntimeCache` 的组件扫描。预热完成后，这部分不再出现在热路径，但仍然是实例创建成本。后续可以让对象池缓存统一提供 ParticleSystem、VisualEffect 和生命周期状态，减少 `GetComponentsInChildren` 以及脚本内部数组的重复保存。

## 11. 10000 怪临界点前的子弹首轮优化

9000 怪完整 Profiler 中，Host 平均帧时为 24.488 ms，`CombatSimulation` 平均 2.859 ms，最大单帧 26.450 ms。当前 30 Hz Tick 的预算约为 33.3 ms，开火后的碰撞和 Effect 峰值会把部分 Tick 推过预算，触发追帧。该轮报告还没有采集到 `Weapon.Projectile.*` 子 marker，因此只能确认 CombatSimulation 尖峰，不能据此分摊到子弹窄相、Effect 或表现发送。

首轮代码改动集中在闪电 Effect。`ApplyLightning` 原来会对同一个节点的每个分支重复执行同一范围的怪物查询。现在一次节点只查询一次，所有分支复用候选数组。分支之间通过现有 `_lightningVisited` 过滤已访问目标，怪物死亡后通过 `TryGetMonsterPosition` 跳过失效槽位，命中语义保持不变。

同时加入以下观测 marker，用于下一轮把持续型 Effect 从普通子弹链路中拆出：

```text
Weapon.Effect.Lightning
Weapon.Effect.StormCloud
Weapon.Effect.DamageArea
```

下一轮应重新构建 HotFix 后，在 9000 和 10024 怪下分别执行不射击、关闭特殊 Effect、完整压力武器三组对照，重点读取 `Projectile.ActiveLoop`、`MonsterQuery`、`MonsterNarrowPhase`、`ResolveContact`、`EffectDispatch` 和三个 Effect marker。

这一步放在预热容量验证之后。对象池已经不再在正式 Tick 创建实例时，再评估是否值得合并两套缓存。

#### 第三阶段：定位 Replication 剩余分配

`Gameplay.TickClock.Replication` 仍有约 5.87 MB 的聚合分配，当前完整调用树已经确认其中包含 VFX 实例化。完成第一阶段后，再对以下范围分别记录 GC Alloc：

- `WeaponPresentationService.FlushServerFrame`；
- `WeaponPresentationService.ApplyFrameLocally`；
- `WeaponPresentationService.ReceiveProjectileImpact`；
- `LocalVFXPool.Play`；
- `LocalVFXPool.CreateInstance`；
- 各待发送事件列表的扩容位置。

如果预热后仍有持续分配，再处理 `_pendingShots`、`_pendingSpawns`、`_pendingImpacts`、`_pendingRemoved` 和 `_pendingSpecialEffects` 的容量增长。当前不先改成新的容器，先用 Marker 证明扩容是否仍发生。

#### 第四阶段：保留网络层当前方案

完整 Profiler 显示 Weapon Unnamed 发送范围 300 帧累计约 22.7 KB GC。当前不继续改 Named/Unnamed 发送协议，也不把网络包合批作为本轮 GC 优化目标。网络层只保留现有独立 Marker，用于后续回归确认。

#### 验收口径

每一阶段都使用相同 10024 怪和压力武器，至少完成一轮完整 Profiler 与三轮轻量压测。重点比较 VFX 实例化次数、Weapon 表现事件数、总 GC、P95 GC 和长帧。只有当 VFX 实例化归零且事件数量一致时，才能把 GC 变化归因于对象池优化。

### 10.4 并发预算与预热实测

测试 `20261122-041224-e42d797f` 验证了第一阶段的并发预算。Host 与 Client 均有效，Host 平均 GC 从上一轮的 51.2 KB/帧降到 13.5 KB/帧，P95 从 310.5 KB/帧降到 44.1 KB/帧。

本轮峰值如下：

| 指标 | 结果 |
|---|---:|
| `VFX_OnHit` 同时播放峰值 | 10 |
| `HitBlood` 同时播放峰值 | 64 |
| Host GC 总量 | 13.87 MB |
| Host 平均 GC | 13.49 KB/帧 |
| Host P95 GC | 44.11 KB/帧 |

随后加入初始化预热后，测试 `20261122-042125-b19f3ed9` 的 Host 平均 GC 进一步降到 12.88 KB/帧，P95 降到 34.76 KB/帧。两个 VFX 池在正式采样期间都维持在 64 个实例以内。

当前结果说明并发预算和启动预热都有效。后续应继续检查完整 Profiler 中 `Instantiate.Produce`、`VFXImpactScaler.Awake` 和 `VFXAutoReturn.Awake` 是否已经从正式采样帧消失，再决定是否需要合并 VFX 组件的重复缓存。

### 10.5 完整 Profiler 验证

完整 Profiler 测试为 `20261122-042716-2949b927`。Host 与 Client 均有效，Host GC 平均 15.0 KB/帧。Profiler 的 300 个正式帧中，`VFXImpactScaler.Awake` 和 `VFXAutoReturn.Awake` 均未出现，说明 VFX 对象池预热已经阻止了命中特效在正式 Tick 中扩容。

`Instantiate.Produce` 仍然存在于主线程树中，当前不能把它全部归因到 VFX。下一步只需继续按调用路径区分其他实例化来源，VFX 相关的两个 Awake 分配已经消失。

### 10.6 最新 15 KB/帧 GC 分布

完整 Profiler 测试 `20261122-042716-2949b927` 的 Host 总 GC Alloc 为 13,352,136 B，轻量报告平均为 15,002 B/帧。按调用路径汇总如下。

| 调用路径 | GC Alloc | 占总量 |
|---|---:|---:|
| `Gameplay.TickClock.Replication` 总范围 | 1,012,020 B | 7.58% |
| `Monster.Network.GroupSnapshot.Send` | 949,662 B | 7.11% |
| `Monster.Network.GroupSchedule.Membership` | 819,756 B | 6.14% |
| `NetworkMessageManager.SendBatch` | 528,216 B | 3.96% |
| `GameplayHUDPresenter.Update` | 311,296 B | 2.33% |
| `BenchmarkRuntimeRecorder.LateUpdate` | 161,722 B | 1.21% |
| `NetworkMessageManager.DeserializeAndHandle.NamedMessage` | 101,738 B | 0.76% |
| `Monster.Network.GroupSchedule.Flush` | 75,600 B | 0.57% |
| `UnityTransport.Update` | 65,960 B | 0.49% |
| `Monster.Network.SendNamedMessage` | 54,400 B | 0.41% |
| `Weapon.Network.Presentation.SendUnnamedMessage` | 20,296 B | 0.15% |
| `Weapon.Projectile.Remove` | 8,224 B | 0.06% |
| `Weapon.Projectile.EffectDispatch` | 7,200 B | 0.05% |
| `Instantiate.Produce` | 1,040 B | 0.01% |
| `Instantiate.Copy` | 880 B | 0.01% |

最新结果中 `VFXImpactScaler.Awake` 和 `VFXAutoReturn.Awake` 没有进入正式采样帧，VFX 对象池扩容已经从主要 GC 来源中移除。Weapon Unnamed 发送、弹丸移除和 Effect 分发合计约 35.7 KB，占比约 0.27%。剩余 GC 的主要优化方向已经转移到 Monster 群组发送、网络批处理和 HUD 更新。

### 10.7 单 Client Unnamed 发送优化

压测始终只有一个远端 Client，但 Monster 群组此前仍使用 `SendUnnamedMessage(IReadOnlyList<ulong>)` 的列表重载。通用 Transport 现在对单 Client 直接调用 NGO 的单目标 Unnamed 重载，保留多 Client 广播路径。

测试 `20261122-045339-d89df141` 有效。Host 平均 GC 从预热后的 12.88 KB/帧降到 11.64 KB/帧，P95 从 34.76 KB/帧降到 32.39 KB/帧。该改动减少了列表广播路径的内部管理开销，未改变包体和消息数量。

### 10.8 单 Client 路径后的完整 Profiler 复测

测试 `20261122-054029-0e178849` 使用相同的 10024 怪、压力武器和单 Client 配置，完整 Profiler 正式采样有效。当前代码已经命中单 Client Unnamed 标量重载。

| 角色 | 平均帧间隔 | Main Thread 平均 | GC 平均/ P95 / 最大 | 发送消息/s | 发送 KiB/s |
|---|---:|---:|---:|---:|---:|
| Host | 38.51 ms | 38.47 ms | 13.99 KB / 43.21 KB / 745.68 KB | 1376.05 | 845.52 |
| Client | 20.15 ms | 20.11 ms | 12.87 KB / 38.26 KB / 970.42 KB | 29.99 | 2.28 |

Host 正式采样中的发送 Marker 如下。

| Marker | 调用样本 | 平均每帧 | P95 单帧 | 最大单帧 |
|---|---:|---:|---:|---:|
| `Monster.Network.SendUnnamedMessage` | 39985 | 0.248 ms | 0.646 ms | 3.226 ms |
| `Monster.Network.RecordSent` | 41181 | 0.064 ms | 0.170 ms | 0.320 ms |
| `NetworkMessageManager.SendBatch` | 由完整 Profiler 读取 | 单帧约 0.03 ms | — | — |

在完整 Profiler 的高 GC 帧中，`Monster.Network.SendUnnamedMessage` 下的 GC 分布约为 2.9 KB 的群组快照、2.8 KB 的成员表和 0.34 KB 的 Flush 快照，合计约 6 KB。该帧同时有约 67 次发送调用，折算约 90 B/次。发送链路的 GC 仍然来自 NGO Unnamed 消息内部的托管路径，当前没有证据表明业务侧的 `_remoteClientIds` 列表发生了复制。

同一高 GC 帧中 `BenchmarkRuntimeRecorder.LateUpdate` 单独出现约 0.7 MB 分配，来源是完整 Profiler 下的运行时采样记录扩容或写入。这个尖峰属于测量工具开销，不能归因于怪物网络。`GameplayHUDPresenter.Update` 同帧约 4.8 KB，`UnityTransport.Update` 约 220 B，`NetworkMessageManager.DeserializeAndHandle.NamedMessage` 约 0.7 KB。

本轮结论是，单 Client 标量重载已经绕过了列表广播入口，但 NGO 的单目标 Unnamed 发送仍保留约 90 B/次的内部分配。下一轮应把 NGO 网络统计与测试记录器分别做 A/B 测量，继续确认剩余分配来自统计事件、消息指标还是其他发送钩子，再决定是否需要调整开发构建的网络统计配置。数组替换不会直接消除这部分分配。

### 10.9 记录器容量预留后的轻量复测

为了避免采样器自身的 `FrameValues` 和逐帧列表扩容污染 GC 数据，`BenchmarkRuntimeRecorder.StartCapture` 现在按本轮采样时长预留采样容量。测试 `20261122-060932-11b51f42` 使用新 Player 完成 10024 怪轻量双端采样。

采样期中途原本在第 512 帧和第 1024 帧出现的记录器扩容尖峰已经消失。Host 全部采样的平均 GC 为 11.54 KB/帧，P95 为 30.79 KB。排除采样开始的前 10 帧后，平均 GC 为 8.97 KB/帧，P95 为 30.43 KB，最大值为 262.73 KB。首帧约 2.72 MB 仍属于采样器启动和运行时初始化，不用于判断稳定网络分配。

本轮应用层网络数据与上一轮保持同一数量级。Host 发送约 1404 条消息/s、862.63 KiB/s，`Monster.Network.SendUnnamedMessage` 平均每帧 0.160 ms，说明记录器容量预留没有改变网络发送量。后续分析应以排除采样器初始化后的稳定区间为准，不能把记录器启动分配或首次容量建立计入网络层热路径。

### 10.10 Marker 对子弹帧耗时测量的影响

当前压测同时使用运行时 `ProfilerMarker` 和 `BenchmarkRuntimeRecorder`。记录器每帧读取约 100 个 Marker，并把结果写入采样列表。游戏代码中的 Marker 则在进入和离开范围时执行插桩。

子弹侧 `Weapon.Projectile.Tick`、`Weapon.Projectile.ActiveLoop`、`Weapon.Projectile.MonsterQuery` 和 `Weapon.Projectile.MonsterNarrowPhase` 属于低频外层范围，适合用来比较优化前后的总成本。`Weapon.Projectile.EffectDispatch`、`Weapon.Projectile.ResolveContact` 和 `Weapon.Projectile.HitSet` 会在单帧内执行大量次，只适合观察成本分布，不能直接当作无插桩发布版本的绝对耗时。

子弹遍历优化保持这些 Marker 不变，因此前后差值仍然具备可比性。绝对帧预算确认时，再使用关闭完整 Profiler 的轻量采样，并单独保留外层 Marker，避免把高频细粒度插桩误判为业务逻辑成本。

### 10.11 ProjectileWorld SoA 热循环访问优化

当前 `ProjectileWorld` 已经按字段拆分为数组，但 `ProjectileSimulation.Simulate` 仍通过多个 `GetX` 和 `SetX` 方法逐字段访问。第一轮优化让模拟循环直接读取 SoA 列数组，减少每颗弹丸的访问器调用和重复取值。`ProjectileState` 继续保留在命中、反弹、穿透、移除和 Effect 事件边界，用于保持现有碰撞与表现语义。

本轮不改变弹丸生命周期、SwapBack、碰撞次数限制、命中集合或网络协议。编译通过后，使用同一压力武器和 10024 怪物配置比较 `Weapon.Projectile.ActiveLoop`、`MonsterQuery`、`MonsterNarrowPhase`、平均帧时和 P95 帧时。

首轮复测 `20261122-070011-3f44ba27` 有效，但与上一轮 `20261122-060932-11b51f42` 的弹丸负载不完全相同。上一轮平均活动弹丸为 53.04，新一轮为 124.58，生成弹丸数量为 5951 和 6216。新一轮 Host 平均帧时为 30.08 ms，不能直接作为回归结论。

按平均活动弹丸数量归一化后，`Weapon.Projectile.ActiveLoop` 从约 0.059 ms/活动弹丸·Tick 降到约 0.028 ms/活动弹丸·Tick。`MonsterQuery` 和 `MonsterNarrowPhase` 的单次调用成本也分别下降约三成。所有 10024 个怪物、弹丸表现槽位和网络工作负载均有效，当前未发现 SwapBack 或弹丸生命周期错误。下一轮需要在相同弹丸负载下重复采样，再确认绝对帧时收益。

第二轮复测 `20261122-070754-5725271f` 继续使用新代码，Host 平均活动弹丸为 98.03，生成弹丸 6148，Host 平均帧时 28.52 ms。`Weapon.Projectile.ActiveLoop` 平均每帧 2.67 ms，在活动弹丸数量接近上一轮两倍时仍与旧基线的 2.68 ms 持平。按活动弹丸归一化后约为 0.027 ms/活动弹丸·Tick，`MonsterQuery` 单次约 0.0072 ms，`MonsterNarrowPhase` 单次约 0.0061 ms，较旧基线分别下降约四成和三成。该轮所有怪物、弹丸和表现槽位验证通过，说明 SoA 热循环访问优化已经产生稳定收益。

## 11. SoA 阶段原迁移计划

本节记录最初的细化顺序。结合第二次直接数组访问复测后的阶段判断，当前不再把第 2、3 项拆成独立的微优化任务，新的切换边界以第 12 节为准。

1. 连续重复容器 SoA，确认 `ActiveLoop`、GC 和事件数量的变化可重复；
2. 把主循环的移动和生命周期计算改为直接读取并行数组，减少 `Read -> 整体 ProjectileState -> Write`；
3. 把穿透、反弹、HitCount 和 Flags 的更新改为数组字段写入；
4. 保留完整 `ProjectileState` 作为 Effect 和表现事件边界；
5. 再评估 ShotRepository 的 Dictionary 查找是否成为主要热点；
6. 最后才考虑把碰撞上下文和 Effect 输入进一步压缩为 AOT 可批处理数据。

每一层都执行编译检查、热更 DLL/AOT 同步、Player 打包、10024 怪测试和 marker 对比。出现事件丢失、表现槽位不完整、网络包溢出或怪物数量错误时，停留在当前层修复。

## 12. SoA 阶段重新评估与 Burst/Job 切换边界

### 12.1 最新量化结果

本次评估使用 `20261122-070754-5725271f` 的 `focus-host/host` 数据，并与直接数组访问前的 `20261122-060932-11b51f42` 对比。两次测试都使用 10024 怪、相同压力武器和 30 秒采样窗口，射击数量和活动弹丸数量仍有自然波动，所以总帧时只作为背景指标，子弹 Marker 按活动弹丸数量归一化。

| 指标 | 直接数组访问前 | 当前 SoA 热循环 | 变化 |
|---|---:|---:|---:|
| Host 平均帧时 | 28.365 ms | 28.522 ms | +0.6%，属于负载波动范围 |
| Host P95 帧时 | 62.352 ms | 57.551 ms | -7.7% |
| 平均活动弹丸 | 53.04 | 98.03 | +84.8% |
| `Weapon.Projectile.ActiveLoop` 平均每帧 | 2.678 ms | 2.670 ms | 基本持平 |
| ActiveLoop 单活动弹丸·Tick | 0.0505 ms | 0.0272 ms | 约下降 46% |
| `MonsterQuery` 单次调用 | 0.010618 ms | 0.007185 ms | 约下降 32% |
| `MonsterNarrowPhase` 单次调用 | 0.009037 ms | 0.006053 ms | 约下降 33% |
| Host 平均 GC | 11.54 KB/帧 | 11.63 KB/帧 | 基本持平 |
| Host P95 GC | 30.79 KB | 27.68 KB | -10.1% |

当前活动弹丸接近旧基线的两倍，`ActiveLoop` 总耗时仍保持在 2.67 ms/帧左右，说明 `ProjectileWorld` 的数组布局和 `Simulate` 无碰撞热路径已经完成了可重复的收益。总帧时没有下降，是因为当前 Host 仍由怪物模拟、复制和表现应用占据主要预算，不能把它解释成子弹优化失效。

### 12.2 当前 SoA 阶段的收口判断

面向对象数据接口与 SoA 容器的第一阶段已经完成。当前代码具备以下稳定边界：

- `ProjectileWorld` 负责并行数组、稳定索引和 SwapBack；
- `ProjectileSimulation` 的无碰撞路径直接读取和写入数组；
- 命中、反弹、穿透、移除和 Effect 事件仍使用 `ProjectileState` 快照，行为和表现协议没有改变；
- 10024 怪下事件数量、网络载荷、弹丸生命周期和表现槽位保持有效。

仍然存在的 `Read/Write` 快照组装、`ShotRepository` 和 `WeaponStatSnapshotRepository` 字典查找，以及命中路径的字段访问，属于下一阶段需要重新划分的数据流。继续逐个增加访问器只能减少少量复制，不能解决 Job 化所需要的托管边界，因此不再把它们作为独立的 SoA 微优化任务。

### 12.3 Burst/Job 的适用范围

当前 `ProjectileSimulation` 不能整体标记为 Burst，原因是同一调用链中仍包含以下主线程和托管行为：

- `Physics.SphereCastNonAlloc` 与 `Physics.OverlapSphereNonAlloc`；
- `MonsterRoomRuntime.QueryMonsterCandidates` 访问托管四叉树和托管数组；
- `WeaponStatSnapshotRepository`、`ShotRepository`、`Dictionary<uint, HashSet<ulong>>` 和 `Stack<HashSet<ulong>>`；
- `DamageMonster`、Effect Dispatcher、C# 事件和表现事件列表；
- `ProjectileWorld.RemoveAtSwapBack` 以及网络发送前的事件整理。

Burst/Job 的第一条边界应放在纯值类型计算段。Job 输入是本 Tick 已经准备好的弹丸字段、怪物位置和候选槽位，Job 输出是移动后的字段和固定布局的碰撞结果。移动、寿命扣减、半径计算、方向计算、候选怪物窄相数学和最近命中选择可以放入这条边界。

主线程继续负责 Unity Physics 查询、怪物伤害提交、命中集合更新、Effect 触发、事件发布、SwapBack 和网络序列化。Job 不直接调用 `MonsterRuntimeService`、Effect 系统或 `WeaponPresentationTransport`。

### 12.4 封包与 Job 的最终分界

表现网络协议是 Job 输出之后的另一条边界。`WeaponPresentationTransport` 继续接收 `ShotContext`、`ProjectileSpawn`、`ProjectileImpact`、`ProjectileState` 和 `WeaponSpecialVfxEvent`，负责 `FastBufferWriter`、事件分块和 Unnamed Message 发送。Burst/Job 不接触 `FastBufferWriter`、NGO Transport 或 VFX 对象。

当前传输层的封包约束继续保持为每帧最多 4096 个事件、每个消息最多 128 个事件、Writer 最大容量 64 KiB。Job 只产生固定值类型结果，主线程在提交伤害和 Effect 后把这些结果写入现有事件缓冲区。这样可以同时保持计算并行化和现有网络协议稳定。

### 12.5 下一轮迁移顺序

1. 为弹丸计算定义固定布局的 Job 输入、Job 输出和碰撞结果结构，不改变现有事件语义；
2. 先把没有命中和没有 Effect 的移动、寿命与字段更新迁移到 `IJobParallelFor`，保留主线程碰撞和提交阶段；
3. 将怪物候选结果改成 Job 可读取的连续缓冲区，再迁移窄相数学和最近命中选择；
4. 最后评估是否值得把 Unity Physics 查询替换为可在 Job 中使用的物理数据源；
5. 每一步都保持 `ProjectilePresentationTransport`、Effect 分发和 `ProjectileWorld` 的生命周期提交在主线程。

当前子弹 `ActiveLoop` 约占 Host 平均帧时的 9.4%，其中 `MonsterQuery` 和 `MonsterNarrowPhase` 合计约占 6.8%。因此 Burst/Job 阶段的验收目标是降低这部分固定成本并验证随弹丸数量增长的扩展曲线，不把它描述成能单独解决 10024 怪 Host 的全部帧时问题。

### 12.6 10024 怪聚集阶段与 VFX 表现峰值

测试 `20261122-161527-18504740` 的 Host 首次重复样本可用于定位峰值，但 Client 前台占比只有 52.2%，整轮不满足正式基线有效性条件，因此不用于正式性能回归结论。

这次样本仍然说明了聚集阶段的两个现象：

- Host `Weapon.Projectile.ActiveLoop` 平均约 3.14 ms/帧，P95 单帧约 9.84 ms，最大约 21.90 ms；其中 `Monster.Projectile.MonsterQuery` 与 `Monster.Projectile.MonsterNarrowPhase` 会随命中候选和穿透事件集中出现峰值；
- Client 平均帧时约 22.50 ms，P95 约 39.91 ms，GPU P95 约 18.75 ms。当前没有独立的 VFX 播放和特殊 Effect Update Marker，所以不能仅凭体感或总 GPU 时间判断 VFX 是主因。

本轮只增加表现层测量，不改变 VFX 行为：

- `Weapon.Presentation.ImpactVfx` 覆盖命中 VFX 的容量判断、旋转计算和对象池播放；
- `Weapon.Presentation.SpecialVfx.Play` 覆盖特殊 Effect 的创建或更新；
- `Weapon.Presentation.SpecialVfx.Update` 覆盖特殊 Effect 的环、线、区域和云层更新。

`WeaponSpecialEffectVfxPlayer` 当前仍会为部分瞬时效果直接创建 `GameObject` 和 `ParticleSystem`，结束后调用 `Destroy`。如果下一轮 Marker 证明该路径在聚集阶段形成主线程尖峰，再把这些瞬时效果迁移到独立的表现对象池。若 Marker 成本很低而 GPU 峰值明显，则优先做命中 VFX 的屏幕空间或距离预算，保留近距离即时反馈，限制远处重复特效数量。
