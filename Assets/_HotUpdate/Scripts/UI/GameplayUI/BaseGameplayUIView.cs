using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>View 负责显示与交互事件，业务订阅和数据转换由 Presenter 实现。</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseGameplayUIView : MonoBehaviour
    {
        [SerializeField] private Selectable _defaultSelection;
        private CanvasGroup _canvasGroup;
        private GameObject _lastSelection;
        private bool _initialized;
        private bool _interactive;

        protected virtual void Awake() => InitializeHidden();

        public void InitializeHidden()
        {
            if (_initialized) 
                return;
            _canvasGroup = GetComponent<CanvasGroup>();
            _initialized = true;
            SetPresentation(false, false);
        }

        /// <summary>
        /// 把抽象的Visible/Interactive变成UI的实际显示、Raycast等状态
        /// </summary>
        public void SetPresentation(bool visible, bool interactive)
        {
            InitializeHidden();
            interactive &= visible;
            //判定是不是刚刚拿到焦点
            bool gainedFocus = interactive && !_interactive;
            //失去交互权时先清焦点
            if (!interactive) 
                RememberAndClearSelection();
            _interactive = interactive;
            //分别控制显隐，交互和遮罩
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = interactive;
            _canvasGroup.blocksRaycasts = interactive;
            OnPresentationChanged(visible, interactive);
            if (gainedFocus) 
                RestoreSelection();
        }

        protected virtual void OnPresentationChanged(bool visible, bool interactive) { }

        private void RememberAndClearSelection()
        {
            //重新获得交互权时回复Selection，失去交互权时保存最后交互的东西方便恢复
            EventSystem system = EventSystem.current;
            //currentSelectedGameObject是当前拥有导航焦点的物体，其实主要用在手柄/键盘交互
            GameObject selected = system == null ? null : system.currentSelectedGameObject;
            if (selected == null || !selected.transform.IsChildOf(transform)) 
                return;
            _lastSelection = selected;
            system.SetSelectedGameObject(null);
        }

        private void RestoreSelection()
        {
            if (EventSystem.current == null) return;
            GameObject target = IsSelectable(_lastSelection) ? _lastSelection :
                _defaultSelection == null ? null : _defaultSelection.gameObject;
            if (IsSelectable(target) && target.transform.IsChildOf(transform))
                EventSystem.current.SetSelectedGameObject(target);
        }

        private static bool IsSelectable(GameObject target)
        {
            if (target == null || !target.activeInHierarchy) return false;
            Selectable selectable = target.GetComponent<Selectable>();
            return selectable != null && selectable.IsActive() && selectable.IsInteractable();
        }
    }
}
