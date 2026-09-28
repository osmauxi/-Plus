using System.Collections.Generic;
using DG.Tweening;
using ProjectGame.HotFix.Voice;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    /// <summary>P：编排 HUD 数据、动态队友条，以及全部 DOTween 触发与回收。</summary>
    public sealed class GameplayHUDPresenter : BaseGameplayPresenter<GameplayHUDView>
    {
        [SerializeField, Min(0)] private float _fastDuration = .16f;
        [SerializeField, Min(0)] private float _bufferDelay = .18f;
        [SerializeField, Min(0)] private float _bufferDuration = .42f;
        private readonly GameplayHUDModel _model = new();
        private readonly GameplayHUDRuntimeBinding _runtime = new();
        private readonly Dictionary<ulong, BarPresentation> _bars = new();
        private readonly HashSet<ulong> _alive = new();
        private readonly List<ulong> _removed = new();
        private Sequence _contentTween;
        private HUDAmmoState _lastAmmo;
        private bool _rendered;

        public override GameplayUIId Id => GameplayUIId.GameplayHUD;

        private sealed class BarPresentation
        {
            public HUDHealthBarView View;
            public ulong ClientId;
            public Sequence HealthTween;
            public Tween ShieldTween;
            public float Health, Buffer, Shield;
            public bool HasValue;
        }

        protected override void OnInitialize()
        {
            _model.Changed += Refresh;
            VoiceManager.Instance.ActivityChanged += HandleVoiceActivityChanged;
        }
        private void Update()
        {
            if (IsInitialized) _runtime.Synchronize(_model);
        }

        protected override void OnShown()
        {
            View.ContentAlpha = 0;
            _contentTween?.Kill();
            _contentTween = DOTween.Sequence().SetUpdate(true)
                .Append(DOTween.To(() => View.ContentAlpha, x => View.ContentAlpha = x, 1, .2f))
                .SetLink(View.gameObject);
        }

        protected override void RenderView()
        {
            _alive.Clear();
            for (int i = 0; i < _model.Players.Count; i++)
            {
                HUDPlayerState state = _model.Players[i];
                _alive.Add(state.Player.EntityId);
                if (!_bars.TryGetValue(state.Player.EntityId, out var bar))
                {
                    var row = state.Player.IsLocal ? View.LocalHealth : View.CreateRemoteHealth();
                    row.gameObject.SetActive(true);
                    bar = new BarPresentation { View = row };
                    _bars.Add(state.Player.EntityId, bar);
                }
                bar.ClientId = state.Player.ClientId;
                bar.View.SetIdentity(state.Player.PlayerName);
                RenderVoiceState(bar);
                AnimateHealth(bar, state);
            }
            _removed.Clear();
            foreach (var pair in _bars)
                if (!_alive.Contains(pair.Key)) _removed.Add(pair.Key);
            foreach (ulong id in _removed)
            {
                BarPresentation bar = _bars[id];
                Kill(bar);
                if (bar.View != View.LocalHealth) View.RemoveRemoteHealth(bar.View);
                _bars.Remove(id);
            }
            RenderAmmo(_model.Ammo);
            _rendered = true;
        }

        private void HandleVoiceActivityChanged(
            ulong clientId,
            VoiceActivityState state)
        {
            foreach (BarPresentation bar in _bars.Values)
            {
                if (bar.ClientId == clientId)
                {
                    bar.View.SetVoiceState(
                        state != VoiceActivityState.Closed,
                        state == VoiceActivityState.Speaking);
                    return;
                }
            }
        }

        private static void RenderVoiceState(BarPresentation bar)
        {
            VoiceActivityState state =
                VoiceManager.Instance.GetActivity(bar.ClientId);

            bar.View.SetVoiceState(
                state != VoiceActivityState.Closed,
                state == VoiceActivityState.Speaking);
        }

        private void AnimateHealth(BarPresentation bar, HUDPlayerState state)
        {
            if (!state.HasHealth)
            {
                Kill(bar);
                bar.HasValue = false;
                bar.Health = bar.Buffer = bar.Shield = 0;
                bar.View.HealthFill = bar.View.BufferFill = bar.View.ShieldFill = 0;
                bar.View.SetNumbers("-- / --", "-- / --");
                return;
            }
            float health = Mathf.Clamp01(state.HealthRatio), shield = Mathf.Clamp01(state.ShieldRatio);
            bar.View.SetNumbers(
                Mathf.CeilToInt(state.Health.CurrentHealth), Mathf.CeilToInt(state.Health.MaxHealth),
                Mathf.CeilToInt(state.Health.CurrentShield), Mathf.CeilToInt(state.Health.MaxShield));
            if (!bar.HasValue || !_rendered)
            {
                Kill(bar); bar.HasValue = true;
                bar.Health = bar.Buffer = health; bar.Shield = shield;
                bar.View.HealthFill = bar.View.BufferFill = health; bar.View.ShieldFill = shield;
                return;
            }
            bar.HealthTween?.Kill();
            bool healing = health > bar.Health;
            bar.View.SetBufferColor(healing ? new Color(.25f, 1f, .65f, .95f) : new Color(1f, .78f, .18f, .95f));
            bar.HealthTween = DOTween.Sequence().SetUpdate(true).SetLink(bar.View.gameObject);
            if (healing)
            {
                bar.HealthTween.Append(DOTween.To(() => bar.Buffer, x => { bar.Buffer = x; bar.View.BufferFill = x; }, health, _fastDuration))
                    .AppendInterval(_bufferDelay)
                    .Append(DOTween.To(() => bar.Health, x => { bar.Health = x; bar.View.HealthFill = x; }, health, _bufferDuration));
            }
            else
            {
                bar.HealthTween.Append(DOTween.To(() => bar.Health, x => { bar.Health = x; bar.View.HealthFill = x; }, health, _fastDuration))
                    .AppendInterval(_bufferDelay)
                    .Append(DOTween.To(() => bar.Buffer, x => { bar.Buffer = x; bar.View.BufferFill = x; }, health, _bufferDuration));
            }
            bar.ShieldTween?.Kill();
            bar.ShieldTween = DOTween.To(() => bar.Shield, x => { bar.Shield = x; bar.View.ShieldFill = x; }, shield, _fastDuration)
                .SetUpdate(true).SetLink(bar.View.gameObject);
        }

        private void RenderAmmo(HUDAmmoState ammo)
        {
            Color color = ammo.HasWeapon && ammo.Current == 0 ? new Color(1f, .3f, .25f) : Color.white;
            View.Ammo.SetNumbers(ammo.HasWeapon, ammo.Current, ammo.Reserve, ammo.IsReloading, color);
            if (_rendered && ammo.HasWeapon && ammo.Current != _lastAmmo.Current)
                View.Ammo.PlayPulse();
            _lastAmmo = ammo;
        }

        protected override void OnHidden() => KillTweens();
        protected override void OnShutdown()
        {
            _model.Changed -= Refresh;
            if (VoiceManager.Instance != null)
                VoiceManager.Instance.ActivityChanged -= HandleVoiceActivityChanged;
            KillTweens();
            foreach (var pair in _bars)
                if (pair.Value.View != View.LocalHealth) View.RemoveRemoteHealth(pair.Value.View);
            _bars.Clear();
        }
        private void KillTweens()
        {
            _contentTween?.Kill(); _contentTween = null;
            View.Ammo.ResetPulse();
            foreach (var pair in _bars) Kill(pair.Value);
        }
        private static void Kill(BarPresentation bar)
        { bar.HealthTween?.Kill(); bar.HealthTween = null; bar.ShieldTween?.Kill(); bar.ShieldTween = null; }
    }
}
