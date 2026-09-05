using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Network.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public sealed class ScopeLifecycleProbe : MonoBehaviour, IScopeBindable, IScopeInitializable, IScopeActivatable
    {
        public NetworkPrefabId Id;
        public static readonly List<string> Calls = new List<string>();
        public static string Failure;
        public static bool ActivatedBeforeCleanup;
        public UniTask BindAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            Calls.Add(Id + ":Bind");
            if (!context.TryGetRoot(NetworkPrefabId.NetworkSessionRoot, out _))
                throw new InvalidOperationException("Bind could not resolve session Root");
            if (Id == NetworkPrefabId.GameRuntimeNetworkRoot && Failure == "Bind")
                throw new InvalidOperationException("Injected Bind failure");
            return UniTask.CompletedTask;
        }
        public async UniTask InitializeAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            Calls.Add(Id + ":Initialize");
            if (Id == NetworkPrefabId.GameRuntimeNetworkRoot && Failure == "Initialize")
                throw new InvalidOperationException("Injected Initialize failure");
            if (Id == NetworkPrefabId.GameRuntimeNetworkRoot && Failure == "Timeout")
                await UniTask.WaitUntil(() => false, cancellationToken: cancellationToken);
        }
        public void Activate(NetworkScopeStageContext context)
        {
            Calls.Add(Id + ":Activate");
            if (Id != NetworkPrefabId.GameRuntimeNetworkRoot) return;
            Scene oldScene = SceneManager.GetSceneByPath("Assets/_HotUpdate/Tests/SceneFlow/Fixtures/PipelineLobby.unity");
            ActivatedBeforeCleanup |= oldScene.IsValid() && oldScene.isLoaded;
            if (Failure == "Activate") throw new InvalidOperationException("Injected Activate failure");
        }
    }
}
