using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Gameplay.EffectRoll
{
    /// <summary>固定三选一页面；只渲染展示模型并转发卡片交互。</summary>
    [DisallowMultipleComponent]
    public sealed class EffectRollView : BaseGameplayUIView
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private EffectRollCardView[] _cards = Array.Empty<EffectRollCardView>();

        public int CardCount => _cards?.Length ?? 0;
        public event Action<int> CardClicked;
        public event Action<int, bool> CardFocusChanged;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < CardCount; i++)
            {
                EffectRollCardView card = _cards[i];
                if (card == null) 
                    continue;
                card.Clicked += HandleCardClicked;
                card.FocusChanged += HandleCardFocusChanged;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < CardCount; i++)
            {
                EffectRollCardView card = _cards[i];
                if (card == null) continue;
                card.Clicked -= HandleCardClicked;
                card.FocusChanged -= HandleCardFocusChanged;
            }
            CardClicked = null;
            CardFocusChanged = null;
        }

        public void Render(string title, IReadOnlyList<EffectRollCardModel> cards)
        {
            if (_titleText != null) 
                _titleText.text = title ?? string.Empty;
            int count = cards?.Count ?? 0;
            for (int i = 0; i < CardCount; i++)
            {
                bool visible = i < count;
                _cards[i].SetCardVisible(visible);
                if (visible) 
                    _cards[i].Render(i, cards[i]);
            }
        }

        public void SetCardsInteractable(bool interactable)
        {
            for (int i = 0; i < CardCount; i++)
                if (_cards[i] != null && _cards[i].gameObject.activeSelf)
                    _cards[i].SetInteractable(interactable);
        }
        /// <summary>
        /// 进入时三个卡片延迟依次出现的效果
        /// </summary>
        public void PlayEnter()
        {
            for (int i = 0; i < CardCount; i++)
                if (_cards[i] != null && _cards[i].gameObject.activeSelf)
                    _cards[i].PlayEnter(i * 0.06f);
        }

        public void PlayCardFocused(int index, bool focused)
        {
            if (IsValidIndex(index)) 
                _cards[index].PlayFocused(focused);
        }

        public void PlayCardPressed(int index)
        {
            if (IsValidIndex(index)) 
                _cards[index].PlayPressed();
        }

        public void SetCardIcon(int index, Sprite sprite)
        {
            if (IsValidIndex(index)) 
                _cards[index].SetIcon(sprite);
        }

        public void ResetCards()
        {
            for (int i = 0; i < CardCount; i++)
                if (_cards[i] != null) _cards[i].ResetVisual();
        }

        protected override void OnPresentationChanged(bool visible, bool interactive)
        {
            SetCardsInteractable(interactive);
            if (!visible) ResetCards();
        }

        private bool IsValidIndex(int index) => index >= 0 && index < CardCount && _cards[index] != null;
        private void HandleCardClicked(int index) => CardClicked?.Invoke(index);
        private void HandleCardFocusChanged(int index, bool focused) => CardFocusChanged?.Invoke(index, focused);
    }
}
