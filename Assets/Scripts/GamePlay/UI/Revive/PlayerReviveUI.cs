using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerReviveUI : MonoBehaviour
{
    public PlayerController player;

    public CanvasGroup canvasGroup;
    public Image ringFill;
    public RectTransform buttonIcon;
    public RectTransform surgeRing;

    private Tween currentSurgeTween;   // 统管当前正在播放的动画
    private float visualProgress = 0f;
    private Transform mainCameraTransform;

    // 记录当前的动画状态，防止每帧重复触发
    private bool isCurrentlyReviving = false;

    private void Awake()
    {
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (mainCameraTransform != null && gameObject.activeSelf)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    private void Update()
    {
        if (player == null || !gameObject.activeSelf) return;

        bool isReviving = player.isBeingRevived.Value;

        // 1. 处理进度条的数值增减
        if (isReviving)
        {
            visualProgress += Time.deltaTime;
        }
        else
        {
            visualProgress = Mathf.Max(0, visualProgress - Time.deltaTime * 0.5f);
        }
        ringFill.fillAmount = visualProgress / player.maxReviveTime;

        // 2. 状态监听：只有当救援状态发生变化时，才切换动画
        if (isReviving != isCurrentlyReviving)
        {
            isCurrentlyReviving = isReviving;
            SwitchSurgeAnimation(isCurrentlyReviving);
        }
    }

    public void ShowUI()
    {
        visualProgress = 0f;
        ringFill.fillAmount = 0f;
        isCurrentlyReviving = false;

        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        canvasGroup.DOFade(1f, 0.3f);

        buttonIcon.localScale = Vector3.zero;
        // 弹出动画结束后，立刻进入“闲置心跳”状态
        buttonIcon.DOScale(1f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            SwitchSurgeAnimation(false);
        });
    }

    public void HideUI()
    {
        if (currentSurgeTween != null) currentSurgeTween.Kill();
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// 核心动画切换逻辑
    /// </summary>
    private void SwitchSurgeAnimation(bool isActiveRescue)
    {
        // 掐断旧动画，并将缩放比例安全归位
        if (currentSurgeTween != null)
        {
            currentSurgeTween.Kill();
            surgeRing.localScale = Vector3.one;
        }

        if (isActiveRescue)
        {
            // 【状态 A：正在被救】—— 急促、连续的吸管涌动
            currentSurgeTween = surgeRing.DOScale(1.15f, 0.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // 按钮图标变半透明，暗示“按键正在生效”
            buttonIcon.GetComponent<Image>().DOFade(0.5f, 0.2f);
        }
        else
        {
            // 【状态 B：无人救援】—— 周期性的闲置心跳 (跳一下 -> 停顿 -> 再跳)
            Sequence idleSeq = DOTween.Sequence();
            idleSeq.Append(surgeRing.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad)) // 快速放大
                   .Append(surgeRing.DOScale(1f, 0.15f).SetEase(Ease.InQuad))  // 快速回弹
                   .AppendInterval(1.5f) // 核心：停顿 1.5 秒
                   .SetLoops(-1, LoopType.Restart); // 无限循环整个序列

            currentSurgeTween = idleSeq;

            // 按钮图标恢复实心，提示玩家“快来按我”
            buttonIcon.GetComponent<Image>().DOFade(1f, 0.2f);
        }
    }
}