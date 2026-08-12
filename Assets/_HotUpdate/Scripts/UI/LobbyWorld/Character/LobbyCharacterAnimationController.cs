using ProjectGame.HotFix.Character;
using UnityEngine;
namespace ProjectGame.HotFix.Lobby
{
    public sealed class LobbyCharacterAnimationController : MonoBehaviour
    {
        [SerializeField] public CharacterAnimationBridge _animationBridge;

        public void EquipWeapon(WeaponView weaponView, WeaponPose pose)
        {
            _animationBridge.BindWeapon(weaponView, pose);
            _animationBridge.TriggerEquip();
        }

        public void UnequipWeapon()
        {
            _animationBridge.UnbindWeapon();
        }
    }
}