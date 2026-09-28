using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    public sealed class HUDAmmoView : MonoBehaviour
    {
        private const float PulsePeak = 1.12f;
        private const float PulseRiseDuration = .08f;
        private const float PulseFallDuration = .14f;

        [SerializeField] private TMP_Text _current, _reserve, _status;
        [SerializeField] private RectTransform _animatedContent;
        private bool _numbersInitialized;
        private bool _lastHasWeapon;
        private bool _lastIsReloading;
        private ushort _lastCurrent;
        private ushort _lastReserve;
        private Color _lastColor;
        private float _pulseElapsed;
        private bool _isPulsing;

        public float PulseScale
        {
            get => _animatedContent.localScale.x;
            set => _animatedContent.localScale = Vector3.one * value;
        }

        public void SetNumbers(bool hasWeapon, ushort current, ushort reserve, bool isReloading, Color color)
        {
            if (!_numbersInitialized || hasWeapon != _lastHasWeapon || current != _lastCurrent)
            {
                if (hasWeapon) _current.SetText("{0:0}", current);
                else _current.text = "--";
            }
            if (!_numbersInitialized || hasWeapon != _lastHasWeapon || reserve != _lastReserve)
            {
                if (hasWeapon) _reserve.SetText("/ {0:0}", reserve);
                else _reserve.text = "/ --";
            }
            if (!_numbersInitialized || isReloading != _lastIsReloading)
                _status.text = isReloading ? "RELOADING" : "AMMO";
            if (!_numbersInitialized || color != _lastColor)
                _current.color = color;

            _numbersInitialized = true;
            _lastHasWeapon = hasWeapon;
            _lastCurrent = current;
            _lastReserve = reserve;
            _lastIsReloading = isReloading;
            _lastColor = color;
        }

        public void PlayPulse()
        {
            _pulseElapsed = 0f;
            _isPulsing = true;
            PulseScale = 1f;
        }

        public void ResetPulse()
        {
            _isPulsing = false;
            _pulseElapsed = 0f;
            PulseScale = 1f;
        }

        private void Update()
        {
            if (!_isPulsing) return;
            _pulseElapsed += Time.unscaledDeltaTime;
            if (_pulseElapsed < PulseRiseDuration)
            {
                PulseScale = Mathf.LerpUnclamped(1f, PulsePeak, _pulseElapsed / PulseRiseDuration);
                return;
            }

            float fallElapsed = _pulseElapsed - PulseRiseDuration;
            if (fallElapsed < PulseFallDuration)
            {
                PulseScale = Mathf.LerpUnclamped(PulsePeak, 1f, fallElapsed / PulseFallDuration);
                return;
            }

            ResetPulse();
        }
    }
}
