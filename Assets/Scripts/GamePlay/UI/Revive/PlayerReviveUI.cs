using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerReviveUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image ringFill;          // 进度条圆环 (Image Type: Filled)
    public RectTransform buttonIcon;// 按键提示图标 (比如 "F" 键的底图)
    public RectTransform surgeRing; // 圆环的父节点，用来做涌动缩放

    private Tween surgeTween;

    public void ShowUI()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);

        // 1. 整体渐显
        canvasGroup.DOFade(1f, 0.3f);

        // 2. 按钮提示做一个果冻弹跳弹出
        buttonIcon.localScale = Vector3.zero;
        buttonIcon.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        ringFill.fillAmount = 0f;
    }

    public void HideUI()
    {
        StopSurge();
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
    }

    public void UpdateProgress(float progressPercent, bool isReviving)
    {
        ringFill.fillAmount = progressPercent;

        // 根据是否正在被按键救援，动态切换涌动状态
        if (isReviving && surgeTween == null)
        {
            StartSurge();
        }
        else if (!isReviving && surgeTween != null)
        {
            StopSurge();
        }
    }

    private void StartSurge()
    {
        // 让圆环有一个像心脏泵血一样的持续缩放
        surgeTween = surgeRing.DOScale(1.1f, 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 让按钮图标变半透明，暗示玩家“正在生效中”
        buttonIcon.GetComponent<Image>().DOFade(0.5f, 0.2f);
    }

    private void StopSurge()
    {
        if (surgeTween != null)
        {
            surgeTween.Kill();
            surgeTween = null;

            // 恢复原样
            surgeRing.DOScale(1f, 0.2f);
            buttonIcon.GetComponent<Image>().DOFade(1f, 0.2f);
        }
    }
}