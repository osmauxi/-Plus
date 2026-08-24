# 玩家角色模型与动画接入记录

本文记录安比 Generic V3 从 UE 导出、Unity 导入到 Gameplay 运行的修复，以及后续角色可以复用和必须单独处理的部分。

## 一、安比当前已完成的修改

### 1. 资源隔离与运行时映射

- Gameplay 模型使用独立预制件 `Character_Gameplay_Anbi_Generic.prefab`。
- 动画使用安比独立 Controller：`Generic/Anbi/V3/Anbi_Gameplay_V3.controller`。
- `PlayerAppearanceController` 按 CharacterId 映射模型地址和 Controller；没有在通用动画代码中引用安比动画。
- 模型、动画和 Lobby 资源互相隔离，后续角色应建立自己的目录、预制件和 Controller。

### 2. Generic Avatar 与动画导入

- 基础模型 Rig 使用 `Generic / Create From This Model`，作为唯一 Avatar 来源。
- 动画 FBX 使用 `Generic / Copy From Other Avatar`，Source 指向该角色基础模型 Avatar。
- 动画 FBX 保持单动画单 FBX，不包含 Mesh、Skin、材质。
- Animator 禁用 Apply Root Motion；玩家位置和旋转只由 Gameplay Simulation/CharacterController 驱动。

### 3. 根骨修复

- 所有剪辑必须使用相同的 Bip001 根基准，不允许每个动画用自己的首帧方向作为基准。
- 安比 Idle、Pivot 曾与 Jog/Aim 的根方向不一致，已在提取出的 `.anim` 中统一根基准。
- 修正根骨时，对 Bip001 直接子节点应用逆补偿，保证世界姿势不因根基准调整而改变。
- 玩家预制件 Root 保持 Position 0、Rotation 0、Scale 1；动画不能写玩家 Root。

### 4. 尺寸与挂点

- 安比 Scale7 包的玩家预制件 Root 保持 Scale 1，Bip001/模型内容保留资源自身比例。
- 右手、左手、背部、胸部、左右髋挂点按模型比例补偿，使挂点最终 lossyScale 为 1。
- 不在玩家根节点缩放角色，否则玩家子节点、武器和 Gameplay 组件会继承缩放。
- 当前安比仍未包含正式 Texture/Material；贴图不属于本次动画修复。

### 5. Animator 结构

Base Layer 当前状态：

- `Idle`：普通站立和瞄准站立共用 Base Idle；瞄准上半身由专用层覆盖。
- `Move`：直接播放 Jog。
- `Aim Move`：角色专属二维瞄准移动 BlendTree。
- `Sprint Start`：完整播放一次原始 Sprint，不循环。
- `Sprint Loop`：从 Sprint 中提取的真正闭合循环。
- `Pivot`、`Hit React`、`Dead`。

关键规则：

- `Base FullBody.Idle` 是表现驱动的公共状态路径契约。
- 松开移动输入时，不等待 Speed 归零或当前过渡结束，固定 0.12 秒 CrossFade 回 Idle。
- Move/Sprint 模式过渡禁止互相反复重置，避免连续点按 Shift 卡在第零帧。
- Pivot 由 Simulation 的强反向事实直接触发，不再要求目标方向仍处于快速旋转 Root 的局部 Backward 象限；最低速度 1.25 m/s、方向点积阈值 -0.15。
- 转入 Pivot 固定过渡 0.2 秒，随后保留 0.1 秒完整 Pivot，再用 0.2 秒过渡回当前 Idle/Move/Sprint。瞄准时不触发 Pivot，下半身继续由 Aim Move 接管。
- Base Layer 开启 IK Pass，供 Humanoid 角色使用；Generic 角色使用自定义双骨骼 IK。

### 6. Sprint 循环修复

- 原 `Anbi_Game_Sprint_Forward` 单次播放正常，但首尾不是同一步态，不能直接循环。
- 原循环接缝右大腿旋转跳变约 90.3 度。
- 原动画改为非循环 `Sprint Start`。
- 从稳定跑步区间提取 `Anbi_Game_Sprint_Loop`，闭合端点后作为持续疾跑循环。
- 新循环 60 Hz 接缝右大腿变化约 5.7 度。

### 7. 玩家移动表现修复

- 普通移动加速度 30、减速度 60，松键滑行显著缩短。
- 最大转速当前为 Free 360、Aim 720、Sprint 420 度/秒。
- 整体转弯侧倾由 12 度降为 3 度；低于 1.5 速度不侧倾，Pivot 期间不侧倾。
- 方向改变使用独立响应倍率：Free 3.2、Aim 2.5、Sprint 2.8。
- 方向倍率只加快速度向量转弯，不改变直线起步和松键刹车。
- 速度低于 3 m/s 时额外提高 Root 转向响应：完全静止时角加/减速度为 3 倍、最大转速为 1.5 倍，接近 3 m/s 时平滑恢复原参数。
- 低速转向加速只处理静止起步和连续换向，高速 Move/Sprint 的转向重量感不变。

### 8. 游戏场景 IK

- 未配置武器 Aim Pivot 的旧 Humanoid 角色继续使用 Unity 原生 `OnAnimatorIK`；配置 Aim Pivot 后与 Generic 共用 LateUpdate 双骨骼求解，避免 IK 读取上一帧的枪械目标。
- 右手锁武器 `MainHandGrip`，左手锁 `OffHandGrip`。
- Generic：`CharacterAnimationBridge` 在 Animator 和表现驱动之后分别执行右、左手双骨骼 IK。
- 安比 Gameplay 预制件已配置左右上臂、前臂、手骨骼引用。
- 武器层级为 `UpperChest/WeaponAimPivot/RightHandWeaponRoot/Weapon`。AimPivot 只由程序控制，RightHandWeaponRoot 继续用于手动调整位置与基础旋转。
- 水平瞄准以 `Muzzle.forward` 为枪管轴，使用同步后的 `AimDirection`；最大追加 Yaw 65 度、追赶速度 900 度/秒。
- Aim 输入仍只有水平轴。绑定武器后先在非瞄准 Idle 自动捕获枪口相对水平 Forward 的完整姿态；瞄准时只替换水平 Forward，保留当前手工调好的 Pitch/Roll，因此不会把带约 90 度基础 Roll 的枪械强制压成平躺状态。
- 角色预制件上的 Aim Pitch/Roll 现在是自动捕获姿态之上的附加校准，安比保持 0；调整 `RightHandWeaponRoot` 后无需再为瞄准姿态复制一套旋转参数。
- 表现顺序固定为 `Animator -> Spine Aim -> Weapon Aim -> Right Hand IK -> Left Hand IK`。
- 握点 Transform 只描述武器物理位置；手腕轴差异保存为角色预制件上的左右手 Rotation Offset，不修改共享武器以迁就某个角色。
- 安比右手基准 Offset 为 0；左手原始握点与骨骼相差约 176.7 度，已记录为安比专属 Offset。右/左旋转权重为 0.8/0.5，最大旋转修正为 55/45 度。
- 安比 UpperChest 下已配置独立 RightElbowHint/LeftElbowHint。有 Hint 时弯肘平面只由 Hint 决定，不再根据 Fire 动画当前肘部翻转符号；无 Hint 的旧角色使用上一帧方向维持半球连续。
- Reload、Hit 时释放左手但保留右手主握；Dead 时双手释放。恢复正常状态后权重重新渐入。
- 若武器仍是右手骨骼子节点，右手 IK 会自动禁用，避免循环依赖造成逐帧漂移。
- 武器 `WeaponView.OffHandGrip` 必须位于手臂可达范围内。当前测试武器在不同动作下超出安比左臂最大伸展距离约 0.40 米，求解器会安全夹到最大可达位置；后续正式武器需要重新制作握点。

## 二、后续角色可以直接复制的内容

可以复制安比 Controller 作为角色专属 Controller 模板，并保留：

- Layer、参数、状态名和状态机拓扑。
- Idle/Move/Aim Move/Sprint Start/Sprint Loop/Pivot/Hit/Dead 的职责划分。
- 过渡条件和公共时间设置。
- `Base FullBody.Idle` 状态路径。
- UpperBody Aim 层的职责、Fire/Reload 触发逻辑。
- `HasMoveInput`、`Speed`、`VelocityX/Z`、`LocomotionMode`、`IsPivoting`、`IsDead`、`IsHitReacting`、`IsReloading`、`Shoot` 等参数契约。

复制后必须把所有 Motion 替换成该角色自己的动画。不得让其他角色复用或适配安比动画。

## 三、每个新角色必须单独处理

### 模型预制件

- 新建角色自己的 Gameplay 预制件和 Addressable 地址。
- Root 必须为 Position 0、Rotation 0、Scale 1。
- 配置 Animator、CharacterAnimationBridge、PlayerModelView。
- PlayerModelView 单独指定 LeanRoot、Spine、Chest、UpperChest 和全部装备挂点。
- 在 UpperChest 下建立 `WeaponAimPivot/RightHandWeaponRoot`；AimPivot 配置到 CharacterAnimationBridge，武器根配置到 PlayerModelView。不要把需要右手 IK 的武器继续挂在手骨骼下。
- Generic IK 单独指定左右上臂、前臂、手；骨骼名称不写入通用代码。
- 每个角色在参考 Idle/Aim Pose 中分别计算左右手 `Inverse(GripRotation) * HandRotation`，把结果保存为该角色 Rotation Offset；禁止为适配角色去旋转共享武器握点。
- 在 UpperChest 下为左右手建立角色专属 Elbow Hint，位置应落在自然弯肘一侧且不能与肩膀到握点的直线重合。
- 按该角色尺寸校准视觉大小和挂点，验证挂点 world/lossy scale 为 1。

### Avatar 与动画

- 每个角色使用自己的基础模型 Avatar。
- 该角色所有动画 Copy From Other Avatar 到自己的 Avatar。
- 检查骨骼节点数量、层级、名称、参考姿势和单位比例完全一致。
- 检查所有动画根骨首帧方向是否一致；不能只确认根骨“没有位移”。
- 检查 Idle、Jog、Sprint、Pivot、Hit、Dead 切换时根骨、骨盆和脚不会旋转或下沉。
- 循环动画必须检查首尾腿、骨盆、手臂旋转差；单次播放正常不代表可以循环。
- Sprint 若包含起步段，应拆成一次性 Start 和稳定 Loop。

### Controller 与 AvatarMask

- 复制 Controller 后逐个替换动画，不共享安比 Motion。
- Generic AvatarMask 保存的是骨骼路径；若新角色路径不同，必须为该角色重新生成/配置 Mask。
- 检查 Aim Move 二维 BlendTree 的方向坐标和动画朝向。
- 检查 Pivot 动画是否对应实际触发方向；没有对应动画的方向不要错误复用。
- 根据该角色动画节奏微调过渡时间，但不要删除公共状态/参数契约。

### IK 与武器

- Humanoid Controller 的目标 Layer 开启 IK Pass。
- Generic 预制件分别配置左右臂三根骨骼；需要时可配置独立 Elbow Hint。
- 每把武器配置 Muzzle、MainHandGrip 与 OffHandGrip；MainHandGrip 是右手骨骼的目标，不一定等于枪模型几何中心。
- 枪械整体位置通过角色预制件的 `RightHandWeaponRoot` 调整，不要移动角色手骨骼或把枪重新挂回右手节点。
- `WeaponAimPivot` 只保留基准旋转，不用于手工调枪；验证 Muzzle.forward 确实沿枪管朝前，Muzzle.up 符合武器标准朝上轴。
- 水平玩法不要直接 LookAt 鼠标地面点。使用水平 AimDirection，以非瞄准 Idle 自动捕获的枪口完整姿态为基准；仅在确有需要时配置附加 Pitch/Roll。
- Fire/Recoil 下持续验证 Elbow Hint 同侧性、手腕最大修正角和 IK 可达性；后坐力应作用于武器 Aim/Recoil Pivot 后再执行双手 IK。
- 在 Idle、Move、Sprint、Aim、Reload、Hit 下验证 IK 权重开关和手臂可达性。
- OffHandGrip 超出最大臂长时应修改武器握点或持枪挂点，不应缩放/拉伸骨骼掩盖问题。

## 四、UE 导出最低要求

- 使用角色原始 Skeleton 和统一的重定向参数。
- 单动画单 FBX，Bake Animation，固定采样率（当前 30 FPS）。
- 动画 FBX 不导出 Mesh、Skin、材质。
- Bip001 的平移和旋转锁定到同一个“角色级标准根基准”，不是各动画自己的首帧。
- 不导出 Gameplay Root Motion；位移由 Unity Simulation 驱动。
- 保持单位、轴向、参考姿势和骨架层级一致。
- 导出后至少检查：根曲线、骨盆高度、脚底接触、循环首尾、快速切换、持续循环。

## 五、每个角色的验收测试

1. Idle 静止 10 秒，Root/Bip001 不累计旋转或位移。
2. Idle、Move、Sprint 反复快速切换，不锁帧、不平移滑步。
3. Sprint 连续播放多个循环，循环接缝不甩腿。
4. 180 度急转只在配置窗口内影响动作，随后正常恢复移动。
5. W/A/S/D 组合形成弧线时，速度方向能及时跟随输入。
6. Aim Idle/Move、Fire、Reload 与 Base 下半身正确分层。
7. Generic/Humanoid IK 分别工作；Reload/Hit/Dead 会释放 IK；连续 Fire 不发生手肘翻面或手腕瞬时反转。
8. 武器、挂点和角色 Root 的 world scale 均符合预期。
9. Addressables 实际加载模型、Controller、武器成功。
10. Unity Console 无 Avatar、曲线、丢失引用或状态参数错误。

Pivot 验收需同时覆盖低速反转（约 1.25 m/s 以上）、斜向大角度反转和 Root 正在快速转向的情况；瞄准移动反转应保持 Aim Move，不应抢占为 Pivot。
