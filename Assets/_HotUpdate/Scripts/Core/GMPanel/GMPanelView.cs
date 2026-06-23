using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace ProjectGame.HotFix.Core.DebugTools
{
    /// <summary>
    /// GM面板View层
    /// </summary>
    public class GMPanelView : MonoBehaviour
    {
        [Header("UI 预制体")]
        [SerializeField] private GameObject _categoryButtonPrefab;
        [SerializeField] private GameObject _commandButtonPrefab;

        [Header("容器节点")]
        [SerializeField] private Transform _categoryContainer;
        [SerializeField] private Transform _commandContainer;
        [SerializeField] private GameObject _panelRoot;

        private readonly List<GameObject> _categoryGos = new List<GameObject>();
        private readonly List<GameObject> _commandGos = new List<GameObject>();

        public bool IsVisible => _panelRoot.activeSelf;

        public void SetVisible(bool isVisible)
        {
             _panelRoot.SetActive(isVisible);
        }

        public void ClearAll()
        {
            ClearCategories();
            ClearCommands();
        }

        public void ClearCategories()
        {
            foreach (var go in _categoryGos) 
                if (go != null) 
                    Destroy(go);
            _categoryGos.Clear();
        }

        public void ClearCommands()
        {
            foreach (var go in _commandGos) 
                if (go != null) 
                    Destroy(go);
            _commandGos.Clear();
        }

        /// <summary>
        /// 生成一个分类按钮，并绑定点击委托
        /// </summary>
        public void CreateCategoryButton(string text, Action onClick)
        {
            GameObject go = Instantiate(_categoryButtonPrefab, _categoryContainer);
            _categoryGos.Add(go);

            Text txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = text;

            Button btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => onClick?.Invoke());
        }

        /// <summary>
        /// 生成一个指令按钮，并绑定点击委托
        /// </summary>
        public void CreateCommandButton(string text, Action onClick)
        {
            GameObject go = Instantiate(_commandButtonPrefab, _commandContainer);
            _commandGos.Add(go);

            Text txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = text;

            Button btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}