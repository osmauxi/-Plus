using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace ProjectGame.HotFix.UI.Gameplay
{
    /// <summary>
    /// Manager 独占一个输入租约；UI 模块和按键读取使用 InputManager 的运行时克隆。
    /// </summary>
    internal sealed class GameplayUIInputBridge : IDisposable
    {
        private readonly InputSystemUIInputModule _module;
        private readonly EventSystem _eventSystem;
        private readonly List<InputActionReference> _references = new();
        private InputManager _input;
        private InputActionAsset _asset;
        private IDisposable _lease;
        /// <summary>
        /// 当前输入系统是否具备切换/使用UI导航的条件。
        /// </summary>
        public bool CanNavigate => _input != null && _input.IsInitialized &&
            _input.BaseContext == InputContext.Gameplay && _input.CurrentContext != InputContext.Disabled;

        public GameplayUIInputBridge(InputSystemUIInputModule module)
        {
            _module = module;
            _eventSystem = module == null ? null : module.GetComponent<EventSystem>();
            SetModuleActive(false);
        }
        /// <summary>
        /// 把当前InputManager的运行时InputActionAsset接到InputSystemUIInputModule上。
        /// </summary>
        public void Bind(InputManager input)
        {
            InputActionAsset asset = input != null && input.IsInitialized ? input.RuntimeInputActions : null;
            if (ReferenceEquals(_input, input) && ReferenceEquals(_asset, asset)) 
                return;
            DisposeBinding();
            _input = input;
            _asset = asset;
            if (_module == null || _asset == null) return;
            _module.enabled = false;
            _module.UnassignActions();
            _module.actionsAsset = _asset;
            _module.point = Reference("UI/Point");
            _module.leftClick = Reference("UI/Click");
            _module.scrollWheel = Reference("UI/ScrollWheel");
            _module.move = Reference("UI/Navigate");
            _module.submit = Reference("UI/Submit");
            // 返回全部经由 Presenter -> Manager 路由，不然会出现一个ESC触发两次返回关闭两个窗口
            _module.cancel = null;
        }

        public void SetBlocked(bool blocked)
        {
            if (_input == null || !_input.IsInitialized || _input.BaseContext != InputContext.Gameplay)
                blocked = false;
            if (blocked && _lease == null && CanNavigate)
                _lease = _input.AcquireContext(InputContext.UI, this);
            if (!blocked)
            {
                SetModuleActive(false);
                _lease?.Dispose();
                _lease = null;
            }
            SetModuleActive(blocked && _input != null && _input.CurrentContext == InputContext.UI);
        }

        private void SetModuleActive(bool active)
        {
            // UI 与 Lobby 会短暂共存，专属 EventSystem 仅在本场景交互时启用。
            if (!active && _module != null) _module.enabled = false;
            if (_eventSystem != null) _eventSystem.enabled = active;
            if (active && _module != null) _module.enabled = true;
        }

        private InputActionReference Reference(string path)
        {
            InputAction action = _asset.FindAction(path, false);
            if (action == null) throw new InvalidOperationException($"Gameplay UI 缺少 InputAction：{path}");
            InputActionReference reference = InputActionReference.Create(action);
            _references.Add(reference);
            return reference;
        }

        private void DisposeBinding()
        {
            SetModuleActive(false);
            if (_module != null)
            {
                _module.enabled = false;
                _module.UnassignActions();
                _module.actionsAsset = null;
            }
            _lease?.Dispose();
            _lease = null;
            if (_input != null) _input.ReleaseContexts(this);
            foreach (InputActionReference reference in _references)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(reference);
                else UnityEngine.Object.DestroyImmediate(reference);
            }
            _references.Clear();
            _asset = null;
            _input = null;
        }

        public void Dispose() => DisposeBinding();
    }
}
