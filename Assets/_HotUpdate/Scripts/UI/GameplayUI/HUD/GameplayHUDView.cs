using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    public sealed class GameplayHUDView : BaseGameplayUIView
    {
        [SerializeField] private HUDHealthBarView _localHealth;
        [SerializeField] private HUDHealthBarView _remoteHealthPrefab;
        [SerializeField] private RectTransform _remoteHealthRoot;
        [SerializeField] private HUDAmmoView _ammo;
        [SerializeField] private TMP_Text _performanceText;
        [SerializeField] private CanvasGroup _content;
        public HUDHealthBarView LocalHealth => _localHealth;
        public HUDAmmoView Ammo => _ammo;
        public float ContentAlpha { get => _content.alpha; set => _content.alpha = value; }
        public void SetPerformance(float fps, float frameTimeMilliseconds) =>
            _performanceText.SetText("FPS {0:0}\n{1:0.0} ms", fps, frameTimeMilliseconds);
        public HUDHealthBarView CreateRemoteHealth() => Instantiate(_remoteHealthPrefab, _remoteHealthRoot, false);
        public void RemoveRemoteHealth(HUDHealthBarView view)
        {
            if (view == null) return;
            view.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(view.gameObject);
            else DestroyImmediate(view.gameObject);
        }
    }
}
