using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI
{
    /// <summary>
    /// 通用异步加载 View 层 —— 挂载在 LoadingUI.prefab 根节点上
    /// 通过 SerializeField 引用 UI 元素，在预制件上拖拽绑定
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private Image _overlayImage;
        [SerializeField] private Image _spinnerImage;
        [SerializeField] private TMP_Text _loadingText;

        /// <summary>
        /// 激活 UI 并启动旋转动画
        /// </summary>
        public void Show(string message)
        {
            if (_loadingText != null)
                _loadingText.text = message;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 停止旋转动画并隐藏 UI（保留 GameObject 以备复用）
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }
}