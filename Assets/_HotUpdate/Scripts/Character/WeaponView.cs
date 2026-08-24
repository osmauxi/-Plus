using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    public class WeaponView : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;
        [Tooltip("右手主握点。为空时使用武器根节点，便于旧武器平滑迁移。")]
        [SerializeField] private Transform _mainHandGrip;
        [SerializeField] private Transform _offHandGrip;

        public Transform Muzzle => _muzzle;
        public Transform MainHandGrip => _mainHandGrip != null ? _mainHandGrip : transform;
        public Transform OffHandGrip => _offHandGrip;
    }
}
