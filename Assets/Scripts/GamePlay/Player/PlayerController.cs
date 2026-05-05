using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("网络状态同步")]
    public NetworkVariable<PlayerStateType> currentNetState = new NetworkVariable<PlayerStateType>(
        PlayerStateType.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isBeingRevived = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("移动手感配置 (Movement Config)")]
    public float maxMoveSpeed = 8f;
    public float timeToMaxSpeed = 0.3f;
    public AnimationCurve accelerationCurve;
    public float decelerationSpeed = 5f;

    [Range(0.1f, 1f)]
    public float backwardSpeedMultiplier = 0.6f;
    public float rotateSmoothTime = 0.05f;
    private float targetAimAngle;

    [Header("环境检测")]
    public LayerMask groundLayer;

    [Header("救援系统")]
    public float maxReviveTime = 3f;  // 救援所需总时间 (未来可被 Buff 修改)
    public float reviveRadius = 2.5f; // 救援判定半径 (未来可被 Buff 修改，比如扩大救援圈)    public GameObject reviveUIContainer; // 包含光圈和进度条的父节点
    public PlayerReviveUI reviveUI;
    private PlayerController currentlyRescuingTarget;

    private Health health;
    private Rigidbody rb;
    public Animator Anim;

    public StateMachine stateMachine { get; private set; }
    Dictionary<PlayerStateType, State> stateDict = new Dictionary<PlayerStateType, State>();

    public Vector3 CurrentMoveInput => currentMoveInput;

    // 内部状态
    private Vector3 currentMoveInput;
    private float moveTimer = 0f;
    private float currentYVelocity = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        stateMachine = new StateMachine();

        // 实例化所有具体状态，绑定到对应的 Animator Bool 名字上
        stateDict = new Dictionary<PlayerStateType, State>()
        {
            // 假设你的 Animator 里对应的 bool 叫 "isIdle", "isMoving", "isDead"
            { PlayerStateType.Idle, new IdleState(stateMachine, "isIdle", this) },
            { PlayerStateType.Moving, new MoveState(stateMachine, "isMoving", this) },
            { PlayerStateType.dead, new DeadState(stateMachine, "isDead", this) }
        };
        stateMachine.Initialize(stateDict[PlayerStateType.Idle]);
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }
    private void HandleDeath()
    {
        ChangeStateServerRpc(PlayerStateType.dead);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (currentNetState.Value != PlayerStateType.dead)
        {
            HandleRescueTeammateInput();
        }

        stateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        stateMachine.CurrentState.FixedUpdate();
    }
    public override void OnNetworkSpawn()
    {
        currentNetState.OnValueChanged += HandleStateChanged;
        if (stateDict.ContainsKey(currentNetState.Value))
        {
            stateMachine.Initialize(stateDict[currentNetState.Value]);
        }
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.RegisterPlayer(this);
        if (!IsOwner)
        {
            this.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        currentNetState.OnValueChanged -= HandleStateChanged;
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnregisterPlayer(this);
    }
    private void HandleStateChanged(PlayerStateType oldState, PlayerStateType newState)
    {
        // 当网络状态改变时，驱动本地老状态机执行 Exit 和 Enter
        if (stateDict.TryGetValue(newState, out State nextState))
        {
            stateMachine.ChangeState(nextState);
        }
    }
    #region 救援
    private void HandleRescueTeammateInput()
    {
        if (InputManager.Instance.CurrentState != InputState.Gameplay) return;

        // 假设 F 键是救援键
        if (Input.GetKey(KeyCode.F))
        {
            if (currentlyRescuingTarget == null)
            {
                // 遍历寻找附近的死者
                foreach (var p in PlayerManager.Instance.AllPlayers)
                {
                    if (p != this && p.currentNetState.Value == PlayerStateType.dead)
                    {
                        if (Vector3.Distance(transform.position, p.transform.position) <= p.reviveRadius)
                        {
                            currentlyRescuingTarget = p;
                            currentlyRescuingTarget.SetRevivingServerRpc(true);
                            break;
                        }
                    }
                }
            }
            else
            {
                // 如果锁定了目标，检查是否跑出了圈子
                if (Vector3.Distance(transform.position, currentlyRescuingTarget.transform.position) > currentlyRescuingTarget.reviveRadius)
                {
                    currentlyRescuingTarget.SetRevivingServerRpc(false);
                    currentlyRescuingTarget = null;
                }
            }
        }
        else
        {
            // 松开 F 键，打断救援
            if (currentlyRescuingTarget != null)
            {
                currentlyRescuingTarget.SetRevivingServerRpc(false);
                currentlyRescuingTarget = null;
            }
        }
    }

    // 注意这里必须加 RequireOwnership = false，因为是活人 (P2) 呼叫死人 (P1) 身上的 RPC
    [ServerRpc(RequireOwnership = false)]
    public void SetRevivingServerRpc(bool state)
    {
        isBeingRevived.Value = state;
    }
    #endregion
    public void RequestStateChange(PlayerStateType targetState)
    {
        ChangeStateServerRpc(targetState);
    }

    [ServerRpc]
    private void ChangeStateServerRpc(PlayerStateType targetState)
    {
        if (currentNetState.Value == PlayerStateType.dead) return; // 死了就别切了
        currentNetState.Value = targetState;
    }

    public void RequestRevive()
    {
        ReviveServerRpc();
    }
    [ServerRpc]
    private void ReviveServerRpc()
    {
        health.currentHealth.Value = 1f;
        health.isDead = false;

        // 2. 状态拨回站立
        currentNetState.Value = PlayerStateType.Idle;
    }
    // ==========================================
    // 暴露给状态机的能力 API (躯干动作)
    // ==========================================

    /// <summary> 收集所有输入 (由活跃状态在 LogicUpdate 中调用) </summary>
    public void CollectInput()
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
        // 锁定 Y 轴高度 (如果你不需要跳跃的话)
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);

        currentMoveInput = (camForward.normalized * v + camRight.normalized * h).normalized;
    }
    /// <summary> 计算瞄准方向 (由活跃状态在 LogicUpdate 中调用) </summary>
    public void HandleAimingCalculation()
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
                targetAimAngle = Quaternion.LookRotation(targetDir).eulerAngles.y;
            }
        }
    }
    /// <summary> 物理移动执行 (包含你的加减速曲线计算，在 PhysicsUpdate 中调用) </summary>
    public void HandleMovement()
    {
        if (currentMoveInput.sqrMagnitude > 0.01f)
            moveTimer += Time.fixedDeltaTime;
        else
            moveTimer -= Time.fixedDeltaTime * decelerationSpeed;

        moveTimer = Mathf.Clamp(moveTimer, 0f, timeToMaxSpeed);

        if (moveTimer <= 0f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        float normalizedTime = moveTimer / timeToMaxSpeed;
        float curveMultiplier = accelerationCurve.Evaluate(normalizedTime);

        float directionDot = Vector3.Dot(transform.forward, currentMoveInput);
        float directionMultiplier = directionDot < -0.1f ? backwardSpeedMultiplier : 1f;

        float currentSpeed = maxMoveSpeed * curveMultiplier * directionMultiplier;
        Vector3 targetVelocity = currentMoveInput * currentSpeed;

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }
    /// <summary> 物理旋转执行 (在 PhysicsUpdate 中调用) </summary>
    public void ApplyRotation()
    {
        float smoothedAngle = Mathf.SmoothDampAngle(rb.rotation.eulerAngles.y, targetAimAngle, ref currentYVelocity, rotateSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0f, smoothedAngle, 0f));
    }
    /// <summary> 瞬间刹车停止 (倒地或被硬控时强制调用) </summary>
    public void ExecuteStop()
    {
        currentMoveInput = Vector3.zero;
        moveTimer = 0f;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
}