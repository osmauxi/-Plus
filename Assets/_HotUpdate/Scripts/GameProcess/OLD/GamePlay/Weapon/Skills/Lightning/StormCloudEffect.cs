using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "StormCloudEffect", menuName = "Roguelike/Effects/StormCloud")]
public class StormCloudEffect : WeaponEffectSO
{
    public float baseStrikeInterval = 1f;
    public float intervalReducePerStack = 0.15f;
    public float baseSearchRadius = 8f;
    public float radiusBonusPerStack = 2f;
    public float damageMultiplier = 1.2f;

    public override void OnEquip(GameObject weaponObj, CharacterStatCollection stats)
    {
        int stacks = GetCurrentStacks(stats);
        if (stacks <= 0) return;

        // 让武器自己挂载一个组件来跑协程，保持 SO 的纯洁
        if (!weaponObj.TryGetComponent<ShockArea>(out var runner))
        {
            runner = weaponObj.AddComponent<ShockArea>();
            runner.Init(this, stats);

            // 【新增】：获得雷云时，播放一次打雷全屏音效震撼一下！
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Skill_ThunderCloud, 1f);
        }
    }
}