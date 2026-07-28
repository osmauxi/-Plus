using Cinemachine;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>在 Cinemachine 最终输出阶段叠加由鼠标位置驱动的小幅镜头旋转。</summary>
    [AddComponentMenu("ProjectGame/Lobby/Camera Mouse Sway Extension")]
    [DisallowMultipleComponent]
    public sealed class LobbyCameraMouseSwayExtension : CinemachineExtension
    {
        [Header("旋转幅度")]
        [SerializeField, Range(0f, 10f)] private float _maxYaw = 2f;
        [SerializeField, Range(0f, 10f)] private float _maxPitch = 1.25f;

        [Header("跟随手感")]
        [SerializeField, Min(0.01f)] private float _smoothTime = 0.2f;
        [SerializeField, Range(0f, 0.9f)] private float _deadZone = 0.05f;
        [SerializeField] private bool _invertY;
        [SerializeField] private bool _recenterWhenMouseLeaves = true;

        private Vector2 _smoothedInput;
        private Vector2 _smoothVelocity;
        private Vector2 _lastTargetInput;

        /// <summary>在 Cinemachine 完成运镜后叠加鼠标旋转，不修改虚拟相机 Transform。</summary>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize || !Application.isPlaying)
                return;

            Vector2 targetInput = ReadMouseInput();
            if (deltaTime < 0f)
            {
                _smoothedInput = targetInput;
                _smoothVelocity = Vector2.zero;
            }
            else
            {
                _smoothedInput = Vector2.SmoothDamp(
                    _smoothedInput,
                    targetInput,
                    ref _smoothVelocity,
                    _smoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);
            }

            float pitchDirection = _invertY ? 1f : -1f;
            Quaternion sway = Quaternion.Euler(
                _smoothedInput.y * _maxPitch * pitchDirection,
                _smoothedInput.x * _maxYaw,
                0f);
            state.OrientationCorrection *= sway;
        }

        /// <summary>读取鼠标屏幕位置并转换为带中心死区的 -1 到 1 输入。</summary>
        private Vector2 ReadMouseInput()
        {
            Vector3 mousePosition = Input.mousePosition;
            bool mouseInsideWindow = Application.isFocused
                && mousePosition.x >= 0f
                && mousePosition.x <= Screen.width
                && mousePosition.y >= 0f
                && mousePosition.y <= Screen.height;

            if (!mouseInsideWindow)
                return _recenterWhenMouseLeaves ? Vector2.zero : _lastTargetInput;

            Vector2 normalizedInput = new Vector2(
                mousePosition.x / Screen.width * 2f - 1f,
                mousePosition.y / Screen.height * 2f - 1f);
            _lastTargetInput = new Vector2(
                ApplyDeadZone(normalizedInput.x),
                ApplyDeadZone(normalizedInput.y));
            return _lastTargetInput;
        }

        /// <summary>移除中心死区，并把剩余输入重新映射到完整范围。</summary>
        private float ApplyDeadZone(float value)
        {
            float absoluteValue = Mathf.Abs(value);
            if (absoluteValue <= _deadZone)
                return 0f;

            float remappedValue = (absoluteValue - _deadZone) / (1f - _deadZone);
            return Mathf.Sign(value) * Mathf.Clamp01(remappedValue);
        }
    }
}
