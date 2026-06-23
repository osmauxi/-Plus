using UnityEngine;
using System.Collections.Generic;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.Core.Config;
using Unity.Netcode;

namespace ProjectGame.HotFix.UI.Lobby
{
    [RequireComponent(typeof(WeaponSelectView))]
    public class WeaponSelectPresenter : BaseLobbyPresenter
    {
        private WeaponSelectView _view;

        // 模拟武器数据库 (正式项目应从 ConfigManager 加载)
        private List<WeaponInfo> _availableWeapons;
        private int _currentIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            _view = GetComponent<WeaponSelectView>();

            // 1. 初始化模拟数据
            InitMockData();

            // 2. 绑定 View 事件
            _view.OnPrevClicked += () => SwitchWeapon(-1);
            _view.OnNextClicked += () => SwitchWeapon(1);
            _view.OnConfirmClicked += HandleConfirm;
        }

        private void InitMockData()
        {
            _availableWeapons = new List<WeaponInfo>
            {
                new WeaponInfo { Id = 2001, Name = "极寒之牙", Description = "散发着极寒气息的狙击枪，一发入魂。", Power = 0.9f, Speed = 0.2f },
                new WeaponInfo { Id = 2002, Name = "红莲战斧", Description = "近战暴力美学，灼烧一切敌人。", Power = 0.8f, Speed = 0.5f },
                new WeaponInfo { Id = 2003, Name = "脉冲冲锋枪", Description = "极致射速，弹雨风暴。", Power = 0.4f, Speed = 0.9f }
            };
        }

        // =========================================================
        // 权威渲染 (M -> V)
        // =========================================================
        protected override void RenderView()
        {
            // 每次数据变化，同步一下当前预览的索引
            var players = LobbyNetworkManager.Instance.LobbyPlayers;
            ulong localId = NetworkManager.Singleton.LocalClientId;

            foreach (var p in players)
            {
                if (p.ClientId == localId)
                {
                    // 根据权威 WeaponId 找到我们在列表里的 Index
                    _currentIndex = _availableWeapons.FindIndex(w => w.Id == p.WeaponId);
                    if (_currentIndex < 0) _currentIndex = 0;
                    break;
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            var info = _availableWeapons[_currentIndex];
            _view.UpdateWeaponInfo(info.Name, info.Description, info.Power, info.Speed);
        }

        // =========================================================
        // 业务逻辑
        // =========================================================

        private void SwitchWeapon(int direction)
        {
            // 环形算法
            _currentIndex = (_currentIndex + direction + _availableWeapons.Count) % _availableWeapons.Count;

            // ?? 核心：直接发 RPC 修改网络权威数据
            // 由于 AvatarResManager 正在盯着数据看，展台上的 3D 模型会立刻异步加载并切换！
            int targetId = _availableWeapons[_currentIndex].Id;
            LobbyNetworkManager.Instance.ChangeWeaponServerRpc(targetId);

            // 本地 UI 先预刷一下，不等网络回调，体验更丝滑
            UpdateUI();
        }

        private void HandleConfirm()
        {
            // 返回概览界面
            LobbyUIManager.Instance.ChangeScreen(LobbyScreenState.Overview);
        }
    }
}