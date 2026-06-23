using UnityEngine;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.DebugTools
{
    [RequireComponent(typeof(GMPanelView))]
    public class GMPresenter : MonoBehaviour
    {
        [Header("快捷键配置")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;

        private GMPanelView _view;
        private string _currentCategory = string.Empty;

        private void Awake()
        {
            _view = GetComponent<GMPanelView>();
            _view.SetVisible(false);

            // 跨场景留存
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            bool nextState = !_view.IsVisible;
            _view.SetVisible(nextState);

            if (nextState)
            {
                // 每次呼出，全量刷新
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            _view.ClearAll();
            var allData = GMCommandRegistry.GetAllCommands();

            if (allData.Count == 0) return;

            bool isFirst = true;
            foreach (var categoryName in allData.Keys)
            {
                string targetCat = categoryName; // 防闭包陷阱

                // 让 View 生成按钮，并把切换分类的逻辑注入进去
                _view.CreateCategoryButton(targetCat, () => SwitchCategory(targetCat));

                if (isFirst)
                {
                    _currentCategory = targetCat;
                    isFirst = false;
                }
            }

            if (!string.IsNullOrEmpty(_currentCategory))
            {
                SwitchCategory(_currentCategory);
            }
        }

        private void SwitchCategory(string categoryName)
        {
            _currentCategory = categoryName;
            _view.ClearCommands(); // 只清理右侧指令区

            var commands = GMCommandRegistry.GetCommandsByCategory(categoryName);
            if (commands == null) return;

            foreach (var cmd in commands)
            {
                // 让 View 生成右侧指令按钮，注入指令触发回调
                _view.CreateCommandButton(cmd.Name, () =>
                {
                    Debug.Log($"<color=yellow>[GM 执行] {cmd.Category} -> {cmd.Name}</color>");
                    cmd.Callback?.Invoke();
                });
            }
        }
    }
}