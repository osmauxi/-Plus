using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Player;
using ProjectGame.HotFix.Gameplay.Weapon;

namespace ProjectGame.HotFix.UI.Gameplay.HUD
{
    public readonly struct HUDPlayerBinding
    {
        public readonly ulong ClientId, EntityId;
        public readonly string PlayerName;
        public readonly PlayerHealthNetworkState Health;
        public readonly bool IsLocal;
        public HUDPlayerBinding(ulong clientId, ulong entityId, string playerName,
            PlayerHealthNetworkState health, bool isLocal)
        { ClientId = clientId; EntityId = entityId; PlayerName = playerName; Health = health; IsLocal = isLocal; }
    }

    public readonly struct HUDPlayerState : IEquatable<HUDPlayerState>
    {
        public readonly HUDPlayerBinding Player;
        public readonly bool HasHealth;
        public readonly PlayerHealthState Health;
        public float HealthRatio => HasHealth ? Health.CurrentHealth / Health.MaxHealth : 0;
        public float ShieldRatio => HasHealth && Health.MaxShield > 0
            ? Health.CurrentShield / Health.MaxShield : 0;
        public HUDPlayerState(HUDPlayerBinding player, bool hasHealth, PlayerHealthState health)
        { Player = player; HasHealth = hasHealth; Health = health; }
        public bool Equals(HUDPlayerState other) => Player.ClientId == other.Player.ClientId &&
            Player.EntityId == other.Player.EntityId && Player.IsLocal == other.Player.IsLocal &&
            Player.PlayerName == other.Player.PlayerName && HasHealth == other.HasHealth &&
            (!HasHealth || Health.Equals(other.Health));
    }

    public readonly struct HUDAmmoState : IEquatable<HUDAmmoState>
    {
        public readonly bool HasLocalPlayer, HasWeapon, IsReloading;
        public readonly ulong EntityId;
        public readonly ushort WeaponId, Current, Reserve;
        public HUDAmmoState(bool hasLocalPlayer, ulong entityId, bool hasWeapon, WeaponRuntimeState weapon)
        {
            HasLocalPlayer = hasLocalPlayer; EntityId = entityId; HasWeapon = hasWeapon;
            WeaponId = weapon.WeaponId; Current = weapon.CurrentAmmo; Reserve = weapon.ReserveAmmo;
            IsReloading = weapon.IsReloading;
        }
        public bool Equals(HUDAmmoState other) => HasLocalPlayer == other.HasLocalPlayer &&
            EntityId == other.EntityId && HasWeapon == other.HasWeapon && WeaponId == other.WeaponId &&
            Current == other.Current && Reserve == other.Reserve && IsReloading == other.IsReloading;
    }

    /// <summary>
    /// M：把玩家身份、只读 Health、武器快照投影为 HUD 状态，不引用 View 或 DOTween。
    /// 每帧读取小型玩家列表，但只有显示字段变化才提交；不会被射击冷却 Tick 反复触发动画。
    /// 读取当前值也能处理 PlayerWeaponController.Unbind 清空事件、同实例换枪等情况。
    /// </summary>
    public sealed class GameplayHUDModel
    {
        private List<HUDPlayerState> _players = new();
        private List<HUDPlayerState> _next = new();
        public IReadOnlyList<HUDPlayerState> Players => _players;
        public HUDAmmoState Ammo { get; private set; }
        public event Action Changed;

        public bool Synchronize(IReadOnlyList<HUDPlayerBinding> roster, IWeaponStateSource localWeapon)
        {
            _next.Clear();
            bool hasLocal = false;
            ulong localEntity = 0;
            if (roster != null)
                for (int i = 0; i < roster.Count; i++)
                {
                    HUDPlayerBinding player = roster[i];
                    PlayerHealthState snapshot = default;
                    bool available = player.Health != null && player.Health.TryGetHealth(out snapshot);
                    _next.Add(new HUDPlayerState(player, available, snapshot));
                    if (player.IsLocal) { hasLocal = true; localEntity = player.EntityId; }
                }
            SortPlayers(_next);
            WeaponRuntimeState weapon = hasLocal && localWeapon != null && localWeapon.Definition != null
                ? localWeapon.CurrentWeaponState : default;
            var ammo = new HUDAmmoState(hasLocal, localEntity, weapon.IsEquipped, weapon);
            bool changed = !Ammo.Equals(ammo) || _players.Count != _next.Count;
            if (!changed)
                for (int i = 0; i < _next.Count; i++)
                    if (!_players[i].Equals(_next[i])) { changed = true; break; }
            if (!changed) return false;
            var previous = _players; _players = _next; _next = previous;
            Ammo = ammo;
            Changed?.Invoke();
            return true;
        }

        private static int ComparePlayers(HUDPlayerState a, HUDPlayerState b) =>
            a.Player.IsLocal != b.Player.IsLocal ? (a.Player.IsLocal ? -1 : 1) : a.Player.ClientId.CompareTo(b.Player.ClientId);

        private static void SortPlayers(List<HUDPlayerState> players)
        {
            // HUD 玩家列表很小；原地插入排序避免 List.Sort(Comparison<T>) 每帧创建比较器包装对象。
            for (int i = 1; i < players.Count; i++)
            {
                HUDPlayerState value = players[i];
                int destination = i;
                while (destination > 0 && ComparePlayers(players[destination - 1], value) > 0)
                {
                    players[destination] = players[destination - 1];
                    destination--;
                }
                players[destination] = value;
            }
        }
    }
}
