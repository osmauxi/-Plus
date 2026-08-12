using System.Collections;
using UnityEngine;

/// <summary>
/// 实体特效管理器：负责模型材质变化、状态颜色切换等所有表现效果
/// </summary>
public class EntityFXManager : MonoBehaviour
{
    [Header("渲染器引用")]
    public Renderer[] modelRenderers;

    [Header("受击反馈")]
    public Color hitFlashColor = Color.white;
    public float hitFlashDuration = 0.1f;

    [Header("状态颜色")]
    public Color poisonColor = new Color(0.2f, 1f, 0.2f); // 绿色
    public Color frozenColor = new Color(0.3f, 0.5f, 1f); // 蓝色

    private MaterialPropertyBlock propBlock;
    private static readonly int ColorPropURP = Shader.PropertyToID("_BaseColor"); // URP用的名字
    private static readonly int EmissionProperty = Shader.PropertyToID("_EmissionColor");

    private Coroutine activeEffectRoutine;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if (modelRenderers == null || modelRenderers.Length == 0)
            modelRenderers = GetComponentsInChildren<Renderer>();
    }

    // ==========================================
    // 1. 受击闪白 (高优先级叠加)
    // ==========================================
    public void PlayHitFlash()
    {
        if (activeEffectRoutine != null) StopCoroutine(activeEffectRoutine);
        activeEffectRoutine = StartCoroutine(FlashRoutine(hitFlashColor, hitFlashDuration));
    }

    private IEnumerator FlashRoutine(Color targetColor, float duration)
    {
        SetAllRenderersColor(targetColor, true); // 开启自发光增强闪烁感
        yield return new WaitForSeconds(duration);
        ResetAllRenderers();
    }

    // ==========================================
    // 2. 状态色切换 (如中毒、冰冻)
    // ==========================================
    public void SetStatusColor(string status)
    {
        switch (status)
        {
            case "Poison": SetAllRenderersColor(poisonColor); break;
            case "Frozen": SetAllRenderersColor(frozenColor); break;
            case "None": ResetAllRenderers(); break;
        }
    }

    // ==========================================
    // 内部底层绘制工具 (0 GC)
    // ==========================================
    private void SetAllRenderersColor(Color col, bool useEmission = false)
    {
        foreach (var rend in modelRenderers)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor(ColorPropURP, col);
            // 如果要发光，给发光属性赋值
            if (useEmission)
            {
                propBlock.SetColor(EmissionProperty, col * 2f); // 乘 2 让它亮瞎眼
            }
            else
            {
                propBlock.SetColor(EmissionProperty, Color.black); // 关掉发光
            }
            rend.SetPropertyBlock(propBlock);
        }
    }

    public void ResetAllRenderers()
    {
        foreach (var rend in modelRenderers)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.Clear();
            rend.SetPropertyBlock(propBlock);
        }
    }
}