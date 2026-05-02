using UnityEngine;

public enum InputState
{
    Gameplay, // 正常游玩模式：允许移动、开火，锁定鼠标
    UI        // UI交互模式：禁止移动、开火，解锁鼠标
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputState CurrentState = InputState.Gameplay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 核心状态切换：统一接管鼠标显隐状态
    /// </summary>
    public void ChangeState(InputState newState)
    {
        CurrentState = newState;

        if (CurrentState == InputState.UI)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (CurrentState == InputState.Gameplay)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // ==========================================
    // 供下游调用的包装属性 (水阀门)
    // ==========================================

    // 移动输入
    public float MoveHorizontal => CurrentState == InputState.Gameplay ? Input.GetAxisRaw("Horizontal") : 0f;
    public float MoveVertical => CurrentState == InputState.Gameplay ? Input.GetAxisRaw("Vertical") : 0f;

    // 视角缩放
    public float ScrollWheel => CurrentState == InputState.Gameplay ? Input.GetAxis("Mouse ScrollWheel") : 0f;

    // 鼠标屏幕坐标 (无论UI还是游戏状态都需要，UI层需要点击，Gameplay需要算射线)
    public Vector3 MousePosition => Input.mousePosition;

    // 武器开火 (你之前 WeaponBase 里的输入也可以接到这里)
    public bool FireHeld => CurrentState == InputState.Gameplay && Input.GetMouseButton(0);
    public bool ReloadPressed => CurrentState == InputState.Gameplay && Input.GetKeyDown(KeyCode.R);

    public bool InteractPressed => CurrentState == InputState.Gameplay && Input.GetKeyDown(KeyCode.F);
}