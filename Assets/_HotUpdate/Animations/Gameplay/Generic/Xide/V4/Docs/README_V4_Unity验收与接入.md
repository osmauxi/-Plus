# 席德 Generic V4 Unity 验收与接入

## 接入结论

- 已按角色隔离规则建立席德专属模型、动画、AvatarMask、Animator Controller、材质和 Gameplay 预制件。
- 原导入包的外层与内层 17 个文件 SHA-256 全部一致；内层重复包已删除，外层资源已迁入正式目录。
- 席德源模型保持 `Scale Factor = 1`，未套用安比/扳机的 Scale7。
- `PlayerRuntimeRoot` 已增加 `CharacterId = 2` 映射，Addressables 地址为 `Character_Gameplay_Xide_Generic`。

## 正式资源位置

- Gameplay 预制件：`Assets/_HotUpdate/Prefabs/Character/Gameplay/Character_Gameplay_Xide_Generic.prefab`
- Controller：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/V4/Xide_Gameplay_V4.controller`
- 基础模型与 Avatar：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/V4/Source/Xide_Game_Generic_Base.fbx`
- 动画源 FBX：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/V4/Source/Animations`
- 运行时动画：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/V4/Clips`
- 专属 AvatarMask：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/Masks/Xide_Game_UpperBody.mask`
- 专属材质：`Assets/_HotUpdate/Animations/Gameplay/Generic/Xide/Materials`
- 旧 Lobby 模型：`Assets/_HotUpdate/Models/Character/Xide/LegacyLobby/Xide_Lobby_Legacy.fbx`
- 旧席德贴图：`Assets/_HotUpdate/Models/Character/Xide/Textures`

旧 Lobby 模型和贴图均通过 Unity GUID 保留移动，`Character_Modle_02.prefab` 的引用没有中断。

## 导入与根骨修复

- 基础模型：`Generic / Create From This Model / Scale Factor 1`，Avatar 有效且非 Humanoid。
- 13 个动画：`Generic / Copy From Other Avatar`，统一引用席德 V4 基础模型 Avatar。
- Unity 动画压缩保持 `Off`，用于保留本批 FBX 内已经完成的关键帧精简结果。
- Aim Idle、四向 Aim Walk、Jog 开启 Loop；Dead、Fire、Hit、ReadyPose、Pivot、Reload、Sprint Start 保持非 Loop。
- 提取后的 14 个运行时动画均包含 2570 个 Transform Binding，缺失骨骼路径为 0。

源包中 ReadyPose 的 `Bip001` 朝向与其他动画不同，Pivot 还包含约 4.891532 的常量根偏移和相反朝向。运行时 `.anim` 已统一为席德角色级根基准：

- Position：`(0, 0, 0)`
- Rotation Quaternion：`(-0.5, 0.5, 0.5, 0.5)`
- Scale：`(1, 1, 1)`

ReadyPose 与 Pivot 使用 `Bip001` 直接子节点逆补偿保留原世界姿势；源 FBX 不修改。Pivot 以 120 Hz 复核，排除被替换的 `Bip001` 后最大世界旋转误差为 0°，最大世界位置误差约为 0.00000103 游戏单位。

## Sprint Loop

- 完整 `Xide_Game_Sprint_Start` 只作为源/回退动画保留，不进入 Animator。
- 对稳定区间进行逐帧比较后，运行时 `Xide_Game_Sprint_Loop` 取席德自己的 41–71 帧，时长 1 秒。
- 最后 2 帧只用于向首帧闭环，不改写中段步态。
- 30 FPS 复核：首尾最大局部旋转差 0°、位置差 0；循环内普通单帧最大旋转约 40.36°，没有额外接缝尖峰。

## Animator

Controller 保留公共参数和状态路径契约，全部 Motion 均替换为席德动画：

- Base：`Idle`、`Move`、`Aim Move`、`Sprint Loop`、`Pivot`、`Hit React`、`Dead`
- UpperBody：`Aim Pose`、`Fire`、`Reload`
- `Aim Move` 使用席德自己的五向 2D BlendTree。
- `Sprint Start` 状态不存在，疾跑进入和持续都由 `Sprint Loop` 管理。
- Controller 对安比/扳机动画和 Mask 的依赖为 0。

编辑器驱动测试已通过：`Idle -> Move -> Sprint Loop -> Pivot -> Sprint Loop -> Idle` 均能按现有参数切换。

## 尺寸、材质与 IK

- 玩家预制件 Root：Position 0、Rotation 0、Scale 1。
- 席德源模型保持真实 1×；Gameplay 仅在 `XideModel` 视觉容器使用角色专属倍率 `2.706335`，使 Idle 渲染高度约为 4.29，与当前安比/扳机一致。
- 全部装备挂点使用反向倍率 `0.369503`，实测 lossyScale 均为 1；武器实测世界 Scale 为 1。
- 新模型 18 个材质槽与旧席德槽序一致，已按索引生成 18 个席德专属材质并继续引用原贴图。
- Generic 左右臂三段骨骼、左右 Elbow Hint、WeaponAimPivot 和 RightHandWeaponRoot 均为席德专属引用。
- 以 02 武器校准后，右手目标距离/臂长约为 `0.8433 / 1.0991`，左手约为 `1.0841 / 1.0991`，双手均在可达范围内。
- 右手 Rotation Offset 为 0；左手 Offset 为席德专属值，未修改共享武器握点。

## 验收结果

- Gameplay 预制件缺失脚本：0。
- `PlayerModelView` 必填引用缺失：0。
- `CharacterAnimationBridge` 必填引用缺失：0。
- 动画 Missing Binding：0。
- Animator 状态契约缺失：0。
- Addressables 实际加载 `Character_Gameplay_Xide_Generic` 成功。
- 旧 Lobby 席德模型引用仍有效，旧 `zzz Model/希德` 路径引用为 0。
- Unity 编译错误：0；验收期间 Console 无角色资源警告或错误。

后续若微调席德动作，只修改席德 V4 的运行时动画或预制件参数；不要修改通用动画代码、共享武器握点，也不要让其他角色复用席德 Motion、Mask 或 IK 校准值。
