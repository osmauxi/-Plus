using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// JoinGameUI 的纯视图层：注册UI事件、暴露DOTween动效方法供P层调用
    /// 作为 OverviewUI 的 Overlay 子面板，由 OverviewPresenter 直接管理
    /// </summary>
    public class JoinGameView : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("按钮 (MainActionButtonView)")]
        [SerializeField] private MainActionButtonView _joinGameActionBtn;   // 点击 → 显示InputField准备加入
        [SerializeField] private MainActionButtonView _createGameActionBtn; // 点击 → StartHost

        [Header("输入区")]
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private TMP_Text _infText;            // InputField子物体, 连接状态信息

        [Header("锚点空物体 (Awake中读取位置后自动删除)")]
        [SerializeField] private Transform _defaultPosAnchor;       // 两按钮默认/收起位置（重叠于此）
        [SerializeField] private Transform _createGameMovPosAnchor; // CreateGame展开目标位置
        [SerializeField] private Transform _joinGameMovPosAnchor;   // JoinGame展开目标位置

        [Header("动效")]
        [SerializeField] private float _tweenDuration = 0.3f;

        // 缓存的位置
        private Vector2 _defaultPos;
        private Vector2 _createGameMovPos;
        private Vector2 _joinGameMovPos;

        // 缓存的RectTransform
        private RectTransform _joinGameBtnRect;
        private RectTransform _createGameBtnRect;

        // 对外事件 —— P层监听
        public event Action OnJoinGameClicked;
        public event Action OnCreateGameClicked;
        public event Action<string> OnJoinSubmit;

        /// <summary>缓存动画位置、绑定加入房间控件并初始化收起状态 </summary>
        private void Awake()
        {
            //缓存按钮RectTransform
            _joinGameBtnRect = _joinGameActionBtn.GetComponent<RectTransform>();
            _createGameBtnRect = _createGameActionBtn.GetComponent<RectTransform>();

            _defaultPos = _defaultPosAnchor.position;
            _createGameMovPos = _createGameMovPosAnchor.position;
            _joinGameMovPos = _joinGameMovPosAnchor.position;
            

            // 初始状态：按钮在DefaultPos，alpha=0，不交互
            ResetToDefault();

            // 注册UI事件
            _joinGameActionBtn.OnClicked += () => OnJoinGameClicked?.Invoke();
            _createGameActionBtn.OnClicked += () => OnCreateGameClicked?.Invoke();
            _ipInputField.onSubmit.AddListener((ip) => OnJoinSubmit?.Invoke(ip));

            // 初始隐藏InputField
            _ipInputField.gameObject.SetActive(false);
        }

        /// <summary>销毁加入房间视图时解除输入框提交事件 </summary>
        private void OnDestroy()
        {
            _ipInputField.onSubmit.RemoveAllListeners();
        }

        /// <summary>
        /// 重置所有UI到默认收起状态（不带动效）
        /// </summary>
        public void ResetToDefault()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _joinGameBtnRect.position = _defaultPos;
            _createGameBtnRect.position = _defaultPos;

            _ipInputField.gameObject.SetActive(false);
            _infText.text = "";
        }

        #region P层调用的动效方法

        /// <summary>
        /// Show 动效：两按钮从 DefaultPos 滑向各自 MovPos，alpha 0→1
        /// </summary>
        public Sequence Show(float? durationOverride = null)
        {
            float dur = durationOverride ?? _tweenDuration;

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            Sequence seq = DOTween.Sequence();

            seq.Join(_createGameBtnRect.DOMove(_createGameMovPos, dur).SetEase(Ease.OutQuad));
            seq.Join(_joinGameBtnRect.DOMove(_joinGameMovPos, dur).SetEase(Ease.OutQuad));
            seq.Join(_canvasGroup.DOFade(1f, dur));

            return seq;
        }

        /// <summary>
        /// Hide 动效：两按钮从 MovPos 滑回 DefaultPos，alpha 1→0，同时隐藏 InputField
        /// </summary>
        public Sequence Hide(float? durationOverride = null)
        {
            float dur = durationOverride ?? _tweenDuration;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            Sequence seq = DOTween.Sequence();

            seq.Join(_createGameBtnRect.DOMove(_defaultPos, dur).SetEase(Ease.OutQuad));
            seq.Join(_joinGameBtnRect.DOMove(_defaultPos, dur).SetEase(Ease.OutQuad));
            seq.Join(_canvasGroup.DOFade(0f, dur));

            seq.OnComplete(() =>
            {
                _ipInputField.gameObject.SetActive(false);
                _infText.text = "";
            });

            return seq;
        }

        /// <summary>
        /// 显示 InputField，让用户输入IP
        /// </summary>
        public void ShowInputField()
        {
            _ipInputField.gameObject.SetActive(true);
            _ipInputField.text = "";
            _ipInputField.ActivateInputField();
        }

        /// <summary>
        /// 隐藏 InputField
        /// </summary>
        public void HideInputField()
        {
            _ipInputField.gameObject.SetActive(false);
            _infText.text = "";
        }

        /// <summary>
        /// 设置 InfText 状态信息
        /// </summary>
        public void SetInfText(string msg)
        {
            _infText.text = msg;
        }

        #endregion
    }
}
