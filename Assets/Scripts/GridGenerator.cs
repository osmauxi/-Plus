using UnityEditor;
using UnityEngine;

public class GridGenerator : EditorWindow
{
    public GameObject floorPrefab;
    public int rows = 7;      // 行数
    public int columns = 7;   // 列数
    public float spacing = 2f; // 每块地板的边长（需要根据你的模型实际尺寸修改）

    [MenuItem("Tools/地编小工具/一键铺设地板阵列")]
    public static void ShowWindow()
    {
        GetWindow<GridGenerator>("地板生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("地板网格生成器", EditorStyles.boldLabel);

        floorPrefab = (GameObject)EditorGUILayout.ObjectField("地板预制件", floorPrefab, typeof(GameObject), false);
        rows = EditorGUILayout.IntField("X轴数量 (行)", rows);
        columns = EditorGUILayout.IntField("Z轴数量 (列)", columns);
        spacing = EditorGUILayout.FloatField("地板间距(模型尺寸)", spacing);

        if (GUILayout.Button("一键铺满！"))
        {
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        if (floorPrefab == null)
        {
            Debug.LogError("请先拖入地板预制件！");
            return;
        }

        // 创建一个父节点保持层级干净
        GameObject parentObj = new GameObject($"FloorGrid_{rows}x{columns}");

        // 注册撤销操作，万一铺错了可以 Ctrl+Z 一秒撤销
        Undo.RegisterCreatedObjectUndo(parentObj, "Generate Floor Grid");

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                // 使用 PrefabUtility 生成，这样生成出来的物体依然保持预制件的连接！
                GameObject newFloor = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab, parentObj.transform);

                // 计算位置
                newFloor.transform.position = new Vector3(x * spacing, 0, z * spacing);

                Undo.RegisterCreatedObjectUndo(newFloor, "Generate Floor Grid");
            }
        }
        Debug.Log("铺设完成！");
    }
}