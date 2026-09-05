using System;
using ProjectGame.HotFix.Gameplay.Map.Generation;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// Server 完成一层地图构建后的结果 
    /// </summary>
    public sealed class MapBuildResult
    {
        public int GenerationId { get; }
        public MapLayout Layout { get; }
        public MapBuildPlan BuildPlan { get; }

        public MapBuildResult(int generationId, MapLayout layout, MapBuildPlan buildPlan)
        {
            GenerationId = generationId;
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            buildPlan.Validate();
            BuildPlan = buildPlan;
        }
    }
}
