using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    /// <summary>
    /// GridGraph策略的Inspector 配置。
    /// 后续接入 Excel 后，可以直接由表数据构造MapGenerationSettings。
    /// </summary>
    [Serializable]
    public sealed class GridMapGenerationProfile
    {
        [SerializeField, Min(2)] private int _minRoomCount = 12;
        [SerializeField, Min(2)] private int _maxRoomCount = 16;
        [SerializeField, Min(1)] private int _minBossDistance = 6;
        [SerializeField, Range(2, 4)] private int _maxConnectionsPerRoom = 4;
        [FormerlySerializedAs("_roomSpacing")]
        [SerializeField, Min(1f)] private float _baseRoomSize = 70f;

        [Tooltip("每层地图生成前确定一次统一缩放；该层的所有房间共享此 Scale。")]
        [SerializeField] private float[] _allowedRoomScales = { 0.75f, 1f, 1.25f };

        [SerializeField, Min(1)] private int _maxGenerationAttempts = 20;

        public void Validate()
        {
            if (_allowedRoomScales == null || _allowedRoomScales.Length == 0)
                throw new InvalidOperationException("GridGraph 至少需要配置一个房间缩放值。");

            for (int i = 0; i < _allowedRoomScales.Length; i++)
            {
                float scale = _allowedRoomScales[i];

                if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
                    throw new InvalidOperationException($"GridGraph 房间缩放值非法：Index={i}，Scale={scale}");
            }

            CreateSettings(0).Validate();
        }

        public MapGenerationSettings CreateSettings(int seed)
        {
            ValidateScaleCandidates();

            // 使用独立随机流选择整层统一 Scale，避免影响拓扑随机序列。
            int scaleSeed = unchecked(seed * 486187739 + 104729);
            System.Random scaleRandom = new System.Random(scaleSeed);
            float roomScale = _allowedRoomScales[scaleRandom.Next(_allowedRoomScales.Length)];

            MapGenerationSettings settings = new MapGenerationSettings(
                _minRoomCount,
                _maxRoomCount,
                _minBossDistance,
                _maxConnectionsPerRoom,
                _baseRoomSize,
                roomScale,
                _maxGenerationAttempts);
            settings.Validate();
            return settings;
        }

        private void ValidateScaleCandidates()
        {
            if (_allowedRoomScales == null || _allowedRoomScales.Length == 0)
                throw new InvalidOperationException("GridGraph 至少需要配置一个房间缩放值。");

            for (int i = 0; i < _allowedRoomScales.Length; i++)
            {
                float scale = _allowedRoomScales[i];

                if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
                    throw new InvalidOperationException($"GridGraph 房间缩放值非法：Index={i}，Scale={scale}");
            }
        }
    }
}
