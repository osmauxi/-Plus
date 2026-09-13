using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Gameplay.EffectRoll
{
    /// <summary>
    /// 单张Effect Roll卡片的纯表现层。只转发输入，并暴露动效方法供Presenter调用。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class EffectRollCardView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _tagText;
        [SerializeField] private TMP_Text _descriptionText;

        [Header("DOTween")]
        [SerializeField] private float _enterDuration = 0.2f;
        [SerializeField] private float _focusDuration = 0.12f;
        [SerializeField] private float _pressedDuration = 0.14f;
        [SerializeField] private float _enterScale = 0.9f;
        [SerializeField] private float _focusScale = 1.05f;
        [SerializeField] private Color _focusedFrameColor = new(1f, 0.82f, 0.25f, 1f);

        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale;
        private Color _baseFrameColor;
        private Tween _scaleTween;
        private Tween _fadeTween;
        private Tween _colorTween;
        private int _index;
        private bool _visualCacheInitialized;

        public Button Button => _button;
        public event Action<int> Clicked;
        public event Action<int, bool> FocusChanged;

        private void Awake()
        {
            EnsureVisualCache();
            ConfigureRaycastTargets();
            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClicked);
            KillTweens();
            Clicked = null;
            FocusChanged = null;
        }

        public void Render(int index, in EffectRollCardModel model)
        {
            _index = index;
            _nameText.text = model.DisplayName;
            _tagText.text = model.Tag;
            _descriptionText.text = model.Description;
            ClearIcon();
        }

        public void SetCardVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) 
                gameObject.SetActive(visible);
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public void SetIcon(Sprite sprite)
        {
            if (_iconImage == null) 
                return;
            _iconImage.sprite = sprite;
            _iconImage.enabled = sprite != null;
        }

        public void ClearIcon()
        {
            if (_iconImage == null) 
                return;
            _iconImage.sprite = null;
            _iconImage.enabled = false;
        }

        public void PlayEnter(float delay)
        {
            EnsureVisualCache();
            _scaleTween?.Kill();
            _fadeTween?.Kill();
            transform.localScale = _baseScale * _enterScale;
            _canvasGroup.alpha = 0f;
            _scaleTween = transform.DOScale(_baseScale, _enterDuration)
                .SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
            _fadeTween = DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value,
                    1f, _enterDuration)
                .SetDelay(delay).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
        }

        public void PlayFocused(bool focused)
        {
            EnsureVisualCache();
            _scaleTween?.Kill();
            _colorTween?.Kill();
            Vector3 targetScale = _baseScale * (focused ? _focusScale : 1f);
            Color targetColor = focused ? _focusedFrameColor : _baseFrameColor;
            _scaleTween = transform.DOScale(targetScale, _focusDuration)
                .SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
            if (_frameImage != null)
                _colorTween = DOTween.To(() => _frameImage.color, value => _frameImage.color = value,
                        targetColor, _focusDuration)
                    .SetUpdate(true).SetLink(gameObject);
        }

        public void PlayPressed()
        {
            EnsureVisualCache();
            _scaleTween?.Kill();
            _scaleTween = transform.DOPunchScale(-Vector3.one * 0.1f, _pressedDuration, 1, 0.35f)
                .SetUpdate(true).SetLink(gameObject);
        }

        public void ResetVisual()
        {
            EnsureVisualCache();
            KillTweens();
            transform.localScale = _baseScale;
            _canvasGroup.alpha = 1f;
            if (_frameImage != null) _frameImage.color = _baseFrameColor;
        }

        public void KillTweens()
        {
            _scaleTween?.Kill();
            _fadeTween?.Kill();
            _colorTween?.Kill();
            _scaleTween = _fadeTween = _colorTween = null;
        }

        public void OnPointerEnter(PointerEventData eventData) => FocusChanged?.Invoke(_index, true);
        public void OnPointerExit(PointerEventData eventData) => FocusChanged?.Invoke(_index, false);
        public void OnSelect(BaseEventData eventData) => FocusChanged?.Invoke(_index, true);
        public void OnDeselect(BaseEventData eventData) => FocusChanged?.Invoke(_index, false);
        private void HandleClicked() => Clicked?.Invoke(_index);

        private void EnsureVisualCache()
        {
            if (_visualCacheInitialized) return;
            if (_canvasGroup == null) 
                _canvasGroup = GetComponent<CanvasGroup>();
            _baseScale = transform.localScale;
            _baseFrameColor = _frameImage == null ? Color.white : _frameImage.color;
            _visualCacheInitialized = true;
        }

        /// <summary>
        /// 卡片文字和图标位于 Button 图像上方，只负责展示，不能拦截按钮射线。
        /// </summary>
        private void ConfigureRaycastTargets()
        {
            _frameImage.raycastTarget = true;
            _iconImage.raycastTarget = false;
            _nameText.raycastTarget = false;
            _tagText.raycastTarget = false;
            _descriptionText.raycastTarget = false;
        }
    }
}
