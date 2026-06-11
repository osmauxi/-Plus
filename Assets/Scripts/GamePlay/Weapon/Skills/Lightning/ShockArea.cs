using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShockArea : MonoBehaviour
{
    private StormCloudEffect effectSO;
    private CharacterStatCollection stats;

    // --- 常驻特效引用 ---
    private GameObject cloudVFX;

    // ==========================================
    // --- 性能优化缓存区 ---
    // ==========================================
    private WeaponBase cachedWeapon;
    private OverloadEffect cachedOverloadEffect;
    private bool hasFoundOverload = false; // 判定成功一次后永久生效

    public void Init(StormCloudEffect so, CharacterStatCollection charStats)
    {
        effectSO = so;
        stats = charStats;

        // 【优化 1】：在初始化时获取一次武器组件，省去每次雷击时的 GetComponent 开销
        cachedWeapon = transform.root.GetComponentInChildren<WeaponBase>();

        StartCoroutine(StormRoutine());
    }

    private void Update()
    {
        int stacks = effectSO.GetCurrentStacks(stats);
        if (stacks <= 0)
        {
            CleanupVFX();
            return;
        }

        float currentRadius = effectSO.baseSearchRadius + (stacks - 1) * effectSO.radiusBonusPerStack;

        if (cloudVFX == null)
        {
            cloudVFX = GlobalLocalVFXPool.Instance.GetVFX("RainCloud", transform.position + Vector3.up * 3.5f);
            if (cloudVFX != null && cloudVFX.TryGetComponent<MonoBehaviour>(out var autoReturn))
                autoReturn.enabled = false;
        }

        Vector3 playerRootPos = transform.root.position;

        if (cloudVFX != null)
        {
            Vector3 targetCloudPos = playerRootPos + Vector3.up * 3.5f;
            cloudVFX.transform.position = Vector3.Lerp(cloudVFX.transform.position, targetCloudPos, Time.deltaTime * 6.5f);
            cloudVFX.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }
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

            Collider[] targets = Physics.OverlapSphere(transform.root.position, currentRadius);
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

                Vector3 startPos = cloudVFX != null ? cloudVFX.transform.position : transform.root.position + Vector3.up * 3.5f;
                Vector3 dir = targetPos - startPos;
                float distance = dir.magnitude;
                Vector3 midPoint = startPos + dir / 2f;
                Quaternion rotation = Quaternion.LookRotation(dir);

                GameObject arc = GlobalLocalVFXPool.Instance.GetVFX("OnLightning", midPoint, rotation, 1f);
                if (arc != null) arc.transform.localScale = new Vector3(1f, 1f, distance);

                GlobalLocalVFXPool.Instance.GetVFX("OnHit_Lightning", targetPos);

                // 【新增】：落雷击中音效
                AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Skill_LightningHit, 0.8f);

                if (NetworkManager.Singleton.IsServer)
                {
                    float damage = stats.GetStatValue(StatType.Damage) * effectSO.damageMultiplier;
                    target.TakeDamage(damage, targetPos, Vector3.down, 0.5f);
                }

                // ==========================================
                // 【优化 2】：基于 Bool 锁的 0 开销缓存检测
                // ==========================================
                if (!hasFoundOverload && cachedWeapon != null)
                {
                    foreach (var effect in cachedWeapon.activeEffects)
                    {
                        if (effect is OverloadEffect overload)
                        {
                            cachedOverloadEffect = overload;
                            hasFoundOverload = true; // 找到了！永久关闭检测门，以后再也不用跑 foreach 了！
                            break;
                        }
                    }
                }

                // 如果已经缓存到了连锁闪电技能，直接极速调用！
                if (hasFoundOverload && cachedOverloadEffect != null)
                {
                    cachedOverloadEffect.TriggerExternalChain(target.gameObject, stats);
                }
            }
        }
    }

    private void OnDestroy()
    {
        CleanupVFX();
    }

    private void CleanupVFX()
    {
        if (cloudVFX != null)
        {
            if (cloudVFX.TryGetComponent<MonoBehaviour>(out var autoReturn)) autoReturn.enabled = true;
            GlobalLocalVFXPool.Instance.ReturnVFX("RainCloud", cloudVFX);
            cloudVFX = null;
        }
    }
}