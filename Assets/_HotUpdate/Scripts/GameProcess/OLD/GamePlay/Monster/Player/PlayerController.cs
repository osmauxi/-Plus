using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IKnockbackable
{
    [Header("网络状态同步")]
    public NetworkVariable<PlayerStateType> currentNetState = new NetworkVariable<PlayerStateType>(
        PlayerStateType.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isBeingRevived = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("移动手感配置 (Movement Config)")]
    public float timeToMaxSpeed = 0.3f;
    public AnimationCurve accelerationCurve;
    public float decelerationSpeed = 5f;

    [Range(0.1f, 1f)]
    public float backwardSpeedMultiplier = 0.6f;
    public float rotateSmoothTime = 0.05f;
    private float targetAimAngle;
    [Header("音效配置")]
    public float footstepInterval = 0.45f; // 在面板里调大这个值，脚步声就会变慢
    private float footstepTimer = 0f;

    [Header("环境检测")]
    public LayerMask groundLayer;

    [Header("救援系统")]
    public float maxReviveTime = 3f;  // 救援所需总时间 (未来可被 Buff 修改)
    public float reviveRadius = 2.5f; // 救援判定半径 (未来可被 Buff 修改，比如扩大救援圈)    public GameObject reviveUIContainer; // 包含光圈和进度条的父节点
    public PlayerReviveUI reviveUI;
    private PlayerController currentlyRescuingTarget;
    private CharacterStatCollection statCollection;

    public Rigidbody Rb => rb;
    public Animator Anim => anim;

    private Health health;
    private Rigidbody rb;
    public Animator anim;

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
        statCollection = GetComponent<CharacterStatCollection>();
        // 实例化所有具体状态，绑定到对应的 Animator Bool 名字上
        stateDict = new Dictionary<PlayerStateType, State>()
        {
            // 假设你的 Animator 里对应的 bool 叫 "isIdle", "isMoving", "isDead"
            { PlayerStateType.Idle, new IdleState(stateMachine, "", this) },
            { PlayerStateType.Moving, new MoveState(stateMachine, "", this) },
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
        if (IsServer)
        {
            currentNetState.Value = PlayerStateType.dead;
        }
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
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;

        UpdateAnimatorParameters();
    }
    public override void OnNetworkSpawn()
    {
        currentNetState.OnValueChanged += HandleStateChanged;
        if (stateDict.ContainsKey(currentNetState.Value))
        {
            stateMachine.Initialize(stateDict[currentNetState.Value]);
        }
        //if (PlayerManager.Instance != null)
        //    PlayerManager.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        currentNetState.OnValueChanged -= HandleStateChanged;
        //if (PlayerManager.Instance != null)
        //    PlayerManager.Instance.UnregisterPlayer(this);
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
        if (OLDInputManager.Instance.CurrentState != InputState.Gameplay) return;

        // 假设 F 键是救援键
        if (Input.GetKey(KeyCode.F))
        {
            if (currentlyRescuingTarget == null)
            {
                // 遍历寻找附近的死者
                //foreach (var p in PlayerManager.Instance.AllPlayers)
                //{
                //    if (p != this && p.currentNetState.Value == PlayerStateType.dead)
                //    {
                //        if (Vector3.Distance(transform.position, p.transform.position) <= p.reviveRadius)
                //        {
                //            currentlyRescuingTarget = p;
                //            currentlyRescuingTarget.SetRevivingServerRpc(true);
                //            break;
                //        }
                //    }
                //}
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
    private void UpdateAnimatorParameters()
    {
        if (anim == null) return;

        if (anim.speed < 0.01f) return;

        // ==========================================
        // 世界坐标输入转局部坐标
        // currentMoveInput 是世界坐标方向 
        // 用它和角色的正前方(forward)做点乘，算出他在往自己眼前的哪个方向走 (-1 到 1)
        // 用它和角色的正右方(right)做点乘，算出他在往自己身体的哪侧走 (-1 到 1)
        // ==========================================
        float velocityZ = Vector3.Dot(currentMoveInput, transform.forward); // 前后移动度
        float velocityX = Vector3.Dot(currentMoveInput, transform.right);   // 左右侧滑度

        // 将算出的相对方向传入 Animator，带有 0.1f 的阻尼防抽搐
        anim.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }
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
        float h = OLDInputManager.Instance.MoveHorizontal;
        float v = OLDInputManager.Instance.MoveVertical;

        float scroll = OLDInputManager.Instance.ScrollWheel;
        if (Mathf.Abs(scroll) > 0.01f && CameraViewManager.instance != null)
        {
            CameraViewManager.instance.AdjustZoom(scroll);
        }

        Vector3 camForward = CameraViewManager.instance.CurrentCameraForward;
        Vector3 camRight = CameraViewManager.instance.CurrentCameraRight;

        camRight.y = 0;
        // 锁定 Y 轴高度 (如果你不需要跳跃的话)
        //transform.position = new Vector3(transform.position.x, 1, transform.position.z);

        currentMoveInput = (camForward.normalized * v + camRight.normalized * h).normalized;
    }
    /// <summary> 计算瞄准方向 (由活跃状态在 LogicUpdate 中调用) </summary>
    public void HandleAimingCalculation()
    {
        if (OLDInputManager.Instance.CurrentState != InputState.Gameplay)
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
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        if (currentMoveInput.sqrMagnitude > 0.01f)
        {
            moveTimer += Time.fixedDeltaTime;
            footstepTimer += Time.fixedDeltaTime;

            // 每走 0.35 秒播放一次脚步声
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                // 随机播放玩家行走音效，加上 0.1f 的音调随机波动防止听觉疲劳
                AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Player_Walk, 0.4f);
            }
        }
        else
        {
            moveTimer -= Time.fixedDeltaTime * decelerationSpeed;
            // 玩家停下时立刻重置，保证下次起步的第一下立刻有声音
            footstepTimer = 0.35f;
        }

        if (currentMoveInput.sqrMagnitude > 0.01f)
            moveTimer += Time.fixedDeltaTime;
        else
            moveTimer -= Time.fixedDeltaTime * decelerationSpeed;

        float safeTimeToMax = Mathf.Max(timeToMaxSpeed, 0.01f);
        moveTimer = Mathf.Clamp(moveTimer, 0f, safeTimeToMax);

        // 3. 完全停下时清空水平速度
        if (moveTimer <= 0f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        // 4. 计算 0~1 的时间进度，并通过 Curve 获取 0~1 的速度乘数
        float normalizedTime = moveTimer / safeTimeToMax;
        float curveMultiplier = accelerationCurve.Evaluate(normalizedTime);

        // 5. 计算后退惩罚 (根据输入方向与角色朝向的点乘判断)
        float directionDot = Vector3.Dot(transform.forward, currentMoveInput);
        float directionMultiplier = directionDot < -0.1f ? backwardSpeedMultiplier : 1f;

        // 6. 【核心】从 Stat 系统获取绝对最大速度！(安全校验，如果缺失组件默认用面板的 8)
        float currentMaxSpeed = statCollection != null ? statCollection.GetStatValue(StatType.MoveSpeed) : 8f;

        // 7. 组合最终速度：最大移速 * 曲线比例(0~1) * 方向惩罚
        float finalSpeed = currentMaxSpeed * curveMultiplier * directionMultiplier;
        Vector3 targetVelocity = currentMoveInput * finalSpeed;

        // 8. 赋值给刚体，严格保留 Y 轴原始速度（不干扰重力和掉落）
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
    [ClientRpc]
    public void TeleportClientRpc(Vector3 targetPos)
    {
        if (IsOwner)
        {
            // 1. 彻底斩断玩家的按键输入缓存，强制刹车！
            ExecuteStop();

            // 2. 强行适配你游戏里的 Y 轴锁定法则
            targetPos.y = 1f;

            // 3. 彻底清空刚体的所有物理动量
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None; // 闭屏插值防止镜头拖尾

            // 4. Unity 官方推荐：对于 Rigidbody，强行位移必须同时修改 transform 和 rb.position
            rb.position = targetPos;
            transform.position = targetPos;

            // 5. 瞬间吸附相机！
            if (CameraViewManager.instance != null)
                CameraViewManager.instance.ForceSnapToPlayer();

            StartCoroutine(RestoreInterpolationRoutine());
        }
    }

    private System.Collections.IEnumerator RestoreInterpolationRoutine()
    {
        yield return new WaitForFixedUpdate();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    #region 击退
    private float knockbackTimer = 0f;

    public void ApplyKnockback(Vector3 force)
    {
        if (!IsServer) return;
        // 服务器收到击退申请，通知属于该玩家的客户端去执行物理模拟
        ApplyKnockbackClientRpc(force);
    }

    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 force)
    {
        if (!IsOwner) return;

        if (knockbackTimer > 0f)
        {
            // 【核心】：续杯僵直时间，直接叠加受力，不清空速度！
            knockbackTimer = 0.2f;
            rb.AddForce(force, ForceMode.Impulse);
        }
        else
        {
            // 正常受击：僵直 0.2 秒，清空玩家主动跑步的速度，施加击退力
            knockbackTimer = 0.2f;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
    #endregion
}
