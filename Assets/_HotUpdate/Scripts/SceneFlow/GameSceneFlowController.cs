using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 负责从 Lobby 进入游戏场景的网络场景流程。
    /// </summary>
    public sealed class GameSceneFlowController : NetworkBehaviour
    {
        public static GameSceneFlowController Instance { get; private set; }

        [Header("Build Scene Paths")]
        [SerializeField] private string _lobbySceneName = "Assets/_HotUpdate/Scenes/LobbyScene.unity";
        [SerializeField] private string _gameRuntimeSceneName = "Assets/_HotUpdate/Scenes/GameRunTimeScene.unity";
        [SerializeField] private string _gameUISceneName = "Assets/_HotUpdate/Scenes/UIGameUIScene.unity";

        private readonly NetworkSceneLoadService _networkSceneLoader = new NetworkSceneLoadService();

        private CancellationTokenSource _flowCts;
        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public override void OnDestroy()
        {
            CancelCurrentFlow();

            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }
        public async UniTask TransitionToGameSceneAsync()
        {
            if (!IsServer)
            {
                Debug.LogWarning(
                    $"[{nameof(GameSceneFlowController)}] 只有 Server / Host 可以发起进入游戏场景。");
                return;
            }

            if (_isTransitioning)
            {
                Debug.LogWarning(
                    $"[{nameof(GameSceneFlowController)}] 已经在进入游戏场景流程中，忽略重复调用。");
                return;
            }

            CancelCurrentFlow();

            _flowCts = new CancellationTokenSource();
            CancellationToken ct = _flowCts.Token;

            _isTransitioning = true;

            try
            {
                Debug.Log("[GameSceneFlowController] 开始进入游戏场景流程。");

                ShowLoadingClientRpc("正在进入游戏...");

                // 给 ClientRpc 一帧时间派发，避免立刻切场景导致 Loading 来不及显示。
                await UniTask.Yield(PlayerLoopTiming.Update, ct);

                // Single 加载 GameRuntimeScene。
                // 这一步会卸载当前普通场景，例如 LobbyScene。
                await _networkSceneLoader.LoadSceneAsync(
                    _gameRuntimeSceneName,
                    ct,
                    LoadSceneMode.Single);

                Debug.Log("[GameSceneFlowController] GameRuntimeScene 加载完成。");

                // Additive 加载纯 UI 场景。
                await _networkSceneLoader.LoadSceneAsync(
                    _gameUISceneName,
                    ct,
                    LoadSceneMode.Additive);

                Debug.Log("[GameSceneFlowController] UIGameUIScene 加载完成。");

                HideLoadingClientRpc();

                Debug.Log("[GameSceneFlowController] 进入游戏场景流程完成。");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[GameSceneFlowController] 进入游戏流程被取消。");
                //把异常往上层抛，让调用方知道流程被取消了。
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSceneFlowController] 进入游戏流程失败: {e}");
                HideLoadingClientRpc();
                throw;
            }
            finally
            {
                _isTransitioning = false;

                _flowCts?.Dispose();
                _flowCts = null;
            }
        }

        public void CancelCurrentFlow()
        {
            if (_flowCts == null)
            {
                return;
            }

            if (!_flowCts.IsCancellationRequested)
            {
                _flowCts.Cancel();
            }

            _flowCts.Dispose();
            _flowCts = null;

            _isTransitioning = false;
        }

        [ClientRpc]
        private void ShowLoadingClientRpc(string message)
        {
            var loading = FindLoadingScreenService();

            if (loading == null)
            {
                Debug.LogWarning(
                    $"[{nameof(GameSceneFlowController)}] 未找到 LoadingScreenService，跳过 Loading 显示。");
                return;
            }

            loading.Show(message);
        }

        [ClientRpc]
        private void HideLoadingClientRpc()
        {
            var loading = FindLoadingScreenService();

            if (loading == null)
            {
                return;
            }

            loading.HideAsync().Forget();
        }

        private static LoadingScreenService FindLoadingScreenService()
        {
            return FindObjectOfType<LoadingScreenService>(true);
        }
    }
}
