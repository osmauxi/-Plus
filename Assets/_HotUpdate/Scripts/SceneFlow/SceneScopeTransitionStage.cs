using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using ProjectGame.HotFix.Network.Runtime;

namespace ProjectGame.HotFix.SceneFlow
{
    public readonly struct SceneScopeTransitionContext
    {
        public NetworkSceneMask PreviousMask { get; }
        public NetworkSceneMask TargetMask { get; }
        public int Revision { get; }

        public SceneScopeTransitionContext(NetworkSceneMask previousMask,NetworkSceneMask targetMask,int revision)
        {
            PreviousMask = previousMask;
            TargetMask = targetMask;
            Revision = revision;
        }
    }

    /// <summary>
    /// 旧 Inspector 扩展点，保留类型以兼容尚未迁移的资源。
    /// 当前管线使用 Network.Runtime 的三个可选接口，在 Root Spawn 时扫描。
    /// </summary>
    public abstract class SceneScopeTransitionStage : MonoBehaviour
    {
        public virtual UniTask BindAsync(SceneScopeTransitionContext context,CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask InitializeAsync(SceneScopeTransitionContext context,CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask ActivateAsync(SceneScopeTransitionContext context,CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
