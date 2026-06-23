#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// 绑定目标为你的 CharacterStatCollection 组件
[CustomEditor(typeof(CharacterStatCollection))]
public class CharacterStatCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制原本的组件内容
        base.OnInspectorGUI();

        CharacterStatCollection targetStats = (CharacterStatCollection)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("进入运行模式 (Play Mode) 后，此处将显示实时面板数值。", MessageType.Info);
            return;
        }

        if (targetStats.Stats == null || targetStats.Stats.Count == 0) return;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("?? 实时动态属性面板 (Debug)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("格式: [基础白字] → [最终绿字]");
        EditorGUILayout.Space(5);

        // 准备几种颜色的字体样式
        GUIStyle buffStyle = new GUIStyle(EditorStyles.label);
        buffStyle.normal.textColor = new Color(0.1f, 0.7f, 0.1f); // 绿色增强

        GUIStyle debuffStyle = new GUIStyle(EditorStyles.label);
        debuffStyle.normal.textColor = new Color(0.8f, 0.2f, 0.2f); // 红色削弱

        GUIStyle normalStyle = new GUIStyle(EditorStyles.label);

        // 开始遍历打印你的 Stats 字典
        foreach (var kvp in targetStats.Stats)
        {
            string statName = kvp.Key.ToString();
            float baseVal = kvp.Value.BaseValue;
            float finalVal = kvp.Value.Value;

            EditorGUILayout.BeginHorizontal();

            // 左边显示属性名
            EditorGUILayout.LabelField(statName, GUILayout.Width(150));

            // 判断最终值和基础值的关系，决定显示什么颜色
            GUIStyle currentStyle = normalStyle;
            if (finalVal > baseVal) currentStyle = buffStyle;
            else if (finalVal < baseVal) currentStyle = debuffStyle;

            // 右边显示数值变化
            EditorGUILayout.LabelField($"{baseVal}  →  {finalVal}", currentStyle);

            EditorGUILayout.EndHorizontal();
        }

        // 强制每帧重绘 Inspector，确保你吃 Buff 瞬间数值立刻跳动
        Repaint();
    }
}
#endif