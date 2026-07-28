using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

        [Header("高亮配置 (可选，用于 Tab 按钮等场景)")]
        [SerializeField] private Graphic _targetGraphic;       // 高亮时变色的图形组件 (Image/TMP_Text)
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.1f, 1f); // 金黄色

        // 向外暴露的纯 C# 事件
        public event Action OnClicked;

        // 内部缓存
        private Vector3 _originalScale;
        private bool _isInitialized = false;

        /// <summary>在首次交互前缓存按钮的初始缩放。</summary>
        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _originalScale = transform.localScale;
            _isInitialized = true;
        }

        /// <summary>启动时预先缓存按钮初始缩放。</summary>
        private void Start()
        {
            EnsureInitialized();
        }

        /// <summary>禁用按钮时终止动效并恢复初始缩放。</summary>
        private void OnDisable()
        {
            if (!_isInitialized) return;

            // UI 隐藏时，立刻杀死动画并强行复原，防止下次打开时变形
            transform.DOKill();
            transform.localScale = _originalScale;
        }

        /// <summary>鼠标进入时平滑放大按钮。</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>鼠标离开时平滑恢复按钮缩放。</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            transform.DOScale(_originalScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>鼠标按下时缩小按钮以模拟物理按压。</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();
            // 按压动画通常要更快一点，显得干脆
            transform.DOScale(_originalScale * _pressScale, _tweenDuration * 0.5f).SetEase(Ease.OutQuad);
        }

        /// <summary>鼠标抬起时把按钮回弹到悬停缩放。</summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            EnsureInitialized();
            transform.DOKill();

            // 抬起时，由于鼠标还在按钮上，所以目标缩放是 HoverScale
            // 使用 Ease.OutBack 产生超过目标值再缩回来的“果汁感”回弹
            transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutBack);
        }

        /// <summary>把指针点击转发为通用按钮点击事件。</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        /// <summary>
        /// 设置高亮状态 (Tab 按钮场景使用)
        /// 通过改变 _targetGraphic 颜色来区分选中/非选中
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            if (_targetGraphic == null) return;
            _targetGraphic.DOKill();
            _targetGraphic.DOColor(highlight ? _highlightColor : _normalColor, _tweenDuration);
        }
    }
}
