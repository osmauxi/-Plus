# 扳机全量动画重新导出要求

## 一、重导目标

本次重新导出扳机的全部动画，目标同时解决：

1. 所有骨骼、所有属性、所有帧都被写入关键帧，导致 FBX 和 Unity `.anim` 体积过大、导入和 Animation 窗口加载缓慢。
2. 大量 Position、Scale 和未参与动作的骨骼曲线完全不变化，却仍然每帧保存相同数值。
3. Jog 当前在 Unity 中补循环时产生 21→22 帧姿势跳变，需要在动画源中制作真正闭合的循环。
4. Sprint Loop 不是默认导出文件；接入时由该角色自己的 StartForward 稳定疾跑区间生成。StartForward 必须保留足够的稳定循环帧，不能只导出起步瞬间。
5. 每个动画只保留实际有效时间范围，不保留无用途的静止尾段或为了补时长而复制的整段满键数据。

本次只重导动画。当前修正后的扳机基础模型、骨架层级、材质和 Gameplay 预制件不需要重导或修改。

## 二、已确认的数据问题

### 1. 全量逐帧 Bake

当前每个原始动画 FBX 都包含 892 条标量曲线，并且每条曲线在每个采样帧都有键。

以 Jog 为例：

- 30 FPS，47 帧。
- 892 条曲线，共 41,924 个键。
- 729 条曲线从头到尾数值完全不变，但仍然各自保存 47 个重复键。

当前全部 14 个 Unity 扳机 Clip 合计约 808,155 个关键帧、110.98 MB。安比 V3 的 14 个 Clip 合计约 78,264 个关键帧、31.19 MB。

这不是骨架损坏，但属于不合格的动画交付优化状态。若沿用相同方式从 UE 直接逐帧 Bake，再导出一次，问题会原样保留。

### 2. Jog 拼接跳变

当前 `Trigger_Game_Jog_Forward.anim` 的内容对应关系：

- Unity 0–21 帧对应原始 FBX 6–27 帧。
- Unity 22–30 帧对应原始 FBX 14–22 帧。
- 21→22 帧实际是原始 27→14 帧的直接跳转。

该边界最大单骨旋转变化约 59.1 度；正常相邻帧约 18–28 度。Pelvis 变化约 7.2 度、Spine1 约 9.0 度、Neck 约 5.9 度，因此会产生身体倾斜跳变和腿部抽动。

原始 FBX 的 27→28 帧连续。此问题来自补循环，不是原动画自身的连续帧错误。

## 三、必须重导的动画清单

| 文件名 | 当前帧数 | 类型 | 重导要求 |
| --- | ---: | --- | --- |
| `Game_Aim_Idle_ADS.fbx` | 241 | 循环 | 只保留一个完整、闭合的瞄准 Idle 周期；若 8 秒内容有重复或静止区间，裁掉无效部分。 |
| `Game_Aim_Walk_Backward.fbx` | 37 | 循环 | 首尾步态、骨盆高度和速度闭合。 |
| `Game_Aim_Walk_Forward.fbx` | 47 | 循环 | 首尾步态、骨盆高度和速度闭合。 |
| `Game_Aim_Walk_Left.fbx` | 38 | 循环 | 首尾步态、骨盆高度和速度闭合。 |
| `Game_Aim_Walk_Right.fbx` | 46 | 循环 | 首尾步态、骨盆高度和速度闭合。 |
| `Game_Dead_StandToCrouch.fbx` | 74 | 单次 | 只保留从站立到死亡最终姿势的有效动作；最终保持姿势只需终点键，不要追加逐帧静止尾段。 |
| `Game_Fire_Shotgun.fbx` | 21 | 单次 | 保留完整开火和后坐力恢复；首帧需能从 Aim Idle 平滑进入。 |
| `Game_Hit_FrontHeavy.fbx` | 25 | 单次 | 保留受击和恢复有效区间，不追加静止尾段。 |
| `Game_Idle_UnarmedReadyPose.fbx` | 2 | 静态姿势 | 静态轨道最多保留一个起始键和必要终点键；不得为每根骨骼生成无意义逐帧重复键。 |
| `Game_Move_JogForward.fbx` | 47 | 循环 | 重新制作完整闭合 Jog，不使用当前 27→14 的直接拼接。 |
| `Game_Pivot_Turn180Left.fbx` | 108 | 单次 | 保留完整 Pivot 动作；开头、转身完成点和结束恢复区间清楚，不追加无效尾段。 |
| `Game_Reload_Rifle_Additive.fbx` | 67 | 单次 | 保留完整换弹；没有变化的骨骼曲线必须精简为单键或最少键。 |
| `Game_Sprint_StartForward.fbx` | 78 | 单次起步 | 只负责从运动进入疾跑的起步段，不把重复循环和静止尾段混在同一个 Clip。 |

接入阶段派生资源（不要求动画导出包提供）：

| 文件名 | 类型 | 要求 |
| --- | --- | --- |
| `Trigger_Game_Sprint_Loop.anim` | 循环 | 由该角色 `Game_Sprint_StartForward.fbx` 的稳定疾跑区间提取并闭合；范围必须按角色动作分析，不能在通用逻辑中写死，也不能直接把完整 StartForward 设置为 Loop。 |

动画导出包应交付上述 13 个 FBX；Sprint Loop 由 Unity 接入流程生成并作为角色专属 `.anim` 保存。

## 四、骨架与动画内容契约

1. 使用当前扳机修正模型所对应的同一个 Skeleton、Retarget 设置和参考姿势。
2. 不得新增、删除、重命名或调整任何骨骼层级。
3. 每个 FBX 的全部动画节点路径必须与基础模型严格对应。
4. 保持 30 FPS 固定采样率。
5. 当前 Gameplay 不使用动画 Root Motion。Bip001 不得产生累计的世界位移或旋转。
6. 保持现有 Scale7 契约：Bip001 Scale 全片恒定为 7，且三个轴一致；其他骨骼 Scale 通常恒定为 1。
7. Bip001 的轴向和根基准必须与当前修正模型以及其他全部扳机动画一致。
8. 单动画单 FBX；动画文件只包含 Skeleton 与 Animation，不包含 Mesh、Skin、材质、贴图、武器、碰撞、相机或灯光。
9. FBX 使用与 UE 管线兼容的版本。Epic 当前文档说明 UE FBX 管线使用 FBX 2020.2，避免用不兼容版本造成额外转换。

## 五、关键帧精简要求

### 1. 禁止原样逐帧满轨道交付

最终 FBX 必须满足：

- 完全恒定的曲线只保留一个键；如果导出器要求闭合端点，最多保留首尾两个相同键。
- 未发生位移的骨骼 Position 不得每帧重复写相同值。
- 未发生缩放的骨骼 Scale 不得每帧重复写相同值。
- 未参与动作的骨骼 Rotation 不得每帧重复写相同值。
- 动态曲线只保留满足姿势误差要求所需的关键帧。
- 除非曲线确实逐帧存在不可删除的高频变化，否则不得出现“曲线长度等于动画总帧数”的满键轨道。

### 2. UE 直接导出后的处理

UE 的动画 FBX 导出可能仍会把动画 Bake 到所有关节。若当前 UE 导出工具没有可靠的 Reduce Keys/Curve Simplification 功能，则必须在 Maya、MotionBuilder、Blender 或等价 DCC 中完成关键帧精简后再交付最终 FBX。

推荐流程：

1. 在 UE 中完成重定向、循环修复和有效帧裁切。
2. 从 UE 导出单动画 FBX。
3. 在 DCC 中导入 FBX，保持原骨架层级、局部坐标和采样时间不变。
4. 对 Position、Rotation、Scale 曲线执行 Key Reduction/Curve Simplification。
5. 常量曲线减为单键；动态曲线按误差阈值减键。
6. 再导出最终交付 FBX，禁止再次启用会把所有精简曲线重新逐帧 Bake 的选项。
7. 将最终 FBX 与精简前版本逐帧比较后再交付。

只在 UE 中用完全相同设置重新执行一次 Export，不算完成本次重导要求。

### 3. 精简误差验收

以 30 FPS 对精简前后的动画逐帧采样：

- 任意骨骼世界旋转误差建议不超过 0.5 度。
- 手、脚、骨盆和武器相关骨骼的世界位置误差建议不超过 0.002 个游戏单位。
- Bip001 的 Position、Rotation 和 Scale 必须严格保持当前数据契约，不允许因减键产生累计漂移。
- 手脚接触帧、开火峰值、受击峰值、Pivot 转身完成点、死亡落地点必须保留必要关键帧。
- 关键帧精简不能改变动作时长、事件时刻和循环相位。

## 六、循环动画要求

适用于 Aim Idle、四向 Aim Walk、Jog 和 Sprint Loop：

1. 首尾必须处于相同步态阶段和接触状态。
2. 首尾 Pelvis 高度、身体倾斜、腿部方向、手臂摆动方向一致。
3. 首尾不仅要匹配姿势，还要匹配运动速度和方向。
4. 足部接触地面时不能被过渡拖动，避免滑步。
5. 若需要补过渡，至少使用 2 个 30 FPS 过渡帧；旋转使用最短路径四元数插值。
6. 不要直接对跨越 0/360 度的 Euler 数值做普通线性插值。
7. 是否包含“与首帧完全相同的闭合末帧”必须在交付说明中注明，Unity 端会据此设置 Loop 接缝。

Jog 当前 raw27→raw14 的拼接不允许保留。应在动画源中重新选择相同步态边界或重新制作闭合周期。

## 七、非循环动画要求

适用于 Dead、Fire、Hit、Pivot、Reload 和 Sprint Start：

1. 首帧必须能从对应 Gameplay 状态自然进入。
2. 动作结束后只保留必要的恢复或最终姿势，不追加无意义静止帧。
3. Fire/Reload 与 Aim Idle 的上半身基准一致。
4. Pivot 的转身主体和完成时刻清楚，不能把多余原地动作混入尾段。
5. Sprint Start 的运行时 Clip 只包含起步；导出的 StartForward FBX 仍需包含可供接入流程提取的稳定疾跑区间，随后生成角色专属 Sprint Loop。
6. Dead 的最终姿势可以保留终点键，但不需要用大量重复键维持。

## 八、交付包结构

建议新建独立包，不覆盖当前正在测试的 V1：

```text
Trigger_Generic_V2_Optimized_Package/
├─ Animations/
│  ├─ Game_Aim_Idle_ADS.fbx
│  ├─ Game_Aim_Walk_Backward.fbx
│  ├─ Game_Aim_Walk_Forward.fbx
│  ├─ Game_Aim_Walk_Left.fbx
│  ├─ Game_Aim_Walk_Right.fbx
│  ├─ Game_Dead_StandToCrouch.fbx
│  ├─ Game_Fire_Shotgun.fbx
│  ├─ Game_Hit_FrontHeavy.fbx
│  ├─ Game_Idle_UnarmedReadyPose.fbx
│  ├─ Game_Move_JogForward.fbx
│  ├─ Game_Pivot_Turn180Left.fbx
│  ├─ Game_Reload_Rifle_Additive.fbx
│  └─ Game_Sprint_StartForward.fbx
└─ README_导出信息.txt
```

`README_导出信息.txt` 必须记录：

- UE 版本。
- FBX 版本。
- 使用的 Skeleton 资源路径或唯一名称。
- 每个动画的帧率、起止帧、总帧数。
- Loop/Non-Loop 类型，以及 StartForward 中可用于生成 Sprint Loop 的稳定帧范围。
- Loop 的有效循环帧范围。
- 是否包含闭合末帧。
- 使用的关键帧精简工具和误差参数。
- 是否保留 Bip001 Scale7。

## 九、Unity 接收验收

重新导入时先进入独立 V2 目录，不覆盖 V1。通过以下检查后再替换 Controller：

1. 所有动画使用 Generic Rig，并 Copy From Other Avatar 到扳机基础模型 Avatar。
2. 所有骨骼路径完整，不出现 Missing Binding。
3. 常量曲线没有逐帧重复键，动态曲线关键帧数量明显下降。
4. 总关键帧和磁盘体积应显著接近安比量级，不再接近当前 80.8 万键/111 MB。
5. 密集源动画与精简动画逐帧姿势误差通过要求。
6. Jog、Aim Walk、Aim Idle，以及由 StartForward 派生的 Sprint Loop 连续循环无接缝抽动或滑步。
7. Idle、Move、Sprint、Pivot、Aim、Fire、Reload、Hit、Dead 全链路切换正常。
8. Bip001 不累计旋转、位移或缩放漂移。
9. 左右手 IK、武器瞄准和肘部 Hint 不因减键发生明显偏移。
10. Unity Console 无 Avatar、曲线、节点路径或丢失引用错误。

## 十、参考资料

- Epic：FBX Animation Pipeline。官方说明单动画单文件，动画导出时只需要 Skeleton，Mesh 可选：<https://dev.epicgames.com/documentation/en-us/unreal-engine/fbx-animation-pipeline-in-unreal-engine>
- Epic：FBX Content Pipeline。官方说明 UE FBX 管线使用 FBX 2020.2：<https://dev.epicgames.com/documentation/en-us/unreal-engine/fbx-content-pipeline>
- Epic：FBX Import Options Reference。可使用 Exported Time、Animated Time 或 Set Range 控制有效动画范围：<https://dev.epicgames.com/documentation/unreal-engine/fbx-import-options-reference-in-unreal-engine>

## 十一、2026-08-28 新导入包验收记录

本次新导入位置：

- 模型：`Assets/_HotUpdate/Animations/Gameplay/Trigger_Game_Generic_Base_Scale7.fbx`
- 动画：`Assets/_HotUpdate/Animations/Gameplay/Animations 1/`

验收结果：不通过，未替换现有 V1。

原因：

1. 新模型与 V1 Source 模型的 SHA-256 完全一致。
2. 新目录中的 13 个动画 FBX 均与 V1 Source 对应文件逐字节完全一致，不是重新导出的动画数据。
3. 新副本在 Unity 中看起来关键帧较少，是因为默认使用了 `Keyframe Reduction`；FBX 源文件中的逐帧 Bake 数据并未改变。
4. 新模型和动画当前均为 `Generic / NoAvatar`，动画没有 Copy From Other Avatar 到扳机基础模型，不能直接替换正式链路。

需要确认导出的新文件确实复制到了独立版本目录，并至少通过文件哈希、关键帧数据和 StartForward 稳定区间三项检查后，再执行正式替换。Sprint Loop 由接入流程派生，不作为导出包缺失项。

## 十二、2026-08-28 V4 验收记录

V4 源曲线精简通过，但整体动画验收不通过，未替换现有 V1。

通过项：

1. 14 个 FBX 与清单 SHA-256 全部一致，并且均与 V1 不同。
2. 在 Unity 关闭 Keyframe Reduction 后，14 个 Clip 合计约 92,125 个导入曲线键，明显低于旧版；原先“所有曲线每帧满键”的问题已基本修复。
3. 每个动画只有一个 Clip、99 个动画路径，且所有路径都存在于扳机基础模型中。
4. Bip001 Position/Rotation 恒定，Scale 恒定为 7。
5. Dead、Fire、Hit、Ready Pose、Pivot、Reload、Sprint Start 与旧源逐帧对比通过，最大约 0.22 度 / 0.0012 游戏单位。

未通过项：

1. V4 的循环动画虽然首尾姿势数值闭合，但闭环修正影响了整段动作，而不是只处理接缝。
2. Jog 中段骨骼世界旋转相对旧源最大偏差约 97.3 度；Aim Walk Forward 约 69.1 度；Sprint Loop 约 80.7 度。腿、脚、骨盆的原始步态被明显改写。
3. Aim Idle、四向 Aim Walk、Jog 和 Sprint Loop 均超过 0.5 度 / 0.002 游戏单位的原始动作保真验收线。
4. V4 的误差报告比较的是“经过相同闭环处理的高精度参考”，没有与未经闭环改写的原始动画直接比较，因此不能证明原动作得到保留。
5. V4 将 `Game_Sprint_StartForward.fbx` 截短为 0–36 帧，并额外交付 Sprint Loop；这不符合当前规则。后续应保留 StartForward 中足够的稳定疾跑区间，由 Unity 接入流程生成角色专属 Sprint Loop。

下一版修正要求：

1. 关键帧精简方案保留。
2. 循环动作不能把首尾差值分摊到全片；优先重新选择真实同相位周期，只允许在接缝附近极少量帧内处理。
3. Jog 若需要补间，按当前要求只在接缝插入约 2 个 30 FPS 过渡帧，并检查脚底接触，不能改变整段腿部动作。
4. 优化版本必须直接与未经闭环改写的原始动画逐帧比较；除明确允许修改的接缝帧外，旋转误差不超过 0.5 度、位置误差不超过 0.002 游戏单位。
5. 导出包只提供完整 StartForward，不要求提供 Sprint Loop；稳定区间及派生方法写入说明。

## 十三、2026-08-28 V4.1 验收与替换记录

V4.1 验收通过，已替换 Gameplay 扳机预制件的运行时 Controller。V1 保留为回退版本和基础模型 Avatar 来源；未通过的 V4 包已删除。

通过项：

1. 13 个 FBX 的 SHA-256 与清单全部一致，且均不同于 V1；包内没有额外交付 Sprint Loop，完整 StartForward 保留 0–77 帧。
2. 所有动画均为 99 个有效节点路径、0 Missing Binding；Bip001 Scale 全程保持 7。
3. 关闭 Unity Keyframe Reduction 后，13 个源 Clip 合计 91,646 个曲线键。除只有两帧的 Ready Pose 外，没有“全部曲线逐帧满键”的旧问题。
4. 直接与未经闭环处理的 V1 原动作逐帧比较，13 个动作全部通过 0.5 度 / 0.002 游戏单位验收线；全组最大值约为 0.256 度 / 0.00124 游戏单位。
5. Back、Left、Right 的首尾姿态已闭合；Aim Idle、Aim Walk Forward、Jog 保留自然周期，没有复现 V4 的全片扭曲。
6. 从 StartForward 的扳机专属稳定区间 36–66 帧生成 `Trigger_Game_Sprint_Loop.anim`，时长 1 秒。Animator 跨接缝最大骨骼步进与普通帧中位值之比为 1.06，没有接缝尖峰。
7. 新 Controller 为 `Assets/_HotUpdate/Animations/Gameplay/Generic/Trigger/V4_1/Trigger_Gameplay_V4_1.controller`，2 层、11 状态、15 个动画引用，V1 Motion 引用数为 0。
8. Gameplay 预制件缺失脚本为 0，Avatar 有效；Addressables 地址 `Character_Gameplay_Trigger_Generic` 实际加载成功。
9. 当时版本的 Idle、Move、Sprint Start、Sprint Loop、Aim Move、Pivot、Hit、Dead、Aim Pose、Fire、Reload 均能由 Animator 实际进入，未发现 NaN/Infinity 或异常缩放；后续当前版本已合并并删除 Sprint Start 状态。
10. 运行测试 Unity Console 为 0 Error / 0 Warning；测试完成后恢复 `BootStrapScene`。

角色通用规则仍然是：Sprint Loop 必须从该角色自己的 StartForward 稳定步态生成，帧段属于角色版本接入数据，不得写死到共享 Gameplay 逻辑，也不得让其他角色复用扳机的 Loop。

## 十四、Sprint 状态合并与 Pivot 起步过滤

- 安比 V3 与扳机 V4.1 Controller 均删除 `Sprint Start` 节点，所有原进入 Start 的过渡直接重定向到角色专属 `Sprint Loop`。
- StartForward FBX 与提取 Clip 继续作为生成 Loop 的源/回退资源保存，但不再参与运行时 Animator。
- 全局 Pivot 最低速度由 1.25 m/s 提高到 4 m/s。
- `PlayerMotor` 在 `MotionPhase.Start` 阶段明确拒绝 Pivot；起步时反复切换方向只改变速度方向，进入稳定 Move 后才允许急转。
