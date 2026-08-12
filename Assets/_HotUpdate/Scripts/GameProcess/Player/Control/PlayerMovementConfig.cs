using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 一种 Locomotion 模式对应的运动响应参数。
    /// Motor 不关心这是 Free、Aim 还是 Sprint，只消费最终传入的 Profile。
    /// </summary>
    [Serializable]
    public sealed class PlayerMovementProfile
    {
        [Header("线性移动")]
        [Tooltip("当前移动模式允许达到的最高平面速度，单位为米/秒。调大：移动更快、制动距离通常更长；调小：移动更慢、更容易精细控制。")]
        [InspectorName("最大移动速度")]
        [SerializeField, Min(0f)] private float _maxSpeed = 5.5f;
        [Tooltip("有移动输入且未超速时，实际速度追赶目标速度的加速度。调大：起步和变向响应更快、更灵敏；调小：提速更缓、惯性感更强。")]
        [InspectorName("移动加速度")]
        [SerializeField, Min(0f)] private float _acceleration = 18f;
        [Tooltip("松开输入或切换到更低目标速度时使用的减速度。调大：更快停下、滑行距离更短；调小：减速更慢、滑行距离更长。")]
        [InspectorName("移动减速度")]
        [SerializeField, Min(0f)] private float _deceleration = 22f;
        [Tooltip("高速强反向输入触发 Pivot 时的专用制动加速度。调大：更快刹停并转向、折返距离更短；调小：反向前冲更明显、折返更沉重。")]
        [InspectorName("急转制动加速度")]
        [SerializeField, Min(0f)] private float _pivotBrakeAcceleration = 30f;

        [Header("朝向旋转")]
        [Tooltip("角色绕 Y 轴旋转时允许达到的最大角速度，单位为度/秒。调大：最大转身速度更快；调小：转身更慢、更有重量感。")]
        [InspectorName("最大旋转速度")]
        [SerializeField, Min(0f)] private float _maxRotationSpeed = 540f;
        [Tooltip("角色角速度追赶目标角速度的加速度，单位为度/秒²。调大：开始转身更迅速、更跟手；调小：转身启动更柔和。")]
        [InspectorName("旋转加速度")]
        [SerializeField, Min(0f)] private float _rotationAcceleration = 1800f;
        [Tooltip("没有有效朝向请求或接近目标方向时，剩余角速度的衰减速度。调大：更快停止旋转、过冲更少；调小：旋转收尾更柔和、惯性更明显。")]
        [InspectorName("旋转减速度")]
        [SerializeField, Min(0f)] private float _rotationDeceleration = 2200f;

        [Header("背向移动惩罚")]
        [Tooltip("移动方向与角色当前朝向的夹角大于该值时，视为背向移动。")]
        [SerializeField, Range(90f, 180f)] private float _backwardAngleThreshold = 120f;

        [Tooltip("背向移动时的最大速度倍率。1=无惩罚，0.6=最大速度降低至60%。")]
        [SerializeField, Range(0.1f, 1f)] private float _backwardSpeedMultiplier = 0.65f;

        public float BackwardAngleThreshold => _backwardAngleThreshold;
        public float BackwardSpeedMultiplier => _backwardSpeedMultiplier;

        public float MaxSpeed => _maxSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float PivotBrakeAcceleration => _pivotBrakeAcceleration;

        public float MaxRotationSpeed => _maxRotationSpeed;
        public float RotationAcceleration => _rotationAcceleration;
        public float RotationDeceleration => _rotationDeceleration;

        public void Validate(string profileName)
        {
            if (_maxSpeed < 0f) throw new InvalidOperationException($"{profileName}.{nameof(_maxSpeed)} 不能小于 0。");
            if (_acceleration <= 0f) throw new InvalidOperationException($"{profileName}.{nameof(_acceleration)} 必须大于 0。");
            if (_deceleration <= 0f) throw new InvalidOperationException($"{profileName}.{nameof(_deceleration)} 必须大于 0。");
            if (_pivotBrakeAcceleration <= 0f) throw new InvalidOperationException($"{profileName}.{nameof(_pivotBrakeAcceleration)} 必须大于 0。");
            if (_maxRotationSpeed < 0f) throw new InvalidOperationException($"{profileName}.{nameof(_maxRotationSpeed)} 不能小于 0。");
            if (_rotationAcceleration <= 0f) throw new InvalidOperationException($"{profileName}.{nameof(_rotationAcceleration)} 必须大于 0。");
            if (_rotationDeceleration <= 0f) throw new InvalidOperationException($"{profileName}.{nameof(_rotationDeceleration)} 必须大于 0。");
        }
    }

    /// <summary>
    /// 玩家基础移动配置。
    ///
    /// 当前直接序列化在 PlayerMotor Inspector。
    /// 等手感稳定后，再考虑从 Excel / ConfigManager 构建。
    /// </summary>
    [Serializable]
    public sealed class PlayerMovementConfig
    {
        [Header("移动模式配置")]
        [Tooltip("自由移动状态使用的速度、加减速和旋转参数。")]
        [InspectorName("自由移动")]
        [SerializeField] private PlayerMovementProfile _free = new();
        [Tooltip("按住瞄准时使用的移动参数。通常可降低速度并调整旋转响应，以便进行精确的四向移动。")]
        [InspectorName("瞄准移动")]
        [SerializeField] private PlayerMovementProfile _aim = new();
        [Tooltip("按住冲刺且体力允许时使用的移动参数。通常将最大速度设置得高于自由移动。")]
        [InspectorName("冲刺移动")]
        [SerializeField] private PlayerMovementProfile _sprint = new();

        [Header("运动状态判定")]
        [Tooltip("移动输入长度低于此值时视为无输入。调大：能过滤更多摇杆漂移，但轻推摇杆更难生效；调小：输入更灵敏，但更容易受到微小噪声影响。")]
        [InspectorName("移动输入死区")]
        [SerializeField, Range(0f, 0.5f)] private float _moveInputDeadZone = 0.05f;

        [Tooltip("实际速度低于此值时，MotionState 会将角色判定为已经静止。调大：更早进入 Idle、忽略更多低速滑动；调小：只有更接近完全静止时才进入 Idle。")]
        [InspectorName("静止速度阈值")]
        [SerializeField, Min(0f)] private float _stopSpeedThreshold = 0.08f;

        [Tooltip("实际速度达到当前模式最大速度的该比例后，运动阶段由 Start 转为 Move。调大：Start 阶段持续更久；调小：更早进入稳定 Move 阶段。")]
        [InspectorName("起步转移动速度比例")]
        [SerializeField, Range(0.05f, 1f)] private float _startToMoveNormalizedSpeed = 0.5f;

        [Header("急转与折返判定")]
        [Tooltip("实际速度低于此值时不触发 Pivot 急转制动。调大：只有高速时才触发急转；调小：中低速反向输入也更容易触发急转。")]
        [InspectorName("急转最低速度")]
        [SerializeField, Min(0f)] private float _pivotMinSpeed = 2f;

        [Tooltip("当前速度方向与目标移动方向的点积小于等于此值时判定为强反向（-1 为完全相反，0 为垂直）。调大：更小的转向角也会触发 Pivot；调小：只有更接近完全反向时才触发。")]
        [InspectorName("急转方向点积阈值")]
        [SerializeField, Range(-1f, 1f)] private float _pivotDirectionDotThreshold = -0.35f;

        [Header("角色朝向判定")]
        [Tooltip("目标朝向向量长度低于此值时视为没有有效朝向请求。调大：忽略更多微小朝向变化、更加稳定；调小：对微小朝向输入更敏感。")]
        [InspectorName("朝向输入死区")]
        [SerializeField, Min(0f)] private float _facingDirectionDeadZone = 0.001f;

        [Tooltip("与目标朝向的夹角小于此值且角速度很低时直接吸附到目标，避免收尾抖动。调大：更早吸附、停止更干脆但可能略显突兀；调小：收尾更平滑精确，但更容易出现微小抖动。")]
        [InspectorName("旋转吸附角度")]
        [SerializeField, Range(0.01f, 5f)] private float _rotationSnapAngle = 0.15f;

        public PlayerMovementProfile Free => _free;
        public PlayerMovementProfile Aim => _aim;
        public PlayerMovementProfile Sprint => _sprint;

        public float MoveInputDeadZone => _moveInputDeadZone;
        public float StopSpeedThreshold => _stopSpeedThreshold;
        public float StartToMoveNormalizedSpeed => _startToMoveNormalizedSpeed;
        public float PivotMinSpeed => _pivotMinSpeed;
        public float PivotDirectionDotThreshold => _pivotDirectionDotThreshold;
        public float FacingDirectionDeadZone => _facingDirectionDeadZone;
        public float RotationSnapAngle => _rotationSnapAngle;

        public void Validate()
        {
            if (_free == null || _aim == null || _sprint == null) throw new InvalidOperationException("PlayerMovementConfig 存在空 MovementProfile。");

            _free.Validate(nameof(Free));
            _aim.Validate(nameof(Aim));
            _sprint.Validate(nameof(Sprint));

            if (_stopSpeedThreshold < 0f) throw new InvalidOperationException($"{nameof(_stopSpeedThreshold)} 不能小于 0。");
            if (_pivotMinSpeed < 0f) throw new InvalidOperationException($"{nameof(_pivotMinSpeed)} 不能小于 0。");
        }
    }
}
