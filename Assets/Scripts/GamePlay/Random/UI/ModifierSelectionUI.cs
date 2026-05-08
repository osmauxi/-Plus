using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ModifierSelectionUI : MonoBehaviour
{
    public static ModifierSelectionUI Instance { get; private set; }

    [Header("UI 引用")]
    public GameObject rootPanel; // 整个黑色半透明遮罩背景
    public TextMeshProUGUI titleText; // 顶部的 "常规武装箱" 或 "异变核心提取"

    [Tooltip("按顺序拖入左、中、右三张卡牌")]
    public ModifierUICard[] uiCards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 游戏开始时默认隐藏
        rootPanel.SetActive(false);
    }

    /// <summary>
    /// 呼出三选一面板
    /// </summary>
    public void ShowPanel(List<ModifierDataSO> choices, Action<string> onCardSelected)
    {
        InputManager.Instance.ChangeState(InputState.UI);
        // 1. 解锁鼠标 (因为你们是射击游戏，平时鼠标可能是隐藏锁定的)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //titleText.text = title;
        rootPanel.SetActive(true);
        Debug.Log(choices.Count);
        // 2. 遍历分配数据
        for (int i = 0; i < uiCards.Length; i++)
        {
            if (i < choices.Count)
            {
                uiCards[i].gameObject.SetActive(true);

                // 给卡牌塞数据，并定义点击后的闭环行为
                uiCards[i].SetupCard(choices[i], (selectedId) =>
                {
                    ClosePanel(); // 点完立马关 UI
                    onCardSelected?.Invoke(selectedId); // 告诉 PlayerModifierHandler 选了哪个
                });
            }
            else
            {
                // 如果抽出来的词条不足 3 个（卡池快空了），就隐藏多余的卡牌
                uiCards[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClosePanel()
    {
        rootPanel.SetActive(false);
        InputManager.Instance.ChangeState(InputState.Gameplay);
        // 恢复鼠标锁定 (请根据你们项目的实际输入系统逻辑进行调整)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}