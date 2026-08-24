using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// PlayerMotor（玩家运动器）访问场景角色实体所需的最小接口。
    /// Motor 只依赖位置、朝向、位移和坐标转换，不再依赖 MonoBehaviour（Unity 组件基类）生命周期。
    /// </summary>
    public interface IPlayerCharacterBody
    {
        /// <summary>角色当前世界位置。</summary>
        Vector3 Position { get; }

        /// <summary>角色当前世界旋转。</summary>
        Quaternion Rotation { get; set; }

        /// <summary>角色当前世界前方向。</summary>
        Vector3 Forward { get; }

        /// <summary>通过角色碰撞实体执行一次世界空间位移。</summary>
        void Move(Vector3 displacement);

        /// <summary>安全地直接恢复世界位置与旋转，用于回滚、传送和复活。</summary>
        void SetPose(Vector3 position, Quaternion rotation);

        /// <summary>把世界空间方向转换到角色本地空间。</summary>
        Vector3 InverseTransformDirection(Vector3 direction);
    }

    /// <summary>
    /// 使用 Transform（变换）和 CharacterController（Unity 角色控制器）实现角色实体接口。
    /// 该类本身是普通 C# 对象，只包装必须留在场景中的 Unity 内置组件。
    /// </summary>
    public sealed class CharacterControllerPlayerBody : IPlayerCharacterBody
    {
        /// <summary>提供位置、旋转和坐标空间转换的场景变换。</summary>
        private readonly Transform _transform;

        /// <summary>执行带碰撞约束位移的 Unity 角色控制器。</summary>
        private readonly CharacterController _characterController;

        /// <inheritdoc />
        public Vector3 Position => _transform.position;

        /// <inheritdoc />
        public Quaternion Rotation
        {
            get => _transform.rotation;
            set => _transform.rotation = value;
        }

        /// <inheritdoc />
        public Vector3 Forward => _transform.forward;

        /// <summary>创建对指定角色 Transform 与 CharacterController 的轻量包装。</summary>
        public CharacterControllerPlayerBody(Transform transform, CharacterController characterController)
        {
            _transform = transform != null ? transform : throw new ArgumentNullException(nameof(transform));
            _characterController = characterController != null
                ? characterController
                : throw new ArgumentNullException(nameof(characterController));
        }

        /// <inheritdoc />
        public void Move(Vector3 displacement)
        {
            _characterController.Move(displacement);
        }

        /// <inheritdoc />
        public void SetPose(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = _characterController.enabled;

            // CharacterController 启用时直接改 Transform 可能使内部位置缓存与场景位置不一致。
            if (wasEnabled)
                _characterController.enabled = false;

            _transform.SetPositionAndRotation(position, rotation);

            if (wasEnabled)
                _characterController.enabled = true;
        }

        /// <inheritdoc />
        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return _transform.InverseTransformDirection(direction);
        }
    }
}
