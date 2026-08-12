using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary> 
    /// 房间预制体的静态资源描述，作为锚点挂载载体。
    /// RoomConnectorSlot由MapVisualBuilder在实例注册时自动收集。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoomView : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private Transform[] _enemySpawnPoints;
        [SerializeField] private Transform[] _playerSpawnPoints;
        [SerializeField] private Transform[] _chestSpawnPoints;
        [SerializeField] private Transform[] _nextLevelPoints;

        public IReadOnlyList<Transform> EnemySpawnPoints => _enemySpawnPoints;
        public IReadOnlyList<Transform> PlayerSpawnPoints => _playerSpawnPoints;
        public IReadOnlyList<Transform> ChestSpawnPoints => _chestSpawnPoints;
        public IReadOnlyList<Transform> NextLevelPoints => _nextLevelPoints;
    }
}