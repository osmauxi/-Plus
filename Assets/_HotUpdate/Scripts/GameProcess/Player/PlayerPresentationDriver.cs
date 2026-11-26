using ProjectGame.HotFix.Gameplay.Pooling;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 把低频 Simulation Root 转换为逐渲染帧的 Presentation Root 
    ///
    /// Simulation Root 仍由 CharacterController、预测、回滚和权威快照独占；
    /// 本组件只移动承载模型与武器挂点的 VisualRoot Camera 也跟随同一个
    /// Presentation Root，因此模型与镜头始终消费同一时间线 
    /// </summary>
    [DefaultExecutionOrder(-300)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    [RequireComponent(typeof(PlayerAppearanceController))]
    public sealed class PlayerPresentationDriver : MonoBehaviour, IPoolable
    {
        private const float PositionEpsilonSqr = 0.00000001f;
        private const float RotationEpsilon = 0.001f;

        [Header("渲染位姿")]
        [Tooltip("相邻 Simulation 样本超过该距离时视为传送，Presentation Root 立即对齐 ")]
        [SerializeField, Min(0.1f)] private float _snapDistance = 2f;
        [Tooltip("相邻 Simulation 样本超过该角度时视为朝向瞬移，不再补帧 ")]
        [SerializeField, Range(1f, 180f)] private float _snapRotationAngle = 90f;

        // 只读取已经由同步层选定的最终 Simulation 时间线，不参与任何网络判定 
        private PlayerSyncController _syncController;
        // 固定存在的 VisualRoot；动态模型和武器挂点都位于它之下 
        private Transform _presentationRoot;

        // 当前补帧段的世界空间起点、终点和已推进时间 
        private Vector3 _fromPosition;
        private Vector3 _toPosition;
        private Quaternion _fromRotation;
        private Quaternion _toRotation;
        private float _elapsed;
        private bool _hasPose;

        /// <summary>模型、武器和本地 Camera 共同使用的逐帧表现节点 </summary>
        public Transform PresentationRoot => _presentationRoot;

        private void Awake()
        {
            _syncController = GetComponent<PlayerSyncController>();
            _presentationRoot = GetComponent<PlayerAppearanceController>().VisualRoot;
            SnapToSimulationPose();
        }

        /// <summary>
        /// 明确位于 PlayerSyncController(-400) 之后、PlayerCameraController(-200) 之前 
        /// Host/Server Authority 和 Owner Prediction 的 Root 只在固定 Tick 变化，需要补帧；
        /// 普通 Remote Observer 已由 PlayerRemoteInterpolation 每渲染帧更新，直接复制即可 
        /// </summary>
        private void LateUpdate()
        {
            if (!_syncController.IsSpawned ||
                (!_syncController.IsServer && !_syncController.IsOwner))
            {
                SnapToSimulationPose();
                return;
            }

            UpdateInterpolatedPose(Time.deltaTime);
        }

        /// <summary>推进本地/Host 表现位姿；只写 VisualRoot，不回写玩家网络根节点 </summary>
        private void UpdateInterpolatedPose(float deltaTime)
        {
            Vector3 simulationPosition = transform.position;
            Quaternion simulationRotation = transform.rotation;
            float duration = _syncController.SimulationTickDeltaTime;

            if (!_hasPose ||
                (simulationPosition - _toPosition).sqrMagnitude >= _snapDistance * _snapDistance ||
                Quaternion.Angle(simulationRotation, _toRotation) >= _snapRotationAngle)
            {
                SnapToSimulationPose();
                return;
            }

            bool positionChanged =
                (simulationPosition - _toPosition).sqrMagnitude > PositionEpsilonSqr;
            bool rotationChanged =
                Quaternion.Angle(simulationRotation, _toRotation) > RotationEpsilon;

            if (positionChanged || rotationChanged)
            {
                // 新 Tick 到达时从“当前已经显示的位置”开始下一段，避免未走完上一段时回跳 
                _fromPosition = EvaluatePosition(duration);
                _fromRotation = EvaluateRotation(duration);
                _toPosition = simulationPosition;
                _toRotation = simulationRotation;
                _elapsed = 0f;
            }

            _elapsed = Mathf.Min(duration, _elapsed + deltaTime);
            ApplyPresentationPose(
                EvaluatePosition(duration),
                EvaluateRotation(duration));
        }

        /// <summary>
        /// 生成、传送、回池或普通 Remote Observer 更新时立即对齐表现节点，清除旧补帧历史 
        /// </summary>
        public void SnapToSimulationPose()
        {
            if (_presentationRoot == null)
                return;

            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            _fromPosition = position;
            _toPosition = position;
            _fromRotation = rotation;
            _toRotation = rotation;
            _elapsed = 0f;
            _hasPose = true;

            ApplyPresentationPose(position, rotation);
        }

        /// <summary>对象池在设置完本次出生 Transform 后调用，因此从新出生点建立表现基线 </summary>
        public void OnRentFromPool()
        {
            SnapToSimulationPose();
        }

        /// <summary>清除上一位 Owner 留下的世界空间补帧状态 </summary>
        public void OnReturnToPool()
        {
            SnapToSimulationPose();
        }

        private Vector3 EvaluatePosition(float duration)
        {
            return Vector3.LerpUnclamped(_fromPosition, _toPosition, Mathf.Clamp01(_elapsed / duration));
        }

        private Quaternion EvaluateRotation(float duration)
        {
            return Quaternion.SlerpUnclamped(_fromRotation, _toRotation, Mathf.Clamp01(_elapsed / duration));
        }

        private void ApplyPresentationPose(Vector3 position, Quaternion rotation)
        {
            _presentationRoot.SetPositionAndRotation(position, rotation);
        }
    }
}
