using System;
using Unity.Netcode;
using UnityEngine;

// 极其纯粹的、面向表现和手感的客户端权威移动控制器
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [Header("移动手感配置 (Movement Config)")]
    public float maxMoveSpeed = 8f;               // 最高移速
    public float timeToMaxSpeed = 0.3f;           // 达到最高速度所需的时间（秒）
    public AnimationCurve accelerationCurve;      // 加速曲线 (X: 0~1时间，Y: 0~1速度倍率。建议：缓出曲线，起步稍微慢一点，然后迅速拔高)
    public float decelerationSpeed = 5f;          // 停下时的减速速率（为了手感干脆，通常设大一点，或者直接归零）

    [Range(0.1f, 1f)]
    public float backwardSpeedMultiplier = 0.6f;  // 后退时的速度惩罚 (60%移速)
    public float rotateSmoothTime = 0.05f;        // 转身平滑时间（给鼠标瞄准一点微小的缓冲，不要太生硬）
    private float targetAimAngle;

    [Header("环境检测")]
    public LayerMask groundLayer;                 // 鼠标射线检测的地面层

    // 核心组件引用
    private Rigidbody rb;
    private Animator anim; // 留给你的动画状态机

    // 内部状态
    private Vector3 currentMoveInput;
    private float moveTimer = 0f;                 // 用于计算加速曲线的当前时间
    private float currentYVelocity = 0f;          // 用于平滑转身的ref变量

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        // 如果不是本地玩家，直接把这个脚本禁掉！
        // 因为位置和旋转已经由 ClientNetworkTransform 自动同步过来了，
        // 远端玩家只需要扮演没有灵魂的显示器，不需要执行任何移动逻辑。
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnregisterPlayer(this);
    }

    private void Update()
    {
        // 收集输入（Update里收集输入最准，FixedUpdate里执行物理）
        CollectInput();

        HandleAimingCalculation();
    }

    private void FixedUpdate()
    {
        // 执行运动学物理移动
        HandleMovement();
        ApplyRotation();
    }
    private void HandleAimingCalculation()
    {
        if (InputManager.Instance.CurrentState != InputState.Gameplay) 
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 aimPoint = ray.GetPoint(rayDistance);
            Vector3 targetDir = aimPoint - transform.position;

            if (targetDir.sqrMagnitude > 0.01f)
            {
                // 只算出目标角度，存起来
                targetAimAngle = Quaternion.LookRotation(targetDir).eulerAngles.y;
            }
        }
    }
    private void ApplyRotation()
    {
        // 在 FixedUpdate 里平滑并应用旋转
        float smoothedAngle = Mathf.SmoothDampAngle(rb.rotation.eulerAngles.y, targetAimAngle, ref currentYVelocity, rotateSmoothTime);

        // 必须使用 rb.MoveRotation！这不会打断 Interpolate！
        rb.MoveRotation(Quaternion.Euler(0f, smoothedAngle, 0f));
    }
    private void CollectInput()
    {
        float h = InputManager.Instance.MoveHorizontal;
        float v = InputManager.Instance.MoveVertical;

        float scroll = InputManager.Instance.ScrollWheel;
        if (Mathf.Abs(scroll) > 0.01f && CameraViewManager.instance != null)
        {
            CameraViewManager.instance.AdjustZoom(scroll);
        }

        Vector3 camForward = CameraViewManager.instance.CurrentCameraForward;
        Vector3 camRight = CameraViewManager.instance.CurrentCameraRight;

        camRight.y = 0;
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);

        currentMoveInput = (camForward.normalized * v + camRight.normalized * h).normalized;
    }

    private void HandleMovement()
    {
        if (currentMoveInput.sqrMagnitude > 0.01f)
        {
            moveTimer += Time.fixedDeltaTime;
        }
        else
        {
            moveTimer -= Time.fixedDeltaTime * decelerationSpeed;
        }

        moveTimer = Mathf.Clamp(moveTimer, 0f, timeToMaxSpeed);

        if (moveTimer <= 0f)
        {
            // 当玩家松开键盘完全停下时，水平速度归零，但必须保留重力下落的速度！
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        float normalizedTime = moveTimer / timeToMaxSpeed;
        float curveMultiplier = accelerationCurve.Evaluate(normalizedTime);

        float directionDot = Vector3.Dot(transform.forward, currentMoveInput);
        float directionMultiplier = 1f;

        if (directionDot < -0.1f)
        {
            directionMultiplier = backwardSpeedMultiplier;
        }

        float currentSpeed = maxMoveSpeed * curveMultiplier * directionMultiplier;

        // 算出目标水平速度
        Vector3 targetVelocity = currentMoveInput * currentSpeed;

        // X 和 Z 采用我们算好的移速，Y 轴完美继承物理引擎当前计算出的重力下落速度
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }

}