using UnityEditor;
using UnityEngine;

internal enum RoomNavigationCostLayer
{
    None,
    Additional,
    Density,
    TotalGrid,
    FlowIntegration
}

[FilePath("Library/RoomNavigationGizmoSettings.asset", FilePathAttribute.Location.ProjectFolder)]
internal sealed class RoomNavigationGizmoSettings : ScriptableSingleton<RoomNavigationGizmoSettings>
{
    [SerializeField] private bool _enabled = true;
    [SerializeField] private bool _selectedRoomOnly = true;
    [SerializeField] private bool _drawGrid = true;
    [SerializeField] private bool _drawStaticCells = true;
    [SerializeField] private bool _fillFloorCells;
    [SerializeField] private bool _drawDynamicBlocked = true;
    [SerializeField] private RoomNavigationCostLayer _costLayer = RoomNavigationCostLayer.TotalGrid;
    [SerializeField] private bool _drawFlowDirections = true;
    [SerializeField] private bool _drawFlowSources = true;
    [SerializeField] private bool _drawRawSeparation = true;
    [SerializeField] private bool _drawFinalVelocity;
    [SerializeField] private bool _drawCellWeightNumbers = true;
    [SerializeField] private bool _includeZeroWeightNumbers = true;
    [SerializeField] private int _weightNumberStride = 2;
    [SerializeField] private bool _autoNormalizeCost;
    [SerializeField] private int _costDisplayMaximum = 40;
    [SerializeField] private int _arrowStride = 2;
    [SerializeField] private float _heightOffset = 0.08f;
    [SerializeField] private float _cellFillScale = 0.9f;
    [SerializeField] private float _costOpacity = 0.35f;
    [SerializeField] private float _flowArrowLength = 0.35f;
    [SerializeField] private float _steeringScale = 0.25f;
    [SerializeField] private float _maximumSteeringLength = 2f;

    public bool Enabled { get => _enabled; set => _enabled = value; }
    public bool SelectedRoomOnly { get => _selectedRoomOnly; set => _selectedRoomOnly = value; }
    public bool DrawGrid { get => _drawGrid; set => _drawGrid = value; }
    public bool DrawStaticCells { get => _drawStaticCells; set => _drawStaticCells = value; }
    public bool FillFloorCells { get => _fillFloorCells; set => _fillFloorCells = value; }
    public bool DrawDynamicBlocked { get => _drawDynamicBlocked; set => _drawDynamicBlocked = value; }
    public RoomNavigationCostLayer CostLayer { get => _costLayer; set => _costLayer = value; }
    public bool DrawFlowDirections { get => _drawFlowDirections; set => _drawFlowDirections = value; }
    public bool DrawFlowSources { get => _drawFlowSources; set => _drawFlowSources = value; }
    public bool DrawRawSeparation { get => _drawRawSeparation; set => _drawRawSeparation = value; }
    public bool DrawFinalVelocity { get => _drawFinalVelocity; set => _drawFinalVelocity = value; }
    public bool DrawCellWeightNumbers { get => _drawCellWeightNumbers; set => _drawCellWeightNumbers = value; }
    public bool IncludeZeroWeightNumbers { get => _includeZeroWeightNumbers; set => _includeZeroWeightNumbers = value; }
    public int WeightNumberStride { get => _weightNumberStride; set => _weightNumberStride = Mathf.Clamp(value, 1, 8); }
    public bool AutoNormalizeCost { get => _autoNormalizeCost; set => _autoNormalizeCost = value; }
    public int CostDisplayMaximum { get => _costDisplayMaximum; set => _costDisplayMaximum = Mathf.Max(1, value); }
    public int ArrowStride { get => _arrowStride; set => _arrowStride = Mathf.Clamp(value, 1, 8); }
    public float HeightOffset { get => _heightOffset; set => _heightOffset = Mathf.Max(0f, value); }
    public float CellFillScale { get => _cellFillScale; set => _cellFillScale = Mathf.Clamp(value, 0.1f, 1f); }
    public float CostOpacity { get => _costOpacity; set => _costOpacity = Mathf.Clamp01(value); }
    public float FlowArrowLength { get => _flowArrowLength; set => _flowArrowLength = Mathf.Max(0.05f, value); }
    public float SteeringScale { get => _steeringScale; set => _steeringScale = Mathf.Max(0f, value); }
    public float MaximumSteeringLength { get => _maximumSteeringLength; set => _maximumSteeringLength = Mathf.Max(0.1f, value); }

    public void SaveSettings() => Save(true);
}

internal sealed class RoomNavigationGizmoWindow : EditorWindow
{
    [MenuItem("Tools/Navigation/Gizmo Settings")]
    private static void Open()
    {
        RoomNavigationGizmoWindow window = GetWindow<RoomNavigationGizmoWindow>();
        window.titleContent = new GUIContent("Navigation Gizmos");
        window.minSize = new Vector2(330f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        RoomNavigationGizmoSettings settings = RoomNavigationGizmoSettings.instance;

        EditorGUILayout.LabelField("Room Navigation Gizmos", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "静态 Grid 可在编辑状态查看。动态阻挡、权重、FlowField 与 Separation 只在 Host/Server 的活动房间中显示。",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();

        settings.Enabled = EditorGUILayout.Toggle("Enabled", settings.Enabled);
        settings.SelectedRoomOnly = EditorGUILayout.Toggle("Selected Room Only", settings.SelectedRoomOnly);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        settings.DrawGrid = EditorGUILayout.Toggle("Grid Lines", settings.DrawGrid);
        settings.DrawStaticCells = EditorGUILayout.Toggle("Static Cells", settings.DrawStaticCells);

        using (new EditorGUI.DisabledScope(!settings.DrawStaticCells))
            settings.FillFloorCells = EditorGUILayout.Toggle("Fill Floor Cells", settings.FillFloorCells);

        settings.DrawDynamicBlocked = EditorGUILayout.Toggle("Dynamic Blocked", settings.DrawDynamicBlocked);
        settings.CellFillScale = EditorGUILayout.Slider("Cell Fill Scale", settings.CellFillScale, 0.1f, 1f);
        settings.HeightOffset = EditorGUILayout.FloatField("Height Offset", settings.HeightOffset);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cost", EditorStyles.boldLabel);
        settings.CostLayer = (RoomNavigationCostLayer)EditorGUILayout.EnumPopup("Displayed Layer", settings.CostLayer);
        settings.AutoNormalizeCost = EditorGUILayout.Toggle("Auto Normalize", settings.AutoNormalizeCost);

        using (new EditorGUI.DisabledScope(settings.AutoNormalizeCost ||
                                           settings.CostLayer == RoomNavigationCostLayer.FlowIntegration))
        {
            settings.CostDisplayMaximum = EditorGUILayout.IntField(
                "Display Maximum", settings.CostDisplayMaximum);
        }

        settings.CostOpacity = EditorGUILayout.Slider("Opacity", settings.CostOpacity, 0f, 1f);
        settings.DrawCellWeightNumbers = EditorGUILayout.Toggle(
            "Cell Weight Numbers", settings.DrawCellWeightNumbers);

        using (new EditorGUI.DisabledScope(!settings.DrawCellWeightNumbers))
        {
            settings.IncludeZeroWeightNumbers = EditorGUILayout.Toggle(
                "Include Zero Values", settings.IncludeZeroWeightNumbers);
            settings.WeightNumberStride = EditorGUILayout.IntSlider(
                "Number Stride", settings.WeightNumberStride, 1, 8);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Flow Field", EditorStyles.boldLabel);
        settings.DrawFlowDirections = EditorGUILayout.Toggle("Directions", settings.DrawFlowDirections);
        settings.DrawFlowSources = EditorGUILayout.Toggle("Sources", settings.DrawFlowSources);
        settings.ArrowStride = EditorGUILayout.IntSlider("Arrow Stride", settings.ArrowStride, 1, 8);
        settings.FlowArrowLength = EditorGUILayout.Slider("Arrow Length", settings.FlowArrowLength, 0.05f, 0.9f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Monster Steering", EditorStyles.boldLabel);
        settings.DrawRawSeparation = EditorGUILayout.Toggle("Raw Separation", settings.DrawRawSeparation);
        settings.DrawFinalVelocity = EditorGUILayout.Toggle("Final Velocity", settings.DrawFinalVelocity);
        settings.SteeringScale = EditorGUILayout.FloatField("Vector Scale", settings.SteeringScale);
        settings.MaximumSteeringLength = EditorGUILayout.FloatField(
            "Maximum Draw Length", settings.MaximumSteeringLength);

        if (!EditorGUI.EndChangeCheck())
            return;

        settings.SaveSettings();
        SceneView.RepaintAll();
    }
}
