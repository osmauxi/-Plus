# 大厅武器 Animator 搭建说明

## 1. 代码调用约定

大厅 Animator 只接收一个 Int 参数：

| 参数 | 类型 | 来源 |
| --- | --- | --- |
| `EquipmentPose` | Int | `Config_Lobby_Weapons.WeaponEquipAnim` |

数值约定：

| 数值 | EquipmentPose | 状态 |
| --- | --- | --- |
| 0 | `Rifle` | 步枪切换动画和步枪待机 |
| 1 | `Pistol` | 手枪切换动画和手枪待机 |

武器 Addressable 模型成功生成后，`CharacterSocketProvider` 会执行：

```csharp
_animator.SetInteger("EquipmentPose", weaponConfig.WeaponEquipAnim);
```

只有武器切换会调用 Animator。道具切换只负责在 `ItemSpawnSlot` 对应挂点生成或替换模型，不会修改任何动画参数。

## 2. Animator Controller 结构

Controller 只需要一个 `EquipmentPose` Int 参数，并保留以下状态：

- `Idle_Unarm`：角色模型刚生成、武器尚未加载完成时的默认状态。
- `Pose_Rifle`：切换步枪时播放一次的过渡动画。
- `Idle_Rifle`：步枪持有待机。
- `Pose_Pistol`：切换手枪时播放一次的过渡动画。
- `Idle_Pistol`：手枪持有待机。

推荐连接方式与当前 `Lobby_Character_Controller` 一致：

1. `Entry -> Idle_Unarm`。
2. `Any State -> Pose_Rifle`，条件为 `EquipmentPose Equals 0`。
3. `Pose_Rifle -> Idle_Rifle`，开启 `Has Exit Time`，不设置额外条件。
4. `Any State -> Pose_Pistol`，条件为 `EquipmentPose Equals 1`。
5. `Pose_Pistol -> Idle_Pistol`，开启 `Has Exit Time`，不设置额外条件。
6. `Idle_Rifle` 在 `EquipmentPose NotEqual 0` 时离开。
7. `Idle_Pistol` 在 `EquipmentPose NotEqual 1` 时离开。

`Pose_Rifle` 和 `Pose_Pistol` 使用非循环动画；两个 `Idle` 状态使用循环动画。

## 3. 角色预制件要求

每个大厅角色预制件需要：

- 根节点或指定节点上的 `Animator`。
- `CharacterSocketProvider`。
- 在 `CharacterSocketProvider._animator` 中绑定该角色的 Animator。
- Animator Controller 绑定 `Lobby_Character_Controller`。
- 关闭 `Apply Root Motion`，避免动画改变展位位置。

不再需要为大厅展示启用 IK Pass，也不再由 `CharacterSocketProvider` 读取 `WeaponVisualPoints`。

## 4. 装备挂点顺序

`EquipmentSlots` 数组不包含 `None`，必须与当前 `EquipmentSlot` 枚举保持以下顺序：

| 数组索引 | EquipmentSlot 数值 | 挂点 |
| --- | --- | --- |
| 0 | 1 | LeftHand |
| 1 | 2 | RightHand |
| 2 | 3 | Back |
| 3 | 4 | Chest |
| 4 | 5 | HipLeft |
| 5 | 6 | HipRight |

换算关系：

```text
EquipmentSlots 数组索引 = (int)EquipmentSlot - 1
```

武器使用 `WeaponSpawnSlot`，道具使用 `ItemSpawnSlot`。装备模型生成后会在挂点下重置为本地位置零和单位旋转，因此模型预制件自身需要处理好轴向及视觉偏移。

## 5. 配置要求

`Config_Lobby_Weapons`：

- `ModleName` 必须是有效的 Addressable 地址。
- `WeaponSpawnSlot` 必须对应有效的 `EquipmentSlot`。
- `WeaponEquipAnim` 只能填写 `0` 或 `1`。

`Config_Lobby_Items`：

- `ModleName` 必须是有效的 Addressable 地址。
- `ItemSpawnSlot` 必须对应有效的 `EquipmentSlot`。
- 不需要配置动画类型。

修改配置表后，需要重新导出 Addressable `.bytes`，确保新增字段已写入二进制数据。

## 6. 验收清单

1. 角色进入大厅时先显示 `Idle_Unarm`。
2. 步枪生成后播放 `Pose_Rifle`，随后进入 `Idle_Rifle`。
3. 手枪生成后播放 `Pose_Pistol`，随后进入 `Idle_Pistol`。
4. 切换武器时旧模型被释放，新模型生成到 `WeaponSpawnSlot`。
5. 切换道具时只替换 `ItemSpawnSlot` 下的模型，Animator 参数保持不变。
6. 切换角色皮肤后，武器、道具和武器姿势重新应用到新角色。
