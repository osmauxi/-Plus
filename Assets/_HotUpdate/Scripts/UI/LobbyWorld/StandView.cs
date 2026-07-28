using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// Stand 预制件 View 层 - 纯引用收集 + Billboard 行为
    /// 挂载在每个 Stand_N 预制件根节点上
    /// </summary>
    public class StandView : MonoBehaviour
    {
        [Header("锚点")]
        [SerializeField] private Transform _playerSpawnPos;
        [SerializeField] private Transform _cameraFocusPos;

        [Header("有玩家时 UI")]
        [SerializeField] private CanvasGroup _playerUIs;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _playerReadyText;

        [Header("无玩家时 UI")]
        [SerializeField] private CanvasGroup _emptyUIs;
        [SerializeField] private Button _emptyClickButton;

        [Header("点击检测")]
        [SerializeField] private BoxCollider _clickCollider;

        // 公开给外部
        public Transform PlayerSpawnPos => _playerSpawnPos;
        public Transform CameraFocusPos => _cameraFocusPos;
        public TMP_Text PlayerNameText => _playerNameText;
        public Button EmptyClickButton => _emptyClickButton;
        public BoxCollider ClickCollider => _clickCollider;

        private Transform _mainCamera;
        private Action<string> _nameEditCompleted;
        private string _nameBeforeEdit;
        private bool _nameInteractable;

        /// <summary>初始化玩家名字的内联输入事件。</summary>
        private void Awake()
        {
            _nameInput.enabled = false;
            _nameInput.onEndEdit.AddListener(CompleteNameEdit);
        }

        /// <summary>缓存用于 Billboard 的主相机。</summary>
        private void Start()
        {
            _mainCamera = Camera.main.transform;
        }

        /// <summary>销毁时解除运行时输入框事件。</summary>
        private void OnDestroy()
        {
            _nameInput.onEndEdit.RemoveListener(CompleteNameEdit);
        }

        /// <summary>让可见的玩家信息始终朝向主相机。</summary>
        private void LateUpdate()
        {
            // Billboard: PlayerUIs 始终面向主相机
            if (_playerUIs.alpha > 0.01f)
            {
                _playerUIs.transform.LookAt(
                    _playerUIs.transform.position + _mainCamera.forward,
                    Vector3.up
                );
            }
        }

        #region View 纯刷新方法

        /// <summary>
        /// 根据是否有玩家设置 CanvasGroup 显隐
        /// </summary>
        public void SetVisible(bool hasPlayer)
        {
            _playerUIs.alpha = hasPlayer ? 1f : 0f;
            _playerUIs.interactable = hasPlayer;
            _playerUIs.blocksRaycasts = hasPlayer;

            _emptyUIs.alpha = hasPlayer ? 0f : 1f;
            _emptyUIs.interactable = !hasPlayer;
            _emptyUIs.blocksRaycasts = !hasPlayer;
        }

        /// <summary>
        /// 设置玩家名字文本
        /// </summary>
        public void SetName(string name)
        {
            if (!_nameInput.enabled)
                _playerNameText.text = name;
        }

        /// <summary>
        /// 控制名字文本的交互（是否可点击改名）
        /// </summary>
        public void SetNameInteractable(bool interactable)
        {
            _nameInteractable = interactable;
            if (!_nameInput.enabled)
                _playerNameText.GetComponent<Button>().interactable = interactable;
        }

        /// <summary>把玩家名字文本切换为可输入状态并等待提交。</summary>
        public void BeginNameEdit(Action<string> onCompleted)
        {
            _nameBeforeEdit = _playerNameText.text;
            _nameEditCompleted = onCompleted;
            _playerNameText.GetComponent<Button>().interactable = false;
            _nameInput.enabled = true;
            _nameInput.text = _nameBeforeEdit;
            _nameInput.Select();
            _nameInput.ActivateInputField();
            _nameInput.MoveTextEnd(false);
        }

        /// <summary>校验并提交内联输入的玩家名字。</summary>
        private void CompleteNameEdit(string enteredName)
        {
            bool wasCanceled = _nameInput.wasCanceled;
            string newName = enteredName.Trim();
            bool isValid = !wasCanceled
                && newName.Length > 0
                && Encoding.UTF8.GetByteCount(newName) <= 29;

            if (!wasCanceled && !isValid)
                Debug.LogWarning("[StandView] 玩家名字不能为空且不能超过 29 个 UTF-8 字节");

            _nameInput.enabled = false;
            _playerNameText.text = isValid ? newName : _nameBeforeEdit;
            _playerNameText.GetComponent<Button>().interactable = _nameInteractable;

            Action<string> completed = _nameEditCompleted;
            _nameEditCompleted = null;
            if (isValid && newName != _nameBeforeEdit)
                completed(newName);
        }

        /// <summary>
        /// 设置准备状态文本和颜色
        /// </summary>
        public void SetReady(bool isReady, bool visible)
        {
            _playerReadyText.text = isReady ? "准备" : "未准备";
            _playerReadyText.color = isReady ? Color.green : Color.red;
            _playerReadyText.gameObject.SetActive(visible);
        }

        #endregion
    }
}
