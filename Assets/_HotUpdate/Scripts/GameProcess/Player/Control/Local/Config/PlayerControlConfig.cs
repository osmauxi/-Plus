using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Movement
{
    /// <summary>
    /// 一种移动模式对应的线性移动与旋转响应参数 
    /// </summary>
    [Serializable]
    public sealed class PlayerMovementProfile
    {
        [Header("线性移动")]
        [Tooltip("当前模式允许达到的最高平面速度（米/秒） 增大：移动更快且制动距离通常更长；减小：移动更慢、更易精细控制 ")]
        [InspectorName("最大移动速度")]
        [SerializeField, Min(0f)] private float _maxSpeed = 5.5f;

        [Tooltip("实际速度追赶目标速度的加速度 增大：起步和变向更灵敏；减小：提速更缓、惯性感更强 ")]
        [InspectorName("移动加速度")]
        [SerializeField, Min(0f)] private float _acceleration = 18f;

        [Tooltip("松开输入或目标速度降低时使用的减速度 增大：更快停下、滑行更短；减小：减速更慢、滑行更长 ")]
        [InspectorName("移动减速度")]
        [SerializeField, Min(0f)] private float _deceleration = 22f;

        [Tooltip("高速强反向触发急转时的制动加速度 增大：更快刹停折返；减小：前冲和重量感更明显 ")]
        [InspectorName("急转制动加速度")]
        [SerializeField, Min(0f)] private float _pivotBrakeAcceleration = 30f;

        [Header("Pivot 短时爆发")]
        [Tooltip("该移动模式触发 Pivot 后，爆发速度保持的秒数 0 表示禁用此模式的 Pivot 爆发 ")]
        [InspectorName("Pivot 爆发持续时间")]
        [SerializeField, Min(0f)] private float _pivotBoostDuration;

        [Tooltip("Pivot 爆发期间在该模式最大移动速度上追加的米/秒 0 表示禁用此模式的 Pivot 爆发 ")]
        [InspectorName("Pivot 速度加成")]
        [SerializeField, Min(0f)] private float _pivotSpeedBonus;

        [Tooltip("持续移动时改变速度方向所使用的响应倍率 只强化弧线转向，不改变直线起步与松键刹车 ")]
        [InspectorName("方向改变响应倍率")]
        [SerializeField, Min(1f)] private float _directionChangeAccelerationMultiplier = 2.5f;

        [Header("朝向旋转")]
        [Tooltip("绕 Y 轴允许达到的最大角速度（度/秒） 增大：转身更快；减小：转身更沉稳 ")]
        [InspectorName("最大旋转速度")]
        [SerializeField, Min(0f)] private float _maxRotationSpeed = 540f;

        [Tooltip("角速度追赶目标角速度的加速度 增大：转身启动更快；减小：转身启动更柔和 ")]
        [InspectorName("旋转加速度")]
        [SerializeField, Min(0f)] private float _rotationAcceleration = 1800f;

        [Tooltip("接近目标朝向时角速度的衰减速度 增大：更快停止、过冲更少；减小：收尾更柔和、惯性更明显 ")]
        [InspectorName("旋转减速度")]
        [SerializeField, Min(0f)] private float _rotationDeceleration = 2200f;

        [Header("背向移动惩罚")]
        [Tooltip("移动方向与当前朝向夹角达到该值时视为背向移动 增大：更难触发背向减速；减小：侧后方移动也更早减速 ")]
        [InspectorName("背向判定角度")]
        [SerializeField, Range(90f, 180f)] private float _backwardAngleThreshold = 120f;

        [Tooltip("背向移动时的最大速度倍率 增大：背向速度更接近正常速度；减小：背向移动惩罚更强 ")]
        [InspectorName("背向速度倍率")]
        [SerializeField, Range(0.1f, 1f)] private float _backwardSpeedMultiplier = 0.65f;

        public float MaxSpeed => _maxSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float PivotBrakeAcceleration => _pivotBrakeAcceleration;
        public float PivotBoostDuration => _pivotBoostDuration;
        public float PivotSpeedBonus => _pivotSpeedBonus;
        public float DirectionChangeAccelerationMultiplier => _directionChangeAccelerationMultiplier;
        public float MaxRotationSpeed => _maxRotationSpeed;
        public float RotationAcceleration => _rotationAcceleration;
        public float RotationDeceleration => _rotationDeceleration;
        public float BackwardAngleThreshold => _backwardAngleThreshold;
        public float BackwardSpeedMultiplier => _backwardSpeedMultiplier;

        public void Validate(string profileName)
        {
            if (_maxSpeed < 0f) throw new InvalidOperationException($"{profileName}.最大移动速度不能小于 0 ");
            if (_acceleration <= 0f) throw new InvalidOperationException($"{profileName}.移动加速度必须大于 0 ");
            if (_deceleration <= 0f) throw new InvalidOperationException($"{profileName}.移动减速度必须大于 0 ");
            if (_pivotBrakeAcceleration <= 0f) throw new InvalidOperationException($"{profileName}.急转制动加速度必须大于 0 ");
            if (_pivotBoostDuration < 0f) throw new InvalidOperationException($"{profileName}.Pivot 爆发持续时间不能小于 0 ");
            if (_pivotSpeedBonus < 0f) throw new InvalidOperationException($"{profileName}.Pivot 速度加成不能小于 0 ");
            if (_directionChangeAccelerationMultiplier < 1f) throw new InvalidOperationException($"{profileName}.方向改变响应倍率不能小于 1 ");
            if (_maxRotationSpeed < 0f) throw new InvalidOperationException($"{profileName}.最大旋转速度不能小于 0 ");
            if (_rotationAcceleration <= 0f) throw new InvalidOperationException($"{profileName}.旋转加速度必须大于 0 ");
            if (_rotationDeceleration <= 0f) throw new InvalidOperationException($"{profileName}.旋转减速度必须大于 0 ");
        }
    }

    [Serializable]
    public sealed class PlayerMovementConfig
    {
        [Header("移动模式配置")]
        [Tooltip("自由移动状态使用的参数 提高速度/响应会更灵敏；降低则更稳重 ")]
        [InspectorName("自由移动")]
        [SerializeField] private PlayerMovementProfile _free = new();

        [Tooltip("按住瞄准时使用的参数 通常降低速度以强化精确移动 ")]
        [InspectorName("瞄准移动")]
        [SerializeField] private PlayerMovementProfile _aim = new();

        [Tooltip("冲刺且体力允许时使用的参数 通常提高最大速度和加速度 ")]
        [InspectorName("冲刺移动")]
        [SerializeField] private PlayerMovementProfile _sprint = new();

        [Header("运动状态判定")]
        [Tooltip("移动输入低于此值时视为无输入 增大：过滤更多漂移但轻推更难生效；减小：更灵敏但更易受噪声影响 ")]
        [InspectorName("移动输入死区")]
        [SerializeField, Range(0f, 0.5f)] private float _moveInputDeadZone = 0.05f;

        [Tooltip("实际速度低于此值时判定为静止 增大：更早进入静止；减小：保留更多低速滑动 ")]
        [InspectorName("静止速度阈值")]
        [SerializeField, Min(0f)] private float _stopSpeedThreshold = 0.08f;

        [Tooltip("实际速度达到最大速度的该比例后从起步转为移动 增大：起步阶段更久；减小：更早进入稳定移动 ")]
        [InspectorName("起步转移动速度比例")]
        [SerializeField, Range(0.05f, 1f)] private float _startToMoveNormalizedSpeed = 0.5f;

        [Header("急转与折返判定")]
        [Tooltip("低于此速度不触发急转制动；起步阶段无论速度如何都不会触发 增大：只有高速稳定移动才急转；减小：中速移动也更易急转 ")]
        [InspectorName("急转最低速度")]
        [SerializeField, Min(0f)] private float _pivotMinSpeed = 4f;

        [Tooltip("速度方向与目标方向点积低于该值时判为强反向 增大：更小转角也会急转；减小：需更接近完全反向 ")]
        [InspectorName("急转方向点积阈值")]
        [SerializeField, Range(-1f, 1f)] private float _pivotDirectionDotThreshold = -0.15f;

        [Header("角色朝向判定")]
        [Tooltip("目标朝向低于此长度时视为无效 增大：朝向更稳定；减小：对微小变化更敏感 ")]
        [InspectorName("朝向输入死区")]
        [SerializeField, Min(0f)] private float _facingDirectionDeadZone = 0.001f;

        [Tooltip("角度小于该值且角速度很低时直接吸附目标 增大：更早吸附；减小：更平滑但更易轻微抖动 ")]
        [InspectorName("旋转吸附角度")]
        [SerializeField, Range(0.01f, 5f)] private float _rotationSnapAngle = 0.15f;

        [Tooltip("实际移动速度低于该值时逐渐提高转向响应，用于静止起步和低速连续换向 ")]
        [InspectorName("低速转向加速阈值")]
        [SerializeField, Min(0.01f)] private float _lowSpeedRotationBoostThreshold = 3f;

        [Tooltip("完全静止时旋转加速度和减速度的倍率；速度接近阈值时平滑回到 1 不会改变高速移动的最大转速 ")]
        [InspectorName("静止转向响应倍率")]
        [SerializeField, Min(1f)] private float _stationaryRotationResponseMultiplier = 3f;

        [Tooltip("完全静止时最大旋转速度的倍率；速度接近阈值时平滑回到 1，高速移动的最大转速不变 ")]
        [InspectorName("静止最大转速倍率")]
        [SerializeField, Min(1f)] private float _stationaryMaxRotationSpeedMultiplier = 1.5f;

        [Header("瞄准身体转向")]
        [Tooltip("瞄准方向与身体朝向夹角超过该值后，Root 开始跟随瞄准方向 ")]
        [InspectorName("身体转向开始角度")]
        [SerializeField, Range(0f, 180f)] private float _aimBodyTurnStartAngle = 60f;

        [Tooltip("Root 跟随期间，夹角降到该值后停止跟随 必须小于开始角度以形成迟滞 ")]
        [InspectorName("身体转向停止角度")]
        [SerializeField, Range(0f, 180f)] private float _aimBodyTurnStopAngle = 20f;

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
        public float LowSpeedRotationBoostThreshold => _lowSpeedRotationBoostThreshold;
        public float StationaryRotationResponseMultiplier => _stationaryRotationResponseMultiplier;
        public float StationaryMaxRotationSpeedMultiplier => _stationaryMaxRotationSpeedMultiplier;
        public float AimBodyTurnStartAngle => _aimBodyTurnStartAngle;
        public float AimBodyTurnStopAngle => _aimBodyTurnStopAngle;

        public void Validate()
        {
            if (_free == null || _aim == null || _sprint == null)
                throw new InvalidOperationException("玩家移动配置存在空的移动模式参数 ");

            _free.Validate(nameof(Free));
            _aim.Validate(nameof(Aim));
            _sprint.Validate(nameof(Sprint));

            if (_aimBodyTurnStartAngle <= 0f || _aimBodyTurnStartAngle > 180f)
                throw new InvalidOperationException("身体转向开始角度必须大于 0 且不超过 180 ");

            if (_lowSpeedRotationBoostThreshold <= 0f)
                throw new InvalidOperationException("低速转向加速阈值必须大于 0 ");

            if (_stationaryRotationResponseMultiplier < 1f)
                throw new InvalidOperationException("静止转向响应倍率不能小于 1 ");

            if (_stationaryMaxRotationSpeedMultiplier < 1f)
                throw new InvalidOperationException("静止最大转速倍率不能小于 1 ");

            if (_aimBodyTurnStopAngle < 0f || _aimBodyTurnStopAngle >= _aimBodyTurnStartAngle)
                throw new InvalidOperationException("身体转向停止角度必须大于等于 0 且小于开始角度 ");
        }
    }
}

namespace ProjectGame.HotFix.Gameplay.Player.Stamina
{
    [Serializable]
    public sealed class PlayerStaminaConfig
    {
        [Header("基础体力")]
        [Tooltip("玩家可持有的最大体力 增大：可连续冲刺更久；减小：体力循环更频繁 ")]
        [InspectorName("最大体力")]
        [SerializeField, Min(1f)] private float _maxStamina = 100f;

        [Header("冲刺消耗")]
        [Tooltip("冲刺每秒消耗的体力 增大：更快耗尽；减小：可持续冲刺更久 ")]
        [InspectorName("每秒冲刺消耗")]
        [SerializeField, Min(0f)] private float _sprintDrainPerSecond = 18f;

        [Header("体力恢复")]
        [Tooltip("恢复阶段每秒补充的体力 增大：恢复更快；减小：再次冲刺需等待更久 ")]
        [InspectorName("每秒恢复量")]
        [SerializeField, Min(0f)] private float _recoveryPerSecond = 25f;

        [Tooltip("停止消耗后开始恢复前的等待秒数 增大：恢复启动更晚；减小：恢复启动更早 ")]
        [InspectorName("恢复延迟")]
        [SerializeField, Min(0f)] private float _recoveryDelay = 0.8f;

        [Tooltip("耗尽后至少恢复到最大体力的该比例才允许再次冲刺 增大：耗尽惩罚更强；减小：更早解除耗尽 ")]
        [InspectorName("耗尽解除比例")]
        [SerializeField, Range(0f, 1f)] private float _exhaustedRecoveryRatio = 0.2f;

        public float MaxStamina => _maxStamina;
        public float SprintDrainPerSecond => _sprintDrainPerSecond;
        public float RecoveryPerSecond => _recoveryPerSecond;
        public float RecoveryDelay => _recoveryDelay;
        public float ExhaustedRecoveryRatio => _exhaustedRecoveryRatio;

        public void Validate()
        {
            if (_maxStamina <= 0f)
                throw new InvalidOperationException("最大体力必须大于 0 ");
        }
    }
}

namespace ProjectGame.HotFix.Gameplay.Player.State
{
    /// <summary>
    /// 玩家动作层的确定性时间配置 
    /// 这里只描述受击占用、连续射击节奏和换弹占用时间；具体武器弹药、伤害与动画片段不属于本配置 
    /// 所有秒数都会在固定模拟 Tick 中向上取整，保证实际状态持续时间不会短于设计值 
    /// </summary>
    [Serializable]
    public sealed class PlayerActionConfig
    {
        [Header("受击反应")]
        [Tooltip("普通受击状态持续时间 状态模拟会按固定 Tick 转换，不依赖动画片段结束 ")]
        [InspectorName("受击持续时间")]
        [SerializeField, Min(0.01f)] private float _hitReactionDuration = 0.2f;

        [Header("射击节奏")]
        [Tooltip("按住射击时两次 ShotSequence 递增之间的最短时间 后续可由武器配置覆盖 ")]
        [InspectorName("射击间隔")]
        [SerializeField, Min(0.01f)] private float _fireInterval = 0.12f;

        [Header("换弹")]
        [Tooltip("第一版固定换弹时间 后续接入武器 Gameplay 配置后由具体武器提供 ")]
        [InspectorName("换弹时间")]
        [SerializeField, Min(0.01f)] private float _reloadDuration = 1.4f;

        /// <summary>普通受击对 Gameplay 操作的占用秒数 </summary>
        public float HitReactionDuration => _hitReactionDuration;
        /// <summary>按住射击时两次 ShotSequence 之间允许的最短秒数 </summary>
        public float FireInterval => _fireInterval;
        /// <summary>一次换弹从开始到重新进入 Ready 的固定占用秒数 </summary>
        public float ReloadDuration => _reloadDuration;

        /// <summary>启动模拟前校验配置，避免无效时长形成零 Tick 状态或每 Tick 连发 </summary>
        public void Validate()
        {
            if (_hitReactionDuration <= 0f)
                throw new InvalidOperationException("受击持续时间必须大于 0 ");
            if (_fireInterval <= 0f)
                throw new InvalidOperationException("射击间隔必须大于 0 ");
            if (_reloadDuration <= 0f)
                throw new InvalidOperationException("换弹时间必须大于 0 ");
        }

        /// <summary>按当前固定步长计算受击需要占用的 Tick 数 </summary>
        public ushort ResolveHitTicks(float tickDeltaTime) =>
            SecondsToTicks(_hitReactionDuration, tickDeltaTime);

        /// <summary>按当前固定步长计算两次射击事件之间的 Tick 数 </summary>
        public ushort ResolveFireIntervalTicks(float tickDeltaTime) =>
            SecondsToTicks(_fireInterval, tickDeltaTime);

        /// <summary>按当前固定步长计算一次换弹需要占用的 Tick 数 </summary>
        public ushort ResolveReloadTicks(float tickDeltaTime) =>
            SecondsToTicks(_reloadDuration, tickDeltaTime);

        /// <summary>
        /// 把秒数转换为可序列化的 ushort Tick 数 
        /// 使用 Ceil 而不是 Round/Floor，避免量化后状态提前结束；至少返回 1，避免同一 Tick 重复进入和退出 
        /// 上限使用 ushort.MaxValue，因此配置极长时间时会被截断，而不会发生整数回绕 
        /// tickDeltaTime 必须来自统一的 PlayerSimulationClock，不能传入渲染帧 Time.deltaTime 
        /// </summary>
        private static ushort SecondsToTicks(float seconds, float tickDeltaTime)
        {
            if (tickDeltaTime <= 0f)
                throw new ArgumentOutOfRangeException(nameof(tickDeltaTime));

            return (ushort)Mathf.Clamp(
                Mathf.CeilToInt(seconds / tickDeltaTime),
                1,
                ushort.MaxValue);
        }
    }
}
