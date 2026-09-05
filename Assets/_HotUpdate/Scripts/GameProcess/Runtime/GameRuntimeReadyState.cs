using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Runtime
{
    /// <summary>A fixed gameplay participant set. A lost participant invalidates the prepared session.</summary>
    internal sealed class GameRuntimeReadyState
    {
        private readonly HashSet<ulong> _participants = new();
        private readonly HashSet<ulong> _pending = new();
        private readonly Dictionary<ulong, string> _failures = new();
        private int _revision;

        public void Begin(IEnumerable<ulong> participants, int revision)
        {
            _revision = revision;
            _participants.Clear();
            _pending.Clear();
            _failures.Clear();
            foreach (ulong id in participants) { _participants.Add(id); _pending.Add(id); }
        }

        public void Complete(int revision, ulong clientId, bool succeeded, string error)
        {
            if (revision != _revision || !_pending.Remove(clientId)) return;
            if (!succeeded) _failures.Add(clientId, LimitError(error));
        }

        public async UniTask WaitAsync(NetworkManager manager, float timeoutSeconds, string operation,
            CancellationToken cancellationToken)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + timeoutSeconds;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (manager == null || !manager.IsServer || !manager.IsListening)
                    throw new OperationCanceledException($"{operation}: Server 已停止");
                foreach (ulong id in _participants)
                    if (!manager.ConnectedClients.ContainsKey(id))
                        throw new InvalidOperationException($"{operation}: 参与玩家 {id} 已断线");
                if (_failures.Count > 0)
                    throw new InvalidOperationException($"{operation}: " +
                        string.Join("; ", _failures.Select(pair => $"Client {pair.Key}: {pair.Value}")));
                if (_pending.Count == 0) return;
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException($"{operation} 超时，未完成 Client=[{string.Join(",", _pending)}]");
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        public static string LimitError(string error)
        {
            if (string.IsNullOrWhiteSpace(error)) return "未知初始化错误";
            return error.Length <= 512 ? error : error.Substring(0, 512);
        }
    }
}
