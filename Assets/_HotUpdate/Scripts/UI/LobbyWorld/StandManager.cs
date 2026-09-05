using System;
using ProjectGame.HotFix.Core.Network;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 展位 UI 控制器 只负责状态展示和交互转发，不持有本地或网络玩家数据 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StandManager : MonoBehaviour
    {
        [SerializeField] private LobbyStandLayout _layout;

        public event Action<int> OnStationClicked;
        public event Action<int> OnStationHoverEntered;
        public event Action<int> OnStationHoverExited;
        public event Action<int, string> OnPlayerNameChanged;
        public event Action<int> OnEmptyStandClicked;

        private LobbyPlayerState?[] _renderedStates;
        private bool[] _localPlayerFlags;
        private bool _nameEditEnabled;
        private int _hoveredStandIndex = -1;//悬停的展位索引


        private void Awake()
        {
            _renderedStates = new LobbyPlayerState?[_layout.Count];
            _localPlayerFlags = new bool[_layout.Count];
            BindStandEvents();
        }

        private void OnDestroy()
        {
            UnbindStandEvents();
        }

        /// <summary>
        /// 把一份玩家状态渲染到指定展位 UI 
        /// </summary>
        public void RenderStand(int index,LobbyPlayerState? state,bool isLocalPlayer,bool showReadyState)
        {
            _renderedStates[index] = state;
            _localPlayerFlags[index] = isLocalPlayer;

            StandView stand = _layout.GetStand(index);
            if (state.HasValue)
            {
                stand.SetVisible(true);
                stand.SetName(state.Value.PlayerName.ToString());
                stand.SetReady(state.Value.IsReady, showReadyState);
                stand.SetNameInteractable(_nameEditEnabled && isLocalPlayer);
                return;
            }

            stand.SetVisible(false);
        }

        /// <summary>
        /// 统一开启或关闭本地玩家的改名交互 
        /// </summary>
        public void SetNameEditEnabled(bool enabled)
        {
            _nameEditEnabled = enabled;
            for (int i = 0; i < _layout.Count; i++)
            {
                bool canEdit = enabled && _renderedStates[i].HasValue && _localPlayerFlags[i];
                _layout.GetStand(i).SetNameInteractable(canEdit);
            }
        }

        /// <summary>
        /// 统一开启或关闭展位 BoxCollider 的点击和悬停检测 
        /// </summary>
        public void SetClickDetectionEnabled(bool enabled)
        {
            if (!enabled && _hoveredStandIndex >= 0)
            {
                OnStationHoverExited?.Invoke(_hoveredStandIndex);
                _hoveredStandIndex = -1;
            }

            for (int i = 0; i < _layout.Count; i++)
                _layout.GetStand(i).ClickCollider.enabled = enabled;
        }

        /// <summary>
        /// 根据展位是否有人，把点击事件转发到上层 
        /// </summary>
        internal void OnClickColliderClicked(int standIndex)
        {
            if (_renderedStates[standIndex].HasValue)
                OnStationClicked?.Invoke(standIndex);
            else
                OnEmptyStandClicked?.Invoke(standIndex);
        }

        /// <summary>
        /// 把展位悬停进入事件开放给大厅上层逻辑 
        /// </summary>
        internal void OnPointerEntered(int standIndex)
        {
            if (_hoveredStandIndex == standIndex)
                return;

            if (_hoveredStandIndex >= 0)
                OnStationHoverExited?.Invoke(_hoveredStandIndex);

            _hoveredStandIndex = standIndex;
            OnStationHoverEntered?.Invoke(standIndex);
        }

        /// <summary>
        /// 把展位悬停离开事件开放给大厅上层逻辑 
        /// </summary>
        internal void OnPointerExited(int standIndex)
        {
            if (_hoveredStandIndex != standIndex)
                return;

            _hoveredStandIndex = -1;
            OnStationHoverExited?.Invoke(standIndex);
        }

        /// <summary>
        /// 绑定所有展位的名字、空位和 3D 点击事件 
        /// </summary>
        private void BindStandEvents()
        {
            for (int i = 0; i < _layout.Count; i++)
            {
                int standIndex = i;
                StandView stand = _layout.GetStand(i);

                Button nameButton = stand.PlayerNameText.GetComponent<Button>();
                nameButton.onClick.RemoveAllListeners();
                nameButton.onClick.AddListener(() => HandleNameClicked(standIndex));

                stand.EmptyClickButton.onClick.RemoveAllListeners();
                stand.EmptyClickButton.onClick.AddListener(() => OnEmptyStandClicked?.Invoke(standIndex));

                StandClickHandler clickHandler = stand.ClickCollider.GetComponent<StandClickHandler>();
                clickHandler.Initialize(standIndex, this);
            }
        }

        /// <summary>
        /// 解除所有由本控制器注册的 UI 事件 
        /// </summary>
        private void UnbindStandEvents()
        {
            for (int i = 0; i < _layout.Count; i++)
            {
                StandView stand = _layout.GetStand(i);
                stand.PlayerNameText.GetComponent<Button>().onClick.RemoveAllListeners();
                stand.EmptyClickButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 校验名字点击目标并向上层发出改名请求 
        /// </summary>
        private void HandleNameClicked(int standIndex)
        {
            if (!_nameEditEnabled)
                return;

            if (!_localPlayerFlags[standIndex])
            {
                Debug.LogWarning($"[StandManager] 展位 {standIndex} 不属于本地玩家，拒绝改名");
                return;
            }

            StandView stand = _layout.GetStand(standIndex);
            stand.BeginNameEdit(newName => OnPlayerNameChanged?.Invoke(standIndex, newName));
        }
    }
}
