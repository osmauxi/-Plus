using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Network.Runtime;
using UnityEngine;

namespace ProjectGame.HotFix.Tests.SceneFlow
{
    public sealed class ScopeBindOnlyProbe : MonoBehaviour, IScopeBindable
    {
        public UniTask BindAsync(NetworkScopeStageContext context, CancellationToken cancellationToken)
        {
            ScopeLifecycleProbe.Calls.Add("BindOnly");
            return UniTask.CompletedTask;
        }
    }
}
