using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Input;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Weapon.Effects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ProjectGame.HotFix.UI.Gameplay.EffectRoll
{
    /// <summary>
    /// Effect Roll的业务编排层：绑定本地玩家、转换数据、控制动效并提交选择
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EffectRollPresenter : BaseGameplayPresenter<EffectRollView>
    {
        private const int OfferSize = 3;
        private static readonly HashSet<string> MissingIconWarnings = new();
        private readonly List<AsyncOperationHandle<Sprite>> _iconHandles = new();
        private readonly List<EffectRollCardModel> _models = new(OfferSize);
        private IEffectRollOfferSource _source;
        private EffectRollOffer _offer;
        private bool _selectionPending;

        public override GameplayUIId Id => GameplayUIId.EffectRoll;

        protected override void OnInitialize()
        {
            View.CardClicked += HandleCardClicked;
            View.CardFocusChanged += HandleCardFocusChanged;
            EnsureSourceBinding();
        }

        private void Update()
        {
            if (!IsInitialized) 
                return;
            EnsureSourceBinding();
            EnsureExistingOfferIsShown();
            HandleServerDebugInput();
        }

        protected override void RenderView()
        {
            //优先拿出业务层当前的权威Offer
            _offer = _source?.CurrentEffectRollOffer ?? _offer;
            ReleaseIcons();
            _models.Clear();
            //无Offer渲染空页面
            if (_offer == null)
            {
                View.Render(string.Empty, _models);
                return;
            }

            int count = Mathf.Min(OfferSize, _offer.Options.Count);
            for (int i = 0; i < count; i++)
            {
                EffectRollOption option = _offer.Options[i];
                // 升级时仍展示完整效果本身，避免暴露“再次按获取顺序应用”之类实现说明。
                string description = option.Presentation.Description;
                string pool = _offer.Pool == WeaponEffectRollPool.Standard ? "普通" : "异变";
                string chaos = _offer.IsChaos ? " · Chaos" : string.Empty;
                string tag = $"{pool}{chaos}    Lv.{option.CurrentLevel} → Lv.{option.NextLevel}";
                _models.Add(new EffectRollCardModel(option.Effect.Id,
                    option.Presentation.DisplayName, description, tag,
                    option.Presentation.IconAddress));
            }

            string title = _offer.Pool == WeaponEffectRollPool.Standard ? "武器强化" : "异变选择";
            if (_offer.IsChaos) 
                title += " · Chaos";
            //数据准备好之后全部丢给View
            View.Render(title, _models);
            BeginLoadIcons(_offer.Id);
        }

        protected override void OnShown()
        {
            View.SetCardsInteractable(IsInteractive && !_selectionPending);
            View.PlayEnter();
        }

        protected override void OnHidden()
        {
            ReleaseIcons();
            View.ResetCards();
        }

        protected override void OnClosed()
        {
            _selectionPending = false;
            _offer = null;
        }

        protected override void OnInteractionChanged(bool interactive)
            => View.SetCardsInteractable(interactive && !_selectionPending);

        protected override void OnShutdown()
        {
            ReleaseIcons();
            BindSource(null);
            View.CardClicked -= HandleCardClicked;
            View.CardFocusChanged -= HandleCardFocusChanged;
        }

        private void EnsureSourceBinding()
        {
            PlayerRuntime localPlayer = PlayerManager.Instance == null ? null : PlayerManager.Instance.LocalPlayer;
            IEffectRollOfferSource candidate = localPlayer == null
                ? null
                : localPlayer.GetComponent<PlayerWeaponController>();
            if (!ReferenceEquals(candidate, _source)) 
                BindSource(candidate);
        }

        private void BindSource(IEffectRollOfferSource source)
        {
            if (_source != null)
            {
                _source.EffectRollOffered -= HandleOffer;
                _source.EffectRollResolved -= HandleResolved;
            }
            _source = source;
            if (_source == null) return;
            _source.EffectRollOffered += HandleOffer;
            _source.EffectRollResolved += HandleResolved;
            if (_source.CurrentEffectRollOffer != null) 
                _offer = _source.CurrentEffectRollOffer;
        }

        private void EnsureExistingOfferIsShown()
        {
            if (_source?.CurrentEffectRollOffer == null || GameplayUIManager.Instance == null ||
                GameplayUIManager.Instance.IsOpen(Id)) return;
            _offer = _source.CurrentEffectRollOffer;
            Navigator.Show(Id);
        }

        private void HandleServerDebugInput()
        {
            InputManager input = InputManager.Instance;
            GameplayUIManager ui = GameplayUIManager.Instance;
            GameplayUIId top = ui == null ? GameplayUIId.None : ui.TopUI;
            if (input == null || !input.IsInitialized || _source == null || ui == null ||
                !ui.IsGameplayActive ||
                (top != GameplayUIId.None && top != GameplayUIId.EffectRoll)) return;

            if (input.EffectRollStandardPressedThisFrame)
                _source.RequestEffectRollBroadcast(WeaponEffectRollPool.Standard);
            else if (input.EffectRollMutationPressedThisFrame)
                _source.RequestEffectRollBroadcast(WeaponEffectRollPool.Mutation);
        }
        /// <summary>
        /// 接收新的Effect Roll，并保证页面打开或刷新
        /// </summary>
        private void HandleOffer(EffectRollOffer offer)
        {
            _offer = offer;
            _selectionPending = false;
            if (IsVisible) 
                Refresh();
            else Navigator.Show(Id);
        }

        private void HandleResolved(uint offerId, ushort effectId, EffectRollSelectionResult result)
        {
            if (_offer == null || _offer.Id != offerId) return;
            _selectionPending = false;
            if (result == EffectRollSelectionResult.Success ||
                result == EffectRollSelectionResult.AcquireRejected ||
                result == EffectRollSelectionResult.Cancelled)
            {
                _offer = null;
                return;
            }
            View.SetCardsInteractable(IsInteractive);
        }

        private void HandleCardClicked(int index)
        {
            if (!IsInteractive || _selectionPending || _offer == null ||
                index < 0 || index >= _offer.Options.Count) return;
            _selectionPending = true;
            View.SetCardsInteractable(false);
            View.PlayCardPressed(index);
            bool sent = _source != null && _source.RequestEffectSelection(
                _offer.Id, _offer.Options[index].Effect.Id);
            if (sent) return;
            _selectionPending = false;
            View.SetCardsInteractable(IsInteractive);
        }

        private void HandleCardFocusChanged(int index, bool focused)
        {
            if (IsInteractive && !_selectionPending) View.PlayCardFocused(index, focused);
        }
        /// <summary>
        /// 加载所有需要的图片资源
        /// </summary>
        private void BeginLoadIcons(uint offerId)
        {
            for (int i = 0; i < _models.Count; i++)
            {
                string address = _models[i].IconAddress;
                if (!string.IsNullOrWhiteSpace(address)) 
                    LoadIconAsync(i, offerId, address);
            }
        }

        private async void LoadIconAsync(int index, uint offerId, string address)
        {
            try
            {
                if (!HasSpriteLocation(address))
                {
                    if (MissingIconWarnings.Add(address))
                        Debug.LogWarning($"[EffectRollPresenter] 图标地址未注册，使用空占位：{address}", this);
                    return;
                }

                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
                _iconHandles.Add(handle);
                Sprite sprite = await handle.Task;
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded && IsVisible &&
                    _offer != null && _offer.Id == offerId && index < _models.Count)
                    View.SetCardIcon(index, sprite);
            }
            catch (Exception exception)
            {
                if (IsVisible && _offer != null && _offer.Id == offerId)
                    Debug.LogWarning($"[EffectRollPresenter] 图标加载失败：{address}\n{exception.Message}", this);
            }
        }

        private static bool HasSpriteLocation(string address)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(address, typeof(Sprite), out IList<IResourceLocation> locations) &&
                    locations != null && locations.Count > 0)
                    return true;
            }
            return false;
        }

        private void ReleaseIcons()
        {
            for (int i = 0; i < _iconHandles.Count; i++)
                if (_iconHandles[i].IsValid()) Addressables.Release(_iconHandles[i]);
            _iconHandles.Clear();
        }
    }
}
