using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Navigation
{
    /// <summary>
    /// 运行时导航障碍标记，只提供障碍范围和是否需要跟踪移动
    /// 标记所有运行时动态影响地块的物体
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RuntimeNavigationObstacle : MonoBehaviour
    {
        [SerializeField] private BoxCollider _bounds;
        [SerializeField] private bool _trackMovement;

        public BoxCollider Bounds => _bounds;
        public bool TrackMovement => _trackMovement;
    }
}