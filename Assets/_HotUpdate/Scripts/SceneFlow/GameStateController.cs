using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Core.Events;
using ProjectGame.HotFix.Gameplay.Events;
using System.Threading;
using Unity.Netcode;

namespace ProjectGame.HotFix.Gameplay.State
{
    public enum GameState : byte
    {
        None = 0,
        GameLoading = 1,
        MapGenerating = 2,
        GamePlaying = 3,
        GameOver = 4,
        ReturningLobby = 5,
    }
    public sealed class GameStateController : NetworkBehaviour
    {
        public static GameStateController Instance { get; private set; }

        private readonly NetworkVariable<GameState> _currentState = new NetworkVariable<GameState>(
            GameState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
            );

        private readonly NetworkVariable<int> _currentLevel =new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
            );

        public GameState CurrentState => _currentState.Value;
        public int CurrentLevel => _currentLevel.Value;

        public bool IsPlaying => _currentState.Value == GameState.GamePlaying;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;

            _currentState.OnValueChanged += HandleStateChanged;
            _currentLevel.OnValueChanged += HandleLevelChanged;

            PublishInitialState();
       }


        public override void OnNetworkDespawn()
        {
            _currentState.OnValueChanged -= HandleStateChanged;
            _currentLevel.OnValueChanged -= HandleLevelChanged;

            if (Instance == this)
            {
                Instance = null;
            }
            base.OnNetworkDespawn();
        }

        public void ChangeStateServer(GameState newState)
        {
            if (!IsServer)
            {
                return;
            }

            if (_currentState.Value == newState)
            {
                return;
            }

            _currentState.Value = newState;
        }
        public void SetLevelServer(int level)
        {
            if (!IsServer)
            {
                return;
            }

            if(level <= 0) 
            {
                return;
            }

            if (_currentLevel.Value == level)
            {
                return;
            }

            _currentLevel.Value = level;
        }

        public void IncreaseLevelServer() 
        {
            if (!IsServer)
            {
                return;
            }

            _currentLevel.Value++;
        }

        /// <summary>
        /// 等待状态切换到目标状态 
        /// 主要给流程控制器使用 
        /// </summary>
        public async UniTask WaitForStateAsync(GameState targetState,CancellationToken cancellationToken) 
        {
            await UniTask.WaitUntil(() => _currentState.Value == targetState, cancellationToken : cancellationToken);
        }

        /// <summary>
        /// 等待状态离开指定状态 
        /// </summary>
        public async UniTask WaitUntilStateExitAsync(GameState state,CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _currentState.Value != state,cancellationToken: cancellationToken);
        }


        private void PublishInitialState()
        {
            LocalEvents.Publish(new GameStateChangedEvent(GameState.None, _currentState.Value));
            LocalEvents.Publish(new GameLevelChangedEvent(0, _currentLevel.Value));

        }

        private void HandleStateChanged(GameState previousState, GameState currentState)
        {
            LocalEvents.Publish(new GameStateChangedEvent(previousState, currentState));
        }

        private void HandleLevelChanged(int previousLevel, int currentLevel)
        {
            LocalEvents.Publish(new GameLevelChangedEvent(previousLevel,currentLevel));
        }
    }
}
