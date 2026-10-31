using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Player;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 缓存玩家位置信息，所有索敌模块都统一访问这里。
    /// </summary>
    public sealed class MonsterPlayerTargetBuffer
    {
        public const int MaximumCapacity = 4;
        private readonly MonsterPlayerTarget[] _items = new MonsterPlayerTarget[MaximumCapacity];
        private readonly Vector2[] _positions = new Vector2[MaximumCapacity];
        private readonly Vector2Int[] _cells = new Vector2Int[MaximumCapacity];
        private readonly bool[] _cellValid = new bool[MaximumCapacity];
        private readonly byte[] _cellWalkable = new byte[MaximumCapacity];

        public int Count { get; private set; }
        internal MonsterPlayerTarget[] Items => _items;
        internal Vector2[] Positions => _positions;
        internal byte[] CellWalkable => _cellWalkable;

        public void Clear() => Count = 0;

        public void Add(Vector2 position)
        {
            _items[Count++] = new MonsterPlayerTarget(position);
            _positions[Count - 1] = position;
        }

        internal void UpdateNavigationCells(
            Transform roomRoot,
            RuntimeNavigationGrid grid,
            float groundY)
        {
            for (int i = 0; i < Count; i++)
            {
                Vector2 position = _items[i].Position;
                bool valid = RoomSpatialUtility.WorldToCell(
                    roomRoot,
                    new Vector3(position.x, groundY, position.y),
                    out Vector2Int cell);

                _cellValid[i] = valid;
                _cells[i] = cell;
                _cellWalkable[i] = (byte)(valid && grid.IsWalkable(cell.x, cell.y) ? 1 : 0);
            }
        }

        internal bool IsCellWalkable(int index) =>
            (uint)index < (uint)Count && _cellValid[index] && _cellWalkable[index] != 0;

        internal Vector2Int GetCell(int index) => _cells[index];
    }

    /// <summary>
    /// 每个Simulation Tick遍历一次PlayerManager/Transform进行信息缓存。
    /// 它只是数据采集适配器，不是怪物可配置行为模块。
    /// </summary>
    public static class MonsterPlayerTargetCapture
    {
        public static void Capture(PlayerManager manager, MonsterPlayerTargetBuffer destination)
        {
            destination.Clear();
            IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (player == null || !player.isActiveAndEnabled || !player.IsSpawned ||
                    !player.GetComponent<PlayerHealthNetworkState>().IsAlive)
                    continue;
                Vector3 position = player.transform.position;
                destination.Add(new Vector2(position.x, position.z));
            }
        }
    }
}
