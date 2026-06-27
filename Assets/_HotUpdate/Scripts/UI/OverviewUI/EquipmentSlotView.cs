using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening; 
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// [View] 单个装备槽位的纯视觉组件
    /// 只负责检测鼠标悬浮、播放 DOTween 动画、抛出点击事件
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EquipmentSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("基础配置")]
        [Tooltip("给这个槽位编个号，比如 0=武器, 1=技能...")]
        public int SlotIndex;

        [Header("动效配置")]
        [SerializeField] private float _hoverOffsetY = 30f;  // 抬升的高度
        [SerializeField] private float _tweenDuration = 0.2f; // 动画时长

        public event Action<int> OnSlotClicked;

        private RectTransform _rectTransform;
        private float _originalY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalY = _rectTransform.anchoredPosition.y; // 记录初始位置
        }

        private void OnDisable()
        {
            // 防止 UI 突然隐藏时 DOTween 还在跑导致报错
            _rectTransform.DOKill();
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _originalY);
        }

        // =========================================================
        // 鼠标事件侦听与 DOTween 表现
        // =========================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 鼠标移入：带有回弹效果的升起 (Ease.OutBack 是神级缓动曲线)
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(_originalY + _hoverOffsetY, _tweenDuration).SetEase(Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 鼠标移出：平滑落下
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(_originalY, _tweenDuration).SetEase(Ease.OutQuad);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 鼠标点击：只抛出事件，绝不写业务逻辑
            OnSlotClicked?.Invoke(SlotIndex);

            // 可选：点下去的瞬间给个小缩放反馈
            transform.DOPunchScale(Vector3.one * -0.1f, 0.15f, 1);
        }
    }
}