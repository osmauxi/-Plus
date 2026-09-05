using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    /// <summary>
    /// 相机事件的统一外部触发接口，负责封装，然后丢到观察者触发 
    /// 调用方只描述“想播放什么效果”，不直接访问Camera、Cinemachine 或具体 Manager 
    /// </summary>
    public static class CameraEffects
    {
        /// <summary>播放一次瞬时效果，例如开枪、爆炸、受击 </summary>
        public static void Play(CameraEffectId id, float intensity = 1f)
        {
            if (id == CameraEffectId.None)
                return;

            LocalEvents.Publish(new CameraEffectPlayRequestedEvent(id, intensity,Vector3.zero,false));
        }
        public static void Play(CameraEffectId id,Vector3 worldDirection,float intensity = 1f)
        {
            if (id == CameraEffectId.None)
                return;

            LocalEvents.Publish(new CameraEffectPlayRequestedEvent(id,intensity,worldDirection,true));
        }
        /// <summary>设置持续效果状态，例如进入/退出瞄准 </summary>
        public static void Set(CameraEffectId id, bool active, float intensity = 1f)
        {
            if (id == CameraEffectId.None)
                return;

            LocalEvents.Publish(new CameraEffectSetRequestedEvent(id, active, intensity));
        }

        /// <summary>
        /// 更新当前 Aim 世界坐标 
        /// 仅负责传递瞄准事实，具体 LookAhead 算法由 CameraCompositionModel 决定 
        /// </summary>
        public static void UpdateAimTarget(Vector3 worldPosition)
        {
            LocalEvents.Publish(new CameraAimTargetUpdatedEvent(worldPosition));
        }
    }
}