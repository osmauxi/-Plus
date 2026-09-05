using ProjectGame.HotFix.Core.Events;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Events
{
    /// <summary>
    /// Camera Service 已完成订阅和 Cinemachine 管线初始化 
    /// 默认目标发布方可据此重发当前目标，避免服务热重启或初始化顺序造成一次性事件丢失 
    /// </summary>
    public readonly struct GameplayCameraServiceReadyEvent : ILocalEvent
    {
    }

    /// <summary>
    /// 请求 Gameplay Camera 跟随一组目标 
    ///
    /// Requester 是请求身份：同一请求方再次发布会更新旧请求，Release 时也用它移除 
    /// Priority 越高越优先；同优先级下最后发布的请求生效，因此过场、房间观察等系统
    /// 可以临时覆盖本地玩家视角，释放后自动回到下一条有效请求 
    /// </summary>
    public readonly struct GameplayCameraTargetRequestedEvent : ILocalEvent
    {
        public readonly object Requester;
        public readonly Transform FollowTarget;
        public readonly Transform LookAtTarget;
        public readonly Vector3 FollowOffset;
        public readonly Vector3 LookAtOffset;
        public readonly int Priority;
        public readonly bool Snap;

        public GameplayCameraTargetRequestedEvent(
            object requester,
            Transform followTarget,
            Transform lookAtTarget = null,
            int priority = 0,
            bool snap = true,
            Vector3 followOffset = default,
            Vector3 lookAtOffset = default)
        {
            Requester = requester;
            FollowTarget = followTarget;
            LookAtTarget = lookAtTarget != null ? lookAtTarget : followTarget;
            FollowOffset = followOffset;
            LookAtOffset = lookAtOffset;
            Priority = priority;
            Snap = snap;
        }
    }

    /// <summary>释放指定请求方的 Camera Target；若它正在生效，相机会恢复到次高优先级请求 </summary>
    public readonly struct GameplayCameraTargetReleasedEvent : ILocalEvent
    {
        public readonly object Requester;

        public GameplayCameraTargetReleasedEvent(object requester)
        {
            Requester = requester;
        }
    }

    /// <summary>
    /// 请求当前 Gameplay Camera 立即清除跟随惯性 
    /// 用于换层、传送、复活和预测硬校正；不改变当前目标 
    /// </summary>
    public readonly struct GameplayCameraSnapRequestedEvent : ILocalEvent
    {
    }

    /// <summary>
    /// Gameplay 世界相机变化通知 玩家输入层通过观察该事件取得屏幕射线相机，
    /// 不需要认识具体 Camera Controller 或查询场景单例 
    /// </summary>
    public readonly struct GameplayWorldCameraChangedEvent : ILocalEvent
    {
        public readonly Camera WorldCamera;

        public GameplayWorldCameraChangedEvent(Camera worldCamera)
        {
            WorldCamera = worldCamera;
        }
    }
}
