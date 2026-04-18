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

    [Header("手感参数")]
    public float viewDistance = 12f;   // 投影距离
    public float viewHeight = 10f;     // 相机高度
    public float followSmoothTime = 0.1f; // 追踪平滑（越小越硬）
    public float rotateSmoothTime = 0.15f; // Q/E 旋转平滑

    [Header("控制按键")]
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    // 状态变量
    private float targetYaw = 0f;
    private float currentYaw = 0f;
    private float yawVelocity = 0f;
    private Vector3 followVelocity = Vector3.zero;

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

        if (MinimapCamera.Instance != null)
            minimapCamera = MinimapCamera.Instance.GetComponent<Camera>();

        // 初始位置同步
        targetYaw = 0f;
        currentYaw = 0f;
        UpdateCameraTransform(true); // 初始时瞬间定位
    }

    public void CameraInitialize(GamePlayStartStruct evt)
    {
        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        if (MinimapCamera.Instance != null)
            minimapCamera = MinimapCamera.Instance.GetComponent<Camera>();

        // 初始位置同步
        targetYaw = 0f;
        currentYaw = 0f;
        UpdateCameraTransform(true); // 初始时瞬间定位
    }

    private void Update()
    {
        if (player == null) return;

        HandleRotationInput();
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // 1. 处理角度平滑
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotateSmoothTime);

        // 2. 更新摄像机 Transform
        UpdateCameraTransform(false);

        // 3. 更新小地图
        UpdateMinimapRotation();
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(rotateRightKey)) targetYaw += 90f;
        if (Input.GetKeyDown(rotateLeftKey)) targetYaw -= 90f;
    }

    private void UpdateCameraTransform(bool instant)
    {
        // 计算纯逻辑旋转（用于计算方向和位移）
        Quaternion yawRotation = Quaternion.Euler(0, currentYaw, 0);
        CurrentCameraForward = yawRotation * Vector3.forward;
        CurrentCameraRight = yawRotation * Vector3.right;

        // 计算目标位置：玩家位置 + 基于 Yaw 偏移的后方距离 + 高度
        Vector3 offset = (-CurrentCameraForward * viewDistance) + (Vector3.up * viewHeight);
        Vector3 targetPos = player.position + offset;

        // 执行跟随平滑（使用 SmoothDamp 模拟有质量的镜头感）
        if (instant)
        {
            virtualCamera.transform.position = targetPos;
            followVelocity = Vector3.zero;
        }
        else
        {
            virtualCamera.transform.position = Vector3.SmoothDamp(
                virtualCamera.transform.position,
                targetPos,
                ref followVelocity,
                followSmoothTime
            );
        }

        // 执行旋转锁定：
        // 俯角计算：LookAt 玩家位置。但为了绝对稳定，我们计算一个固定的 Pitch 角度
        // 俯角 = Atan(高度/距离)
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