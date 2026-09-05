using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 展位 ClickCollider 点击检测辅助脚本
    /// 挂载在 ClickCollider 的 GameObject 上，通过 IPointerClickHandler 转发事件到 StandManager
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class StandClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private int _standIndex = -1;
        private StandManager _standManager;

        /// <summary>
        /// 由 StandManager.BindStandEvents 调用进行初始化
        /// </summary>
        public void Initialize(int standIndex, StandManager manager)
        {
            _standIndex = standIndex;
            _standManager = manager;
        }

        /// <summary>把 EventSystem 的指针点击转发到对应展位 </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            _standManager.OnClickColliderClicked(_standIndex);
        }

        /// <summary>把 EventSystem 的指针进入事件转发到对应展位 </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _standManager.OnPointerEntered(_standIndex);
        }

        /// <summary>把 EventSystem 的指针离开事件转发到对应展位 </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _standManager.OnPointerExited(_standIndex);
        }
    }
}
