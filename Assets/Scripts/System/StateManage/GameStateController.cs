using MyScripts.Core;
using Unity.Netcode;
using UnityEngine;
using static NetEventCenter;

public enum GameState 
{
    GameLoading, 
    MapExchanging, 
    MapGenerating,
    GamePlaying,
    MapClear,
    GameOver,
}
public class GameStateController : NetworkBehaviour
{
    public static GameStateController instance;

    public NetworkVariable<GameState> currentNetState = new NetworkVariable<GameState>(GameState.GameLoading);
    public NetworkVariable<bool> isSolo = new NetworkVariable<bool>(false);
    public NetworkVariable<int> CurrentLevel = new NetworkVariable<int>(1);



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

       
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentNetState.OnValueChanged += HandleNetworkState;
        if (IsServer)
        {
            // 监听客户端连接
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayerCount;
        }
        NetEventCenter.Instance.Subscribe<GamePlayStartStruct>(OnGameStart);
    }
    private void CheckPlayerCount(ulong clientId)
    {
        if (!IsServer) return;

        // 当连接人数达到 2 时，且当前处于 MapExchanging (等待中)
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2 &&
            currentNetState.Value == GameState.MapExchanging)
        {
            // 触发转场至地图生成
            ChangeState(GameState.MapGenerating);
            GameDirector.Instance.AdvanceToNextLayer(CurrentLevel.Value);
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetEventCenter.Instance.Unsubscribe<GamePlayStartStruct>(OnGameStart);
    }
    #region NetworkStateLogic
        private void HandleNetworkState(GameState previousState, GameState newstate)
        {
            switch (newstate)
            {
                case GameState.GameLoading:
                    break;
                case GameState.MapGenerating:
                    HandleMapSpawnState();
                    break;
                case GameState.GamePlaying:
                    HandlePlayState();
                    break;
                case GameState.MapClear:
                    break;
                case GameState.MapExchanging:
                    break;
       
            }
        
        }
        private void HandleWaitingToStartState()
        { 
        }
        private void HandleMapSpawnState()
        {
            StartCoroutine(MapGenerator.instance.PreGenerateMap());
        }
        private void HandlePlayState() 
        {
            if(IsServer)
                NetEventCenter.Instance.Send<GamePlayStartStruct>(new GamePlayStartStruct());
        }
        public void OnGameStart(GamePlayStartStruct evt,ulong sendeId) 
        {
        if (!NetUtils.Filter<GamePlayStartStruct>(evt, sendeId, true)) 
            {
                return;
            }
            LocalEventCenter.Instance.EventTrigger(evt);
        }
        public void ChangeState(GameState state) 
        {
            if(!IsServer)
                return;
            currentNetState.Value = state;
        }
        #endregion
}

