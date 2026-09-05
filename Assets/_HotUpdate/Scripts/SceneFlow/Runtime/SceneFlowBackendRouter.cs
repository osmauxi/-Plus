using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using ProjectGame.HotFix.Network.Runtime;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 单个场景在双加载场景的场景切换中所对应的物理场景信息
    /// </summary>
    internal readonly struct PhysicalSceneReference
    {
        public string AddressableAddress { get; }
        public string NgoSceneName { get; }

        public PhysicalSceneReference(string addressableAddress,string ngoSceneName)
        {
            AddressableAddress = addressableAddress;
            NgoSceneName = ngoSceneName;
        }
    }

    /// <summary>
    /// 根据选择的加载模式为本次场景加载/卸载提供具体的加载/卸载服务
    /// </summary>
    internal sealed class SceneFlowBackendRouter
    {
        private readonly NetworkSceneBackend _backend;
        private readonly AddressableSceneBarrier _addressableBarrier;
        private readonly NgoSceneLoadService _ngoSceneLoader;
        private readonly float _timeoutSeconds;

        public SceneFlowBackendRouter(NetworkRuntimeBootstrap bootstrap,AddressableSceneBarrier addressableBarrier,float timeoutSeconds)
        {
            if (bootstrap == null)
                throw new ArgumentNullException(nameof(bootstrap));

            _backend = bootstrap.SceneBackend;
            _addressableBarrier = addressableBarrier;
            _timeoutSeconds = timeoutSeconds;

            if (_backend == NetworkSceneBackend.NgoIntegrated)
            {
                _ngoSceneLoader = new NgoSceneLoadService(bootstrap.NetworkManager);
            }
        }

        public UniTask LoadAsync(PhysicalSceneReference scene,CancellationToken cancellationToken)
        {
            switch (_backend)
            {
                case NetworkSceneBackend.Addressables:
                    EnsureAddressableBarrier();
                    return _addressableBarrier.LoadForAllClientsAsync(
                        scene.AddressableAddress,
                        _timeoutSeconds,
                        cancellationToken);

                case NetworkSceneBackend.NgoIntegrated:
                    return _ngoSceneLoader.LoadSceneAsync(
                        scene.NgoSceneName,
                        _timeoutSeconds,
                        cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public UniTask UnloadAsync(PhysicalSceneReference scene,CancellationToken cancellationToken)
        {
            switch (_backend)
            {
                case NetworkSceneBackend.Addressables:
                    EnsureAddressableBarrier();
                    return _addressableBarrier.UnloadForAllClientsAsync(
                        scene.AddressableAddress,
                        _timeoutSeconds,
                        cancellationToken);

                case NetworkSceneBackend.NgoIntegrated:
                    return _ngoSceneLoader.UnloadSceneAsync(
                        scene.NgoSceneName,
                        _timeoutSeconds,
                        cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnsureAddressableBarrier()
        {
            if (_addressableBarrier == null)
            {
                throw new InvalidOperationException(
                    "Addressables Backend 缺少 AddressableSceneBarrier");
            }
        }
    }
}
