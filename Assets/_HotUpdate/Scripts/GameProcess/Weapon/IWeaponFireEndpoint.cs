namespace ProjectGame.HotFix.Gameplay.Weapon.Network
{
    /// <summary>
    /// 传输层和单个同步总控之间的最小路由契约 
    /// </summary>
    public interface IWeaponFireEndpoint
    {
        uint OwnerEntityId { get; }
        ulong OwnerClientId { get; }

        void ReceiveFireCommand(ulong senderClientId, in FireCommand command);
    }
}