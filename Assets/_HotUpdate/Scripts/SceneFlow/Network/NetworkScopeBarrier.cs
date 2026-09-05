using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using ProjectGame.HotFix.Network.Runtime;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>全端 Scope 阶段和 ACK；本地资源/接口生命周期仍由 ScopeManager 管理。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkScopeBarrier : NetworkBehaviour
    {
        private enum ScopePhase { None, Prepare, RootReady, PreCommitStages, Commit, StopObsolete, Cleanup, Activate, StopRollback, Rollback, StopRecovery, Recovery }

        private readonly NetworkBarrierState _barrier = new NetworkBarrierState();
        private readonly SceneFlowLocalOperation _localOperation = new SceneFlowLocalOperation();
        private NetworkScopePrepareContext _localContext;
        private int _scopeRevision;
        private int _localRevision;
        private ScopePhase _activePhase;
        private ScopePhase _localPhase;
        private bool _phaseRunning;
        private bool _localAborted;
        private string _activationFailure;
        private int _activationDispatchedRevision;
        private int _activatedLocalRevision;
        private CancellationTokenSource _lifetime;

        public int Revision => IsServer ? _scopeRevision : _localRevision;
        public bool IsLocalRuntimeReady => !_localAborted &&
            ((_localContext != null && _localContext.IsRuntimeReady) ||
             (_activatedLocalRevision > 0 && _activatedLocalRevision == _localRevision));
        public event Action<int, string> ActivationFailed;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _lifetime = new CancellationTokenSource();
            _scopeRevision = _localRevision = 0;
            _activePhase = _localPhase = ScopePhase.None;
            _localContext = null;
            _phaseRunning = _localAborted = false;
            _activationDispatchedRevision = _activatedLocalRevision = 0;
        }

        public override void OnNetworkDespawn()
        {
            _lifetime?.Cancel();
            _localOperation.Cancel();
            _localContext?.Invalidate();
            _lifetime?.Dispose();
            _lifetime = null;
            base.OnNetworkDespawn();
        }

        public UniTask PrepareForAllClientsAsync(NetworkSceneMask targetMask, float timeoutSeconds, CancellationToken cancellationToken)
        {
            EnsureServer();
            if (_localContext != null) throw new InvalidOperationException("仍存在未结束的 NetworkScope Context");
            return RunPhaseForAllAsync(ScopePhase.Prepare, NextRevision(), targetMask, timeoutSeconds, cancellationToken);
        }

        public void SpawnPreparedScope()
        {
            EnsureServer();
            ScopeManager.SpawnPreparedScope(RequireLocalContext(_scopeRevision));
        }

        public UniTask WaitForRootsReadyForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
            => RunPhaseForAllAsync(ScopePhase.RootReady, _scopeRevision, 0, timeoutSeconds, cancellationToken);

        // Bind + Initialize form one RuntimeReady ACK. Activate is never pre-commit.
        public UniTask RunPreCommitStagesForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
            => RunPhaseForAllAsync(ScopePhase.PreCommitStages, _scopeRevision, 0, timeoutSeconds, cancellationToken);

        public UniTask CommitForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
            => RunPhaseForAllAsync(ScopePhase.Commit, _scopeRevision, 0, timeoutSeconds, cancellationToken);

        public async UniTask CleanupObsoleteScopeForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
        {
            await RunPhaseForAllAsync(ScopePhase.StopObsolete, _scopeRevision, 0, timeoutSeconds, cancellationToken);
            await RunPhaseForAllAsync(ScopePhase.Cleanup, _scopeRevision, 0, timeoutSeconds, cancellationToken);
        }

        public async UniTask RollbackForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
        {
            await RunPhaseForAllAsync(ScopePhase.StopRollback, _scopeRevision, 0, timeoutSeconds, cancellationToken);
            await RunPhaseForAllAsync(ScopePhase.Rollback, _scopeRevision, 0, timeoutSeconds, cancellationToken);
        }

        public async UniTask ResetForRecoveryForAllClientsAsync(float timeoutSeconds, CancellationToken cancellationToken)
        {
            int revision = NextRevision();
            await RunPhaseForAllAsync(ScopePhase.StopRecovery, revision, 0, timeoutSeconds, cancellationToken);
            await RunPhaseForAllAsync(ScopePhase.Recovery, revision, 0, timeoutSeconds, cancellationToken);
        }

        private int NextRevision()
        {
            EnsureServer();
            if (_phaseRunning) throw new InvalidOperationException("Scope 阶段尚未结束，不能覆盖 Revision");
            return ++_scopeRevision;
        }

        private async UniTask RunPhaseForAllAsync(ScopePhase phase, int revision, NetworkSceneMask targetMask,
            float timeoutSeconds, CancellationToken cancellationToken)
        {
            NetworkManager manager = EnsureServer();
            SceneFlowLocalOperation.ValidateTimeout(timeoutSeconds);
            if (_phaseRunning) throw new InvalidOperationException($"Scope 阶段 {_activePhase} 尚未结束");
            _phaseRunning = true;
            _activePhase = phase;
            _barrier.Begin(manager, revision, $"NetworkScope {phase}");
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token))
            {
                try
                {
                    if (phase == ScopePhase.Rollback || phase == ScopePhase.Recovery)
                    {
                        // Drain local writers before Server Despawn; clients drain before Release.
                        await _localOperation.CancelAndDrainAsync(timeoutSeconds, linked.Token);
                        if (phase == ScopePhase.Recovery) ScopeManager.DespawnSceneScopedRootsForRecovery();
                        else if (_localContext != null) ScopeManager.DespawnPreparedScopeRoots(_localContext);
                    }
                    else if (phase == ScopePhase.Cleanup)
                        ScopeManager.DespawnObsoleteScopeRoots(RequireLocalContext(revision));

                    ExecuteScopePhaseClientRpc(revision, (int)phase, (ulong)targetMask, timeoutSeconds);
                    Exception localFailure = null;
                    // Host participates through ClientRpc exactly once.
                    if (!manager.IsClient)
                    {
                        try { await ExecuteLocalPhaseAsync(revision, phase, targetMask, timeoutSeconds, linked.Token); }
                        catch (Exception exception) { localFailure = exception; }
                    }
                    await _barrier.WaitAsync(manager, timeoutSeconds, linked.Token);
                    if (localFailure != null)
                        throw new InvalidOperationException($"Dedicated Server 本机 {phase} 失败", localFailure);
                }
                catch
                {
                    AbortLocal(revision);
                    if (IsSpawned && manager.IsListening) AbortScopeClientRpc(revision);
                    throw;
                }
                finally { _activePhase = ScopePhase.None; _phaseRunning = false; }
            }
        }

        [ClientRpc]
        private void ExecuteScopePhaseClientRpc(int revision, int phase, ulong targetMask, float timeoutSeconds)
            => ExecuteClientPhaseAsync(revision, (ScopePhase)phase, (NetworkSceneMask)targetMask, timeoutSeconds).Forget();

        private async UniTaskVoid ExecuteClientPhaseAsync(int revision, ScopePhase phase,
            NetworkSceneMask targetMask, float timeoutSeconds)
        {
            if (revision < _localRevision || (revision == _localRevision && phase <= _localPhase)) return;
            bool succeeded = false;
            string error = string.Empty;
            try
            {
                await ExecuteLocalPhaseAsync(revision, phase, targetMask, timeoutSeconds, LifetimeToken);
                succeeded = true;
            }
            catch (Exception exception)
            {
                error = NetworkBarrierState.LimitRpcError(exception.Message);
                Debug.LogError($"[NetworkScopeBarrier] Revision={revision}, {phase} 失败：{exception}");
            }
            if (IsSpawned && IsClient && NetworkManager.IsListening)
                ConfirmScopePhaseServerRpc(revision, (int)phase, succeeded, error);
        }

        private async UniTask ExecuteLocalPhaseAsync(int revision, ScopePhase phase,
            NetworkSceneMask targetMask, float timeoutSeconds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool startsRevision = phase == ScopePhase.Prepare || phase == ScopePhase.StopRecovery;
            if (revision < _localRevision || (!startsRevision && revision != _localRevision))
                throw new InvalidOperationException("Scope Revision 不匹配");
            if (phase == ScopePhase.StopRollback || phase == ScopePhase.Rollback ||
                phase == ScopePhase.StopRecovery || phase == ScopePhase.Recovery)
                await _localOperation.CancelAndDrainAsync(timeoutSeconds, cancellationToken);
            else if (_localAborted && revision == _localRevision)
                throw new OperationCanceledException("本轮 Scope 已取消");
            else if (!startsRevision && (int)phase != (int)_localPhase + 1)
                throw new InvalidOperationException($"Scope 阶段顺序错误：{_localPhase} → {phase}");
            if (startsRevision) { _localRevision = revision; _localAborted = false; }
            _localPhase = phase;
            await _localOperation.RunAsync(async token =>
            {
                switch (phase)
                {
                    case ScopePhase.Prepare:
                        if (_localContext != null) throw new InvalidOperationException("本机仍有未结束的 Scope Context");
                        // Assign before checking cancellation: rollback owns even a just-finished Prepare.
                        _localContext = await ScopeManager.PrepareScopeAsync(targetMask, token);
                        token.ThrowIfCancellationRequested();
                        break;
                    case ScopePhase.RootReady:
                        await SceneFlowLocalOperation.WaitAsync(
                            () => ScopeManager.AreRequiredRootsReady(RequireLocalContext(revision)),
                            timeoutSeconds, "等待本地 NetworkRoot Ready 超时", token);
                        break;
                    case ScopePhase.PreCommitStages:
                        await ScopeManager.RunPreCommitStagesAsync(RequireLocalContext(revision), revision, token);
                        break;
                    case ScopePhase.Commit:
                        ScopeManager.CommitPreparedScope(RequireLocalContext(revision));
                        break;
                    case ScopePhase.StopObsolete:
                        await ScopeManager.ShutdownRootsAsync(RequireLocalContext(revision), false, false, token);
                        break;
                    case ScopePhase.StopRollback:
                        await ScopeManager.ShutdownRootsAsync(_localContext, true, false, token);
                        break;
                    case ScopePhase.StopRecovery:
                        await ScopeManager.ShutdownRootsAsync(_localContext, false, true, token);
                        break;
                    case ScopePhase.Cleanup:
                        NetworkScopePrepareContext cleanup = RequireLocalContext(revision);
                        await SceneFlowLocalOperation.WaitAsync(() => ScopeManager.AreObsoleteRootsDespawned(cleanup),
                            timeoutSeconds, "等待旧 NetworkRoot Despawn 超时", token);
                        ScopeManager.ReleaseObsoleteScopePrefabs(cleanup);
                        // Context survives physical scene cleanup and is released only after Activate.
                        break;
                    case ScopePhase.Rollback:
                        if (_localContext == null) break;
                        NetworkScopePrepareContext rollback = RequireLocalContext(revision);
                        if (rollback.IsCommitted) throw new InvalidOperationException("已 Commit 的 Scope 不支持回滚");
                        await SceneFlowLocalOperation.WaitAsync(() => ScopeManager.ArePreparedScopeRootsDespawned(rollback),
                            timeoutSeconds, "等待回滚 NetworkRoot Despawn 超时", token);
                        ScopeManager.ReleaseRolledBackScopePrefabs(rollback);
                        _localContext = null;
                        break;
                    case ScopePhase.Recovery:
                        _localContext?.Invalidate();
                        await SceneFlowLocalOperation.WaitAsync(ScopeManager.AreSceneScopedRootsDespawned,
                            timeoutSeconds, "等待 Recovery Root Despawn 超时", token);
                        ScopeManager.ReleaseSceneScopedPrefabsForRecovery();
                        _localContext = null;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(phase));
                }
            }, timeoutSeconds, cancellationToken);
        }

        /// <summary>Authoritative release after Cleanup and scene unload. No success ACK.</summary>
        public void ActivateForAllClients()
        {
            NetworkManager manager = EnsureServer();
            if (_phaseRunning) throw new InvalidOperationException("Scope 屏障尚未结束");
            if (_activationDispatchedRevision == _scopeRevision) return;
            _activationDispatchedRevision = _scopeRevision;
            _activationFailure = null;
            if (!manager.IsClient) ActivateLocal(_scopeRevision);
            ActivateScopeClientRpc(_scopeRevision);
            if (_activationFailure != null) throw new InvalidOperationException(_activationFailure);
        }

        [ClientRpc]
        private void ActivateScopeClientRpc(int revision)
        {
            if (revision != _localRevision || _localPhase >= ScopePhase.Activate) return;
            try { ActivateLocal(revision); }
            catch (Exception exception)
            {
                string error = NetworkBarrierState.LimitRpcError(exception.Message);
                Debug.LogError($"[NetworkScopeBarrier] Activate 失败：{exception}");
                if (IsServer) HandleActivationFailure(revision, error);
                else if (IsSpawned && NetworkManager.IsListening) ReportActivationFailureServerRpc(revision, error);
            }
        }

        private void ActivateLocal(int revision)
        {
            if (_localAborted) throw new OperationCanceledException("Scope 已取消");
            if (_localPhase != ScopePhase.Cleanup) throw new InvalidOperationException("Cleanup 后才能 Activate");
            _localPhase = ScopePhase.Activate;
            ScopeManager.ActivatePreparedScope(RequireLocalContext(revision));
            _activatedLocalRevision = revision;
            _localContext = null;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportActivationFailureServerRpc(int revision, string error, ServerRpcParams rpcParams = default)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(rpcParams.Receive.SenderClientId))
                HandleActivationFailure(revision, $"Client {rpcParams.Receive.SenderClientId}: {error}");
        }

        /// <summary>Report an asynchronous Gameplay startup failure; Activate still has no success ACK.</summary>
        public void ReportRuntimeFailure(int revision, string error)
        {
            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening ||
                revision != _localRevision || _localPhase != ScopePhase.Activate || _localAborted) return;
            error = NetworkBarrierState.LimitRpcError(error);
            if (IsServer) HandleActivationFailure(revision, error);
            else ReportActivationFailureServerRpc(revision, error);
        }

        private void HandleActivationFailure(int revision, string error)
        {
            if (revision != _scopeRevision || revision != _activationDispatchedRevision || _activationFailure != null) return;
            _activationFailure = error;
            ActivationFailed?.Invoke(revision, error);
        }

        [ClientRpc]
        private void AbortScopeClientRpc(int revision) => AbortLocal(revision);

        private void AbortLocal(int revision)
        {
            if (revision != _localRevision) return;
            _localAborted = true;
            _localOperation.Cancel();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConfirmScopePhaseServerRpc(int revision, int phase, bool succeeded, string error, ServerRpcParams rpcParams = default)
        {
            if (_phaseRunning && (ScopePhase)phase == _activePhase)
                _barrier.Complete(revision, rpcParams.Receive.SenderClientId, succeeded, error);
        }

        private CancellationToken LifetimeToken => _lifetime?.Token ?? new CancellationToken(true);
        private NetworkScopeManager ScopeManager => NetworkRuntimeBootstrap.Instance.ScopeManager;

        private NetworkManager EnsureServer()
        {
            NetworkManager manager = NetworkManager;
            if (manager == null || !manager.IsServer || !manager.IsListening || !IsSpawned || _lifetime == null)
                throw new InvalidOperationException("NetworkScope 屏障只能由已 Spawn 的 Server/Host 发起");
            if (NetworkRuntimeBootstrap.Instance == null || !NetworkRuntimeBootstrap.Instance.IsInitialized)
                throw new InvalidOperationException("NetworkRuntime 尚未初始化");
            return manager;
        }

        private NetworkScopePrepareContext RequireLocalContext(int revision)
        {
            if (revision != _localRevision || _localContext == null)
                throw new InvalidOperationException($"本机不存在 Revision={revision} 的 Scope Context");
            return _localContext;
        }
    }
}
