using UnityEditor;
using UnityEngine;

internal enum RoomNavigationGizmoMode
{
    [InspectorName("关闭")]
    Disabled,
    [InspectorName("基础导航网格")]
    NavigationGrid,
    [InspectorName("寻路 Grid 权重")]
    GridCost,
    [InspectorName("流场积分值")]
    FlowIntegration,
    [InspectorName("流场方向")]
    FlowDirections,
    [InspectorName("怪物转向")]
    MonsterSteering,
    [InspectorName("怪物四叉树分布")]
    MonsterSpatialIndex
}

[FilePath("Library/RoomNavigationGizmoSettings.asset", FilePathAttribute.Location.ProjectFolder)]
internal sealed class RoomNavigationGizmoSettings : ScriptableSingleton<RoomNavigationGizmoSettings>
{
    [SerializeField] private RoomNavigationGizmoMode _displayMode =
        RoomNavigationGizmoMode.MonsterSpatialIndex;
    [SerializeField] private bool _selectedRoomOnly = true;
    [SerializeField] private bool _drawGrid = true;
    [SerializeField] private bool _drawStaticCells = true;
    [SerializeField] private bool _fillFloorCells;
    [SerializeField] private bool _drawDynamicBlocked = true;
    [SerializeField] private bool _drawTopologyReferenceDirections;
    [SerializeField] private bool _drawFlowProgressCorrections = true;
    [SerializeField] private bool _drawRawSeparation = true;
    [SerializeField] private bool _drawFinalVelocity;
    [SerializeField] private bool _drawMonsterSpatialCounts;
    [SerializeField] private bool _drawMonsterHitRadius;
    [SerializeField] private bool _drawLastMonsterSpatialQuery = true;
    [SerializeField] private bool _drawCostNumbers;
    [SerializeField] private bool _includeZeroCostNumbers;
    [SerializeField] private int _costNumberStride = 2;
    [SerializeField] private bool _autoNormalizeCost = true;
    [SerializeField] private int _costDisplayMaximum = 40;
    [SerializeField] private int _arrowStride = 2;
    [SerializeField] private float _heightOffset = 0.08f;
    [SerializeField] private float _cellFillScale = 0.9f;
    [SerializeField] private float _costOpacity = 0.35f;
    [SerializeField] private float _flowArrowLength = 0.35f;
    [SerializeField] private float _steeringScale = 0.25f;
    [SerializeField] private float _maximumSteeringLength = 2f;

    public RoomNavigationGizmoMode DisplayMode { get => _displayMode; set => _displayMode = value; }
    public bool SelectedRoomOnly { get => _selectedRoomOnly; set => _selectedRoomOnly = value; }
    public bool DrawGrid { get => _drawGrid; set => _drawGrid = value; }
    public bool DrawStaticCells { get => _drawStaticCells; set => _drawStaticCells = value; }
    public bool FillFloorCells { get => _fillFloorCells; set => _fillFloorCells = value; }
    public bool DrawDynamicBlocked { get => _drawDynamicBlocked; set => _drawDynamicBlocked = value; }
    public bool DrawTopologyReferenceDirections { get => _drawTopologyReferenceDirections; set => _drawTopologyReferenceDirections = value; }
    public bool DrawFlowProgressCorrections { get => _drawFlowProgressCorrections; set => _drawFlowProgressCorrections = value; }
    public bool DrawRawSeparation { get => _drawRawSeparation; set => _drawRawSeparation = value; }
    public bool DrawFinalVelocity { get => _drawFinalVelocity; set => _drawFinalVelocity = value; }
    public bool DrawMonsterSpatialCounts { get => _drawMonsterSpatialCounts; set => _drawMonsterSpatialCounts = value; }
    public bool DrawMonsterHitRadius { get => _drawMonsterHitRadius; set => _drawMonsterHitRadius = value; }
    public bool DrawLastMonsterSpatialQuery { get => _drawLastMonsterSpatialQuery; set => _drawLastMonsterSpatialQuery = value; }
    public bool DrawCostNumbers { get => _drawCostNumbers; set => _drawCostNumbers = value; }
    public bool IncludeZeroCostNumbers { get => _includeZeroCostNumbers; set => _includeZeroCostNumbers = value; }
    public int CostNumberStride { get => _costNumberStride; set => _costNumberStride = Mathf.Clamp(value, 1, 8); }
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
        EditorGUI.BeginChangeCheck();

        settings.DisplayMode = (RoomNavigationGizmoMode)EditorGUILayout.EnumPopup(
            "显示模式", settings.DisplayMode);
        settings.SelectedRoomOnly = EditorGUILayout.Toggle("仅显示选中房间", settings.SelectedRoomOnly);
        settings.HeightOffset = EditorGUILayout.FloatField("绘制高度偏移", settings.HeightOffset);

        EditorGUILayout.HelpBox(GetModeDescription(settings.DisplayMode), MessageType.Info);

        switch (settings.DisplayMode)
        {
            case RoomNavigationGizmoMode.NavigationGrid:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("基础导航网格", EditorStyles.boldLabel);
                settings.DrawGrid = EditorGUILayout.Toggle("网格线", settings.DrawGrid);
                settings.DrawStaticCells = EditorGUILayout.Toggle(
                    "静态阻挡单元", settings.DrawStaticCells);
                using (new EditorGUI.DisabledScope(!settings.DrawStaticCells))
                {
                    settings.FillFloorCells = EditorGUILayout.Toggle(
                        "填充地面单元", settings.FillFloorCells);
                }
                settings.DrawDynamicBlocked = EditorGUILayout.Toggle(
                    "动态阻挡单元", settings.DrawDynamicBlocked);
                settings.CellFillScale = EditorGUILayout.Slider(
                    "单元填充比例", settings.CellFillScale, 0.1f, 1f);
                break;

            case RoomNavigationGizmoMode.GridCost:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("寻路 Grid 权重", EditorStyles.boldLabel);
                settings.AutoNormalizeCost = EditorGUILayout.Toggle(
                    "自动归一化", settings.AutoNormalizeCost);
                using (new EditorGUI.DisabledScope(settings.AutoNormalizeCost))
                {
                    settings.CostDisplayMaximum = EditorGUILayout.IntField(
                        "显示上限", settings.CostDisplayMaximum);
                }
                DrawCostSettings(settings);
                break;

            case RoomNavigationGizmoMode.FlowIntegration:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("流场积分值", EditorStyles.boldLabel);
                DrawCostSettings(settings);
                break;

            case RoomNavigationGizmoMode.FlowDirections:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("流场方向", EditorStyles.boldLabel);
                settings.DrawTopologyReferenceDirections = EditorGUILayout.Toggle(
                    "拓扑参考方向", settings.DrawTopologyReferenceDirections);
                settings.DrawFlowProgressCorrections = EditorGUILayout.Toggle(
                    "方向进展纠正格", settings.DrawFlowProgressCorrections);
                settings.ArrowStride = EditorGUILayout.IntSlider(
                    "箭头显示间隔", settings.ArrowStride, 1, 8);
                settings.FlowArrowLength = EditorGUILayout.Slider(
                    "箭头长度", settings.FlowArrowLength, 0.05f, 0.9f);
                break;

            case RoomNavigationGizmoMode.MonsterSteering:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("怪物转向", EditorStyles.boldLabel);
                settings.DrawRawSeparation = EditorGUILayout.Toggle(
                    "原始分离向量", settings.DrawRawSeparation);
                settings.DrawFinalVelocity = EditorGUILayout.Toggle(
                    "最终速度", settings.DrawFinalVelocity);
                settings.SteeringScale = EditorGUILayout.FloatField(
                    "向量缩放", settings.SteeringScale);
                settings.MaximumSteeringLength = EditorGUILayout.FloatField(
                    "最大绘制长度", settings.MaximumSteeringLength);
                break;

            case RoomNavigationGizmoMode.MonsterSpatialIndex:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("怪物四叉树分布", EditorStyles.boldLabel);
                settings.DrawMonsterSpatialCounts = EditorGUILayout.Toggle(
                    "叶节点怪物数量", settings.DrawMonsterSpatialCounts);
                settings.DrawMonsterHitRadius = EditorGUILayout.Toggle(
                    "受击半径", settings.DrawMonsterHitRadius);
                settings.DrawLastMonsterSpatialQuery = EditorGUILayout.Toggle(
                    "最近一次弹丸查询", settings.DrawLastMonsterSpatialQuery);
                EditorGUILayout.HelpBox(
                    "该模式会在编辑器绘制前确保空间索引已构建。进行性能采样时请切换到“关闭”，避免 Gizmos 与调试构建开销影响结果。",
                    MessageType.Warning);
                break;
        }

        bool changed = EditorGUI.EndChangeCheck();
        EditorGUILayout.EndScrollView();
        if (!changed)
            return;

        settings.SaveSettings();
        SceneView.RepaintAll();
    }

    private static void DrawCostSettings(RoomNavigationGizmoSettings settings)
    {
        settings.CostOpacity = EditorGUILayout.Slider(
            "颜色不透明度", settings.CostOpacity, 0f, 1f);
        settings.CellFillScale = EditorGUILayout.Slider(
            "单元填充比例", settings.CellFillScale, 0.1f, 1f);
        settings.DrawCostNumbers = EditorGUILayout.Toggle(
            "显示数值", settings.DrawCostNumbers);

        using (new EditorGUI.DisabledScope(!settings.DrawCostNumbers))
        {
            settings.IncludeZeroCostNumbers = EditorGUILayout.Toggle(
                "显示零值", settings.IncludeZeroCostNumbers);
            settings.CostNumberStride = EditorGUILayout.IntSlider(
                "数值显示间隔", settings.CostNumberStride, 1, 8);
        }
    }

    private static string GetModeDescription(RoomNavigationGizmoMode mode)
    {
        return mode switch
        {
            RoomNavigationGizmoMode.Disabled =>
                "关闭全部导航调试绘制。",
            RoomNavigationGizmoMode.NavigationGrid =>
                "查看静态网格、静态阻挡与运行时动态阻挡。静态部分可在非运行状态查看。",
            RoomNavigationGizmoMode.GridCost =>
                "查看寻路实际使用的 Grid 附加权重，即基础附加权重与怪物密度权重之和。",
            RoomNavigationGizmoMode.FlowIntegration =>
                "查看每个可达单元到最近流场源点的累计寻路成本。该数值不是单格权重。",
            RoomNavigationGizmoMode.FlowDirections =>
                "查看流场最终方向与源点；可附加显示拓扑参考方向和方向进展纠正格。",
            RoomNavigationGizmoMode.MonsterSteering =>
                "查看怪物分离向量和最终移动速度。",
            RoomNavigationGizmoMode.MonsterSpatialIndex =>
                "单独查看怪物四叉树叶节点。颜色由青到紫表示层级加深，红色表示密度升高。",
            _ => string.Empty
        };
    }
}
