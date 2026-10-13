using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 缓存一个本地地图视觉对象下的渲染组件。
    /// 只切换渲染，不停用 GameObject、碰撞体、Portal 或运行时脚本。
    /// </summary>
    internal sealed class LocalVisualSet
    {
        private readonly Renderer[] _renderers;
        private readonly bool[] _rendererDefaults;
        private readonly Light[] _lights;
        private readonly bool[] _lightDefaults;
        private bool _isVisible = true;

        public bool IsVisible => _isVisible;

        public LocalVisualSet(Component root)
        {
            if (root == null)
            {
                _renderers = System.Array.Empty<Renderer>();
                _rendererDefaults = System.Array.Empty<bool>();
                _lights = System.Array.Empty<Light>();
                _lightDefaults = System.Array.Empty<bool>();
                return;
            }

            _renderers = root.GetComponentsInChildren<Renderer>(true);
            _rendererDefaults = new bool[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _rendererDefaults[i] = _renderers[i] != null && _renderers[i].enabled;

            _lights = root.GetComponentsInChildren<Light>(true);
            _lightDefaults = new bool[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
                _lightDefaults[i] = _lights[i] != null && _lights[i].enabled;
        }

        public void SetVisible(bool visible)
        {
            if (_isVisible == visible)
                return;

            _isVisible = visible;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = visible && _rendererDefaults[i];
            }

            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i] != null)
                    _lights[i].enabled = visible && _lightDefaults[i];
            }
        }
    }
}
