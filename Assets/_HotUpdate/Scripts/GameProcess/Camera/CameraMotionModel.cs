using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    /// <summary>
    /// 摄像机自身运动状态的纯计算模块 
    /// 不持有Transform / Cinemachine / 输入系统，只接收外部指令并计算最终Yaw、ViewHeight 与 FOV 
    /// </summary>
    public sealed class CameraMotionModel
    {
		#region Configuration and Persistent Effects
		//会出现多种Modifier状态同时作用，所以使用字典而不是单个值
		//与下面三层状态设计不同，这里处理的是多个同生命周期Effect互相覆盖的问题
		private readonly Dictionary<CameraEffectId, float> _zoomModifiers = new();
        private readonly Dictionary<CameraEffectId, float> _fovModifiers = new();

        private float _rotationSmoothTime;

        private float _minViewHeight;
        private float _maxViewHeight;
        private float _baseZoomSmoothTime;

        private float _minFov;
        private float _maxFov;
        private float _baseFovSmoothTime;

		#endregion

		#region Runtime State
		//SmoothDamp基础的平滑状态三元组
		//Target表示"想去哪里"
		//Current表示"当前在哪里"
		//Velocity表示"当前状态下我的速度"，表示上一帧的运动趋势
		//Velocity本身是通过Mathf.SmoothDamp进行修改的
		//SmoothDamp为了让整个平滑过渡的过程显得自然（有加速、匀速、减速的过程），系统必须记住上一帧它的速度有多快。
		private float _targetYaw;
        private float _currentYaw;
        private float _yawVelocity;

        //Zoom，FOV使用三层计算模式，最终值根据 Base/Modifier/Kick 三个值来判定，他们的方法逻辑基本相同。
        //主要是应对不同Effect对原始相机状态的修改后的恢复问题，将他们彻底拆开，
        //从根本上阻止不同生命周期的临时表现状态污染基础状态导致的数据混乱问题
        //以Zoom变量为例：
        //Zoom Base 只代表玩家自己的基础观察距离
        private float _targetViewHeight;
        private float _currentViewHeight;
        private float _heightVelocity;
        //Zoom Modifier 是持久性的相机状态偏移，比如Aim
        private float _effectZoomOffset;
        private float _currentZoomSmoothTime;
        //Zoom Kick 是一次性距离冲击，如开枪的震动效果
        private float _zoomKickOffset;
        private float _targetZoomKickOffset;
        private float _zoomKickVelocity;
        private float _zoomKickSmoothTime;
        private float _zoomKickReleaseSmoothTime;
        private float _zoomKickHoldRemaining;

		//FOV同样分为 Base/Modifier/Kick 三层，避免Aim与射击冲击互相覆盖 
		private float _baseFov;
        private float _currentFov;
        private float _fovVelocity;

        private float _effectFovOffset;
        private float _currentFovSmoothTime;

        private float _fovKickOffset;
        private float _targetFovKickOffset;
        private float _fovKickVelocity;
        private float _fovKickSmoothTime;
        private float _fovKickReleaseSmoothTime;
        private float _fovKickHoldRemaining;

        #endregion

        #region Public State

        /// <summary>平滑后的当前世界 Yaw 角 </summary>
        public float CurrentYaw => _currentYaw;

        /// <summary>包含基础缩放、持续修饰器和瞬时冲击的当前观察高度 </summary>
        public float CurrentViewHeight => _currentViewHeight;

        /// <summary>包含基础 FOV、持续修饰器和瞬时冲击的当前视野角 </summary>
        public float CurrentFov => _currentFov;

        /// <summary>输入命令累计后的目标世界 Yaw 角 </summary>
        public float TargetYaw => _targetYaw;

        /// <summary>只包含玩家缩放输入的目标观察高度 </summary>
        public float TargetBaseViewHeight => _targetViewHeight;

        /// <summary>不包含任何效果层的基础视野角 </summary>
        public float BaseFov => _baseFov;

        #endregion

        #region Commands and Simulation

        /// <summary>初始化或调试刷新时，重新建立一套确定的镜头运动状态 </summary>
        public void Reset(
            float initialYaw,
            float viewHeight,
            float rotationSmoothTime,
            float minViewHeight,
            float maxViewHeight,
            float zoomSmoothTime,
            float baseFov,
            float minFov,
            float maxFov,
            float fovSmoothTime,
            bool clearPersistentModifiers = true)
        {
            _rotationSmoothTime = Mathf.Max(0f, rotationSmoothTime);

            _minViewHeight = Mathf.Max(0f, minViewHeight);
            _maxViewHeight = Mathf.Max(_minViewHeight, maxViewHeight);
            _baseZoomSmoothTime = Mathf.Max(0f, zoomSmoothTime);

            _minFov = Mathf.Max(1f, minFov);
            _maxFov = Mathf.Max(_minFov, maxFov);
            _baseFovSmoothTime = Mathf.Max(0f, fovSmoothTime);

            _targetYaw = initialYaw;
            _currentYaw = initialYaw;
            _yawVelocity = 0f;

            _targetViewHeight = Mathf.Clamp(viewHeight, _minViewHeight, _maxViewHeight);
            _currentViewHeight = _targetViewHeight;
            _heightVelocity = 0f;

            if (clearPersistentModifiers)
                _zoomModifiers.Clear();

            RecalculateZoomModifier();
            _currentZoomSmoothTime = _baseZoomSmoothTime;

            _zoomKickOffset = 0f;
            _targetZoomKickOffset = 0f;
            _zoomKickVelocity = 0f;
            _zoomKickSmoothTime = _baseZoomSmoothTime;
            _zoomKickReleaseSmoothTime = _baseZoomSmoothTime;
            _zoomKickHoldRemaining = 0f;

            _baseFov = Mathf.Clamp(baseFov, _minFov, _maxFov);
            _currentFov = _baseFov;
            _fovVelocity = 0f;

            if (clearPersistentModifiers)
                _fovModifiers.Clear();

            RecalculateFovModifier();
            _currentFovSmoothTime = _baseFovSmoothTime;

            _fovKickOffset = 0f;
            _targetFovKickOffset = 0f;
            _fovKickVelocity = 0f;
            _fovKickSmoothTime = _baseFovSmoothTime;
            _fovKickReleaseSmoothTime = _baseFovSmoothTime;
            _fovKickHoldRemaining = 0f;
        }

        /// <summary>输入的是最终角度增量，Motion 不关心这个值来自 Q/E 还是其他输入 </summary>
        public void AddYaw(float degrees)
        {
            _targetYaw += degrees;
        }

        /// <summary>修改玩家基础观察距离，不影响 Aim、爆炸等额外镜头效果 </summary>
        public void AddBaseZoom(float delta)
        {
            _targetViewHeight = Mathf.Clamp(_targetViewHeight + delta, _minViewHeight, _maxViewHeight);
            _currentZoomSmoothTime = _baseZoomSmoothTime;
        }

        /// <summary>设置一个持续 Zoom Modifier，不同 EffectId 可以同时叠加 </summary>
        public void SetZoomModifier(CameraEffectId id, float offset, float smoothTime)
        {
            if (id == CameraEffectId.None)
                return;
            //为0说明此状态无效
            if (Mathf.Approximately(offset, 0f))
                _zoomModifiers.Remove(id);
            else
                _zoomModifiers[id] = offset;

            RecalculateZoomModifier();

			//即使Modifier被移除，也保留退出效果自己的过渡时间 
			//这里也隐藏一个状态切换，RecalculateZoomModifier重置_effectZoomOffset后，
			//接下来的Tick会使用被删除状态的ZoomSmoothTime进行平滑，也就使用了具体移除状态的数据进行平滑
			//在多个Effect连续触发时，offset可以叠加，但是只会取最后一次的SmoothTime做平滑，是当前系统的取舍
			_currentZoomSmoothTime = Mathf.Max(0f, smoothTime);
        }

		/// <summary>
		/// 添加一次瞬时Zoom Kick，连续射击时可以累积
		/// holdTime控制当前使用谁的Smooth参数，前两个float为具体使用的Smooth参数
		/// </summary>
		/// <param name="offset"></param> 偏移量
		/// <param name="attackSmoothTime"></param> 冲击建立的有多快
		/// <param name="releaseSmoothTime"></param> 冲击消失的有多快
		/// <param name="holdTime"></param> 所有冲击什么时候切到Release状态(他能维持多久)
        //Kick本身只取最新值，旧Kick的参数会在新Kick到时被覆盖，这里只累加了Offset
		//比如Remaining这里直接取Max,会延长旧Kick的生命周期，这是当前架构的取舍
		public void PlayZoomKick(float offset, float attackSmoothTime, float releaseSmoothTime, float holdTime = 0.03f)
        {
            //这里没有直接覆盖，而是+=，允许了Kick Effect的相互叠加
            _targetZoomKickOffset += offset;
            _zoomKickSmoothTime = Mathf.Max(0f, attackSmoothTime);
            _zoomKickReleaseSmoothTime = Mathf.Max(0f, releaseSmoothTime);
            
            _zoomKickHoldRemaining = Mathf.Max(_zoomKickHoldRemaining, holdTime);
        }

        /// <summary>设置持续 FOV Modifier，例如 Aim 时持续缩小视野 </summary>
        public void SetFovModifier(CameraEffectId id, float offset, float smoothTime)
        {
            if (id == CameraEffectId.None)
                return;

            if (Mathf.Approximately(offset, 0f))
                _fovModifiers.Remove(id);
            else
                _fovModifiers[id] = offset;

            RecalculateFovModifier();

            // AimExit 等效果可以拥有独立于进入过程的恢复速度 
            _currentFovSmoothTime = Mathf.Max(0f, smoothTime);
        }

        /// <summary>
        /// 添加一次瞬时 FOV Kick 
        /// 正值通常产生视野突然扩张的冲击感，负值则产生瞬间收紧效果 
        /// </summary>
        public void PlayFovKick(float offset, float attackSmoothTime, float releaseSmoothTime, float holdTime = 0.03f)
        {
            _targetFovKickOffset += offset;
            _fovKickSmoothTime = Mathf.Max(0f, attackSmoothTime);
            _fovKickReleaseSmoothTime = Mathf.Max(0f, releaseSmoothTime);
            _fovKickHoldRemaining = Mathf.Max(_fovKickHoldRemaining, holdTime);
        }

        /// <summary>推进纯计算状态，由 Controller 每帧调用 </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;
            //更新Yaw
            _currentYaw = Mathf.SmoothDampAngle(
                _currentYaw,
                _targetYaw,
                ref _yawVelocity,
                _rotationSmoothTime,
                Mathf.Infinity,
                deltaTime);

			//更新瞬时Kick状态，这里先平滑了一次_zoomKickOffset，这里是第一次平滑
			UpdateZoomKick(deltaTime);
            UpdateFovKick(deltaTime);

			//三层数值求和得出targetViewHeight
			float targetViewHeight = ResolveFinalViewHeight();
			//Current Zoom向最终Target平滑，这里是第二次平滑
			_currentViewHeight = Mathf.SmoothDamp(
                _currentViewHeight,
                targetViewHeight,
                ref _heightVelocity,
                _currentZoomSmoothTime,
                Mathf.Infinity,
                deltaTime);

			//求最终 FOV Target
			float targetFov = ResolveFinalFov();
			//Current FOV向最终Target平滑
			_currentFov = Mathf.SmoothDamp(
                _currentFov,
                targetFov,
                ref _fovVelocity,
                _currentFovSmoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        /// <summary>传送或强制 Snap 时立即对齐当前运动目标，并清除平滑历史 </summary>
        public void Snap()
        {
            _currentYaw = _targetYaw;
            _currentViewHeight = ResolveFinalViewHeight();
            _currentFov = ResolveFinalFov();

            _yawVelocity = 0f;
            _heightVelocity = 0f;
            _fovVelocity = 0f;

            _zoomKickVelocity = 0f;
            _fovKickVelocity = 0f;
        }

        #endregion

        #region Calculation Helpers
        private void UpdateZoomKick(float deltaTime)
        {
			//Remaining是HoldTime，管理状态具体的持续时间
            //Remaining跑完后，之后用ReleaseSmoothTime往0做平滑(Kick失效后的回正)
			if (_zoomKickHoldRemaining > 0f)
            {
				//KickSmoothTime初始存的是attackSmoothTime，以此做平滑
				_zoomKickHoldRemaining -= deltaTime;

                if (_zoomKickHoldRemaining <= 0f)
                {
					//Remaining跑完后开始Release阶段，用ReleaseSmoothTime更新_zoomKickSmoothTime
					//类似隐式状态机，由HoldRemaining、TargetKickOffset和当前SmoothTime共同隐式表达Kick所处阶段。
					_targetZoomKickOffset = 0f;
                    _zoomKickSmoothTime = _zoomKickReleaseSmoothTime;
                }
            }

            _zoomKickOffset = Mathf.SmoothDamp(
                _zoomKickOffset,
                _targetZoomKickOffset,
                ref _zoomKickVelocity,
                _zoomKickSmoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        private void UpdateFovKick(float deltaTime)
        {
            if (_fovKickHoldRemaining > 0f)
            {
                _fovKickHoldRemaining -= deltaTime;

                if (_fovKickHoldRemaining <= 0f)
                {
                    _targetFovKickOffset = 0f;
                    _fovKickSmoothTime = _fovKickReleaseSmoothTime;
                }
            }

            _fovKickOffset = Mathf.SmoothDamp(
                _fovKickOffset,
                _targetFovKickOffset,
                ref _fovKickVelocity,
                _fovKickSmoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        private void RecalculateZoomModifier()
        {
            _effectZoomOffset = 0f;

            foreach (float offset in _zoomModifiers.Values)
                _effectZoomOffset += offset;
        }

        private void RecalculateFovModifier()
        {
            _effectFovOffset = 0f;

            foreach (float offset in _fovModifiers.Values)
                _effectFovOffset += offset;
        }

        private float ResolveFinalViewHeight()
        {
            return Mathf.Clamp(
                _targetViewHeight + _effectZoomOffset + _zoomKickOffset,
                _minViewHeight,
                _maxViewHeight);
        }

        private float ResolveFinalFov()
        {
            return Mathf.Clamp(
                _baseFov + _effectFovOffset + _fovKickOffset,
                _minFov,
                _maxFov);
        }

        #endregion
    }
}
