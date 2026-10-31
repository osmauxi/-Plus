using UnityEngine;

namespace ProjectGame.Core.Navigation
{
    /// <summary>房间根节点固定时使用的二维坐标变换；运行时只传值，不触碰 Transform。</summary>
    public readonly struct RoomGridTransformData
    {
        private readonly float _worldToLocalM00;
        private readonly float _worldToLocalM02;
        private readonly float _worldToLocalM20;
        private readonly float _worldToLocalM22;
        private readonly float _worldToLocalOffsetX;
        private readonly float _worldToLocalOffsetZ;
        private readonly float _localToWorldM00;
        private readonly float _localToWorldM02;
        private readonly float _localToWorldM20;
        private readonly float _localToWorldM22;
        private readonly float _localToWorldOffsetX;
        private readonly float _localToWorldOffsetZ;
        private readonly float _halfSize;
        private readonly float _inverseCellSize;

        public RoomGridTransformData(
            Matrix4x4 worldToLocal,
            Matrix4x4 localToWorld,
            float groundY,
            float halfSize,
            float cellSize)
        {
            _worldToLocalM00 = worldToLocal.m00;
            _worldToLocalM02 = worldToLocal.m02;
            _worldToLocalM20 = worldToLocal.m20;
            _worldToLocalM22 = worldToLocal.m22;
            _worldToLocalOffsetX = worldToLocal.m01 * groundY + worldToLocal.m03;
            _worldToLocalOffsetZ = worldToLocal.m21 * groundY + worldToLocal.m23;
            _localToWorldM00 = localToWorld.m00;
            _localToWorldM02 = localToWorld.m02;
            _localToWorldM20 = localToWorld.m20;
            _localToWorldM22 = localToWorld.m22;
            _localToWorldOffsetX = localToWorld.m03;
            _localToWorldOffsetZ = localToWorld.m23;
            _halfSize = halfSize;
            _inverseCellSize = 1f / Mathf.Max(cellSize, 0.000001f);
        }

        public Vector2 WorldToLocalPosition(Vector2 worldPosition) =>
            new(
                _worldToLocalM00 * worldPosition.x +
                    _worldToLocalM02 * worldPosition.y + _worldToLocalOffsetX,
                _worldToLocalM20 * worldPosition.x +
                    _worldToLocalM22 * worldPosition.y + _worldToLocalOffsetZ);

        public Vector2 WorldToLocalVector(Vector2 worldVector) =>
            new(
                _worldToLocalM00 * worldVector.x + _worldToLocalM02 * worldVector.y,
                _worldToLocalM20 * worldVector.x + _worldToLocalM22 * worldVector.y);

        public Vector2 LocalToWorldVector(Vector2 localVector) =>
            new(
                _localToWorldM00 * localVector.x + _localToWorldM02 * localVector.y,
                _localToWorldM20 * localVector.x + _localToWorldM22 * localVector.y);

        public Vector2 LocalCellCenterToWorld(int x, int y)
        {
            float localX = -_halfSize + (x + 0.5f) / _inverseCellSize;
            float localZ = -_halfSize + (y + 0.5f) / _inverseCellSize;
            return new Vector2(
                _localToWorldM00 * localX + _localToWorldM02 * localZ + _localToWorldOffsetX,
                _localToWorldM20 * localX + _localToWorldM22 * localZ + _localToWorldOffsetZ);
        }

        public bool TryGetCell(Vector2 localPosition, int width, int height, out int cellIndex)
        {
            int x = Mathf.FloorToInt((localPosition.x + _halfSize) * _inverseCellSize);
            int y = Mathf.FloorToInt((localPosition.y + _halfSize) * _inverseCellSize);
            if ((uint)x >= (uint)width || (uint)y >= (uint)height)
            {
                cellIndex = -1;
                return false;
            }

            cellIndex = y * width + x;
            return true;
        }
    }

    /// <summary>纯数据移动约束；候选速度顺序与原 HotFix 逻辑保持一致。</summary>
    public static class MovementConstraintKernel
    {
        public static Vector2 Constrain(
            RoomGridTransformData transform,
            byte[] walkable,
            int width,
            int height,
            Vector2 position,
            Vector2 navigationVelocity,
            Vector2 desiredVelocity,
            float deltaTime)
        {
            if (desiredVelocity.sqrMagnitude <= 0.000001f)
                return Vector2.zero;

            if (IsMovementWalkable(
                    transform, walkable, width, height,
                    position, desiredVelocity, deltaTime))
                return desiredVelocity;

            Vector2 local = transform.WorldToLocalVector(desiredVelocity);
            Vector2 slideX = transform.LocalToWorldVector(new Vector2(local.x, 0f));
            Vector2 slideZ = transform.LocalToWorldVector(new Vector2(0f, local.y));

            bool canX = slideX.sqrMagnitude > 0.000001f &&
                        IsMovementWalkable(
                            transform, walkable, width, height,
                            position, slideX, deltaTime);
            bool canZ = slideZ.sqrMagnitude > 0.000001f &&
                        IsMovementWalkable(
                            transform, walkable, width, height,
                            position, slideZ, deltaTime);

            if (canX && canZ)
            {
                float xScore = Vector2.Dot(slideX, navigationVelocity);
                float zScore = Vector2.Dot(slideZ, navigationVelocity);
                if (Mathf.Abs(xScore - zScore) > 0.000001f)
                    return xScore > zScore ? slideX : slideZ;

                return slideX.sqrMagnitude >= slideZ.sqrMagnitude ? slideX : slideZ;
            }

            if (canX)
                return slideX;
            if (canZ)
                return slideZ;

            return IsMovementWalkable(
                    transform, walkable, width, height,
                    position, navigationVelocity, deltaTime)
                ? navigationVelocity
                : Vector2.zero;
        }

        private static bool IsMovementWalkable(
            RoomGridTransformData transform,
            byte[] walkable,
            int width,
            int height,
            Vector2 position,
            Vector2 velocity,
            float deltaTime)
        {
            Vector2 local = transform.WorldToLocalPosition(position + velocity * deltaTime);
            if (!transform.TryGetCell(local, width, height, out int cellIndex))
                return false;
            return (uint)cellIndex < (uint)walkable.Length && walkable[cellIndex] != 0;
        }
    }
}
