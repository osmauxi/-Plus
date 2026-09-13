using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Spawning;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary> 
    /// 房间预制体的静态资源描述，作为锚点挂载载体 
    /// RoomConnectorSlot由MapVisualBuilder在实例注册时自动收集 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoomView : MonoBehaviour
    {
        [Header("Spawn Region")]
        [SerializeField] private Vector3 _spawnRegionCenter = new Vector3(0f, 5f, 0f);
        [SerializeField] private Vector3 _spawnRegionSize = new Vector3(60f, 20f, 60f);
        [SerializeField] private LayerMask _spawnProbeMask = ~0;
        [SerializeField] private LayerMask _groundMask = 1 << 3;

        [Header("Semantic Anchors")]
        [SerializeField] private Transform[] _chestSpawnPoints;
        [SerializeField] private Transform[] _nextLevelPoints;

        public RoomSpawnRegion SpawnRegion => new RoomSpawnRegion(transform,
            _spawnRegionCenter, _spawnRegionSize, _spawnProbeMask, _groundMask);

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.15f, 0.85f, 0.35f, 1f);
            Gizmos.DrawWireCube(_spawnRegionCenter, _spawnRegionSize);
            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
