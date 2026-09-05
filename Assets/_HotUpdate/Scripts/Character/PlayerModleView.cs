using System;
using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerModelView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationBridge _animationBridge;

        [Header("Gameplay Animation Bones")]
        [Tooltip("非瞄准转弯时追加全身侧倾的模型表现根或专用 Pivot；不要指定会被 Animator 驱动的运动根骨骼 ")]
        [SerializeField] private Transform _leanRoot;
        [Tooltip("程序化水平瞄准的第一段脊柱；Generic 模型需要显式配置，Humanoid 可自动解析 ")]
        [SerializeField] private Transform _animationSpine;
        [Tooltip("程序化水平瞄准的第二段脊柱；Generic 模型需要显式配置，Humanoid 可自动解析 ")]
        [SerializeField] private Transform _animationChest;
        [Tooltip("程序化水平瞄准的第三段脊柱；Generic 模型需要显式配置，Humanoid 可自动解析 ")]
        [SerializeField] private Transform _animationUpperChest;

        [Header("Equipment Sockets")]
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        [Tooltip("独立于手臂、通常位于胸部骨骼下的武器表现根 配置后，RightHand 装备挂在这里并由右手 IK 追随武器主握点；为空时兼容旧角色，继续挂在右手 ")]
        [SerializeField] private Transform _rightHandWeaponRoot;
        [SerializeField] private Transform _back;
        [SerializeField] private Transform _chest;
        [SerializeField] private Transform _hipLeft;
        [SerializeField] private Transform _hipRight;

        public Animator Animator => _animator;
        public CharacterAnimationBridge AnimationBridge => _animationBridge;
        public Transform LeanRoot => _leanRoot;
        public Transform AnimationSpine => _animationSpine;
        public Transform AnimationChest => _animationChest;
        public Transform AnimationUpperChest => _animationUpperChest;

        public Transform GetEquipmentSocket(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.LeftHand => _leftHand,
                EquipmentSlot.RightHand => _rightHandWeaponRoot != null
                    ? _rightHandWeaponRoot
                    : _rightHand,
                EquipmentSlot.Back => _back,
                EquipmentSlot.Chest => _chest,
                EquipmentSlot.HipLeft => _hipLeft,
                EquipmentSlot.HipRight => _hipRight,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "无效的装备槽位 ")
            };
        }
    }
}
