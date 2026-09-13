using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    /// <summary>V：只保存引用和应用显示值；不读业务源、不启动 Tween。</summary>
    public sealed class HUDHealthBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerName, _healthText, _shieldText;
        [SerializeField] private Image _healthFill, _bufferFill, _shieldFill;
        public float HealthFill { get => _healthFill.fillAmount; set => _healthFill.fillAmount = Mathf.Clamp01(value); }
        public float BufferFill { get => _bufferFill.fillAmount; set => _bufferFill.fillAmount = Mathf.Clamp01(value); }
        public float ShieldFill { get => _shieldFill.fillAmount; set => _shieldFill.fillAmount = Mathf.Clamp01(value); }

        public void SetIdentity(string playerName)
        {
            _playerName.richText = false;
            _playerName.text = playerName;
        }
        public void SetNumbers(string health, string shield) { _healthText.text = health; _shieldText.text = shield; }
        public void SetBufferColor(Color color) => _bufferFill.color = color;
    }
}
