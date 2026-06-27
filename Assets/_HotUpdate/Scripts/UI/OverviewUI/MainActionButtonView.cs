using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// [View] 通用动态按钮视图 (自带果汁手动效)
    /// 处理悬停放大、按压缩小、松开回弹，并向外抛出纯 C# 点击事件
    /// </summary>
    public class MainActionButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("动效配置 (缩放倍率)")]
        [SerializeField] private float _hoverScale = 1.05f;  // 鼠标悬浮时放大 1.05 倍
        [SerializeField] private float _pressScale = 0.95f;  // 按下去时缩小到 0.95 倍
        [SerializeField] private float _tweenDuration = 0.15f; // 动画时长

        // 向外暴露的纯 C# 事件
        public event Action OnClicked;

        // 内部缓存
        private Vector3 _originalScale;
        private bool _isInitialized = false;

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _originalScale = transform.localScale;
            _isInitialized = true;
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private void OnDisable()
        {
            if (!_isInitialized) return;

            // UI 隐藏时，立刻杀死动画并强行复原，防止下次打开时变形
            transform.DOKill();
            transform.localScale = _originalScale;
        }

        // 1. 鼠标悬浮：平滑放大
        public void OnPointerEnter(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        // 2. 鼠标移出：平滑恢复原状
        public void OnPointerExit(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            transform.DOScale(_originalScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        // 3. 鼠标按下：瞬间缩小，模拟物理按压
        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            // 按压动画通常要更快一点，显得干脆
            transform.DOScale(_originalScale * _pressScale, _tweenDuration * 0.5f).SetEase(Ease.OutQuad);
        }

        // 4. 鼠标抬起：带有 Q 弹回弹效果的恢复
        public void OnPointerUp(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();

            // 抬起时，由于鼠标还在按钮上，所以目标缩放是 HoverScale
            // 使用 Ease.OutBack 产生超过目标值再缩回来的“果汁感”回弹
            transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutBack);
        }

        // 5. 真正的点击事件触发
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }
    }
}