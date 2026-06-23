using MyScripts.Core;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager instance;

    [Header("相机基础配置")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform player;
    public Camera minimapCamera;

    [Header("平滑与跟随参数")]
    public float viewDistance = 12f;      // 投影距离
    public float viewHeight = 10f;        // 相机高度
    public float followSmoothTime = 0.05f;// 追踪平滑（建议0.05~0.1）
    public float rotateSmoothTime = 0.15f;// 旋转平滑

    [Header("滚轮缩放配置")]
    public float minViewHeight = 4f;      // 最近高度
    public float maxViewHeight = 18f;     // 最远高度
    public float zoomSpeed = 10f;         // 滚轮灵敏度
    public float zoomSmoothTime = 0.1f;   // 缩放平滑度

    [Header("控制按键")]
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    // 状态变量
    private float targetYaw = 0f;
    private float currentYaw = 0f;
    private float yawVelocity = 0f;
    private Vector3 followVelocity = Vector3.zero;

    // 缩放状态变量
    private float targetViewHeight;
    private float heightVelocity = 0f;
    private float heightToDistanceRatio; // 核心：高度与距离的黄金比例

    // 物理同步目标点（核心防抽搐变量）
    private Vector3 targetPosition;

    // 导出的逻辑方向（给 PlayerController 使用）
    public Vector3 CurrentCameraForward { get; private set; }
    public Vector3 CurrentCameraRight { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // 彻底切断 Cinemachine 的内置跟随，我们要手动控制它的 Transform
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
        }

        LocalEventCenter.Instance.AddEventListener<GamePlayStartStruct>(CameraInitialize);
    }

    private void OnDestroy()
    {
        LocalEventCenter.Instance.RemoveEventListener<GamePlayStartStruct>(CameraInitialize);
    }

    public void CameraInitialize(Transform playerobj)
    {
        player = playerobj;
        InitializeInternalState();
    }

    public void CameraInitialize(GamePlayStartStruct evt)
    {
        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        InitializeInternalState();
    }

    private void InitializeInternalState()
    {
        if (MinimapCamera.Instance != null)
            minimapCamera = MinimapCamera.Instance.GetComponent<Camera>();

        targetYaw = 0f;
        currentYaw = 0f;
        targetViewHeight = viewHeight;
        heightToDistanceRatio = viewDistance / viewHeight;

        ForceSnapToPlayer(); // 初始时瞬间定位，杜绝滑动入场
    }

    private void Update()
    {
        if (player == null) return;
        HandleRotationInput();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // 1. 在物理帧中计算绝对准确的目标点 (此时物理引擎刚算完玩家的位置)
        // 这样计算出的 targetPosition 是没有物理/渲染采样误差的
        Quaternion yawRotation = Quaternion.Euler(0, currentYaw, 0);
        CurrentCameraForward = yawRotation * Vector3.forward;
        CurrentCameraRight = yawRotation * Vector3.right;

        Vector3 offset = (-CurrentCameraForward * viewDistance) + (Vector3.up * viewHeight);
        targetPosition = player.position + offset;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // 2. 处理旋转角度平滑
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotateSmoothTime);

        // 3. 处理缩放平滑 (高度过渡)
        viewHeight = Mathf.SmoothDamp(viewHeight, targetViewHeight, ref heightVelocity, zoomSmoothTime);
        // 联动计算后方距离，保持俯视角绝对不变
        viewDistance = viewHeight * heightToDistanceRatio;

        // 4. 执行最终位移平滑 (向 FixedUpdate 中算出的同步目标点靠拢)
        virtualCamera.transform.position = Vector3.SmoothDamp(
            virtualCamera.transform.position,
            targetPosition,
            ref followVelocity,
            followSmoothTime
        );

        // 5. 执行旋转锁定
        // 俯角 = Atan(高度/距离)
        float pitch = Mathf.Atan2(viewHeight, viewDistance) * Mathf.Rad2Deg;
        virtualCamera.transform.rotation = Quaternion.Euler(pitch, currentYaw, 0);

        UpdateMinimapRotation();
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(rotateRightKey)) targetYaw += 90f;
        if (Input.GetKeyDown(rotateLeftKey)) targetYaw -= 90f;
    }

    // 开放给玩家控制器的滚轮缩放接口
    public void AdjustZoom(float scrollDelta)
    {
        targetViewHeight -= scrollDelta * zoomSpeed;
        targetViewHeight = Mathf.Clamp(targetViewHeight, minViewHeight, maxViewHeight);
    }

    /// <summary>
    /// 强制将相机瞬间吸附到玩家当前位置
    /// （通常在玩家跨房间传送、复活或刚生成时调用）
    /// </summary>
    public void ForceSnapToPlayer()
    {
        if (player == null) return;

        Quaternion yawRotation = Quaternion.Euler(0, currentYaw, 0);
        CurrentCameraForward = yawRotation * Vector3.forward;
        CurrentCameraRight = yawRotation * Vector3.right;

        Vector3 offset = (-CurrentCameraForward * viewDistance) + (Vector3.up * viewHeight);
        targetPosition = player.position + offset;

        virtualCamera.transform.position = targetPosition;
        followVelocity = Vector3.zero; // 清空惯性

        float pitch = Mathf.Atan2(viewHeight, viewDistance) * Mathf.Rad2Deg;
        virtualCamera.transform.rotation = Quaternion.Euler(pitch, currentYaw, 0);
    }

    private void UpdateMinimapRotation()
    {
        if (minimapCamera != null)
        {
            minimapCamera.transform.eulerAngles = new Vector3(
                minimapCamera.transform.eulerAngles.x,
                minimapCamera.transform.eulerAngles.y,
                -currentYaw
            );
        }
    }
}