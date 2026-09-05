using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 超时后不能假装操作已经结束，要进行管理
    /// 泛用性场景转换管线的调用和管理方法
    /// </summary>
    internal sealed class SceneFlowLocalOperation
    {
        private Execution _current;
        /// <summary>
        /// 本地事务状态
        /// </summary>
        private sealed class Execution
        {
            public readonly CancellationTokenSource Cancellation = new CancellationTokenSource();
            public bool Completed;
            public Exception Error;
        }

        public bool IsRunning => _current != null && !_current.Completed;

        public async UniTask RunAsync(Func<CancellationToken, UniTask> operation,float timeoutSeconds, CancellationToken cancellationToken)
        {
            ValidateTimeout(timeoutSeconds);
            cancellationToken.ThrowIfCancellationRequested();
            //只允许一个受理的场景切换操作
            if (IsRunning)
                throw new InvalidOperationException("本机上一个 SceneFlow 操作尚未退出");

            var execution = new Execution();
            _current = execution;
            ExecuteAsync(execution, operation).Forget();
            try
            {
                await WaitAsync(() => execution.Completed, timeoutSeconds,
                    "本机 SceneFlow 操作超时", cancellationToken);
                if (execution.Error != null)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(execution.Error).Throw();
            }
            finally
            {
                if (!execution.Completed)
                    execution.Cancellation.Cancel();
            }
        }

        public void Cancel()
        {
            if (IsRunning) _current.Cancellation.Cancel();
        }
        /// <summary>
        /// 强制清理旧操作
        /// </summary>
        public async UniTask CancelAndDrainAsync(float timeoutSeconds, CancellationToken cancellationToken)
        {
            Execution execution = _current;
            if (execution == null || execution.Completed) return;
            execution.Cancellation.Cancel();
            await WaitAsync(() => execution.Completed, timeoutSeconds,
                "取消后的 SceneFlow 操作未退出，禁止并发回滚", cancellationToken);
        }

        private static async UniTaskVoid ExecuteAsync(Execution execution,Func<CancellationToken, UniTask> operation)
        {
            try
            {
                await operation(execution.Cancellation.Token);
                execution.Cancellation.Token.ThrowIfCancellationRequested();
            }
            catch (Exception exception) { execution.Error = exception; }
            finally
            {
                execution.Completed = true;
                execution.Cancellation.Dispose();
            }
        }
        /// <summary>
        /// 通用轮询方法
        /// </summary>
        internal static async UniTask WaitAsync(Func<bool> condition, float timeoutSeconds,string message, CancellationToken cancellationToken)
        {
            ValidateTimeout(timeoutSeconds);
            double deadline = Time.realtimeSinceStartupAsDouble + timeoutSeconds;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (condition()) 
                    return;
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException(message);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        internal static void ValidateTimeout(float timeoutSeconds)
        {
            if (float.IsNaN(timeoutSeconds) || float.IsInfinity(timeoutSeconds) || timeoutSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
        }
    }
}
