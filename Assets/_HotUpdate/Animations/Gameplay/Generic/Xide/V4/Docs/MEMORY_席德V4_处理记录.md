# 席德 Generic V4 处理记忆

这份文件记录从 UE 资产到 Unity Generic 交付包的完整处理路径，供后续角色复用和问题回溯。

## 1. 输入与保护原则

- 输入模型：`/Game/席德/席德`
- 输入 Skeleton：`/Game/席德/席德_Skeleton`
- 动画来源：`/Game/Retargeting/Need` 中既定 13 个 Gameplay 动作
- 源动作标准骨架：`/Game/Characters/Mannequins/Meshes/SKM_Quinn_Simple`
- 不改席德原 FBX、Skeleton 层级、骨骼名称、参考姿势、蒙皮或材质资产。
- 所有重定向设置和动画写入 `/Game/Retargeting/Xide/GenericV4` 独立目录。
- 工作目录与成品目录均拒绝覆盖非空目标，避免误覆盖旧版本。

## 2. 资产预检

1. 读取席德 Skeletal Mesh 所引用的 Skeleton。
2. 核对 Skeleton 共 257 个骨骼。
3. 核对 `Bip001`、Pelvis、Spine、Neck、Head、双臂、双腿、Foot 和 Toe 等核心骨骼全部存在。
4. 核对席德模型包围盒约为 `[-54.0142, -13.2869, -0.0179]` 到 `[54.0142, 20.7552, 153.471]` cm，高度约 153.49 cm。
5. 因原模型已是正常厘米尺度，拒绝 Scale7。Scale7 试验会得到约 1074 cm 高的角色，只保留为内部排错结果，不进入交付包。

## 3. UE 重定向

1. 复制已验证链定义的目标 IK Rig，得到 `IKR_Xide_GenericV4`。
2. 将复制品的 Preview/Target Mesh 改为席德模型。
3. 验证 Retarget Root 为 `Bip001-Pelvis`，Root Motion Bone 为 `Bip001`，链数量不少于 11。
4. 复制已验证的 Mannequin→角色 IK Retargeter，得到 `RTG_Mannequin_Xide_GenericV4`。
5. Source Mesh 指向 Quinn，Target Mesh 和 Target IK Rig 指向席德。
6. 对 `Need` 中 13 个动作执行 Batch Retarget，输出为席德 Skeleton 动画副本。
7. 对每个副本设置：`Enable Root Motion = false`、`Force Root Lock = true`、`Root Motion Root Lock = Anim First Frame`。
8. 重定向资产只使用席德原 Skeleton，不创建替代 Skeleton，不调整骨骼层级或蒙皮。

## 4. UE FBX 导出

- 基础模型单独导出为 `Xide_Game_Generic_Base.fbx`。
- 每个动画单独导出为一个 FBX，关闭 Preview Mesh 和 Morph Target 导出。
- 动画文件只包含 Skeleton 与 Animation，不带 Mesh/Skin/材质/贴图。
- 使用 30 FPS 固定采样和 FBX 2020.2 兼容管线。
- 最初导出属于逐帧 Bake 中间件，不作为最终交付。

## 5. Root 锁定、裁切和循环策略

1. 将 `Bip001` 的 Translation/Rotation 锁定到动作第一帧，Scale 保持 `(1,1,1)`，去除累计的整体位置和旋转变化。
2. 保留动作自身局部骨骼变化，不改写原步态。
3. Aim Idle、Aim Walk Forward、Jog 原周期已足够接近闭合，不做接缝修正。
4. Aim Walk Backward 取源 18–36 帧；Left 取 17–36；Right 取 22–45。
5. Back/Left/Right 只允许最后 2 帧向首帧闭合，禁止把首尾差值分摊到全片。
6. Dead、Fire、Hit、Ready Pose、Pivot、Reload 保留既定完整有效区间。
7. Sprint Start 完整保留 0–77 帧，不截短，不在交付包中附加 Sprint Loop。Unity 侧从该角色自己的稳定区间派生 Loop，候选为 36–66 帧。

## 6. FBX 曲线精简

使用 FBX SDK 曲线优化器直接修改 FBX 数据，而不是依赖 Unity Importer：

- Translation 容差：0.005 局部 FBX 单位
- Rotation 容差：0.05°
- Scale 容差：0.00001
- 常量曲线：1 个键
- 动态曲线：保留满足逐帧世界姿势误差所需的最少键

总采样键由 1,787,949 降至 84,244，减少 95.288%。所有最终动画哈希均不同于逐帧 Bake 中间文件。

## 7. 验收方法

1. 对最终动画与未做全片闭环改写的 1× 密集参考逐帧采样。
2. 用完整父子层级累计变换，比较每根骨骼的世界旋转和世界位置。
3. 普通帧阈值：世界旋转 ≤0.5°、世界位置 ≤0.2 cm。
4. Back/Left/Right 只在误差统计中排除被明确授权的最后 2 个接缝帧；其余全部帧必须过线。
5. 单独检查循环首尾差，确认 Aim Idle、四向 Aim Walk、Jog 无明显接缝尖峰。
6. 验证基础模型为 257 骨骼、1 Mesh、1 Skin、257 Cluster；动画为 257 骨骼、0 Mesh、0 Skin、单 Animation Stack。
7. 验证每个动画的 `Bip001` T/R/S 各为 1 个常量键，Scale 为 1，无累计漂移。
8. 检查交付包严格为 1 模型 + 13 动画，不含 `Sprint Loop`、Scale7 试验文件或密集中间文件。

## 8. Unity 接入规则

- 模型：Generic / Create From This Model / Scale Factor 1。
- 动画：Generic / Copy From Other Avatar，引用席德 V4 模型 Avatar。
- Apply Root Motion 关闭，由 Gameplay 代码驱动位置和朝向。
- 首轮关闭 Importer Keyframe Reduction，以验证 FBX 自身精简结果。
- 只给 Aim Idle、四向 Aim Walk、Jog 开启 Loop Time。
- Sprint Loop 必须由席德 StartForward 自己生成，不写死到共享逻辑，不复用安比或扳机的 Loop。
- 重点检查 Missing Binding、手部武器握持、脚底接触、Pivot 时机及状态切换。

## 9. 常见失败与回避

- 不能仅复制旧 FBX 后依靠 Unity Keyframe Reduction；必须确认文件哈希和 FBX 内曲线真的变化。
- 不能把闭环差值摊到整段动画；这会重写中段步态，产生类似旧 V4 的几十度偏差。
- 不能为满足循环而截短完整 StartForward，也不能额外交付未经接入验证的 Sprint Loop。
- 不能因为其他角色需要 Scale7 就对席德机械套用相同倍数；先测原始包围盒和厘米尺度。
- 不能修改骨架层级、参考姿势或蒙皮去“修好”动画；若 IK Retargeter 无法解决，应另开 DCC 源模型处理流程并保留原件。

