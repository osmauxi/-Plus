# 席德 Generic V4 导出与验收说明

## 交付结论

本包按当前 Gameplay Generic 动画规则制作并通过离线验收：1 个席德基础模型、13 个单动画 FBX，不包含额外的 `Sprint Loop`。原始席德 FBX、Skeleton 层级、骨骼名称、参考姿势和蒙皮均未修改。

席德原模型高度约为 153.49 cm，尺寸本来就是正常角色尺度，因此本包保持物理尺寸 1×，Unity 导入 `Scale Factor = 1`。**不得再对席德套用安比/扳机的 Scale7 处理**；那会把角色放大到约 10.74 m。

## 基础信息

- Unreal Engine：5.8.1
- FBX SDK/目标管线版本：2020.2
- 运行时 Rig：Unity Generic
- 帧率：30 FPS
- UE Skeletal Mesh：`/Game/席德/席德`
- UE Skeleton：`/Game/席德/席德_Skeleton`
- 骨架根节点：`Bip001`
- 骨骼节点数：257
- 基础模型结构：1 Mesh、1 Skin、257 Skin Cluster
- 席德模型包围盒高度：约 153.49 cm
- 模型物理缩放：1×；Unity `Scale Factor = 1`

本次在 UE 中创建的独立工作资产：

- IK Rig：`/Game/Retargeting/Xide/GenericV4/IKR_Xide_GenericV4`
- IK Retargeter：`/Game/Retargeting/Xide/GenericV4/RTG_Mannequin_Xide_GenericV4`
- 动画资产目录：`/Game/Retargeting/Xide/GenericV4/Animations`

这些资产是复制后针对席德建立的工作副本，不会反向修改席德原 Skeleton 或模型。

## 文件清单与时间范围

| 文件 | 类型 | 源帧范围 | 输出帧范围 | 闭环处理 |
| --- | --- | ---: | ---: | --- |
| `Game_Aim_Idle_ADS.fbx` | Loop | 0–240 | 0–240 | 原周期保留，不改接缝 |
| `Game_Aim_Walk_Backward.fbx` | Loop | 18–36 | 0–18 | 仅最后 2 帧闭环 |
| `Game_Aim_Walk_Forward.fbx` | Loop | 0–46 | 0–46 | 原周期保留，不改接缝 |
| `Game_Aim_Walk_Left.fbx` | Loop | 17–36 | 0–19 | 仅最后 2 帧闭环 |
| `Game_Aim_Walk_Right.fbx` | Loop | 22–45 | 0–23 | 仅最后 2 帧闭环 |
| `Game_Dead_StandToCrouch.fbx` | Non-Loop | 0–73 | 0–73 | 无 |
| `Game_Fire_Shotgun.fbx` | Non-Loop | 0–20 | 0–20 | 无 |
| `Game_Hit_FrontHeavy.fbx` | Non-Loop | 0–24 | 0–24 | 无 |
| `Game_Idle_UnarmedReadyPose.fbx` | Static Pose | 0–1 | 0–1 | 无 |
| `Game_Move_JogForward.fbx` | Loop | 0–46 | 0–46 | 原周期保留，不改接缝 |
| `Game_Pivot_Turn180Left.fbx` | Non-Loop | 0–107 | 0–107 | 无 |
| `Game_Reload_Rifle_Additive.fbx` | Non-Loop | 0–66 | 0–66 | 无 |
| `Game_Sprint_StartForward.fbx` | Non-Loop Source | 0–77 | 0–77 | 完整保留 |

`Game_Sprint_StartForward.fbx` 没有截短。Unity 接入阶段应从席德自己的稳定疾跑区间派生专属 Sprint Loop；当前候选区间为 36–66 帧。不要把完整 StartForward 直接设为循环，也不要复用其他角色的 Sprint Loop。

## Root 与动画文件契约

- Gameplay 由代码驱动整体位置与朝向，动画不提供累计 Root Motion。
- 13 个动画中的 `Bip001` Translation、Rotation、Scale 都是常量轨道，各为 1 个键；不会产生随时间累计的位移、旋转或缩放。
- `Bip001` Scale 恒定为 `(1, 1, 1)`。
- Root 使用各动画第一帧作为锁定基准，因此个别动作可有不同的常量局部基准，但全片不会漂移。
- 每个动画 FBX 只有 Skeleton + 1 个 Animation Stack，不含 Mesh、Skin、材质、贴图、武器、碰撞、相机或灯光。
- 每个动画的 257 个骨骼节点与基础模型一致，未新增、删除、重命名或改变层级。

## 关键帧精简与误差验收

精简参数：

- Translation：0.005 FBX 局部单位
- Rotation：0.05°
- Scale：0.00001
- 完全恒定的曲线压缩为 1 个键

13 个动画由 1,787,949 个逐帧采样键精简至 84,244 个键，减少约 95.288%。动画 FBX 总体积由约 44.313 MiB 降至 23.773 MiB；最终 13 个文件的哈希均与逐帧 Bake 输入不同，确认不是依赖 Unity `Keyframe Reduction` 产生的表面变化。

最终 FBX 直接与未经全片闭环改写的 1× 密集参考逐帧比较。除明确允许修改的 Back/Left/Right 最后 2 帧外，全组最大世界误差为：

- 最大世界旋转误差：0.294511°
- 最大世界位置误差：0.123738 cm
- 验收阈值：0.5° / 0.2 cm

| 动画 | 最大旋转误差 | 最大位置误差 |
| --- | ---: | ---: |
| Aim Idle | 0.294511° | 0.122111 cm |
| Aim Walk Backward（排除最后 2 帧） | 0.193173° | 0.067585 cm |
| Aim Walk Forward | 0.207082° | 0.123737 cm |
| Aim Walk Left（排除最后 2 帧） | 0.210036° | 0.086704 cm |
| Aim Walk Right（排除最后 2 帧） | 0.203183° | 0.087902 cm |
| Dead | 0.265762° | 0.119605 cm |
| Fire | 0.208214° | 0.093564 cm |
| Hit | 0.215820° | 0.090930 cm |
| Ready Pose | 0° | 0 cm |
| Jog | 0.164052° | 0.103076 cm |
| Pivot | 0.221467° | 0.097907 cm |
| Reload | 0.204455° | 0.098555 cm |
| Sprint Start | 0.230847° | 0.103090 cm |

Back/Left/Right 的闭环差异严格限制在最后 2 帧；若把这 2 帧也纳入原动作比较，其峰值分别约为 `8.65° / 4.03 cm`、`11.21° / 4.45 cm`、`11.04° / 6.43 cm`。这是显式接缝修正，不是把误差分摊到整段动作。

循环首尾闭合复核：

- Aim Idle：0.136023° / 0.000005 局部位置单位
- Aim Walk Backward：0° / 0
- Aim Walk Forward：0.028584° / 0.002333
- Aim Walk Left：0° / 0
- Aim Walk Right：0° / 0
- Jog：0.017547° / 0.000882

## Unity 导入建议

1. 将 `Xide_Game_Generic_Base.fbx` 的 Rig 设为 `Generic`，Avatar Definition 选 `Create From This Model`。
2. 模型和所有动画的 `Scale Factor` 保持 `1`，不要再乘 7。
3. 13 个动画的 Rig 设为 `Generic`，Avatar Definition 选 `Copy From Other Avatar`，指向席德 V4 基础模型的 Avatar。
4. 首轮验收关闭 Unity Importer 的 `Keyframe Reduction`，以便确认精简已经真实写入 FBX；通过后再决定是否启用 Unity 侧二次压缩。
5. `Apply Root Motion` 关闭。
6. Aim Idle、Aim Walk Forward/Backward/Left/Right、Jog 开启 `Loop Time`；其他动作关闭。
7. `Sprint Start` 不循环；从 36–66 帧候选区间测试并派生席德专属 Sprint Loop。
8. 检查全部骨骼路径无 Missing Binding，再测试瞄准、脚底接触、武器握持、Pivot、Hit、Dead 和 Reload。

基础模型 FBX保留材质槽/材质引用，但贴图仍是项目外部资产；Unity 若未自动匹配，需要按席德原材质重新挂接贴图。

