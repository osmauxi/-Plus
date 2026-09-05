using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.CameraSystem;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Events
{
    /// <summary>
    /// 请求播放一次瞬时镜头效果。
    /// Direction 是可选世界空间方向，仅在当前 Effect 需要方向性表现时使用。
    /// </summary>
    public readonly struct CameraEffectPlayRequestedEvent : ILocalEvent
    {
        public readonly CameraEffectId Id;
        public readonly float Intensity;
        public readonly Vector3 Direction;
        public readonly bool HasDirection;

        public CameraEffectPlayRequestedEvent(CameraEffectId id,float intensity,Vector3 direction,bool hasDirection)
        {
            Id = id;
            Intensity = intensity;
            Direction = direction;
            HasDirection = hasDirection;
        }
    }

    /// <summary>开启或关闭持续镜头效果，如瞄准 </summary>
    public readonly struct CameraEffectSetRequestedEvent : ILocalEvent
    {
        /// <summary>要切换的语义效果标识</summary>
        public readonly CameraEffectId Id;

        /// <summary>启用还是关闭该效果</summary>
        public readonly bool Active;

        /// <summary>启用时的强度系数，关闭时忽略</summary>
        public readonly float Intensity;

        public CameraEffectSetRequestedEvent(CameraEffectId id, bool active, float intensity)
        {
            Id = id;
            Active = active;
            Intensity = intensity;
        }
    }

    /// <summary>
    /// Aim 状态下持续提供当前瞄准世界坐标 
    /// 这里只传递事实数据，不负责决定镜头应该偏移多少 
    /// </summary>
    public readonly struct CameraAimTargetUpdatedEvent : ILocalEvent
    {
        /// <summary>当前指针射线与 Gameplay 水平面相交的世界坐标 </summary>
        public readonly Vector3 WorldPosition;

        public CameraAimTargetUpdatedEvent(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
        }
    }
}
