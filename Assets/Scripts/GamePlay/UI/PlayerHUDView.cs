using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerHUDView : MonoBehaviour
{
    [Header("血条绑定")]
    public Image topHealthBar;    // 顶层真实血条 (比如绿色)
    public Image bufferHealthBar; // 底层缓冲血条 (比如白色/红色)

    [Header("弹药绑定")]
    public TextMeshProUGUI currentAmmoText; 
    public TextMeshProUGUI maxAmmoText;

    [Header("换弹UI绑定")]
    public CanvasGroup reloadBarGroup; // 用来控制整体显隐的 CanvasGroup
    public Image reloadFillImage;      // 用来读条的 Image (Image Type 必须设为 Filled)

    [Header("UI 颜色配置")]
    public Color normalAmmoColor = Color.white;
    public Color warningAmmoColor = Color.red;
    public Color damageBufferColor = Color.white;
    public Color healBufferColor = Color.green;

    private float currentHealthPercent = 1f;

    private void Awake()
    {
        if(reloadBarGroup != null)
            reloadBarGroup.alpha = 0f;
    }
    /// <summary>
    /// 更新血条 (包含增减双向缓冲动效)
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        float targetPercent = currentHealth / maxHealth;

        // 如果是扣血
        if (targetPercent < currentHealthPercent)
        {
            bufferHealthBar.color = damageBufferColor;

            // 1. 真实血条瞬间掉下去
            topHealthBar.DOKill();
            topHealthBar.fillAmount = targetPercent;

            // 2. 缓冲条停顿 0.2 秒后，花 0.5 秒平滑追赶真实血条
            bufferHealthBar.DOKill();
            bufferHealthBar.DOFillAmount(targetPercent, 0.5f).SetDelay(0.2f).SetEase(Ease.OutCubic);
        }
        // 如果是回血
        else if (targetPercent > currentHealthPercent)
        {
            bufferHealthBar.color = healBufferColor;

            // 1. 缓冲条(绿条)瞬间涨上去
            bufferHealthBar.DOKill();
            bufferHealthBar.fillAmount = targetPercent;

            // 2.真实血条花 0.5 秒平滑涨上来
            topHealthBar.DOKill();
            topHealthBar.DOFillAmount(targetPercent, 0.5f).SetEase(Ease.OutCubic);
        }

        currentHealthPercent = targetPercent;
    }

    /// <summary>
    /// 更新弹药与低弹警告特效
    /// </summary>
    public void UpdateAmmo(int currentAmmo, int maxAmmo, bool isWarning)
    {
        // 兼容队友面板没有弹药 UI 的情况
        if (currentAmmoText == null || maxAmmoText == null) return;

        currentAmmoText.text = currentAmmo.ToString();
        // 可以在这里加上斜杠，或者你直接在 Unity 里加个静态文本 "/"
        maxAmmoText.text = maxAmmo.ToString();

        // 1. 打断当前弹药文本上可能正在进行的动画（防止高速连发时动画错乱）
        currentAmmoText.transform.DOKill(complete: true);

        // 2. 确定基础状态：警戒状态基础放大 1.3 倍，颜色变红
        float baseScale = isWarning ? 1.3f : 1.0f;
        currentAmmoText.color = isWarning ? warningAmmoColor : normalAmmoColor;

        // 3. 执行跳跃动效：瞬间变大 0.5 倍，然后在 0.15 秒内弹簧般回落到基础大小
        currentAmmoText.transform.localScale = Vector3.one * (baseScale + 0.5f);
        currentAmmoText.transform.DOScale(baseScale, 0.15f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 播放换弹读条动画
    /// </summary>
    /// <param name="duration">换弹需要的时间</param>
    public void PlayReloadAnimation(float duration)
    {
        if (reloadBarGroup == null || reloadFillImage == null) return;

        // 打断之前可能没播完的动画
        reloadFillImage.DOKill();
        reloadBarGroup.DOKill();

        // 1. 瞬间重置进度为 0，并显示 UI
        reloadFillImage.fillAmount = 0f;
        reloadBarGroup.alpha = 1f;

        // 2. 用 DOTween 在 duration 秒内线性 (Linear) 填满进度条
        reloadFillImage.DOFillAmount(1f, duration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 3. 读条满了之后，平滑隐藏掉这个 UI
                reloadBarGroup.DOFade(0f, 0.2f);
            });
    }
}