using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Map.View;

namespace ProjectGame.HotFix.Gameplay.Map.Generation
{
    public readonly struct MapRuntimeBuiltEvent : ILocalEvent
    {
        public int GenerationId { get; }
        public MapLayout Layout { get; }
        public MapBuildPlan BuildPlan { get; }

        public MapRuntimeBuiltEvent(int generationId, MapLayout layout, MapBuildPlan buildPlan)
        {
            GenerationId = generationId;
            Layout = layout;
            BuildPlan = buildPlan;
        }
    }

    public readonly struct MapRuntimeClearingEvent : ILocalEvent
    {
        public int GenerationId { get; }

        public MapRuntimeClearingEvent(int generationId)
        {
            GenerationId = generationId;
        }
    }
}
