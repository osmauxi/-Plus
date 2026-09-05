using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// [View] 单个装备槽位的纯视觉组件
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EquipmentSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("基础配置")]
        public int SlotIndex;

        [Header("动效配置")]
        [SerializeField] private float _hoverOffsetY = 30f;
        [SerializeField] private float _tweenDuration = 0.2f;

        public event Action<int> OnSlotClicked;

        private RectTransform _rectTransform;
        private float _originalY;
        private Vector3 _originalScale;

        /// <summary>缓存装备槽的初始位置与缩放 </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalY = _rectTransform.anchoredPosition.y;
            _originalScale = transform.localScale;
        }

        /// <summary>禁用时终止动效并恢复装备槽初始外观 </summary>
        private void OnDisable()
        {
            _rectTransform.DOKill();
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _originalY);
            transform.localScale = _originalScale; // 禁用时强制洗白，防止带着畸变隐藏
        }

        /// <summary>鼠标进入时抬升装备槽并播放悬停动效 </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _rectTransform.DOKill();
            transform.localScale = _originalScale; // 防御：只要鼠标滑入，强行矫正缩放
            _rectTransform.DOAnchorPosY(_originalY + _hoverOffsetY, _tweenDuration).SetEase(Ease.OutBack);
        }

        /// <summary>鼠标离开时把装备槽恢复到初始位置 </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _rectTransform.DOKill();
            transform.localScale = _originalScale; // 防御：只要鼠标滑出，强行矫正缩放
            _rectTransform.DOAnchorPosY(_originalY, _tweenDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>点击时上报槽位索引并播放反馈动效 </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(SlotIndex);
            transform.localScale = _originalScale;
            transform.DOPunchScale(Vector3.one * -0.1f, 0.15f, 1);
        }
    }
}
