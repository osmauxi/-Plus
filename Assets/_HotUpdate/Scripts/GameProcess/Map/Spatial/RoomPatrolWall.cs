using System;
using ProjectGame.HotFix.Gameplay.Navigation;
using ProjectGame.HotFix.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map
{
    /// <summary>房间本地坐标内往返的导航障碍；在怪物模拟前移动，各端共用 NGO 服务器时间。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RuntimeNavigationObstacle), typeof(BoxCollider))]
    public sealed class RoomPatrolWall : MonoBehaviour
    {
        [Tooltip("相对于父节点的巡回起点，随房间 Scale 放大。")]
        [SerializeField] private Vector3 _pointA;
        [Tooltip("相对于父节点的巡回终点，随房间 Scale 放大。")]
        [SerializeField] private Vector3 _pointB;
        [Tooltip("单程移动秒数。调整房间 Scale 后保持相同的巡回周期。")]
        [SerializeField, Min(0.1f)] private float _travelSeconds = 12f;
        [Tooltip("每次抵达端点后停留的秒数。")]
        [SerializeField, Min(0f)] private float _endpointWaitSeconds = 1f;
        [Tooltip("相对于服务器会话时间的相位偏移，使多面墙错开运动。")]
        [SerializeField, Min(0f)] private float _phaseOffsetSeconds;

        private NetworkSimulationClock _clock;

        private void OnEnable()
        {
            // 地图由已初始化 Gameplay 网络服务的关卡流程租借，回池时解除订阅。
            _clock = GameNetworkRuntime.Gameplay.Clock;
            _clock.Subscribe(GameplayTickPhase.PreSimulation, HandleTick);
            HandleTick(0);
        }

        private void OnDisable()
        {
            if (_clock != null)
            {
                _clock.Unsubscribe(GameplayTickPhase.PreSimulation, HandleTick);
                _clock = null;
            }
            transform.localPosition = _pointA;
        }

        private void HandleTick(uint tick)
        {
            transform.localPosition = EvaluatePosition(NetworkManager.Singleton.ServerTime.Time);
        }

        private Vector3 EvaluatePosition(double serverTime)
        {
            double leg = _travelSeconds + _endpointWaitSeconds;
            double phase = (serverTime + _phaseOffsetSeconds) % (leg * 2d);
            bool returning = phase >= leg;
            double elapsed = returning ? phase - leg : phase;
            float progress = (float)Math.Min(elapsed / _travelSeconds, 1d);
            return returning ? Vector3.Lerp(_pointB, _pointA, progress) : Vector3.Lerp(_pointA, _pointB, progress);
        }

        private void OnValidate()
        {
            _travelSeconds = Mathf.Max(0.1f, _travelSeconds);
            _endpointWaitSeconds = Mathf.Max(0f, _endpointWaitSeconds);
            _phaseOffsetSeconds = Mathf.Max(0f, _phaseOffsetSeconds);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.parent.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.55f, 0.1f);
            Gizmos.DrawLine(_pointA, _pointB);
            Gizmos.DrawWireSphere(_pointA, 0.5f);
            Gizmos.DrawWireSphere(_pointB, 0.5f);
        }
    }
}
