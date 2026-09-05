using System;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Network.Runtime
{
    /// <summary>
    /// Network Runtime启动根，在NetworkManager启动前进行场景加载管线的初始化
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    public sealed class NetworkRuntimeBootstrap : MonoBehaviour
    {
        public static NetworkRuntimeBootstrap Instance { get; private set; }

        [Header("Network Runtime")]
        [SerializeField] private NetworkPrefabCatalog _catalog;
        [SerializeField] private NetworkSceneBackend _sceneBackend = NetworkSceneBackend.Addressables;

        private NetworkManager _networkManager;

        public NetworkManager NetworkManager => _networkManager;
        public NetworkSceneBackend SceneBackend => _sceneBackend;
        public NetworkPrefabRegistry PrefabRegistry { get; private set; }
        public NetworkScopeManager ScopeManager { get; private set; }
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _networkManager = GetComponent<NetworkManager>();
        }

        /// <summary>
        /// 在NGO启动前调用
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (_catalog == null)
                throw new InvalidOperationException("NetworkRuntimeBootstrap 未配置 NetworkPrefabCatalog");

            if (_networkManager == null)
                throw new InvalidOperationException("NetworkRuntimeBootstrap 找不到 NetworkManager");

            ConfigureSceneBackend();

            PrefabRegistry = new NetworkPrefabRegistry(_catalog, _networkManager);
            ScopeManager = new NetworkScopeManager(_catalog,PrefabRegistry,_networkManager);

            IsInitialized = true;
            Debug.Log($"[NetworkRuntimeBootstrap] 初始化完成，SceneBackend={_sceneBackend}");
        }

        private void ConfigureSceneBackend()
        {
            if (_networkManager.IsListening)
                throw new InvalidOperationException(
                    "必须在 NetworkManager 启动前配置 Scene Backend");

            switch (_sceneBackend)
            {
                case NetworkSceneBackend.Addressables:
                    _networkManager.NetworkConfig.EnableSceneManagement = false;
                    _networkManager.NetworkConfig.ForceSamePrefabs = false;
                    break;

                case NetworkSceneBackend.NgoIntegrated:
                    _networkManager.NetworkConfig.EnableSceneManagement = true;
                    _networkManager.NetworkConfig.ForceSamePrefabs = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 在NGO没有关的时候才能调用
        /// </summary>
        public void ResetAfterShutdown()
        {
            if (_networkManager.IsListening || _networkManager.ShutdownInProgress || ScopeManager.IsPreparing)
                throw new InvalidOperationException("NGO/Prepare 尚未结束，禁止重置 NetworkRuntime");
            PrefabRegistry.ReleaseAll(retainSessionSeeds: true);
            ScopeManager = new NetworkScopeManager(_catalog, PrefabRegistry, _networkManager);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
