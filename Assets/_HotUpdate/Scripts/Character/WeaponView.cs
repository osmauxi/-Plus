using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    public class WeaponView : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _offHandGrip;

        public Transform Muzzle => _muzzle;
        public Transform OffHandGrip => _offHandGrip;
    }
}