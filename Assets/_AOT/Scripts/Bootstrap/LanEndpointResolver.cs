using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectGame.Bootstrap
{
    public static class LanEndpointResolver
    {
        private const int Attempts = 3;
        private const int AttemptTimeoutMilliseconds = 700;

        public static async UniTask<LanResolution> ResolveAsync()
        {
            byte[] request = Encoding.UTF8.GetBytes(LanDiscoveryProtocol.Request);
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                LanResolution? resolution = await DiscoverOnceAsync(request);
                if (resolution.HasValue)
                    return resolution.Value;
            }

            LanEndpoint fallback = LanEndpoint.Fallback;
            Debug.LogWarning(
                $"[Bootstrap] 未收到局域网服务器广播响应，回退到 {fallback}");
            return new LanResolution(
                fallback,
                HotUpdateReleaseManifest.CreateFallback(fallback),
                false);
        }

        private static async Task<LanResolution?> DiscoverOnceAsync(byte[] request)
        {
            using var client = new UdpClient(0) { EnableBroadcast = true };
            try
            {
                await client.SendAsync(
                    request,
                    request.Length,
                    new IPEndPoint(
                        IPAddress.Broadcast,
                        LanDiscoveryProtocol.Port));

                Task<UdpReceiveResult> receiveTask = client.ReceiveAsync();
                Task completed = await Task.WhenAny(
                    receiveTask,
                    Task.Delay(AttemptTimeoutMilliseconds));
                if (completed != receiveTask)
                    return null;

                UdpReceiveResult packet = await receiveTask;
                LanDiscoveryResponse response = JsonUtility.FromJson<LanDiscoveryResponse>(
                    Encoding.UTF8.GetString(packet.Buffer));
                if (response == null || response.Magic != LanDiscoveryProtocol.Response)
                    return null;

                var endpoint = new LanEndpoint(
                    string.IsNullOrWhiteSpace(response.Scheme)
                        ? "http"
                        : response.Scheme,
                    packet.RemoteEndPoint.Address.ToString(),
                    response.ContentPort,
                    response.GamePort);
                return new LanResolution(endpoint, response.ToManifest(), true);
            }
            catch (SocketException)
            {
                return null;
            }
        }
    }

    public readonly struct LanResolution
    {
        public LanResolution(
            LanEndpoint endpoint,
            HotUpdateReleaseManifest manifest,
            bool discovered)
        {
            Endpoint = endpoint;
            Manifest = manifest;
            Discovered = discovered;
        }

        public LanEndpoint Endpoint { get; }
        public HotUpdateReleaseManifest Manifest { get; }
        public bool Discovered { get; }
    }
}
