using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 在Addressable场景管线中补足NGO原本自带的全端Scene同步能力
    /// Server发起一次场景加载/卸载操作，所有Client在本机执行相同操作，并通过Barrier确认完成
    /// 发出加载请求 -> 各端执行本机加载 
    /// -> Client发出ServerRpc确认 -> Server收集所有Client确认 -> Barrier完成
    /// 然后才是注册/注销NetworkPrefab,生成/销毁Root等操作
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class AddressableSceneBarrier : NetworkBehaviour
    {
        private enum SceneOperation
        {
            Load = 0,
            Unload = 1
        }

        private readonly NetworkBarrierState _barrier = new NetworkBarrierState();
        private readonly AddressableSceneLoadService _sceneLoader = AddressableSceneLoadService.Shared;
        
        //防止新的操作覆盖旧的操作，导致Barrier状态混乱
        private int _operationRevision;
        private bool _isRunning;
        private int _localRevision;
        private readonly SceneFlowLocalOperation _localOperation = new SceneFlowLocalOperation();
        private CancellationTokenSource _lifetime;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _lifetime = new CancellationTokenSource();
            _operationRevision = _localRevision = 0;
            _isRunning = false;
        }

        public override void OnNetworkDespawn()
        {
            _lifetime?.Cancel();
            _localOperation.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            base.OnNetworkDespawn();
        }

        public UniTask LoadForAllClientsAsync(string sceneAddress,float timeoutSeconds,CancellationToken cancellationToken)
        {
            return RunForAllClientsAsync(
                SceneOperation.Load,
                sceneAddress,
                LoadSceneMode.Additive,
                timeoutSeconds,
                cancellationToken);
        }

        public UniTask UnloadForAllClientsAsync(string sceneAddress,float timeoutSeconds,CancellationToken cancellationToken)
        {
            return RunForAllClientsAsync(
                SceneOperation.Unload,
                sceneAddress,
                LoadSceneMode.Additive,
                timeoutSeconds,
                cancellationToken);
        }

        private async UniTask RunForAllClientsAsync(
            SceneOperation operation,
            string sceneAddress,
            LoadSceneMode loadMode,
            float timeoutSeconds,
            CancellationToken cancellationToken)
        {
            NetworkManager networkManager = NetworkManager;
            if (networkManager == null || !networkManager.IsServer || !networkManager.IsListening || !IsSpawned || _lifetime == null)
            {
                throw new InvalidOperationException("Addressables Scene 屏障只能由 Server/Host 发起");
            }

            if (_isRunning)
                throw new InvalidOperationException("已有 Addressables Scene 屏障正在执行");
            SceneFlowLocalOperation.ValidateTimeout(timeoutSeconds);
            //初始化barrier
            _isRunning = true;
            int revision = ++_operationRevision;
            string operationName = $"Addressables Scene {operation}: {sceneAddress}";
            _barrier.Begin(networkManager, revision, operationName);

            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token))
            try
            {
                RunSceneOperationClientRpc((int)operation,sceneAddress,(int)loadMode,revision,timeoutSeconds);
                Exception localFailure = null;
                //考虑纯Server的情况，让Server本地执行一次操作，Host不会走这里
                if (!networkManager.IsClient)
                {
                    try
                    {
                        await RunSerializedLocalAsync(operation, sceneAddress, loadMode, revision, timeoutSeconds, linked.Token);
                    }
                    catch (Exception exception) { localFailure = exception; }
                }
                //这里让Host一起走ClientRPC触发本地场景加载，一般情况下会区分Host和Client的场景加载
                //这里为了简化逻辑，Host也走ClientRPC
                //Server端等待所有Client完成操作，或者超时/取消
                await _barrier.WaitAsync(networkManager,timeoutSeconds,linked.Token);
                if (localFailure != null)
                    throw new InvalidOperationException($"Dedicated Server 本机 {operationName} 失败", localFailure);
            }
            catch
            {
                _localOperation.Cancel();
                if (IsSpawned && networkManager.IsListening) CancelSceneOperationClientRpc(revision);
                throw;
            }
            finally
            {
                _isRunning = false;
            }
        }
        //直接传递枚举其实没有什么区别，这里传int接类型转换装高手
        [ClientRpc]
        private void RunSceneOperationClientRpc(
            int operationValue,
            string sceneAddress,
            int loadModeValue,
            int revision, float timeoutSeconds)
        {
            //RPC等不了UniTask，所以这里直接调Forget()
            RunSceneOperationOnClientAsync(
                    (SceneOperation)operationValue,
                    sceneAddress,
                    (LoadSceneMode)loadModeValue,
                    revision, timeoutSeconds)
                .Forget();
        }

        private async UniTaskVoid RunSceneOperationOnClientAsync(
            SceneOperation operation,
            string sceneAddress,
            LoadSceneMode loadMode,
            int revision, float timeoutSeconds)
        {
            if (revision <= _localRevision) return;
            bool succeeded = false;
            string error = string.Empty;

            try
            {
                //本地跑场景加载
                await RunSerializedLocalAsync(operation, sceneAddress, loadMode, revision, timeoutSeconds,
                    _lifetime?.Token ?? new CancellationToken(true));
                succeeded = true;
            }
            catch (Exception exception)
            {
                error = NetworkBarrierState.LimitRpcError(exception.Message);
                Debug.LogError(
                    $"[AddressableSceneBarrier] 本机 {operation} 失败：" +
                    $"{sceneAddress}\n{exception}");
            }
            //返回场景加载结果给Server
            if (IsSpawned && IsClient && NetworkManager.IsListening)
                ConfirmSceneOperationServerRpc(revision,succeeded,error);
        }

        private async UniTask RunSerializedLocalAsync(SceneOperation operation, string sceneAddress,
            LoadSceneMode loadMode, int revision, float timeoutSeconds, CancellationToken cancellationToken)
        {
            _localRevision = revision;
            await _localOperation.CancelAndDrainAsync(timeoutSeconds, cancellationToken);
            await _localOperation.RunAsync(token => RunLocalAsync(operation, sceneAddress, loadMode, token),
                timeoutSeconds, cancellationToken);
        }

        [ClientRpc]
        private void CancelSceneOperationClientRpc(int revision)
        {
            if (revision == _localRevision) _localOperation.Cancel();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConfirmSceneOperationServerRpc(int revision,bool succeeded,string error,ServerRpcParams rpcParams = default)
        {
            if (_isRunning) _barrier.Complete(revision,rpcParams.Receive.SenderClientId,succeeded,error);
        }

        private UniTask RunLocalAsync(SceneOperation operation,string sceneAddress,LoadSceneMode loadMode,CancellationToken cancellationToken)
        {
            switch (operation)
            {
                case SceneOperation.Load:
                    return LoadLocalAsync(sceneAddress,loadMode,cancellationToken);

                case SceneOperation.Unload:
                    return _sceneLoader.UnloadSceneAsync(sceneAddress,cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private async UniTask LoadLocalAsync(string sceneAddress,LoadSceneMode loadMode,CancellationToken cancellationToken)
        {
            await _sceneLoader.LoadSceneAsync(sceneAddress,loadMode,cancellationToken);
        }
    }
}
