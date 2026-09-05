using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Gameplay.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public sealed class BootstrapServiceProbe : MonoBehaviour, IGameRuntimeService
    {
        public static readonly List<string> Calls = new();
        public static bool FailSecond;
        public static bool HoldFirst;
        public static bool StoppedWhileSpawned;
        public string ServiceName;
        public bool IsInitialized { get; private set; }
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            Calls.Add(ServiceName + ":Initialize");
            if (ServiceName == "A" && HoldFirst)
                await UniTask.WaitUntil(() => !HoldFirst, cancellationToken: cancellationToken);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            if (ServiceName == "B" && FailSecond) throw new InvalidOperationException("probe B failure");
            IsInitialized = true;
        }
        public async UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            StoppedWhileSpawned = GetComponentInParent<NetworkObject>().IsSpawned;
            Calls.Add(ServiceName + ":Shutdown");
            IsInitialized = false;
        }
    }
}
