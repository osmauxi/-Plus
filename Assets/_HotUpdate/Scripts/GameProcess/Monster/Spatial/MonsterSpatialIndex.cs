using System;
using ProjectGame.Core.Spatial;
using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    public readonly struct MonsterSpatialNodeDebugData
    {
        public readonly Vector2 Center;
        public readonly float Size;
        public readonly byte Depth;
        public readonly int MonsterCount;
        public readonly bool IsLeaf;
        public readonly uint MortonCode;

        public MonsterSpatialNodeDebugData(
            Vector2 center,
            float size,
            byte depth,
            int monsterCount,
            bool isLeaf,
            uint mortonCode)
        {
            Center = center;
            Size = size;
            Depth = depth;
            MonsterCount = monsterCount;
            IsLeaf = isLeaf;
            MortonCode = mortonCode;
        }
    }

    public readonly struct MonsterSpatialIndexDiagnostics
    {
        public readonly int BuildCount;
        public readonly int QueryCount;
        public readonly long TotalCandidateCount;
        public readonly long TotalVisitedNodeCount;
        public readonly int P50CandidateCount;
        public readonly int P95CandidateCount;
        public readonly int MaximumCandidateCount;
        public readonly int P50VisitedNodeCount;
        public readonly int P95VisitedNodeCount;
        public readonly int MaximumVisitedNodeCount;

        public MonsterSpatialIndexDiagnostics(
            int buildCount,
            int queryCount,
            long totalCandidateCount,
            long totalVisitedNodeCount,
            int p50CandidateCount,
            int p95CandidateCount,
            int maximumCandidateCount,
            int p50VisitedNodeCount,
            int p95VisitedNodeCount,
            int maximumVisitedNodeCount)
        {
            BuildCount = buildCount;
            QueryCount = queryCount;
            TotalCandidateCount = totalCandidateCount;
            TotalVisitedNodeCount = totalVisitedNodeCount;
            P50CandidateCount = p50CandidateCount;
            P95CandidateCount = p95CandidateCount;
            MaximumCandidateCount = maximumCandidateCount;
            P50VisitedNodeCount = p50VisitedNodeCount;
            P95VisitedNodeCount = p95VisitedNodeCount;
            MaximumVisitedNodeCount = maximumVisitedNodeCount;
        }
    }

    /// <summary>
    /// 房间级空间索引适配器。HotFix 负责 MonsterWorld、生命周期和诊断，
    /// 纯四叉树数组与紧密循环由 AOT Kernel 执行。
    /// </summary>
    public sealed class MonsterSpatialIndex
    {
        public const int BaseDepth = MonsterQuadTreeKernel.BaseDepth;
        public const int MaxDepth = MonsterQuadTreeKernel.MaxDepth;
        public const int SplitThreshold = MonsterQuadTreeKernel.SplitThreshold;

        private static readonly ProfilerMarker BuildMarker =
            new("Monster.SpatialIndex.Build");
        private static readonly ProfilerMarker QueryMarker =
            new("Monster.SpatialIndex.Query");

        // 四叉树内核，负责索引构建和查询
        private readonly MonsterQuadTreeKernel _kernel;
        //保存每个slot的位置
        private Vector2[] _spatialPositions;
        //标记每个slot是否活跃，1表示活跃，0表示不活跃
        private byte[] _spatialActiveMask;
        private bool _isBuilt;
        private bool _performanceDiagnosticsEnabled;
        private int[] _candidateCountHistogram;
        private int[] _visitedNodeCountHistogram;
        private int _diagnosticBuildCount;
        private int _diagnosticQueryCount;
        private long _diagnosticTotalCandidateCount;
        private long _diagnosticTotalVisitedNodeCount;
        private int _diagnosticMaximumCandidateCount;
        private int _diagnosticMaximumVisitedNodeCount;

        public bool IsBuilt => _isBuilt;
        public int BuildVersion { get; private set; }
        public int NodeCount => _kernel.NodeCount;
        public Vector2 RootCenter => _kernel.RootCenter;
        public float RootSize => _kernel.RootSize;
        public Vector2 LastQueryStart { get; private set; }
        public Vector2 LastQueryEnd { get; private set; }
        public float LastQueryRadius { get; private set; }
        public int LastQueryCandidateCount { get; private set; }
        public int LastQueryVisitedNodeCount { get; private set; }
        public bool HasQueryDebugData { get; private set; }

        public MonsterSpatialIndex(Transform roomRoot, float roomHalfSize, int initialMonsterCapacity)
        {
            CalculateWorldBounds(roomRoot, roomHalfSize, out Vector2 min, out Vector2 max);
            Vector2 rootCenter = (min + max) * 0.5f;
            float rootHalfSize = Mathf.Max(max.x - min.x, max.y - min.y) * 0.5f;
            //AOT内核不直接绑定到MonsterWorld，而是通过HotFix层传入位置数组和活跃掩码数组
            _spatialPositions = new Vector2[initialMonsterCapacity];
            _spatialActiveMask = new byte[initialMonsterCapacity];
            _kernel = new MonsterQuadTreeKernel(rootCenter, rootHalfSize, initialMonsterCapacity);
            _kernel.SetPositionBuffer(_spatialPositions);
        }

        public void Invalidate() => _isBuilt = false;

        /// <summary>
        /// Editor Gizmo 绘制入口。仅在四叉树显示模式下主动补建尚未被弹丸查询触发的索引。
        /// </summary>
        public void EnsureBuiltForDebug(MonsterWorld world) => EnsureBuilt(world);

        public void BeginPerformanceDiagnostics()
        {
            _candidateCountHistogram = new int[_spatialPositions.Length + 1];
            _visitedNodeCountHistogram = new int[_kernel.NodeCapacity + 1];
            _diagnosticBuildCount = 0;
            _diagnosticQueryCount = 0;
            _diagnosticTotalCandidateCount = 0L;
            _diagnosticTotalVisitedNodeCount = 0L;
            _diagnosticMaximumCandidateCount = 0;
            _diagnosticMaximumVisitedNodeCount = 0;
            _performanceDiagnosticsEnabled = true;
        }

        public MonsterSpatialIndexDiagnostics EndPerformanceDiagnostics()
        {
            _performanceDiagnosticsEnabled = false;
            return new MonsterSpatialIndexDiagnostics(
                _diagnosticBuildCount,
                _diagnosticQueryCount,
                _diagnosticTotalCandidateCount,
                _diagnosticTotalVisitedNodeCount,
                Percentile(_candidateCountHistogram, _diagnosticQueryCount, 0.50f),
                Percentile(_candidateCountHistogram, _diagnosticQueryCount, 0.95f),
                _diagnosticMaximumCandidateCount,
                Percentile(_visitedNodeCountHistogram, _diagnosticQueryCount, 0.50f),
                Percentile(_visitedNodeCountHistogram, _diagnosticQueryCount, 0.95f),
                _diagnosticMaximumVisitedNodeCount);
        }

        public void Clear()
        {
            _kernel.Clear();
            _isBuilt = false;
            HasQueryDebugData = false;
            LastQueryCandidateCount = 0;
            LastQueryVisitedNodeCount = 0;
        }

        public void Add(MonsterWorld world, int slot)
        {
            if (!_isBuilt)
                return;

            EnsureInputCapacity(world.SlotCount);
            Vector2 position = world.Motion[slot].Position;
            _spatialPositions[slot] = position;
            _spatialActiveMask[slot] = 1;
            _kernel.Add(slot, position);
        }

        public void Remove(int slot)
        {
            if (!_isBuilt)
                return;

            _spatialActiveMask[slot] = 0;
            _kernel.Remove(slot);
        }

        public void Relocate(MonsterWorld world, int slot)
        {
            if (!_isBuilt)
                return;

            Vector2 position = world.Motion[slot].Position;
            _spatialPositions[slot] = position;
            _kernel.Relocate(slot, position);
        }

        public int QuerySweptCircle(
            MonsterWorld world,
            Vector2 start,
            Vector2 end,
            float radius,
            int[] results)
        {
            if (world.AliveCount == 0)
            {
                LastQueryStart = start;
                LastQueryEnd = end;
                LastQueryRadius = radius;
                LastQueryCandidateCount = 0;
                LastQueryVisitedNodeCount = 0;
                HasQueryDebugData = true;
                return 0;
            }

            EnsureBuilt(world);

            using (QueryMarker.Auto())
            {
                LastQueryStart = start;
                LastQueryEnd = end;
                LastQueryRadius = radius;
                HasQueryDebugData = true;

                int resultCount = _kernel.QuerySweptCircle(start, end, radius, results);
                int visitedNodeCount = _kernel.LastQueryVisitedNodeCount;
                LastQueryCandidateCount = resultCount;
                LastQueryVisitedNodeCount = visitedNodeCount;
                if (_performanceDiagnosticsEnabled)
                {
                    _diagnosticQueryCount++;
                    _diagnosticTotalCandidateCount += resultCount;
                    _diagnosticTotalVisitedNodeCount += visitedNodeCount;
                    _diagnosticMaximumCandidateCount = Math.Max(_diagnosticMaximumCandidateCount, resultCount);
                    _diagnosticMaximumVisitedNodeCount = Math.Max(_diagnosticMaximumVisitedNodeCount, visitedNodeCount);
                    _candidateCountHistogram[resultCount]++;
                    _visitedNodeCountHistogram[visitedNodeCount]++;
                }
                return resultCount;
            }
        }

        public bool TryGetDebugNode(int nodeIndex, out MonsterSpatialNodeDebugData data)
        {
            if (!_kernel.TryGetNode(nodeIndex, out MonsterQuadTreeNodeData node))
            {
                data = default;
                return false;
            }

            data = new MonsterSpatialNodeDebugData(
                node.Center,
                node.Size,
                node.Depth,
                node.MonsterCount,
                node.IsLeaf,
                node.MortonCode);
            return true;
        }
        /// <summary>
        /// 查询前确保索引已构建，且是最新的
        /// </summary>
        private void EnsureBuilt(MonsterWorld world)
        {
            if (_isBuilt)
                return;

            using (BuildMarker.Auto())
            {
                EnsureInputCapacity(world.SlotCount);
                for (int slot = 0; slot < world.SlotCount; slot++)
                {
                    _spatialPositions[slot] = world.Motion[slot].Position;
                    _spatialActiveMask[slot] = world.Meta[slot].IsActive ? (byte)1 : (byte)0;
                }

                _kernel.Build(_spatialPositions, _spatialActiveMask, world.SlotCount);
                _isBuilt = true;
                BuildVersion++;
                if (_performanceDiagnosticsEnabled)
                    _diagnosticBuildCount++;
            }
        }

        private void EnsureInputCapacity(int count)
        {
            if (_spatialPositions.Length >= count)
                return;

            int previousCapacity = _spatialPositions.Length;
            int capacity = Math.Max(count, previousCapacity == 0 ? 64 : previousCapacity * 2);
            Array.Resize(ref _spatialPositions, capacity);
            Array.Resize(ref _spatialActiveMask, capacity);
            _kernel.SetPositionBuffer(_spatialPositions);
        }

        private static int Percentile(int[] histogram, int sampleCount, float percentile)
        {
            if (sampleCount == 0)
                return 0;

            int target = Mathf.CeilToInt(sampleCount * percentile);
            int accumulated = 0;
            for (int value = 0; value < histogram.Length; value++)
            {
                accumulated += histogram[value];
                if (accumulated >= target)
                    return value;
            }
            return histogram.Length - 1;
        }

        private static void CalculateWorldBounds(
            Transform roomRoot,
            float halfSize,
            out Vector2 min,
            out Vector2 max)
        {
            Vector3 a = roomRoot.TransformPoint(new Vector3(-halfSize, 0f, -halfSize));
            Vector3 b = roomRoot.TransformPoint(new Vector3(-halfSize, 0f, halfSize));
            Vector3 c = roomRoot.TransformPoint(new Vector3(halfSize, 0f, -halfSize));
            Vector3 d = roomRoot.TransformPoint(new Vector3(halfSize, 0f, halfSize));

            min = new Vector2(
                Mathf.Min(a.x, b.x, c.x, d.x),
                Mathf.Min(a.z, b.z, c.z, d.z));
            max = new Vector2(
                Mathf.Max(a.x, b.x, c.x, d.x),
                Mathf.Max(a.z, b.z, c.z, d.z));
        }
    }
}
