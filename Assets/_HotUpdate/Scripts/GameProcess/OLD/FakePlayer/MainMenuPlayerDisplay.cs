using System.Collections;
using UnityEngine;
using UnityEngine.VFX; // 【新增】：引入 VFX Graph 命名空间！

public class MainMenuPlayerDisplay : MonoBehaviour
{
    [Header("核心引用")]
    public Animator anim;
    public Transform firePoint;
    public GameObject dummyBulletPrefab;

    [Header("视觉特效 (纯表现)")]
    [Tooltip("枪口的 VFX Graph 组件")]
    public VisualEffect fireVFX;           // 【修改】：改为 VisualEffect 组件
    [Tooltip("抛壳的普通粒子组件")]
    public ParticleSystem shellEject;
    [Tooltip("换弹时的粒子组件")]
    public ParticleSystem reloadVFX;       // 【新增】：换弹粒子特效

    [Header("表演节奏配置")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public int minShots = 3;
    public int maxShots = 8;
    public float fireRate = 0.15f;

    private void Start()
    {
        StartCoroutine(ShowcaseRoutine());
    }

    private IEnumerator ShowcaseRoutine()
    {
        while (true)
        {
            // 1. 发呆站岗
            float idleWait = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleWait);

            // 2. 突突突开枪
            int shots = Random.Range(minShots, maxShots + 1);
            for (int i = 0; i < shots; i++)
            {
                Shoot();
                yield return new WaitForSeconds(fireRate);
            }

            // 3. 停顿一下准备换弹
            yield return new WaitForSeconds(0.5f);
            Reload();

            // 4. 等待换弹动作播完
            yield return new WaitForSeconds(1.5f);
        }
    }

    private void Shoot()
    {
        if (anim != null) anim.SetTrigger("Shoot");

        // ==========================================
        // 【核心修改】：通过 SendEvent 触发你的 VFX 开火事件
        // 注意这里完全保留了你要求的拼写 "VFX_OnFIre"
        // ==========================================
        if (fireVFX != null) fireVFX.SendEvent("VFX_OnFIre");

        if (shellEject != null) shellEject.Play();

        // 现场实例化假子弹
        if (dummyBulletPrefab != null && firePoint != null)
        {
            Instantiate(dummyBulletPrefab, firePoint.position, firePoint.rotation);
        }

        // 播放枪声
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Bullet_Normal, 0.6f);
        }
    }

    private void Reload()
    {
        if (anim != null) anim.SetTrigger("Reload");

        // ==========================================
        // 【新增】：播放换弹时的视觉特效
        // ==========================================
        if (reloadVFX != null) reloadVFX.Play();

        // 播放换弹音效
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Reload, 0.8f);
        }
    }
}