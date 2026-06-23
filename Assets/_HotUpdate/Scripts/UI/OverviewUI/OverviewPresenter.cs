using ProjectGame.HotFix.Core.Network;
using ProjectGame.HotFix.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    [RequireComponent(typeof(OverviewView))]
    public class OverviewPresenter : BaseLobbyPresenter
    {
        private OverviewView _view;

        protected override void Awake()
        {
            base.Awake();

            _view = GetComponent<OverviewView>();

            _view.OnReadyClicked += HandleReadyClicked;
            _view.OnWeaponSelectClicked += HandleWeaponSelectClicked;
            _view.OnSettingsClicked += HandleSettingsClicked;
        }

        // =========================================================
        // 权威数据的翻译官 (M -> V)
        // =========================================================
        protected override void RenderView()
        {
            var players = LobbyNetworkManager.Instance.LobbyPlayers;
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            bool foundLocal = false;
            LobbyPlayerState localState = default;

            foreach (var p in players)
            {
                if (p.ClientId == localClientId)
                {
                    localState = p;
                    foundLocal = true;
                    break;
                }
            }

            if (!foundLocal) return;

            _view.SetReadyState(localState.IsReady);

            // 未来这里调用 ConfigManager，用 localState.WeaponId 去配表里查出武器名字传给 View
            _view.SetWeaponInfo(localState.WeaponId);
        }


        private void HandleReadyClicked()
        {
            LobbyNetworkManager.Instance.ToggleReadyServerRpc();
        }

        private void HandleWeaponSelectClicked()
        {
            LobbyUIManager.Instance.ChangeScreen(LobbyScreenState.WeaponSelect);
        }

        private void HandleSettingsClicked()
        {
            Debug.Log("[Overview] 触发系统设置打开逻辑");
        }
    }
}