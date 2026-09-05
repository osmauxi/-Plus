# 扳机 Generic V4.1 Unity 验收与接入

## 结论

V4.1 于 2026-08-28 验收通过，并已替换扳机 Gameplay 预制件的运行时 Controller。V1 保留为回退版本以及基础模型 Avatar 来源；未通过的 V4 已删除。

## 当前资源

- 源动画：`Source/Animations`，共 13 个 FBX。
- 提取动画：`Clips`，共 14 个 `.anim`，其中 Sprint Loop 为 Unity 接入阶段派生资源。
- Controller：`Trigger_Gameplay_V4_1.controller`。
- Gameplay 预制件：`Assets/_HotUpdate/Prefabs/Character/Gameplay/Character_Gameplay_Trigger_Generic.prefab`。
- Addressables 地址：`Character_Gameplay_Trigger_Generic`。
- Avatar：继续使用 `Generic/Trigger/V1/Source/Trigger_Game_Generic_Base_Scale7.fbx` 中的扳机 Avatar；这只是同角色骨架来源，不是复用 V1 动画。

## 源数据验收

- 13 个 FBX 的 SHA-256 与 V4.1 清单全部一致，且均不同于 V1。
- 每个动画只有 1 个 Clip、99 个动画路径，Missing Binding 为 0。
- 关闭 Unity Keyframe Reduction 后合计 91,646 个曲线键；除两帧 Ready Pose 外，没有旧版“所有曲线逐帧满键”的问题。
- 13 个动作直接对比未经闭环改写的 V1 原动作全部通过：最大世界旋转误差约 0.256 度，最大世界位置误差约 0.00124 游戏单位，低于 0.5 度 / 0.002 的验收线。
- V4 曾出现的 Jog、Aim Walk、Sprint 全片姿势扭曲没有在 V4.1 复现。

## Sprint Loop 生成

- 导出包只提供完整 `Game_Sprint_StartForward.fbx`，范围为 0–77 帧。
- 对扳机 V4.1 分析后的最佳稳定同相位候选为 36–66 帧。
- Unity 从该区间生成 `Trigger_Game_Sprint_Loop.anim`，30 FPS、1.000 秒、Loop Time 与 Loop Pose 开启。
- 完整 StartForward 同时保留为非循环源/回退资源；Animator 已删除 `Sprint Start` 状态，进入与持续疾跑统一使用 `Trigger_Game_Sprint_Loop.anim`。
- Animator 跨接缝采样的最大单帧骨骼旋转为普通帧中位值的 1.06 倍，没有额外接缝尖峰。
- Aim Idle、四向 Aim Walk、Jog 与 Sprint Loop 的 Animator 接缝步进/普通帧中位值之比均不超过 1.14；Jog 为 0.98，Sprint Loop 为 1.06。

36–66 只属于扳机 V4.1 的角色版本数据。后续角色必须根据自己的 StartForward 重新分析稳定区间；不得把此帧段写入共享运行逻辑，也不得复用扳机的 Sprint Loop。

## 运行链路验收

- Controller：2 层、11 个状态、15 个动画引用，V1 Motion 引用为 0。
- 预制件：缺失脚本 0，Avatar 有效，`TriggerModel` Rotation 为 0，角色 Root Scale 为 1。
- 尺寸：同一 Idle 采样下，安比/扳机渲染包围盒高度约为 4.297 / 4.283，匹配。
- Addressables：`Character_Gameplay_Trigger_Generic` 实际加载成功。
- Animator：Idle、Move、Sprint Loop、Aim Move、Pivot、Hit、Dead、Aim Pose、Fire、Reload 全部可进入，无异常 Transform；`Sprint Start` 节点已删除。
- Pivot：最低触发速度提高到 4 m/s，并在 `MotionPhase.Start` 阶段强制禁止；起步换向不会触发 Pivot。
- Console：0 Error / 0 Warning。

## 后续微调边界

Animator 状态机拓扑与 Motion 引用已经接通。后续若要微调扳机动作细节，应只修改 V4.1 的角色专属 Clip 或 Controller 参数，不修改安比动画、不让其他角色适配扳机动画，也不把扳机的帧段、骨骼路径或尺寸补偿写入通用 Gameplay 代码。

## 2026-09-05 Generic IK 引用修复

- 运行场景复查发现扳机 `CharacterAnimationBridge` 的 `WeaponAimPivot`、左右上臂/前臂/手和两个 Elbow Hint 序列化引用为空。节点本身存在，但 `HasGenericRightHandIKBones` 与 `HasGenericLeftHandIKBones` 因此恒为 false，Generic LateUpdate 双骨骼 IK 不会执行。
- 上述 9 个引用已重新绑定到扳机预制件自己的骨架和辅助节点；未修改通用 IK 代码，也未写死扳机骨骼路径到运行时逻辑。
- `UpperBody Aim` 层曾残留引用安比 AvatarMask，现已改为 `Trigger_Game_UpperBody.mask`。
- Controller 内两个未被任何状态引用的旧 BlendTree 子资源已经移除，安比资源依赖为 0。
- Addressables 实例化后 `HasGenericRightHandIKBones` 与 `HasGenericLeftHandIKBones` 均为 true。独立 Play Mode 测试使用 Weapon00：左右 IK 权重均达到 1.000，右手到 MainHandGrip 的位置误差约 0.00004，左手到 OffHandGrip 的位置误差为 0。
