# 怪物宏观重构路径与表现层去 GameObject 化方案

> 状态：路线设计，尚未进入实现阶段
>
> 适用基线：四叉树核心 AOT 迁移、怪物统一空间快照完成后的下一阶段

## 1. 结论先行

当前项目可以开始准备消灭“每只怪物一个 GameObject”的表现架构，但不应把它理解为删除所有 GameObject。

需要保留的仍然包括：

- 房间级运行服务，例如 `MonsterRuntimeService`、`MonsterViewRuntimeService`；
- 摄像机、场景、UI、特效等场景级对象；
- 作为资源载体的 `ScriptableObject`、Mesh、Material、VAT 纹理。

本方案要消灭的是每只怪物对应的：

- `GameObject`；
- `Transform`；
- `MonsterViewHandle` `MonoBehaviour`；
- 通过对象池维护的单体表现实例。

怪物玩法状态已经主要下沉到 `MonsterWorld` SoA，标准怪物 Collider 也已经不再是命中主路径。当前剩余的表现对象主要用于提供 VAT 资源配置、保存 Slot 绑定和承载历史上的表现入口。因此，去 GameObject 化在架构上是可行的，但必须先把这些静态资源配置从 Prefab/`MonsterViewHandle.Awake` 中抽离出来。

当前 `MonsterVATRenderWorld` 已经使用 `Graphics.DrawMeshInstanced` 按 RenderGroup 批量绘制，并在每个实例上上传位置、VAT 当前帧、下一帧、混合权重和受击闪烁参数。这意味着真正的无 GameObject 表现路径并不是重新发明渲染系统，而是让现有批量 VAT 渲染直接接管 Slot。

## 2. 当前架构边界

### 2.1 已经下沉到数据层的部分

当前怪物玩法侧主要依赖：

- `MonsterWorld` 的运行时 SoA 数组；
- `MonsterOccupancySnapshot`；
- 房间级四叉树空间查询；
- Density、Separation、寻路、战斗和网络快照；
- `MonsterPresentationFrame` 的只读表现快照。

Weapon 系统通过 `MonsterRuntimeService`、房间空间查询和战斗事件访问怪物，不应该从 `MonsterViewWorld` 或怪物 GameObject 读取位置、朝向、生命或碰撞信息。

因此，怪物表现层原则上应该是：

```text
MonsterWorld / Replica
        ↓
MonsterPresentationFrame
        ↓
MonsterRenderWorld
        ↓
GPU VAT instancing
```

而不是：

```text
MonsterWorld
        ↓
Monster GameObject / Transform / Collider
        ↓
Weapon、Physics、其他系统读取表现对象
```

### 2.2 当前仍然存在的表现对象依赖

静态检查发现，玩法层已经没有必要依靠怪物表现对象，但表现服务自身仍然依赖 GameObject：

1. `MonsterViewRuntimeService` 通过 `LocalObjectPool` 租借怪物 Prefab。
2. `MonsterViewWorld` 保存 `IMonsterViewHandle[]`，每个 Slot 都对应一个表现句柄。
3. `MonsterViewHandle.Awake` 从 `SkinnedMeshRenderer`、Transform 和 Prefab 组件中生成 `MonsterVATRenderProfile`。
4. `MonsterViewHandle` 关闭 Renderer，但对象本身仍然存在并由对象池管理。
5. `BenchmarkRuntimeCoordinator` 和 `BenchmarkRuntimeRecorder` 仍通过 `FindObjectOfType<MonsterViewRuntimeService>()` 以及 `ActiveViewCount` 验证表现是否就绪。

这些依赖不是玩法计算依赖，而是表现资源初始化、生命周期和测试门禁依赖。它们必须先迁移，再删除 GameObject。

## 3. 无 GameObject 时如何实例化外形

无 GameObject 不等于没有实例化外形，而是把“实例”从 Unity 对象实例改成 GPU Instance 数据。

### 3.1 将 Prefab 上的静态配置烘焙成 Render Profile

当前 `MonsterViewHandle.Awake` 读取的内容大部分是静态数据：

- `MonsterVATAsset`；
- VAT Material；
- 原始材质；
- 根节点缩放；
- Renderer 到根节点的局部矩阵；
- 动画交叉淡入时间；
- 移动判定阈值；
- 受击闪烁时间和颜色；
- 阴影模式、接收阴影和 Layer。

这些数据不应该在运行时通过 `GetComponentInChildren` 从每个 Prefab 实例读取，而应该在编辑器或资源构建阶段生成：

```text
MonsterView Prefab
        ↓ 编辑器烘焙
MonsterRenderProfileAsset / MonsterRenderProfileCatalog
        ↓ 运行时按 ConfigIndex 查表
MonsterVATRenderProfile
```

可以保留现有 `MonsterVATRenderProfile` 结构，但将其来源从 `MonsterViewHandle.Awake` 改为 Catalog。Catalog 中每种怪物只保留一份 Profile，所有相同外形的怪物共享 Mesh、Material、VAT 纹理和动画片段。

### 3.2 Slot 只保存渲染状态

替代 `IMonsterViewHandle[]` 的是渲染世界内部的 Slot 状态数组：

```text
MonsterRenderWorld
 ├─ Active[]
 ├─ GroupIndex[]
 ├─ Position[]
 ├─ Yaw[]
 ├─ PreviousPosition[]
 ├─ SourceClip[]
 ├─ TargetClip[]
 ├─ DesiredLocomotionClip[]
 ├─ SourceTime[]
 ├─ TargetTime[]
 ├─ TransitionElapsed[]
 ├─ TransitionDuration[]
 ├─ AttackPlaying[]
 └─ HitFlashRemaining[]
```

这些数据只描述“这个 Slot 应该如何绘制”，不描述任何 Unity 对象。

### 3.3 生成外形

Spawn 时不再执行 `Instantiate`、`Rent` 或 `GetComponent`，而是：

1. 根据 `ConfigIndex` 查找 `MonsterVATRenderProfile`；
2. 为 Slot 写入 Position、Yaw 和 GroupIndex；
3. 初始化 Idle 动画状态；
4. 将 Slot 加入对应 RenderGroup 的索引列表。

Render 时：

1. 遍历每个 RenderGroup 的活跃 Slot；
2. 生成每个 Slot 的 TRS 矩阵；
3. 计算 VAT 当前帧、下一帧和混合权重；
4. 写入批量数组；
5. 调用 `Graphics.DrawMeshInstanced`。

因此，外形仍然由 Mesh、Material 和 VAT 纹理组成，只是不再通过每个怪物的 Renderer 组件提交。

## 4. 无 GameObject 时如何播放动画

当前 VAT 动画已经不依赖 Animator。`MonsterVATRenderWorld` 维护每个 Slot 的：

- SourceClip；
- TargetClip；
- SourceTime；
- TargetTime；
- TransitionElapsed；
- TransitionDuration；
- Blend。

这些字段可以直接保留并移动到无 GameObject 的 RenderWorld 中。

每帧只需要更新数据：

```text
移动状态变化 → Idle/Walk 目标 Clip
攻击序列变化 → Attack Clip
受击序列变化 → HitFlashRemaining
时间推进     → SourceTime / TargetTime
```

真正的顶点动画由 VAT Shader 根据：

- Position Texture；
- Normal Texture；
- CurrentFrame；
- NextFrame；
- Blend；

在 GPU 上完成。没有 Animator、没有 AnimatorStateMachine、没有每只怪物的 Renderer，也不需要为每只怪物创建动画对象。

## 5. 当前表现层与无 GameObject 表现层对比

| 项目 | 当前表现层 | 无 GameObject 表现层 |
|---|---|---|
| 每只怪物 | 一个池化 GameObject | 一个 Slot 状态记录 |
| 位置更新 | `MonsterViewWorld` 找到 View，再调用 VAT 状态 | 直接写入 Position/Yaw 数组 |
| 外形资源 | 从 Prefab/Handle 获取 | 从 Profile Catalog 查表 |
| 动画状态 | VAT 状态，但挂在 RenderWorld | VAT 状态直接挂在 RenderWorld |
| 绘制 | 已经是 `Graphics.DrawMeshInstanced` | 仍然是批量 Instanced 绘制 |
| Spawn | Pool Rent、BindSlot、GetComponent 间接成本 | 写入 Slot 和 GroupIndex |
| Despawn | Return GameObject、关闭 View | 清理 Active/Group 状态 |
| Transform | 每个怪物一个 Transform | 无怪物 Transform |
| Collider | 标准怪物已经移除 | 不变，继续由空间查询负责命中 |
| 网络和玩法 | 通过表现快照同步 | 仍然通过表现快照同步 |

重点是：当前绘制已经批量化，所以去 GameObject 后 DrawCall 不会自动大幅下降。收益主要来自对象生命周期、Transform、组件管理、内存和表现层同步间接调用的减少。

## 6. 预计可以优化多少

不能只根据代码静态估算最终收益，应分成四类测量。

### 6.1 DrawCall

预计收益接近于零。

当前 `MonsterVATRenderWorld` 已经按照 RenderGroup 和 256 个实例一批调用 `Graphics.DrawMeshInstanced`。删除 GameObject 不会自动把已经存在的批量绘制再减少一次。

如果后续把批次数组扩大、改成 `Graphics.RenderMeshInstanced` 或 BatchRendererGroup，才可能继续降低提交开销，但这属于第二阶段渲染优化，不是消灭 GameObject 的直接收益。

### 6.2 Host/Client 每帧 CPU

可能减少，但不能先给出固定毫秒数。

当前表现层仍然存在：

- `MonsterViewWorld.ApplyReplica` 的 Slot 遍历；
- `_views[slot]` 查找；
- View 句柄接口调用；
- `MonsterVATRenderWorld.SetPose` 的间接路径；
- 对象状态和池化对象生命周期检查。

无 GameObject 后可以把这些收敛为一次直接的 RenderState 数组更新。收益大小取决于：

- 是否每帧同步 5012 个 Slot；
- 是否只有部分怪物处于可见范围；
- 是否存在频繁 Spawn/Despawn；
- 当前 Profiler 中 `Monster.Presentation.Apply` 和 `Monster.VAT.Update` 的实际比例。

由于旧架构最大的 `Monster.View.ApplyAuthority` 已经被移除，所以表现层去 GameObject 化不应被宣传为一定能解决 Host 单帧个位数问题。它更像是进一步清理表现开销，为后续 Burst/Job/ECS 提供干净边界。

### 6.3 内存

这是最明确的收益之一：5012 个怪物不再各自持有 GameObject、Transform、MonoBehaviour 和 Renderer 层级。

但实际节省量取决于 Prefab 层级和组件数量，不能从当前源码准确推出。应使用 Unity Memory Profiler 对比以下两种场景：

```text
5012 个池化 MonsterViewHandle
vs
5012 个 MonsterRenderWorld Slot
```

需要单独记录：

- GameObject 数量；
- Transform 数量；
- Renderer/MonoBehaviour 数量；
- Managed Heap；
- Native Object 内存；
- Render Texture、Mesh、Material 是否重复。

外形资源不会按怪物数量复制，因为两种方案都应该共享 Mesh、Material 和 VAT 资产。

### 6.4 Spawn/Despawn 峰值

这是无 GameObject 方案最可能出现明显收益的场景。

当前 Spawn 需要：

- 从对象池 Rent；
- 设置实例状态；
- `GetComponent<MonsterViewHandle>`；
- Bind Slot；
- 生成 VAT Profile；
- 写入 View 和 VAT 映射。

无 GameObject 后只需要写入 Slot 状态并加入 RenderGroup，避免了对象池操作和对象层级状态恢复。即使稳定 5012 怪时每帧收益不大，在怪物批量生成、批量死亡和波次切换时也会更稳定。

## 7. 建议的迁移路径

### 阶段 A：先增加无 GameObject 表现后端，不删除旧路径

新增一个表现后端选择：

```text
MonsterPresentationBackendKind.GameObjectPool
MonsterPresentationBackendKind.InstancedVat
```

两者共同消费 `MonsterPresentationFrame`，不改变网络、玩法和战斗逻辑。

目标是让同一份快照能够分别驱动：

- 旧的 `MonsterViewWorld`；
- 新的 `MonsterRenderWorld`。

### 阶段 B：抽离 Render Profile

将 `MonsterViewHandle.Awake` 中的资源读取逻辑变成编辑器烘焙或资源构建步骤。

运行时不再依靠：

- Prefab 实例；
- `SkinnedMeshRenderer`；
- `GetComponentInChildren`；
- Transform 局部矩阵临时计算。

需要保留一个旧路径作为校验工具，直到 Profile Catalog 与旧 Prefab 绘制结果一致。

### 阶段 C：实现无 GameObject RenderWorld

把现有 `MonsterVATRenderWorld` 的 InstanceState 直接扩展成独立 RenderWorld：

- 移除 `IMonsterViewHandle[]`；
- 以 `ConfigIndex → RenderProfile` 替代 `Slot → ViewHandle → RenderProfile`；
- Spawn/Despawn 只操作数组和 Group 列表；
- ApplyReplica 直接更新 RenderWorld；
- 继续使用现有 VAT Shader 和批量绘制路径。

### 阶段 D：迁移测试和诊断依赖

当前自动化测试仍有表现层门禁，需要改成：

- Replica SlotCount/ActiveCount；
- RenderWorld SlotCount/ActiveCount；
- RenderGroup 活跃实例数；
- PresentationFrame Applied 计数。

不能继续依赖 `FindObjectOfType<MonsterViewRuntimeService>()` 判断每只怪物是否通过 GameObject 创建。

### 阶段 E：A/B 量化后删除旧路径

只有以下条件同时满足，才删除对象池和 `MonsterViewHandle`：

- 旧、新后端的表现快照计数一致；
- 位置、朝向、攻击、受击表现一致；
- VAT 当前帧、下一帧和淡入结果一致；
- Host/Client 双端压测通过；
- Spawn/Despawn、重连、房间结束无泄漏；
- Memory Profiler 证明 GameObject 数量确实归零；
- Benchmark readiness/recorder 已经改为新后端判定。

## 8. 与 `Physics.SyncTransforms()` 的关系

删除怪物表现 GameObject 后，不能直接据此删除 `Physics.SyncTransforms()`。

`Physics.SyncTransforms()` 是否可以取消，取决于整个 Tick 中是否还有任何 Physics 查询依赖 Transform：

- 玩家碰撞体；
- 近战 Physics 查询；
- 尚未迁移的子弹 Collider；
- 特效或其他玩法对象的 Transform 驱动碰撞。

正确顺序是：

1. 先确认怪物不再依赖 Collider/Transform；
2. 再迁移或删除子弹 Collider；
3. 审计近战和其他 Physics 查询；
4. 将 `Physics.SyncTransforms()` 改为按真实消费者条件执行；
5. 最后量化是否可以完全移除。

表现层去 GameObject 化会减少 Transform 数量，但不自动证明 PhysicsSync 已无语义需求。

## 9. 与后续三种计算后端的关系

无 GameObject 表现层应当先完成，因为它能把表现从计算后端中彻底隔离出来。

最终计算后端仍然可以保持三种可切换策略：

```text
Managed SoA Backend
Burst + Job SoA Backend
ECS/DOTS Backend
```

三种后端只输出：

- Movement Result；
- Damage/Push/Chain Events；
- Replication Changes；
- PresentationFrame 输入数据。

它们不应该知道：

- 怪物是否有 GameObject；
- 使用哪种 VAT Renderer；
- 是否经过对象池；
- 具体使用哪种表现后端。

这样才能分别比较：

```text
Managed SoA + GameObject Presentation
Managed SoA + Instanced VAT Presentation
Burst/Job SoA + Instanced VAT Presentation
ECS/DOTS + Instanced VAT Presentation
```

否则计算后端和表现后端同时变化，量化结果无法归因。

## 10. 推荐执行顺序

```text
统一快照量化
    ↓
记录完整 ProjectileSimulation 与 Presentation 成本
    ↓
抽离 MonsterRenderProfile Catalog
    ↓
新增无 GameObject VAT 表现后端
    ↓
旧表现后端/新表现后端 A-B 对比
    ↓
迁移 Benchmark readiness 与 Recorder
    ↓
删除 MonsterViewHandle、对象池和每怪物 GameObject
    ↓
子弹 Managed SoA
    ↓
重新量化 Monster EntityTick 与 CombatSimulation
    ↓
Burst Kernel
    ↓
Burst + Job
    ↓
ECS/DOTS 对照后端
```

## 11. 本阶段结论

可以开始消灭每只怪物的表现 GameObject，但第一步不是直接删除 Prefab，而是把 Prefab 只保留为资源来源，先生成可独立加载的 `MonsterRenderProfile`。

当前 VAT 批量绘制系统已经提供了无 GameObject 的主要渲染基础。迁移后的关键变化是：

- 用 Render Profile Catalog 替代 Prefab 实例读取资源；
- 用 RenderWorld Slot 数组替代 `MonsterViewHandle[]`；
- 用 GPU Instance 替代每怪物 GameObject；
- 用 VAT Shader 参数替代 Animator/Renderer 状态；
- 用快照计数替代 GameObject 数量作为测试门禁。

预期最大收益是对象数量、内存、Spawn/Despawn 峰值和表现层管理复杂度下降；稳定 5012 怪场景的纯 DrawCall 收益预计很小，因为当前已经使用 Instanced VAT。是否能显著降低 Host 帧耗时，必须以 `Monster.Presentation.Apply`、`Monster.VAT.Update`、Spawn/Despawn 和 Memory Profiler 的 A/B 结果为准。
