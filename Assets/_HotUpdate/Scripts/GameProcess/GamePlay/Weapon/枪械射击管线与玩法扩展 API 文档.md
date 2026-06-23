> # 枪械射击管线与玩法扩展 API 文档
>
> 本文档旨在阐述本项目中射击管线的底层架构，以及程序研发人员如何在该框架下进行安全、解耦的玩法拓展。请在开发前仔细阅读核心类的职责边界与挂载规范。
>
> ## 一、 核心架构与基础运行逻辑
>
> 本项目的射击系统采用**数据快照注入**与**“生命周期钩子”**相结合的设计模式。一颗子弹从触发到销毁的完整管线如下：
>
> 1. **输入与拦截阶段：** 实体（玩家输入或怪物 AI）调用 `WeaponBase.TryFire()`。此时遍历武器挂载的所有 `IWeaponEffect`，触发 `OnBeforeFire` 钩子。若任意特效返回 `false`，则立刻中断射击管线。
> 2. **快照计算阶段：** `WeaponBase` 向所绑定的 `CharacterStatCollection` 请求当前各项属性（伤害、射速、多重射击数量等）的最终计算值（包含所有修饰器的加成），形成数据快照。
> 3. **物理同步阶段：** 计算弹道散布与动量继承，通过 `ServerRpc` 向服务器发送生成指令。
> 4. **生成与注入阶段：** 服务器端（或 Host）从 `LocalObjectPool` 中提取 `ProjectileBase` 实例，将**数据快照**与**特效列表的引用（IWeaponEffect List）**一并注入给子弹实体，并触发 `OnProjectileSpawn` 钩子。
> 5. **飞行与碰撞阶段：** 子弹由物理引擎驱动飞行。当 `OnTriggerEnter` 检测到有效碰撞时，遍历特效列表，触发 `OnHit` 钩子执行具体业务逻辑（如造成伤害、连锁闪电）。
> 6. **回收阶段：** 穿透/弹射次数耗尽或达到最大存活时间，触发 `OnDestroy` 钩子，随后归还至对象池。
>
> ------
>
> ## 二、 核心类字典与挂载规范
>
> 在进行拓展开发时，请严格遵循以下类的职责划分，切勿跨模块修改底层逻辑。
>
> ### 1. `CharacterStatCollection` (属性状态集合)
>
> - **基础作用：** 集中管理实体的所有数值属性（如血量、伤害、移速）。通过 `StatModifier` 实现基于基础值的动态加减乘除，提供实时的属性结算。
> - **挂载对象：** 玩家预制件或怪物预制件的根节点。
>
> ### 2. `WeaponBase` (武器基类)
>
> - **基础作用：** 处理武器级的逻辑，包括弹药管理、射击冷却、射线视差校正、扇形多重弹道计算，并维护当前武器激活的 `IWeaponEffect` 列表。
> - **挂载对象：** 武器预制件的根节点（通常作为玩家/怪物手部节点的子物体）。
> - **配置要求：** 必须在 Inspector 面板中将其 `stats` 字段绑定到所属实体的 `CharacterStatCollection` 组件上，并配置 `firePoint`（发射点 Transform）。
>
> ### 3. `ProjectileBase` (子弹基类)
>
> - **基础作用：** 作为数据与特效的物理载体。负责基于给定的速度和动量继承进行移动，处理与 Tag 相关的阵营过滤判定，以及弹射/穿透的物理行为。
> - **挂载对象：** 子弹预制件的根节点。
> - **配置要求：** 所在 GameObject 必须同时挂载 `Rigidbody` 与 `Collider`（需勾选 Is Trigger）。
>
> ### 4. `IWeaponEffect` (特效/词条接口)
>
> - **基础作用：** 暴露射击管线中的关键生命周期节点。所有的特殊子弹机制、枪械被动能力均通过实现此接口来完成。
> - **挂载对象：** **禁止挂载**。必须作为纯 C# 类实现。系统会在运行时通过代码实例化（`new`）并添加至 `WeaponBase.activeEffects` 列表中。
>
> ------
>
> ## 三、 玩法扩展开发指南
>
> 所有的枪械机制拓展，**均不应修改上述核心基类的代码**，而是通过新建类继承指定的接口来实现。系统目前提供两个核心拓展接口：`IWeaponEffect`（基础生命周期）与 `IUpgradeableEffect`（堆叠升级机制）。
>
> ### 1.1 接口方法说明
>
> 实现 `IWeaponEffect` 需覆盖以下 6 个钩子方法：
>
> - `OnEquip`: 词条/机制被添加到武器时触发。
> - `OnBeforeFire`: 射击动作执行前触发。返回 `false` 可覆写/阻断原生开火逻辑。
> - `OnAfterFire`: 射击动作完成后触发（如后坐力处理）。
> - `OnProjectileSpawn`: 子弹实例生成并注入数据后触发（如修改子弹材质、大小）。
> - `OnHit`: 子弹击中有效目标时触发（如执行额外伤害、Debuff 附加、连锁等）。
> - `OnDestroy`: 子弹生命周期结束被回收前触发（如销毁特效、尸爆等）。
>
> #### 1.2 词条堆叠与升级接口 (`IUpgradeableEffect`)
>
> 本架构遵循**接口隔离原则。若你开发的词条支持“多次拾取后数值叠加”或“形态升级”，需额外继承此接口。
>
> - `Upgrade()`: 当实体（玩家/武器）尝试获取一个已存在的同类型特效时，`WeaponBase` 不会生成新实例，而是将其强转为 `IUpgradeableEffect` 并调用此方法。
> - **开发规范：** 在此方法内进行内部变量的累加运算，以实现量变（如扩大判定半径、增加伤害倍率）。
>
> ### 2. 开发范例：吸血反噬机制 (`VampireRecoilEffect.cs`)
>
> 
>
> ```c#
> using UnityEngine;
> 
> /// <summary>
> /// 范例：可升级的吸血反噬机制
> /// 功能：开火前扣除自身生命值，命中敌人时恢复生命值。
> /// 升级机制：重复获取该词条时，提升吸血量。
> /// </summary>
> public class VampireRecoilEffect : IWeaponEffect, IUpgradeableEffect
> {
>     // 内部状态变量
>     private int currentLevel = 1;
>     private float healthCost = 1f; // 开火消耗生命值
>     private float healAmount = 2f; // 击中恢复生命值
> 
>     // ==========================================
>     // 接口：IUpgradeableEffect 实现
>     // ==========================================
>     public void Upgrade()
>     {
>         currentLevel++;
>         healAmount += 1.5f; // 每次升级，单发吸血量增加 1.5
>         Debug.Log($"[System] 吸血诅咒已升级至 Level {currentLevel}，当前吸血量: {healAmount}");
>     }
> 
>     // ==========================================
>     // 接口：IWeaponEffect 实现
>     // ==========================================
>     public void OnEquip(GameObject weaponObj, CharacterStatCollection stats) 
>     { 
>         // 初始化逻辑
>     }
> 
>     public bool OnBeforeFire(WeaponBase weapon, CharacterStatCollection stats)
>     {
>         float currentHealth = stats.GetStatValue(StatType.MaxHealth); // 示例：读取当前血量
>         
>         // 校验逻辑：生命值不足以支付开火代价时，管线阻断，拒绝开火
>         if (currentHealth <= healthCost)
>         {
>             return false; 
>         }
>         
>         // 执行自身扣血逻辑
>         // weapon.GetComponentInParent<Health>().TakeDamage(healthCost);
>         return true; 
>     }
> 
>     public void OnAfterFire(WeaponBase weapon, CharacterStatCollection stats) { }
> 
>     public void OnProjectileSpawn(ProjectileBase projectile, CharacterStatCollection stats)
>     {
>         // 变更实体表现：随等级提升，子弹颜色逐渐加深（插值计算）
>         if (projectile.TryGetComponent<Renderer>(out Renderer ren))
>         {
>             float intensity = Mathf.Clamp01(currentLevel / 5f);
>             ren.material.color = Color.Lerp(Color.red, new Color(0.5f, 0, 0), intensity);
>         }
>     }
> 
>     public void OnHit(ProjectileBase projectile, GameObject target, Vector3 hitPoint, CharacterStatCollection stats)
>     {
>         if (target.CompareTag("Enemy"))
>         {
>             // 执行吸血逻辑，使用随等级成长的 healAmount 变量
>             // projectile.owner.GetComponent<Health>().Heal(healAmount);
>         }
>     }
> 
>     public void OnDestroy(ProjectileBase projectile, Vector3 destroyPoint, CharacterStatCollection stats) { }
> }
> ```
>
> 当然，Assets/Scripts/GamePlay/Weapon/Samples中也存在一个样例脚本。
>
> ------
>
> ## 四、 资产与对象池注册规范
>
> 开发中产出的预制件（Prefab），必须通过对象池进行实例化与回收，禁止直接使用 `Instantiate` 或 `Destroy`，我为此专门写了LocalObjectPool与SyncObjectPool两个对象池用于使用。
>
> 1. **非同步物理实体（如子弹、粒子特效）：**
>
>    - 使用`LocalObjectPool.instance.GetT("YourID", position, rotation)`获取。
>
>    - **注册方式：**Assets/Prefab/PoolManager就是我的对象池预制件，同时挂载了LocalObjectPool与SyncObjectPool两个脚本，将预制件放置于 `Assets/_Cooperation/_GameplayFeatures/Weapons/你的目录` 下，并在场景中的 `LocalObjectPool` 组件 Inspector 面板中新增 Item，填入ID并拖入预制件。
>
>    - **测试方式 ：**如果你需要测试功能，可以复制我的TestScene场景（Assets/Scenes）放入Assets/-Cooperation/-Scenes中，在此复制场景中进行子弹测试，在你的脚本中获取玩家的WeaponBase脚本，然后在Update中调用类似我在WeaponBase的HandleInput方法中的检测按键触发方法，**不能修改WeaponBase脚本中的任何东西，这项操作是在你的脚本中进行的** 。
>
>      
>
> 2. **需要网络状态同步的实体（如新怪物）：**
>
>    - 必须包含 `NetworkObject` 组件。
>    - **注册方式：** 在 `NetworkManager` 的 `NetworkPrefabs` 列表中注册，并统一经由服务器（ServerRpc）进行 Spawn。
>    - 子弹基本用不上这个功能，在之后的怪物开发中会详细说。