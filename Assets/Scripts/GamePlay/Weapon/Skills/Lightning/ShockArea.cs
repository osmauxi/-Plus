using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShockArea : MonoBehaviour
{ 
    private StormCloudEffect effectSO;
    private CharacterStatCollection stats;
   
    public void Init(StormCloudEffect so, CharacterStatCollection charStats)
    {
        effectSO = so;
        stats = charStats;
        StartCoroutine(StormRoutine());
    }
   
    private IEnumerator StormRoutine()
    {
        while (true)
        {
            int stacks = effectSO.GetCurrentStacks(stats);
            if (stacks <= 0) { yield return new WaitForSeconds(1f); continue; }
   
            float currentInterval = Mathf.Max(0.2f, effectSO.baseStrikeInterval - (stacks - 1) * effectSO.intervalReducePerStack);
            float currentRadius = effectSO.baseSearchRadius + (stacks - 1) * effectSO.radiusBonusPerStack;
   
            yield return new WaitForSeconds(currentInterval);
   
            Collider[] targets = Physics.OverlapSphere(transform.position, currentRadius);
            List<Health> validEnemies = new List<Health>();
   
            foreach (var t in targets)
            {
                if (t.CompareTag("Enemy") && t.transform.root.TryGetComponent<Health>(out var h) && !h.isDead)
                    validEnemies.Add(h);
            }
   
            if (validEnemies.Count > 0)
            {
                Health target = validEnemies[Random.Range(0, validEnemies.Count)];
                Vector3 targetPos = target.transform.position;
   
                GlobalLocalVFXPool.Instance.GetVFX("LightningStrike", targetPos);
   
                if (NetworkManager.Singleton.IsServer)
                {
                    float damage = stats.GetStatValue(StatType.Damage) * effectSO.damageMultiplier;
                    target.TakeDamage(damage, targetPos, Vector3.down, 0.5f);
   
                    // 此处略去了 HandleShockArea 的重叠判定（保持你之前的区域叠加逻辑即可）
                }
            }
        }
    }
 
}