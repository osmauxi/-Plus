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
        [SerializeField] private Image _voiceImage;
        [SerializeField] private Color _voiceIdleColor =
            new Color(0.48f, 0.52f, 0.58f, 0.9f);
        [SerializeField] private Color _voiceSpeakingColor =
            new Color(0.35f, 1f, 0.62f, 1f);
        private string _lastPlayerName;
        private int _lastCurrentHealth, _lastMaxHealth, _lastCurrentShield, _lastMaxShield;
        private bool _numbersInitialized;
        public float HealthFill { get => _healthFill.fillAmount; set => _healthFill.fillAmount = Mathf.Clamp01(value); }
        public float BufferFill { get => _bufferFill.fillAmount; set => _bufferFill.fillAmount = Mathf.Clamp01(value); }
        public float ShieldFill { get => _shieldFill.fillAmount; set => _shieldFill.fillAmount = Mathf.Clamp01(value); }

        public void SetIdentity(string playerName)
        {
            if (_lastPlayerName == playerName) return;
            _lastPlayerName = playerName;
            _playerName.richText = false;
            _playerName.text = playerName;
        }

        public void SetNumbers(string health, string shield)
        {
            _numbersInitialized = false;
            if (_healthText.text != health) _healthText.text = health;
            if (_shieldText.text != shield) _shieldText.text = shield;
        }

        public void SetNumbers(int currentHealth, int maxHealth, int currentShield, int maxShield)
        {
            if (!_numbersInitialized || currentHealth != _lastCurrentHealth || maxHealth != _lastMaxHealth)
                _healthText.SetText("{0:0} / {1:0}", currentHealth, maxHealth);
            if (!_numbersInitialized || currentShield != _lastCurrentShield || maxShield != _lastMaxShield)
                _shieldText.SetText("{0:0} / {1:0}", currentShield, maxShield);

            _numbersInitialized = true;
            _lastCurrentHealth = currentHealth;
            _lastMaxHealth = maxHealth;
            _lastCurrentShield = currentShield;
            _lastMaxShield = maxShield;
        }
        public void SetBufferColor(Color color) => _bufferFill.color = color;

        /// <summary>
        /// P 层传入麦克风状态，View 只负责显隐和颜色。
        /// </summary>
        public void SetVoiceState(bool microphoneOpen, bool speaking)
        {
            _voiceImage.gameObject.SetActive(microphoneOpen);
            if (microphoneOpen)
                _voiceImage.color =
                    speaking ? _voiceSpeakingColor : _voiceIdleColor;
        }
    }
}
