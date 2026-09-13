using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    public sealed class HUDAmmoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _current, _reserve, _status;
        [SerializeField] private RectTransform _animatedContent;
        public float PulseScale
        {
            get => _animatedContent.localScale.x;
            set => _animatedContent.localScale = Vector3.one * value;
        }
        public void SetNumbers(string current, string reserve, string status, Color color)
        { _current.text = current; _reserve.text = reserve; _status.text = status; _current.color = color; }
    }
}
