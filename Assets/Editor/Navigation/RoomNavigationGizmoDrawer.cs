using ProjectGame.HotFix.Gameplay.Map;
using ProjectGame.HotFix.Gameplay.Map.View;
using ProjectGame.HotFix.Gameplay.Monsters;
using ProjectGame.HotFix.Gameplay.Navigation;
using UnityEditor;
using UnityEngine;

internal static class RoomNavigationGizmoDrawer
{
    private static readonly Color GridColor = new(0.25f, 0.8f, 1f, 0.45f);
    private static readonly Color EmptyColor = new(0.35f, 0.35f, 0.35f, 0.25f);
    private static readonly Color FloorColor = new(0.1f, 0.8f, 0.25f, 0.08f);
    private static readonly Color StaticObstacleColor = new(0.55f, 0.08f, 0.05f, 0.55f);
    private static readonly Color DynamicObstacleColor = new(1f, 0f, 0.65f, 0.72f);
    private static readonly Color SeparationColor = new(0f, 1f, 1f, 0.95f);
    private static readonly Color FinalVelocityColor = new(1f, 0.82f, 0f, 0.95f);

    private static GUIStyle _labelStyle;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.InSelectionHierarchy)]
    private static void DrawRoomNavigation(RoomView view, GizmoType gizmoType)
    {
        RoomNavigationGizmoSettings settings = RoomNavigationGizmoSettings.instance;
        if (!settings.Enabled || view == null || view.SpatialData == null)
            return;

        bool selected = (gizmoType & (GizmoType.Selected | GizmoType.InSelectionHierarchy)) != 0;
        if (settings.SelectedRoomOnly && !selected)
            return;

        MonsterRoomRuntime runtime = ResolveRuntime(view);
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;

        Gizmos.matrix = view.transform.localToWorldMatrix;

        if (settings.DrawStaticCells)
            DrawStaticCells(view.SpatialData, settings);

        if (runtime != null && settings.CostLayer != RoomNavigationCostLayer.None)
            DrawCostLayer(view.transform, runtime, settings);

        if (runtime != null && settings.DrawDynamicBlocked)
            DrawDynamicBlocked(runtime.NavigationGrid, settings);

        if (settings.DrawGrid)
            DrawGrid(view.SpatialData, settings.HeightOffset + 0.05f);

        if (runtime != null && (settings.DrawFlowDirections || settings.DrawFlowSources))
            DrawFlowField(runtime, settings);

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;

        if (runtime != null && (settings.DrawRawSeparation || settings.DrawFinalVelocity))
            DrawMonsterSteering(runtime, settings);
    }

    private static MonsterRoomRuntime ResolveRuntime(RoomView view)
    {
        if (!Application.isPlaying)
            return null;

        MonsterRuntimeService service = MonsterRuntimeService.Instance;
        MonsterRoomRuntime runtime = service != null ? service.AuthorityRoom : null;

        return runtime != null && runtime.RoomRoot == view.transform ? runtime : null;
    }

    private static void DrawGrid(RoomSpatialData spatialData, float localY)
    {
        Gizmos.color = GridColor;

        float min = -RoomSpatialData.HalfSize;
        float maxX = min + spatialData.Width * RoomSpatialData.CellSize;
        float maxY = min + spatialData.Height * RoomSpatialData.CellSize;

        for (int x = 0; x <= spatialData.Width; x++)
        {
            float localX = min + x * RoomSpatialData.CellSize;
            Gizmos.DrawLine(
                new Vector3(localX, localY, min),
                new Vector3(localX, localY, maxY));
        }

        for (int y = 0; y <= spatialData.Height; y++)
        {
            float localZ = min + y * RoomSpatialData.CellSize;
            Gizmos.DrawLine(
                new Vector3(min, localY, localZ),
                new Vector3(maxX, localY, localZ));
        }
    }

    private static void DrawStaticCells(RoomSpatialData spatialData, RoomNavigationGizmoSettings settings)
    {
        float y = settings.HeightOffset;

        for (int cellY = 0; cellY < spatialData.Height; cellY++)
        {
            for (int cellX = 0; cellX < spatialData.Width; cellX++)
            {
                RoomSpatialCell cell = spatialData.Get(cellX, cellY);

                switch (cell)
                {
                    case RoomSpatialCell.Empty:
                        Gizmos.color = EmptyColor;
                        break;
                    case RoomSpatialCell.Floor when settings.FillFloorCells:
                        Gizmos.color = FloorColor;
                        break;
                    case RoomSpatialCell.Obstacle:
                        Gizmos.color = StaticObstacleColor;
                        break;
                    default:
                        continue;
                }

                DrawCell(cellX, cellY, y, settings.CellFillScale);
            }
        }
    }

    private static void DrawDynamicBlocked(
        RuntimeNavigationGrid grid,
        RoomNavigationGizmoSettings settings)
    {
        Gizmos.color = DynamicObstacleColor;
        float y = settings.HeightOffset + 0.035f;

        for (int cellY = 0; cellY < grid.Height; cellY++)
        {
            for (int cellX = 0; cellX < grid.Width; cellX++)
            {
                if (grid.TryGetDebugCell(cellX, cellY, out NavigationGridCellDebugData cell) &&
                    cell.DynamicBlocked)
                {
                    DrawCell(cellX, cellY, y, settings.CellFillScale * 0.92f);
                }
            }
        }
    }

    private static void DrawCostLayer(
        Transform roomRoot,
        MonsterRoomRuntime runtime,
        RoomNavigationGizmoSettings settings)
    {
        int maximum = settings.AutoNormalizeCost ||
                      settings.CostLayer == RoomNavigationCostLayer.FlowIntegration
            ? FindMaximumCost(runtime, settings.CostLayer)
            : settings.CostDisplayMaximum;

        maximum = Mathf.Max(1, maximum);
        float y = settings.HeightOffset + 0.018f;

        for (int cellY = 0; cellY < runtime.NavigationGrid.Height; cellY++)
        {
            for (int cellX = 0; cellX < runtime.NavigationGrid.Width; cellX++)
            {
                if (!TryGetCost(runtime, settings.CostLayer, cellX, cellY, out int cost))
                    continue;

                if (cost > 0)
                {
                    Gizmos.color = EvaluateCostColor(
                        Mathf.Clamp01((float)cost / maximum), settings.CostOpacity);
                    DrawCell(cellX, cellY, y, settings.CellFillScale * 0.86f);
                }

                if (!settings.DrawCellWeightNumbers ||
                    cost == 0 && !settings.IncludeZeroWeightNumbers ||
                    cellX % settings.WeightNumberStride != 0 ||
                    cellY % settings.WeightNumberStride != 0)
                {
                    continue;
                }

                Vector3 local = GetCellCenter(cellX, cellY, y + 0.08f);
                Handles.Label(roomRoot.TransformPoint(local), cost.ToString(), GetLabelStyle());
            }
        }
    }

    private static int FindMaximumCost(MonsterRoomRuntime runtime, RoomNavigationCostLayer layer)
    {
        int maximum = 1;

        for (int y = 0; y < runtime.NavigationGrid.Height; y++)
        {
            for (int x = 0; x < runtime.NavigationGrid.Width; x++)
            {
                if (TryGetCost(runtime, layer, x, y, out int cost) && cost != int.MaxValue)
                    maximum = Mathf.Max(maximum, cost);
            }
        }

        return maximum;
    }

    private static bool TryGetCost(
        MonsterRoomRuntime runtime,
        RoomNavigationCostLayer layer,
        int x,
        int y,
        out int cost)
    {
        cost = 0;

        if (layer == RoomNavigationCostLayer.FlowIntegration)
        {
            if (!runtime.TryGetFlowFieldDebugCell(x, y, out FlowFieldCellDebugData flow) ||
                !flow.IsReachable)
            {
                return false;
            }

            cost = flow.Cost;
            return true;
        }

        if (!runtime.NavigationGrid.TryGetDebugCell(x, y, out NavigationGridCellDebugData grid) ||
            !grid.IsWalkable)
        {
            return false;
        }

        cost = layer switch
        {
            RoomNavigationCostLayer.Additional => grid.AdditionalCost,
            RoomNavigationCostLayer.Density => grid.DensityCost,
            RoomNavigationCostLayer.TotalGrid => grid.TotalAdditionalCost,
            _ => 0
        };

        return true;
    }

    private static void DrawFlowField(
        MonsterRoomRuntime runtime,
        RoomNavigationGizmoSettings settings)
    {
        if (!runtime.HasFlowField)
            return;

        float y = settings.HeightOffset + 0.085f;
        int stride = settings.ArrowStride;

        for (int cellY = 0; cellY < runtime.NavigationGrid.Height; cellY++)
        {
            for (int cellX = 0; cellX < runtime.NavigationGrid.Width; cellX++)
            {
                if (!runtime.TryGetFlowFieldDebugCell(cellX, cellY, out FlowFieldCellDebugData flow) ||
                    !flow.IsReachable)
                {
                    continue;
                }

                Vector3 center = GetCellCenter(cellX, cellY, y);
                Gizmos.color = GetSourceColor(flow.SourceIndex);

                if (settings.DrawFlowSources && flow.IsSource)
                    Gizmos.DrawSphere(center, RoomSpatialData.CellSize * 0.13f);

                if (!settings.DrawFlowDirections ||
                    flow.Direction.sqrMagnitude <= 0.000001f ||
                    cellX % stride != 0 ||
                    cellY % stride != 0)
                {
                    continue;
                }

                Vector2 direction = flow.Direction.normalized;
                Vector3 vector = new Vector3(direction.x, 0f, direction.y) *
                                 settings.FlowArrowLength * RoomSpatialData.CellSize;
                DrawArrow(center - vector * 0.5f, vector, RoomSpatialData.CellSize * 0.12f);
            }
        }
    }

    private static void DrawMonsterSteering(
        MonsterRoomRuntime runtime,
        RoomNavigationGizmoSettings settings)
    {
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = Matrix4x4.identity;

        float worldY = runtime.GroundY + settings.HeightOffset + 0.16f;

        for (int slot = 0; slot < runtime.World.SlotCount; slot++)
        {
            if (!runtime.TryGetMonsterNavigationDebug(slot, out MonsterNavigationDebugData monster))
                continue;

            Vector2 position = monster.Position;
            Vector3 origin = new Vector3(position.x, worldY, position.y);

            if (settings.DrawRawSeparation)
            {
                Vector3 vector = ClampDrawVector(monster.RawSeparation, settings);

                if (vector.sqrMagnitude > 0.000001f)
                {
                    Gizmos.color = SeparationColor;
                    DrawArrow(origin, vector, 0.16f);
                }
            }

            if (settings.DrawFinalVelocity)
            {
                Vector3 vector = ClampDrawVector(monster.FinalVelocity, settings);

                if (vector.sqrMagnitude > 0.000001f)
                {
                    Gizmos.color = FinalVelocityColor;
                    DrawArrow(origin + Vector3.up * 0.035f, vector, 0.16f);
                }
            }
        }

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }

    private static Vector3 ClampDrawVector(
        Vector2 vector,
        RoomNavigationGizmoSettings settings)
    {
        Vector3 result = new Vector3(vector.x, 0f, vector.y) * settings.SteeringScale;
        float maximum = settings.MaximumSteeringLength;

        return result.sqrMagnitude > maximum * maximum
            ? result.normalized * maximum
            : result;
    }

    private static void DrawCell(int x, int y, float localY, float scale)
    {
        float size = RoomSpatialData.CellSize * scale;
        Gizmos.DrawCube(
            GetCellCenter(x, y, localY),
            new Vector3(size, 0.012f, size));
    }

    private static Vector3 GetCellCenter(int x, int y, float localY)
    {
        return new Vector3(
            -RoomSpatialData.HalfSize + (x + 0.5f) * RoomSpatialData.CellSize,
            localY,
            -RoomSpatialData.HalfSize + (y + 0.5f) * RoomSpatialData.CellSize);
    }

    private static void DrawArrow(Vector3 start, Vector3 vector, float headLength)
    {
        if (vector.sqrMagnitude <= 0.000001f)
            return;

        Vector3 tip = start + vector;
        Vector3 direction = vector.normalized;
        Vector3 side = new Vector3(-direction.z, 0f, direction.x);
        float actualHeadLength = Mathf.Min(headLength, vector.magnitude * 0.45f);
        Vector3 back = tip - direction * actualHeadLength;

        Gizmos.DrawLine(start, tip);
        Gizmos.DrawLine(tip, back + side * actualHeadLength * 0.55f);
        Gizmos.DrawLine(tip, back - side * actualHeadLength * 0.55f);
    }

    private static Color EvaluateCostColor(float normalized, float alpha)
    {
        Color color = normalized < 0.5f
            ? Color.Lerp(new Color(0f, 0.45f, 1f), Color.yellow, normalized * 2f)
            : Color.Lerp(Color.yellow, new Color(1f, 0.05f, 0f), (normalized - 0.5f) * 2f);

        color.a = alpha;
        return color;
    }

    private static Color GetSourceColor(int sourceIndex)
    {
        if (sourceIndex < 0)
            return Color.white;

        Color color = Color.HSVToRGB(Mathf.Repeat(sourceIndex * 0.6180339f, 1f), 0.72f, 1f);
        color.a = 0.95f;
        return color;
    }

    private static GUIStyle GetLabelStyle()
    {
        if (_labelStyle != null)
            return _labelStyle;

        _labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        return _labelStyle;
    }
}
