# 扳机 Generic 动画 V4.1：导出与验收

## 本版修正结论

V4.1 已撤销 V4 的“全片分布式闭环修正”。优化动画直接与未经闭环改写的原始 FBX 采样姿态比较，不再使用经过相同修正的参考动画掩盖误差。

- 交付包只含接入规则要求的 13 个动画 FBX。
- `Game_Sprint_StartForward.fbx` 恢复为完整 0–77 帧。
- 不交付 `Game_Sprint_Loop.fbx`；由 Unity 接入阶段从 StartForward 的稳定区间派生。
- Aim Idle、Aim Walk Forward、Jog 的源首尾已经足够接近，因此完全不做接缝改写。
- Aim Walk Backward、Left、Right 只允许修改输出末尾 2 帧；在此之前的整段动作保持原源姿态。
- 13 个 V4.1 FBX 均与旧满键源文件不同，也均与 V4 文件不同。
- FBX 内采样关键帧从 688,743 个减少为 66,137 个，减少 90.3974%。
- 13 个旧源 FBX 合计 17,972,000 bytes；V4.1 合计 10,155,680 bytes，减少 43.4917%。

## V4 失败原因与 V4.1 修法

V4 使用的 Hermite 闭环函数把首尾差值和速度差分摊到了整个动画。它虽然让末帧等于首帧，却改变了中段步态，Jog、Aim Walk Forward 和 Sprint Loop 分别出现约 97.3°、69.1°、80.7° 的世界旋转偏差。

V4 的误差基准又使用了相同的闭环函数，因此只能证明“压缩结果接近已经改写过的动作”，不能证明原动作得到保留。

V4.1 的处理规则：

1. 从原始 FBX 指定帧段直接逐帧采样。
2. 只做关键帧精简，不改变正常动作帧的数值。
3. 已自然闭合的循环完全不做接缝处理。
4. 需要补接缝的 Backward、Left、Right，仅在最后 2 个 30 FPS 帧内用局部 Hermite 过渡到首帧；旋转终点选择最近的等价 Euler 周期。
5. 原始动作对比时明确排除这 2 个获准修改的接缝帧，其余帧必须通过 0.5° / 0.2 cm 验收线。

## 基本规格

- Unreal Engine：5.8.1
- FBX SDK：2020.2
- 帧率：30 FPS
- Unity Rig：Generic
- 骨架节点：99
- 每个 FBX：1 个动画栈、无 Mesh、无 Skin、无材质
- UE Skeleton：`/Game/Retargeting/Trigger/GenericV3/SK_Trigger_GenericV3_Source_Skeleton`
- 根节点：`Bip001`
- 根节点 Position/Rotation：每项恒定，不产生累计运动
- 根节点 Scale：全片恒定 `7,7,7`
- Unity Scale Factor：1
- 关键帧精简容差：局部位置 0.005 FBX 单位、局部旋转 0.05°、缩放 0.00001

## 13 个动画及循环规则

| 文件 | 原始取帧 | 输出帧 | 用途 | 接缝处理 |
|---|---:|---:|---|---|
| `Game_Aim_Idle_ADS.fbx` | 0–240 | 0–240 | Loop | 无；保留源姿态 |
| `Game_Aim_Walk_Backward.fbx` | 18–36 | 0–18 | Loop | 只修改末尾 2 帧 |
| `Game_Aim_Walk_Forward.fbx` | 0–46 | 0–46 | Loop | 无；保留源姿态 |
| `Game_Aim_Walk_Left.fbx` | 17–36 | 0–19 | Loop | 只修改末尾 2 帧 |
| `Game_Aim_Walk_Right.fbx` | 22–45 | 0–23 | Loop | 只修改末尾 2 帧 |
| `Game_Dead_StandToCrouch.fbx` | 0–73 | 0–73 | Non-Loop | 无 |
| `Game_Fire_Shotgun.fbx` | 0–20 | 0–20 | Non-Loop | 无 |
| `Game_Hit_FrontHeavy.fbx` | 0–24 | 0–24 | Non-Loop | 无 |
| `Game_Idle_UnarmedReadyPose.fbx` | 0–1 | 0–1 | Static Pose | 无 |
| `Game_Move_JogForward.fbx` | 0–46 | 0–46 | Loop | 无；保留源姿态 |
| `Game_Pivot_Turn180Left.fbx` | 0–107 | 0–107 | Non-Loop | 无 |
| `Game_Reload_Rifle_Additive.fbx` | 0–66 | 0–66 | Non-Loop | 无 |
| `Game_Sprint_StartForward.fbx` | 0–77 | 0–77 | Non-Loop + 派生来源 | 无；完整保留 |

StartForward 中建议先以源 36–66 帧作为稳定疾跑候选区间，在 Unity 中按该角色脚步接触相位复核后生成 `Trigger_Game_Sprint_Loop.anim`。这只是接入阶段的候选范围，不在导出包中额外交付 Sprint Loop，也不得把完整 StartForward 直接设为 Loop。

## 与未经闭环改写原始动作的逐帧验收

比较覆盖全部 99 个骨骼并累计完整父子世界变换。Backward、Left、Right 的“正常动作区”不含获准修改的最后 2 个接缝帧；其余动画比较全部帧。

| 动画 | 正常动作区最大世界旋转误差 | 最大世界位置误差 |
|---|---:|---:|
| Aim Idle | 0.270556° | 0.120804 cm |
| Aim Walk Backward（不含末 2 帧） | 0.220217° | 0.083585 cm |
| Aim Walk Forward | 0.231172° | 0.094577 cm |
| Aim Walk Left（不含末 2 帧） | 0.199744° | 0.100549 cm |
| Aim Walk Right（不含末 2 帧） | 0.192254° | 0.069192 cm |
| Dead | 0.243248° | 0.119152 cm |
| Fire | 0.206717° | 0.099616 cm |
| Hit | 0.190340° | 0.085923 cm |
| Ready Pose | 0.000000° | 0.000000 cm |
| Jog | 0.205019° | 0.124048 cm |
| Pivot | 0.234742° | 0.092563 cm |
| Reload | 0.219779° | 0.101722 cm |
| Sprint Start 0–77 | 0.236861° | 0.104761 cm |

整体最大值为 0.270556° / 0.124048 cm，低于 0.5° / 0.2 cm 验收线。V4 中出现的 69–97° 中段偏差已不存在。

三个获准处理的接缝在最后 2 帧内相对源动作会有较大差异，这是闭环过渡本身：Backward 最大 8.6524° / 4.0218 cm，Left 最大 11.2123° / 6.8884 cm，Right 最大 11.0423° / 6.9689 cm。修改范围没有扩散到更早帧。三者首尾 FBX 姿态已数值闭合；最终仍需在 Unity 检查这两个过渡帧的脚底接触与滑步观感。

## Unity 导入

1. 导入独立 V4.1 目录，不覆盖 V1/V4。
2. Rig 设为 `Generic`，Avatar Definition 从当前扳机基础模型复制。
3. Scale Factor 设为 `1`，关闭 `Apply Root Motion`。
4. 首次 A/B 验收关闭 Unity `Keyframe Reduction`，避免把导入器压缩与 FBX 源修改混淆。
5. Aim Idle、四向 Aim Walk、Jog 启用 `Loop Time`。
6. Aim Idle、Aim Walk Forward、Jog 包含源动画自身的近似闭合末帧；Backward、Left、Right 包含经末尾两帧过渡后与首帧相同的闭合末帧。
7. StartForward 保持 Non-Loop；从候选稳定区间派生角色专属 Sprint Loop。
8. 优先检查 Backward/Left/Right 的最后 2 帧、Jog 中段，以及 StartForward 36–66 帧的稳定跑步区间。

## 基础模型

本次只重导动画，不重导模型。继续复用：

`Saved/Exports/Trigger_Generic_V3_Scale7_Package/Trigger_Game_Generic_Base_Scale7.fbx`

SHA-256：`3E3242B5F87ED67F805EAEEC288AFA33AA77A8AFF58E9ECC2468A2CECD6F1470`

完整动画哈希见 `V4.1_SHA256.txt`。
