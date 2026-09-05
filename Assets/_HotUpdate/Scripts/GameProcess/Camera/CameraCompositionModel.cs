using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    /// <summary>
    /// 摄像机构图偏移的纯计算模块，只接收世界空间事实并计算最终构图偏移 
    /// </summary>
    public sealed class CameraCompositionModel
    {
        #region Configuration

        //Aim LookAhead参数，表达“玩家想看哪里” 
        private float _maxAimOffset;
        private float _aimDeadZone;
        private float _fullAimDistance;
        private float _aimSmoothTime;
        private float _aimReturnSmoothTime;

        //Movement LookAhead参数，表达“玩家正在往哪里移动” 
        private float _maxMovementOffset;
        private float _movementDeadZoneSpeed;
        private float _fullMovementSpeed;
        private float _movementSmoothTime;
        private float _movementReturnSmoothTime;

        //Aim时通常弱化甚至完全关闭 Movement LookAhead，
        //防止“移动方向”和“瞄准方向”同时争夺构图中心 
        private float _aimMovementWeight;

        //视觉速度先经过一次轻微平滑，再用于求加速度。
        //否则Render Pose的微小帧间抖动在除以deltaTime后会被放大成很大的假加速度。
        private float _velocitySmoothTime;

        //Acceleration LookAhead参数。
        private float _maxAccelerationOffset;
        private float _accelerationDeadZone;
        private float _fullAcceleration;
        private float _accelerationSmoothTime;
        private float _accelerationReturnSmoothTime;

        //Movement Offset改变方向时围绕玩家旋转的最大角速度。
        private float _movementTurnSpeed;
        #endregion

        #region Runtime State

        private bool _aimActive;
        //同Motion的三元组设计，拆分了运动和Aim两个offset状态
        private Vector3 _targetAimOffset;
        private Vector3 _currentAimOffset;
        private Vector3 _aimOffsetVelocity;

        //Movement拆成方向 + 长度。
        //长度负责SmoothDamp，方向负责围绕玩家Pivot旋转
        private Vector3 _targetMovementDirection;
        private Vector3 _currentMovementDirection;

        private float _targetMovementMagnitude;
        private float _currentMovementMagnitude;
        private float _movementMagnitudeVelocity;

        //Acceleration是短时间额外叠加的构图冲击
        private Vector3 _targetAccelerationOffset;
        private Vector3 _currentAccelerationOffset;
        private Vector3 _accelerationOffsetVelocity;

        private Vector3 _lastTrackedPosition;
        private bool _hasMovementSample;

        //速度必须先做轻微平滑，再求差分得到加速度。
        private Vector3 _smoothedVelocity;
        private Vector3 _velocitySmoothVelocity;
        private Vector3 _lastSmoothedVelocity;

        #endregion

        #region Public State

        /// <summary>当前是否启用了瞄准前视构图 </summary>
        public bool AimActive => _aimActive;

        /// <summary>
        /// 最终构图偏移由 Aim、Movement 和 Acceleration 三条通道共同组成。
        /// Movement的方向本身已经经过Turn Pivot响应。
        /// </summary>
        public Vector3 CurrentOffset =>
            _currentAimOffset +
            _currentMovementDirection * _currentMovementMagnitude +
            _currentAccelerationOffset;

        #endregion

        #region Commands and Simulation

        /// <summary>初始化全部构图参数并清除上一轮运行状态</summary>
        public void Reset(
            float maxAimOffset,
            float aimDeadZone,
            float fullAimDistance,
            float aimSmoothTime,
            float aimReturnSmoothTime,
            float maxMovementOffset,
            float movementDeadZoneSpeed,
            float fullMovementSpeed,
            float movementSmoothTime,
            float movementReturnSmoothTime,
            float velocitySmoothTime,
            float maxAccelerationOffset,
            float accelerationDeadZone,
            float fullAcceleration,
            float accelerationSmoothTime,
            float accelerationReturnSmoothTime,
            float movementTurnSpeed,
            float aimMovementWeight)
        {
            _maxAimOffset = Mathf.Max(0f, maxAimOffset);
            _aimDeadZone = Mathf.Max(0f, aimDeadZone);
            _fullAimDistance = Mathf.Max(_aimDeadZone + 0.01f, fullAimDistance);
            _aimSmoothTime = Mathf.Max(0f, aimSmoothTime);
            _aimReturnSmoothTime = Mathf.Max(0f, aimReturnSmoothTime);

            _maxMovementOffset = Mathf.Max(0f, maxMovementOffset);
            _movementDeadZoneSpeed = Mathf.Max(0f, movementDeadZoneSpeed);
            _fullMovementSpeed = Mathf.Max(_movementDeadZoneSpeed + 0.01f, fullMovementSpeed);
            _movementSmoothTime = Mathf.Max(0f, movementSmoothTime);
            _movementReturnSmoothTime = Mathf.Max(0f, movementReturnSmoothTime);

            _velocitySmoothTime = Mathf.Max(0f, velocitySmoothTime);

            _maxAccelerationOffset = Mathf.Max(0f, maxAccelerationOffset);
            _accelerationDeadZone = Mathf.Max(0f, accelerationDeadZone);
            _fullAcceleration = Mathf.Max(_accelerationDeadZone + 0.01f, fullAcceleration);
            _accelerationSmoothTime = Mathf.Max(0f, accelerationSmoothTime);
            _accelerationReturnSmoothTime = Mathf.Max(0f, accelerationReturnSmoothTime);

            _movementTurnSpeed = Mathf.Max(0f, movementTurnSpeed);
            _aimMovementWeight = Mathf.Clamp01(aimMovementWeight);

            _aimActive = false;

            _targetAimOffset = Vector3.zero;
            _currentAimOffset = Vector3.zero;
            _aimOffsetVelocity = Vector3.zero;

            _targetMovementDirection = Vector3.zero;
            _currentMovementDirection = Vector3.zero;

            _targetMovementMagnitude = 0f;
            _currentMovementMagnitude = 0f;
            _movementMagnitudeVelocity = 0f;

            _targetAccelerationOffset = Vector3.zero;
            _currentAccelerationOffset = Vector3.zero;
            _accelerationOffsetVelocity = Vector3.zero;

            ResetMovementTracking();
        }

        /// <summary>
        /// 开启或关闭Aim构图 
        /// </summary>
        public void SetAimActive(bool active)
        {
            _aimActive = active;

            if (!active)
                _targetAimOffset = Vector3.zero;
        }

        /// <summary>
        /// 根据玩家位置和瞄准世界点计算Aim LookAhead 
        /// 当前Gameplay忽略Y轴
        /// </summary>
        public void UpdateAimTarget(Vector3 origin, Vector3 aimWorldPosition)
        {
            if (!_aimActive)
                return;

            Vector3 direction = aimWorldPosition - origin;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance <= _aimDeadZone)
            {
                _targetAimOffset = Vector3.zero;
                return;
            }
            //InverseLerp求distance在_aimDeadZone与_fullAimDistance之间的0-1进度
            //这里算出的strength是线性关系，为了轻微离开deadZone时不造成大幅度的相机偏移
            //我们额外采用SmoothStep将线性关系映射为S形
            float strength = Mathf.InverseLerp(_aimDeadZone, _fullAimDistance, distance);

            //SmoothStep让偏移从DeadZone边缘开始时更加柔和
            strength = SmoothStep01(strength);

            _targetAimOffset = direction.normalized * (_maxAimOffset * strength);
        }

        /// <summary>
        /// 根据Target相邻Render Pose推导视觉移动速度 
        /// </summary>
        public void UpdateMovementTarget(Vector3 trackedPosition, float deltaTime)
        {
            //第一帧缺少参照，算不了
            if (!_hasMovementSample)
            {
                _lastTrackedPosition = trackedPosition;
                _hasMovementSample = true;
                return;
            }

            if (deltaTime <= 0f)
            {
                _lastTrackedPosition = trackedPosition;
                return;
            }
            //根据上个渲染帧位置跟当前渲染帧位置判定速度，以此参照
            Vector3 displacement = trackedPosition - _lastTrackedPosition;
            _lastTrackedPosition = trackedPosition;

            displacement.y = 0f;

            Vector3 rawVelocity = displacement / deltaTime;

            //渲染状态本身存在插值和帧间微小波动。
            //先平滑速度再求加速度，可以显著减少Camera高频抖动。
            _smoothedVelocity = Vector3.SmoothDamp(
                         _smoothedVelocity,
                         rawVelocity,
                         ref _velocitySmoothVelocity,
                         _velocitySmoothTime,
                         Mathf.Infinity,
                         deltaTime);

            Vector3 acceleration =(_smoothedVelocity - _lastSmoothedVelocity) / deltaTime;

            acceleration.y = 0f;
            _lastSmoothedVelocity = _smoothedVelocity;

            UpdateMovementLookAhead(_smoothedVelocity);
            UpdateAccelerationLookAhead(acceleration);
        }

        /// <summary>推进Aim与Movement两条独立构图通道 </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            UpdateAim(deltaTime);
            UpdateMovement(deltaTime);
            UpdateAcceleration(deltaTime);
        }

        /// <summary>
        /// Target更换或丢失时清除Movement的采样历史 
        /// 否则新旧Target之间的位置差会被错误解释成高速移动 
        /// </summary>
        public void ResetMovementTracking()
        {
            _lastTrackedPosition = default;
            _hasMovementSample = false;

            _smoothedVelocity = Vector3.zero;
            _velocitySmoothVelocity = Vector3.zero;
            _lastSmoothedVelocity = Vector3.zero;

            _targetMovementDirection = Vector3.zero;
            _currentMovementDirection = Vector3.zero;

            _targetMovementMagnitude = 0f;
            _currentMovementMagnitude = 0f;
            _movementMagnitudeVelocity = 0f;

            _targetAccelerationOffset = Vector3.zero;
            _currentAccelerationOffset = Vector3.zero;
            _accelerationOffsetVelocity = Vector3.zero;
        }

        /// <summary>
        /// 传送或强制Snap时重新建立Movement基准 
        /// Teleport本身不能被解释成Movement LookAhead 
        /// </summary>
        public void Snap(Vector3 trackedPosition)
        {
            _currentAimOffset = _targetAimOffset;
            _aimOffsetVelocity = Vector3.zero;

            ResetMovementTracking();

            _lastTrackedPosition = trackedPosition;
            _hasMovementSample = true;
        }
        #endregion

        #region Aim

        private void UpdateAim(float deltaTime)
        {
            float smoothTime =_aimActive ? _aimSmoothTime : _aimReturnSmoothTime;

            _currentAimOffset = Vector3.SmoothDamp(
                _currentAimOffset,
                _targetAimOffset,
                ref _aimOffsetVelocity,
                smoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        #endregion

        #region Movement

        private void UpdateMovementLookAhead(Vector3 velocity)
        {
            float speed = velocity.magnitude;

            if (speed <= _movementDeadZoneSpeed)
            {
                _targetMovementMagnitude = 0f;
                return;
            }

            float strength = Mathf.InverseLerp(
                _movementDeadZoneSpeed,
                _fullMovementSpeed,
                speed);

            strength = SmoothStep01(strength);

            float weight = _aimActive? _aimMovementWeight : 1f;

            _targetMovementDirection = velocity.normalized;
            _targetMovementMagnitude = _maxMovementOffset * strength * weight;
        }

        private void UpdateMovement(float deltaTime)
        {
            float smoothTime = _targetMovementMagnitude > 0.0001f
                    ? _movementSmoothTime
                    : _movementReturnSmoothTime;

            _currentMovementMagnitude = Mathf.SmoothDamp(
                _currentMovementMagnitude,
                _targetMovementMagnitude,
                ref _movementMagnitudeVelocity,
                smoothTime,
                Mathf.Infinity,
                deltaTime);

            if (_targetMovementDirection.sqrMagnitude <= 0.0001f)
                return;

            //第一次建立有效方向时直接采用目标方向，
            //后续转向才进入 Pivot 旋转。
            if(_currentMovementDirection.sqrMagnitude <= 0.0001f)
            {
                _currentMovementDirection = _targetMovementDirection;
                return;
            }

            //与直接SmoothDamp Vector不同，
            //RotateTowards会让已有Offset围绕玩家发生旋转，因此大角度转向时不会从构图中心直线穿过去
            float maxRadians = _movementTurnSpeed * Mathf.Deg2Rad * deltaTime;

            _currentMovementDirection = Vector3.RotateTowards(
                                            _currentMovementDirection,
                                            _targetMovementDirection,
                                            maxRadians,
                                            0f);

            _currentMovementDirection.y = 0f;

            if (_currentMovementDirection.sqrMagnitude > 0.0001f)
                _currentMovementDirection.Normalize();
        }

        #endregion

        #region Acceleration

        private void UpdateAccelerationLookAhead(Vector3 acceleration)
        {
            float magnitude = acceleration.magnitude;

            if (magnitude <= _accelerationDeadZone)
            {
                _targetAccelerationOffset = Vector3.zero;
                return;
            }

            float strength = Mathf.InverseLerp(
                _accelerationDeadZone,
                _fullAcceleration,
                magnitude);

            strength = SmoothStep01(strength);

            float weight = _aimActive ? _aimMovementWeight : 1f;

            _targetAccelerationOffset = acceleration.normalized * (_maxAccelerationOffset * strength * weight);
        }

        private void UpdateAcceleration(float deltaTime)
        {
            float smoothTime = _targetAccelerationOffset.sqrMagnitude > 0.0001f ? _accelerationSmoothTime : _accelerationReturnSmoothTime;

            _currentAccelerationOffset = Vector3.SmoothDamp(
                _currentAccelerationOffset,
                _targetAccelerationOffset,
                ref _accelerationOffsetVelocity,
                smoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        #endregion


        #region 计算方法
        /// <summary>
        /// 平滑阶梯插值，将一个线性的进度，转换成一个具有缓入缓出效果的S型曲线进度
        /// </summary>
        private static float SmoothStep01(float value)
        {
            value = Mathf.Clamp01(value);
            //函数在0-1表现为起步慢，中间快，收尾慢，类似生物k/2那个图
            return value * value * (3f - 2f * value);
        }

        #endregion
    }
}
