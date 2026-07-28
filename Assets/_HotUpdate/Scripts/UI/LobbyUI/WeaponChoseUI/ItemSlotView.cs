using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 负责显示物品格数据、选中状态和鼠标交互。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ItemSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI 引用")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _highlightFrame;

        [Header("动效配置")]
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _tweenDuration = 0.15f;

        /// <summary>
        /// 点击时向 Presenter 返回当前物品 ID。
        /// </summary>
        public event Action<int> OnClicked;

        private int _itemId;
        private int _bindVersion;
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private AsyncOperationHandle<Sprite> _iconHandle;

        /// <summary>
        /// 缓存矩形组件和初始缩放，并初始化高亮状态。
        /// </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
            _highlightFrame.gameObject.SetActive(false);
        }

        /// <summary>
        /// 回收格子时复位动效，并释放当前图标资源。
        /// </summary>
        private void OnDisable()
        {
            _bindVersion++;
            ReleaseIcon();
            _rectTransform.DOKill();
            _rectTransform.localScale = _originalScale;
            _highlightFrame.gameObject.SetActive(false);
        }

        /// <summary>
        /// 销毁格子时释放仍由该格子持有的图标资源。
        /// </summary>
        private void OnDestroy()
        {
            _bindVersion++;
            ReleaseIcon();
        }

        /// <summary>
        /// 绑定物品基础信息，并从配置中的 Addressables 地址异步加载图标。
        /// </summary>
        public void Bind(ItemSlotData data)
        {
            _bindVersion++;
            //所有绑定都从这里开始，这里持有的handle还是上次加载的资源handle，
            //理论上这里就已经完成了上次加载的资源释放，所以之后所有的_bindVersion检测都只是return不进行重复释放
            //当然，还有Disable与Destroy的释放
            ReleaseIcon();

            _itemId = data.Id;
            _nameText.text = data.Name;
            LoadIconAsync(data.IconPath, _bindVersion).Forget();
        }

        /// <summary>
        /// 设置当前格子的选中高亮状态。
        /// </summary>
        public void SetHighlight(bool active)
        {
            _highlightFrame.gameObject.SetActive(active);
        }

        /// <summary>
        /// 鼠标进入时播放放大动效。
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _rectTransform.DOKill();
            _rectTransform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// 鼠标离开时恢复初始缩放。
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _rectTransform.DOKill();
            _rectTransform.DOScale(_originalScale, _tweenDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// 鼠标点击时抛出物品 ID 并播放按压反馈。
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(_itemId);
            transform.DOPunchScale(Vector3.one * -0.05f, 0.12f, 1);
        }

        /// <summary>
        /// 异步加载图标，并阻止对象池复用后的旧请求覆盖新数据。
        /// </summary>
        private async UniTask LoadIconAsync(string iconAddress, int bindVersion)
        {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
            _iconHandle = handle;

            try
            {
                await handle.ToUniTask();
                if (bindVersion != _bindVersion)
                {
                    return;
                }

                _iconImage.sprite = handle.Result;
                _iconImage.enabled = true;
            }
            catch (Exception exception)
            {
                if (bindVersion != _bindVersion)
                {
                    return;
                }

                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                _iconHandle = default;
                Debug.LogError($"ItemSlot 图标加载失败，Address: {iconAddress}\n{exception}", this);
            }
        }

        /// <summary>
        /// 释放当前 Addressables 图标句柄并清空图片显示。
        /// </summary>
        private void ReleaseIcon()
        {
            if (_iconHandle.IsValid())
            {
                Addressables.Release(_iconHandle);
            }

            _iconHandle = default;
            _iconImage.sprite = null;
            _iconImage.enabled = false;
        }
    }
}
