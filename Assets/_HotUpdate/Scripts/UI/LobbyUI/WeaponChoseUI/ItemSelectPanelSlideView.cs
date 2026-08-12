using DG.Tweening;
using System;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// ItemSelect 子面板的纯 View 表现，只负责水平方向的显隐动画。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ItemSelectPanelSlideView : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField] private float _hiddenPositionX;
        [SerializeField] private float _shownPositionX;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float _moveDuration = 0.55f;

        private RectTransform _rectTransform;
        private float _positionY;
        private Tween _tween;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _positionY = _rectTransform.anchoredPosition.y;
            ResetHiddenImmediately();
        }

        private void OnDestroy()
        {
            KillTween();
        }

        /// <summary>从配置的隐藏 X 坐标滑入显示位置，其他坐标保持不变。</summary>
        public Tween Show()
        {
            KillTween();
            SetPositionX(_hiddenPositionX);
            _tween = _rectTransform
                .DOAnchorPosX(_shownPositionX, _moveDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => _tween = null);
            return _tween;
        }

        /// <summary>从当前位置滑回配置的隐藏 X 坐标。</summary>
        public Tween Hide(Action onComplete = null)
        {
            KillTween();
            _tween = _rectTransform
                .DOAnchorPosX(_hiddenPositionX, _moveDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _tween = null;
                    onComplete?.Invoke();
                });
            return _tween;
        }

        /// <summary>立即终止动画并复位到隐藏位置。</summary>
        public void ResetHiddenImmediately()
        {
            KillTween();

            if (_rectTransform == null)
                _rectTransform = (RectTransform)transform;

            SetPositionX(_hiddenPositionX);
        }

        private void SetPositionX(float positionX)
        {
            _rectTransform.anchoredPosition = new Vector2(positionX, _positionY);
        }

        private void KillTween()
        {
            if (_tween == null)
                return;

            _tween.Kill();
            _tween = null;
        }
    }
}
