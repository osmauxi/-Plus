using System;
using UnityEngine;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>角色可挂载装备的逻辑槽位。</summary>
    public enum EquipmentSlot
    {
        None,
        LeftHand,
        RightHand,
        Back,
        Chest,
        HipLeft,
        HipRight,
    }

    /// <summary>大厅武器对应的 Animator 姿势编号。</summary>
    public enum EquipmentPose
    {
        Rifle = 0,
        Pistol = 1,
    }

    /// <summary>提供角色装备挂点，并把武器姿势传入 Animator。</summary>
    [DisallowMultipleComponent]
    public class CharacterSocketProvider : MonoBehaviour
    {
        private static readonly int EquipmentPoseHash = Animator.StringToHash("EquipmentPose");
        private static readonly string DoEquip = "DoEquip";

        public Transform[] EquipmentSlots;

        [SerializeField] private Animator _animator;

        /// <summary>按 EquipmentSlot 获取对应挂点，数组索引比枚举值小 1。</summary>
        public Transform GetEquipmentSocket(EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.None)
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "None 没有对应的角色挂点");

            int index = (int)slot - 1;
            if ((uint)index >= (uint)EquipmentSlots.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slot),
                    slot,
                    $"角色只配置了 {EquipmentSlots.Length} 个装备挂点");
            }

            return EquipmentSlots[index];
        }

        /// <summary>把武器配置中的姿势整数传入 Animator。</summary>
        public void SetEquipmentPose(int equipmentPose)
        {
            _animator.SetInteger(EquipmentPoseHash, equipmentPose);
        }

        public void TriggerEquip() 
        {
            _animator.SetTrigger(DoEquip);
        }
    }
}
