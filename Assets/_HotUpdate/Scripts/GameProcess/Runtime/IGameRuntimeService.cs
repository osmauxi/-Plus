using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectGame.HotFix.Gameplay.Runtime
{
    /// <summary>
    /// GameRuntimeScene 内核心服务的统一生命周期接口 
    /// 所有提供服务性质的总站脚本都实现这个接口 
    /// </summary>
    public interface IGameRuntimeService
    {
        bool IsInitialized { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken);

        UniTask ShutdownAsync(CancellationToken cancellationToken);
    }
}