using System;
using UnityEngine;

namespace ProjectGame.HotFix.Character
{
    /// <summary>
    /// Character Prefab 的通用动画桥接层。
    ///
    /// 这里只放 Lobby 与 Gameplay 都成立的动画语义，
    /// 不负责移动、攻击、跳跃等 Gameplay 专属状态。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterAnimationBridge : MonoBehaviour
    {
        // 暂时沿用原 Animator 参数名，避免为了重构重新修改 Animator。
        private static readonly int EquipmentPoseHash = Animator.StringToHash("EquipmentPose");
        private static readonly int DoEquipHash = Animator.StringToHash("DoEquip");

        private Animator _animator;
        private WeaponView _weaponView;

        public Animator Animator => _animator;
        public WeaponView CurrentWeaponView => _weaponView;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 绑定当前武器表现，并切换对应持械姿势。
        /// WeaponPose 的数值需要保持与现有 Animator / 配表一致。
        /// </summary>
        public void BindWeapon(WeaponView weaponView, WeaponPose pose)
        {
            _weaponView = weaponView;
            SetWeaponPose(pose);
        }

        public void UnbindWeapon()
        {
            _weaponView = null;
        }

        public void SetWeaponPose(WeaponPose pose)
        {
            Animator.SetInteger(EquipmentPoseHash, (int)pose);
        }

        public void TriggerEquip()
        {
            Animator.SetTrigger(DoEquipHash);
        }

        /// <summary>
        /// Lobby 与 Gameplay 共用左手武器 IK。
        /// Animator Controller 对应 Layer 需要开启 IK Pass。
        /// </summary>
        private void OnAnimatorIK(int layerIndex)
        {
            Animator animator = Animator;

            if (animator == null)
                return;

            Transform offHandGrip = _weaponView != null ? _weaponView.OffHandGrip : null;

            if (offHandGrip == null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
                return;
            }

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, offHandGrip.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, offHandGrip.rotation);
        }
    }
}