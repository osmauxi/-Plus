using UnityEditor;
using UnityEngine;

internal enum RoomNavigationCostLayer
{
    [InspectorName("无")]
    None,
    [InspectorName("附加权重")]
    Additional,
    [InspectorName("密度权重")]
    Density,
    [InspectorName("网格总权重")]
    TotalGrid,
    [InspectorName("流场积分值")]
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
    [SerializeField] private bool _drawMonsterSpatialTree = true;
    [SerializeField] private bool _drawMonsterSpatialCounts;
    [SerializeField] private bool _drawMonsterHitRadius;
    [SerializeField] private bool _drawLastMonsterSpatialQuery = true;
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
    public bool DrawMonsterSpatialTree { get => _drawMonsterSpatialTree; set => _drawMonsterSpatialTree = value; }
    public bool DrawMonsterSpatialCounts { get => _drawMonsterSpatialCounts; set => _drawMonsterSpatialCounts = value; }
    public bool DrawMonsterHitRadius { get => _drawMonsterHitRadius; set => _drawMonsterHitRadius = value; }
    public bool DrawLastMonsterSpatialQuery { get => _drawLastMonsterSpatialQuery; set => _drawLastMonsterSpatialQuery = value; }
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
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Navigation/可视化设置")]
    private static void Open()
    {
        RoomNavigationGizmoWindow window = GetWindow<RoomNavigationGizmoWindow>();
        window.titleContent = new GUIContent("导航可视化");
        window.minSize = new Vector2(330f, 650f);
        window.Show();
    }

    private void OnGUI()
    {
        RoomNavigationGizmoSettings settings = RoomNavigationGizmoSettings.instance;
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("房间导航可视化", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "静态网格可在编辑状态查看。动态阻挡、权重、流场、怪物转向与空间索引只在 Host/Server 的活动房间中显示。",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();

        settings.Enabled = EditorGUILayout.Toggle("启用", settings.Enabled);
        settings.SelectedRoomOnly = EditorGUILayout.Toggle("仅显示选中房间", settings.SelectedRoomOnly);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("网格", EditorStyles.boldLabel);
        settings.DrawGrid = EditorGUILayout.Toggle("网格线", settings.DrawGrid);
        settings.DrawStaticCells = EditorGUILayout.Toggle("静态阻挡单元", settings.DrawStaticCells);

        using (new EditorGUI.DisabledScope(!settings.DrawStaticCells))
            settings.FillFloorCells = EditorGUILayout.Toggle("填充地面单元", settings.FillFloorCells);

        settings.DrawDynamicBlocked = EditorGUILayout.Toggle("动态阻挡单元", settings.DrawDynamicBlocked);
        settings.CellFillScale = EditorGUILayout.Slider("单元填充比例", settings.CellFillScale, 0.1f, 1f);
        settings.HeightOffset = EditorGUILayout.FloatField("绘制高度偏移", settings.HeightOffset);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("权重", EditorStyles.boldLabel);
        settings.CostLayer = (RoomNavigationCostLayer)EditorGUILayout.EnumPopup("显示层", settings.CostLayer);
        settings.AutoNormalizeCost = EditorGUILayout.Toggle("自动归一化", settings.AutoNormalizeCost);

        using (new EditorGUI.DisabledScope(settings.AutoNormalizeCost ||
                                           settings.CostLayer == RoomNavigationCostLayer.FlowIntegration))
        {
            settings.CostDisplayMaximum = EditorGUILayout.IntField(
                "显示上限", settings.CostDisplayMaximum);
        }

        settings.CostOpacity = EditorGUILayout.Slider("不透明度", settings.CostOpacity, 0f, 1f);
        settings.DrawCellWeightNumbers = EditorGUILayout.Toggle(
            "单元权重数值", settings.DrawCellWeightNumbers);

        using (new EditorGUI.DisabledScope(!settings.DrawCellWeightNumbers))
        {
            settings.IncludeZeroWeightNumbers = EditorGUILayout.Toggle(
                "显示零值", settings.IncludeZeroWeightNumbers);
            settings.WeightNumberStride = EditorGUILayout.IntSlider(
                "数值显示间隔", settings.WeightNumberStride, 1, 8);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("流场", EditorStyles.boldLabel);
        settings.DrawFlowDirections = EditorGUILayout.Toggle("方向", settings.DrawFlowDirections);
        settings.DrawFlowSources = EditorGUILayout.Toggle("源点", settings.DrawFlowSources);
        settings.ArrowStride = EditorGUILayout.IntSlider("箭头显示间隔", settings.ArrowStride, 1, 8);
        settings.FlowArrowLength = EditorGUILayout.Slider("箭头长度", settings.FlowArrowLength, 0.05f, 0.9f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("怪物转向", EditorStyles.boldLabel);
        settings.DrawRawSeparation = EditorGUILayout.Toggle("原始分离向量", settings.DrawRawSeparation);
        settings.DrawFinalVelocity = EditorGUILayout.Toggle("最终速度", settings.DrawFinalVelocity);
        settings.SteeringScale = EditorGUILayout.FloatField("向量缩放", settings.SteeringScale);
        settings.MaximumSteeringLength = EditorGUILayout.FloatField(
            "最大绘制长度", settings.MaximumSteeringLength);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("怪物空间索引", EditorStyles.boldLabel);
        settings.DrawMonsterSpatialTree = EditorGUILayout.Toggle(
            "四叉树叶节点", settings.DrawMonsterSpatialTree);
        settings.DrawMonsterSpatialCounts = EditorGUILayout.Toggle(
            "叶节点怪物数量", settings.DrawMonsterSpatialCounts);
        settings.DrawMonsterHitRadius = EditorGUILayout.Toggle(
            "受击半径", settings.DrawMonsterHitRadius);
        settings.DrawLastMonsterSpatialQuery = EditorGUILayout.Toggle(
            "最近一次弹丸查询", settings.DrawLastMonsterSpatialQuery);

        bool changed = EditorGUI.EndChangeCheck();
        EditorGUILayout.EndScrollView();
        if (!changed)
            return;

        settings.SaveSettings();
        SceneView.RepaintAll();
    }
}
