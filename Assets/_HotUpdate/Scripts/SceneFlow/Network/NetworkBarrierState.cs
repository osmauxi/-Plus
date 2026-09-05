using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// Server发起的ACK状态的等待器，记录这轮操作还在等谁、谁失败了，
    /// 并一直等到所有参与Client都落定，或者超时/取消
    /// </summary>
    internal sealed class NetworkBarrierState
    {
        //还没执行完的ClientId列表，也就是需要等待的ClientId列表
        private readonly HashSet<ulong> _pendingClients = new HashSet<ulong>();
        //失败的ClientId列表，包含错误信息
        private readonly Dictionary<ulong, string> _failures = new Dictionary<ulong, string>();

        public int Revision { get; private set; }
        //当前操作的名称，主要用于日志输出
        public string Operation { get; private set; }
        private double _startedAt;

        /// <summary>
        /// 开启一轮新的操作，清空之前的状态，记录当前Revision和Operation
        /// </summary>
        public void Begin(NetworkManager networkManager,int revision,string operation)
        {
            Begin(networkManager.ConnectedClientsIds, revision, operation);
        }

        internal void Begin(IEnumerable<ulong> clientIds, int revision, string operation)
        {
            Revision = revision;
            Operation = operation;
            _startedAt = Time.realtimeSinceStartupAsDouble;
            _pendingClients.Clear();
            _failures.Clear();
            //这里塞的是ConnectedClientsIds，这其中不包含Server自己，所以纯Server不需要等待自己的ACK
            //Host会包含Server与Client，所以Host会等待自己的ACK
            foreach (ulong clientId in clientIds)
                _pendingClients.Add(clientId);
        }

        public void Complete(int revision,ulong clientId,bool succeeded,string error)
        {
            //这里!_pendingClients.Remove(clientId)在判断的同时就已经把当前Client从等待队列删掉了
            if (revision != Revision || !_pendingClients.Remove(clientId))
                return;

            if (!succeeded)
            {
                _failures[clientId] = string.IsNullOrWhiteSpace(error)
                    ? "未知错误"
                    : LimitRpcError(error);
            }
        }
        /// <summary>
        /// 服务器等待所有客户端完成操作，或超时/失败
        /// </summary>
        public async UniTask WaitAsync(NetworkManager networkManager,float timeoutSeconds,CancellationToken cancellationToken)
        {
            SceneFlowLocalOperation.ValidateTimeout(timeoutSeconds);
            double deadline = _startedAt + timeoutSeconds;

            //即使已有失败，也等待其余客户端落定，避免Rollback与尚未结束的Prepare/Load并发
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer)
                    throw new OperationCanceledException($"等待 {Operation} 时 Server 已停止");
                RemoveDisconnectedClients(networkManager);
                if (_pendingClients.Count == 0) break;
                //realtimeSinceStartupAsDouble不会受Time.timeScale影响，适合用于超时检测
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                {
                    throw new TimeoutException(
                        $"等待 {Operation} 超时；未完成 ClientId：" +
                        string.Join(",", _pendingClients) +
                        FormatFailures());
                }
                //这里是每帧检测
                await UniTask.Yield(PlayerLoopTiming.Update,cancellationToken);
            }

            if (_failures.Count > 0)
            {
                throw new InvalidOperationException($"{Operation} 失败" + FormatFailures());
            }
        }
        /// <summary>
        /// 限制RPC不要把过长的错误信息传给Server
        /// </summary>
        public static string LimitRpcError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "未知错误";

            const int maxLength = 512;
            return message.Length <= maxLength ? message : message.Substring(0, maxLength);
        }

        private void RemoveDisconnectedClients(NetworkManager networkManager)
        {
            var connected = new HashSet<ulong>(networkManager.ConnectedClientsIds);
            _pendingClients.RemoveWhere(clientId => !connected.Contains(clientId));
        }

        private string FormatFailures()
        {
            if (_failures.Count == 0)
                return string.Empty;

            return "；" + string.Join("；",_failures.Select(pair => $"Client {pair.Key}: {pair.Value}"));
        }
    }
}
