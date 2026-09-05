using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Network.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 在网络会话开始之前，准备好Addressable场景切换所需的网络组件
    /// </summary>
    [RequireComponent(typeof(NetworkRuntimeBootstrap))]
    public sealed class NetworkSessionBootstrap : MonoBehaviour
    {
        public const string PrefabAddress = "Assets/_HotUpdate/Prefabs/Network/NetworkBootstrap.prefab";
        public static NetworkSessionBootstrap Instance { get; private set; }
        private static AsyncOperationHandle<GameObject> _prefabHandle;
        private static UniTaskCompletionSource<NetworkSessionBootstrap> _creating;

        [SerializeField] private NetworkPrefabId[] _seedIds = { NetworkPrefabId.NetworkSessionRoot };
        [SerializeField] private NetworkPrefabId[] _preconnectPrefabIds = { NetworkPrefabId.LobbyNetworkRoot };
        [SerializeField, Min(1)] private float _timeoutSeconds = 45f;
        private NetworkRuntimeBootstrap _runtime;
        private NetworkManager _network;
        private UniTaskCompletionSource _prepared;
        private UniTaskCompletionSource _lobbyReady;
        private bool _hasSession;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _runtime = GetComponent<NetworkRuntimeBootstrap>();
            _network = GetComponent<NetworkManager>();
            DontDestroyOnLoad(gameObject);
            _network.OnServerStarted += HandleServerStarted;
            _network.OnClientStarted += HandleClientStarted;
        }

        public static async UniTask<NetworkSessionBootstrap> EnsureAvailableAsync(CancellationToken cancellationToken)
        {
            if (Instance != null) 
            { 
                await Instance.PrepareAsync(cancellationToken); 
                return Instance; 
            }
            if (_creating != null) 
                return await _creating.Task.AttachExternalCancellation(cancellationToken);
            _creating = new UniTaskCompletionSource<NetworkSessionBootstrap>();
            try
            {
                //获取并实例化NetworkBootstrap
                _prefabHandle = Addressables.LoadAssetAsync<GameObject>(PrefabAddress);
                GameObject prefab = await _prefabHandle.ToUniTask(cancellationToken: cancellationToken);
                GameObject root = Instantiate(prefab);
                var bootstrap = root.GetComponent<NetworkSessionBootstrap>();
                if (bootstrap == null) 
                {
                    Destroy(root); throw new InvalidOperationException("NetworkBootstrap 缺少会话引导"); 
                }
                await bootstrap.PrepareAsync(cancellationToken);
                _creating.TrySetResult(bootstrap);
                return bootstrap;
            }
            catch (Exception exception)
            {
                _creating.TrySetException(exception);
                if (Instance != null) Destroy(Instance.gameObject);
                if (_prefabHandle.IsValid()) Addressables.Release(_prefabHandle);
                throw;
            }
            finally { _creating = null; }
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            if (_prepared != null) { await _prepared.Task.AttachExternalCancellation(cancellationToken); return; }
            _prepared = new UniTaskCompletionSource();
            try
            {
                _runtime.Initialize();
                foreach (NetworkPrefabId id in _seedIds)
                    await _runtime.PrefabRegistry.PreparePersistentSeedAsync(id, cancellationToken);
                foreach (NetworkPrefabId id in _preconnectPrefabIds)
                    await _runtime.PrefabRegistry.PrepareAsync(id, cancellationToken);
                _prepared.TrySetResult();
            }
            catch (Exception exception) { _prepared.TrySetException(exception); throw; }
        }

        /// <summary>
        /// 每次启动StartHost/Client前调用，他确保上一个会话不与当前会话重叠
        /// </summary>
        public async UniTask PrepareConnectionAsync(CancellationToken cancellationToken)
        {
            await PrepareAsync(cancellationToken);
            if (_network.IsListening) 
                return;
            //等旧Session清理干净
            await SceneFlowLocalOperation.WaitAsync(() => !_network.ShutdownInProgress,
                _timeoutSeconds, "等待上一会话关闭超时", cancellationToken);
            if (_hasSession)
            {
                _runtime.ResetAfterShutdown();
                _hasSession = false;
            }
            foreach (NetworkPrefabId id in _preconnectPrefabIds)
                if (!_runtime.PrefabRegistry.IsPrepared(id))
                    await _runtime.PrefabRegistry.PrepareAsync(id, cancellationToken);
            _lobbyReady = null;
        }
        /// <summary>
        /// 在Server启动时调用，将根网络组件Enable并保证Spawn
        /// </summary>
        private void HandleServerStarted()
        {
            _hasSession = true;
            try
            {
                foreach (NetworkPrefabId id in _seedIds)
                {
                    NetworkObject root = _runtime.PrefabRegistry.GetPersistentSeed(id);
                    root.gameObject.SetActive(true);
                    if (!root.IsSpawned) 
                        root.Spawn(false);
                }
                _lobbyReady = new UniTaskCompletionSource();
                EnterInitialLobbyAsync().Forget();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NetworkSessionBootstrap] 会话种子启动失败：{exception}");
                _network.Shutdown();
            }
        }

        private void HandleClientStarted() => _hasSession = true;

        private async UniTask EnterInitialLobbyAsync()
        {
            try
            {
                //离开NGO启动回调，等Host玩家登记完成后启动第一轮屏障。
                await UniTask.Yield();
                await GameSceneFlowController.Instance.TransitionToLobbySceneAsync();
                _lobbyReady.TrySetResult();
            }
            catch (Exception exception)
            {
                _lobbyReady.TrySetException(exception);
                Debug.LogError($"[NetworkSessionBootstrap] 初始 Lobby 失败：{exception}");
                _network.Shutdown();
            }
        }

        public async UniTask WaitForLobbyReadyAsync(CancellationToken cancellationToken)
        {
            await SceneFlowLocalOperation.WaitAsync(() => _lobbyReady != null,
                _timeoutSeconds, "初始 Lobby 尚未启动", cancellationToken);
            await _lobbyReady.Task.AttachExternalCancellation(cancellationToken);
        }

        private void OnDestroy()
        {
            if (_network != null) _network.OnServerStarted -= HandleServerStarted;
            if (_network != null) _network.OnClientStarted -= HandleClientStarted;
            if (Instance != this) return;
            Instance = null;
            if (_runtime != null && _runtime.IsInitialized && !_network.IsListening)
                _runtime.PrefabRegistry.ReleaseAll();
            if (_prefabHandle.IsValid()) Addressables.Release(_prefabHandle);
        }
    }
}
