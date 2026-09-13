using System.Collections.Generic;
using ProjectGame.HotFix.Core.Session;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.Gameplay.Player;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    /// <summary>运行时数据适配：ClientId 取 Lobby 名字，NetworkObjectId 取生命实体，绝不混用。</summary>
    public sealed class GameplayHUDRuntimeBinding
    {
        private readonly List<HUDPlayerBinding> _roster = new();
        private PlayerRuntime _local;
        private PlayerWeaponController _weapon;

        public void Synchronize(GameplayHUDModel model)
        {
            _roster.Clear();
            PlayerManager manager = PlayerManager.Instance;
            PlayerRuntime local = manager != null && manager.IsInitialized ? manager.LocalPlayer : null;
            if (local != _local || (local != null && _weapon == null))
            {
                _local = local;
                _weapon = local == null ? null : local.GetComponent<PlayerWeaponController>();
            }
            if (manager != null && manager.IsInitialized)
                for (int i = 0; i < manager.RuntimePlayers.Count; i++)
                {
                    PlayerRuntime player = manager.RuntimePlayers[i];
                    if (player == null || !player.IsSpawned) continue;
                    string name = GameSessionContext.TryGetPlayer(player.ClientId, out var session)
                        ? session.PlayerName : null;
                    // 名单可能先于 Session 数据就绪；回退只用于标识，不用它替换实际 Lobby 用户名。
                    if (string.IsNullOrWhiteSpace(name)) name = "Client " + player.ClientId;
                    _roster.Add(new HUDPlayerBinding(player.ClientId, player.NetworkObjectId, name, player == local));
                }
            model.Synchronize(_roster, PlayerHealthRuntimeService.Instance, _weapon);
        }
    }
}
