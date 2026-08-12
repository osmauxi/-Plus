using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// Overview 消息卡片的纯 View 表现：更新文本并控制滑入、停留和滑出动画。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class OverviewMessageCardView : MonoBehaviour
    {
        [Header("View References")]
        [SerializeField] private TMP_Text _messageText;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float _moveDuration = 0.55f;
        [SerializeField] private float _hiddenPositionX = -205f;

        private RectTransform _rectTransform;
        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private Sequence _sequence;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _shownPosition = new Vector2(0f, _rectTransform.anchoredPosition.y);
            RecalculateHiddenPosition();
            ResetImmediately();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        /// <summary>
        /// 从屏幕左侧滑入，停留指定时长后再滑回屏幕外。
        /// 新消息会立即替换当前消息并重新开始完整动画。
        /// </summary>
        public void Show(string message, float visibleDuration)
        {
            KillSequence();
            RecalculateHiddenPosition();

            _messageText.text = message;
            _rectTransform.anchoredPosition = _hiddenPosition;

            _sequence = DOTween.Sequence()
                .SetLink(gameObject)
                .SetUpdate(true)
                .Append(_rectTransform
                    .DOAnchorPos(_shownPosition, _moveDuration)
                    .SetEase(Ease.OutCubic))
                .AppendInterval(Mathf.Max(0f, visibleDuration))
                .Append(_rectTransform
                    .DOAnchorPos(_hiddenPosition, _moveDuration)
                    .SetEase(Ease.InCubic))
                .OnComplete(() => _sequence = null);
        }

        /// <summary>立即终止动画并把卡片放回屏幕左侧。</summary>
        public void ResetImmediately()
        {
            KillSequence();
            RecalculateHiddenPosition();
            _rectTransform.anchoredPosition = _hiddenPosition;

            if (_messageText != null)
                _messageText.text = string.Empty;
        }

        private void RecalculateHiddenPosition()
        {
            _hiddenPosition = new Vector2(
                _hiddenPositionX,
                _shownPosition.y);
        }

        private void KillSequence()
        {
            if (_sequence == null)
                return;

            _sequence.Kill();
            _sequence = null;
        }
    }
}
