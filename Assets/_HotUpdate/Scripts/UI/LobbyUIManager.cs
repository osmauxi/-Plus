using UnityEngine;
using System.Collections.Generic;

namespace ProjectGame.HotFix.UI.Lobby
{
    /// <summary>
    /// 负责统筹所有Presenter的生命周期，处理状态机切换等，连接不同的UI面板与运镜
    /// </summary>
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager Instance { get; private set; }

        [Header("挂载所有的子面板 P 区")]
        [SerializeField] private BaseLobbyPresenter[] _presenters;

        private Dictionary<LobbyScreenState, BaseLobbyPresenter> _presenterDict;

        private LobbyScreenState _currentState = LobbyScreenState.None;

        private void Awake()
        {
            if(Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            InitializePresenters();
        }

        private void Start()
        {
            //初始默认进入概览界面
            ChangeScreen(LobbyScreenState.Overview);
        }

        private void InitializePresenters()
        {
            _presenterDict = new Dictionary<LobbyScreenState, BaseLobbyPresenter>(5);

            foreach(var p in _presenters)
            {
                if(p.AssociatedState == LobbyScreenState.None)
                {
                    Debug.LogError($"[LobbyUIManager] 有面板未分配状态！物体名: {p.gameObject.name}");
                    continue;
                }

                _presenterDict[p.AssociatedState] = p;
            }
        }

        /// <summary>
        /// 一键切换UI面板
        /// </summary>
        public void ChangeScreen(LobbyScreenState newState)
        {
            if(_currentState == newState) 
                return;

            //让旧状态彻底停机装死
            if(_currentState != LobbyScreenState.None && _presenterDict.ContainsKey(_currentState))
            {
                _presenterDict[_currentState].Sleep();
            }

            //唤醒新状态
            if(_presenterDict.ContainsKey(newState))
            {
                _presenterDict[newState].WakeUp();
            }
            else
            {
                Debug.LogError($"[LobbyUIManager] 试图切换到未注册的状态: {newState}");
            }

            _currentState = newState;
        }
    }
}