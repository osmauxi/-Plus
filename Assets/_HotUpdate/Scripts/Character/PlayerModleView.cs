using System;
using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerModelView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationBridge _animationBridge;

        [Header("Equipment Sockets")]
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _back;
        [SerializeField] private Transform _chest;
        [SerializeField] private Transform _hipLeft;
        [SerializeField] private Transform _hipRight;

        public Animator Animator => _animator;
        public CharacterAnimationBridge AnimationBridge => _animationBridge;

        public Transform GetEquipmentSocket(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.LeftHand => _leftHand,
                EquipmentSlot.RightHand => _rightHand,
                EquipmentSlot.Back => _back,
                EquipmentSlot.Chest => _chest,
                EquipmentSlot.HipLeft => _hipLeft,
                EquipmentSlot.HipRight => _hipRight,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "无效的装备槽位。")
            };
        }
    }
}