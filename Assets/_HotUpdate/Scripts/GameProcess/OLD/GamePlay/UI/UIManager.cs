using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 单例模式，方便所有 Presenter 随时呼叫
    public static UIManager Instance { get; private set; }

    [Header("常驻 HUD 引用 (供 Presenter 绑定)")]
    public PlayerHUDView mainPlayerView;
    public PlayerHUDView teammateView;

    [Header("全屏功能面板")]
    public GameObject statsAndModsPanel; // 未来的属性面板
    public GameObject pausePanel;        // 未来的暂停面板

    private void Awake()
    {
        // 经典的单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 游戏刚开始时，确保只显示 HUD，隐藏其他全屏面板
        CloseAllPanels();
    }

    // ==========================================
    // 面板开关管理逻辑
    // ==========================================

    /// <summary>
    /// 打开属性与词条面板 (未来由 PlayerStatsPresenter 按 Tab 键调用)
    /// </summary>
    public void ToggleStatsPanel(bool isOpen)
    {
        statsAndModsPanel.SetActive(isOpen);

        if (isOpen)
        {
            // 比如：打开面板时，呼出鼠标指针
            OLDInputManager.Instance.ChangeState(InputState.UI);
        }
        else
        {
            // 关闭面板时，恢复游戏操作
            OLDInputManager.Instance.ChangeState(InputState.Gameplay);
        }
    }

    private void CloseAllPanels()
    {
        if (statsAndModsPanel != null) statsAndModsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }
}