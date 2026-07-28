using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 显示改键等待提示并阻止底层 UI 交互。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class RebindOverlayView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _promptText;

        private CanvasGroup _canvasGroup;

        /// <summary>
        /// 缓存遮罩 CanvasGroup 并默认隐藏。
        /// </summary>
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        /// <summary>
        /// 显示等待按键提示并拦截底层射线。
        /// </summary>
        public void Show()
        {
            _promptText.text = "请按下新的按键\n按 ESC 取消";
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 隐藏等待按键提示并释放底层交互。
        /// </summary>
        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
