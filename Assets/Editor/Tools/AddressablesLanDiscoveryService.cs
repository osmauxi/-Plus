using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProjectGame.Bootstrap;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

[InitializeOnLoad]
public static class AddressablesLanDiscoveryService
{
    private static UdpClient _listener;
    private static Task _listenerTask;

    static AddressablesLanDiscoveryService()
    {
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.quitting += Stop;
    }

    public static bool IsRunning => _listener != null;
    public static string LastError { get; private set; } = string.Empty;

    public static bool Start(int contentPort, int gamePort, bool useHttps)
    {
        Stop();
        try
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string remoteBuildDirectory =
                settings.RemoteCatalogBuildPath.GetValue(settings);
            string manifestPath = Path.Combine(
                remoteBuildDirectory,
                "release-manifest.json");
            HotUpdateReleaseManifest manifest = File.Exists(manifestPath)
                ? JsonUtility.FromJson<HotUpdateReleaseManifest>(
                    File.ReadAllText(manifestPath))
                : HotUpdateReleaseManifest.CreateFallback(LanEndpoint.Fallback);
            var response = new LanDiscoveryResponse
            {
                Scheme = useHttps ? "https" : "http",
                ContentPort = contentPort,
                GamePort = gamePort,
                BaselineVersion = manifest.BaselineVersion,
                ContentRevision = manifest.ContentRevision,
                ProtocolVersion = manifest.ProtocolVersion
            };
            _listener = new UdpClient(new IPEndPoint(
                IPAddress.Any,
                LanDiscoveryProtocol.Port));
            _listener.EnableBroadcast = true;
            UdpClient activeListener = _listener;
            _listenerTask = Task.Run(() => Listen(
                activeListener,
                remoteBuildDirectory,
                response));
            LastError = string.Empty;
            Debug.Log(
                $"[LAN Discovery] 已启动 UDP {LanDiscoveryProtocol.Port}，" +
                $"发布 {response.BaselineVersion}.{response.ContentRevision}，" +
                $"内容端口 {contentPort}，游戏端口 {gamePort}");
            return true;
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            Stop();
            Debug.LogException(exception);
            return false;
        }
    }

    public static void Stop()
    {
        UdpClient listener = _listener;
        _listener = null;
        listener?.Close();
        listener?.Dispose();
        _listenerTask = null;
    }

    private static void Listen(
        UdpClient listener,
        string remoteBuildDirectory,
        LanDiscoveryResponse fallbackResponse)
    {
        try
        {
            while (ReferenceEquals(_listener, listener))
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] request = listener.Receive(ref sender);
                if (Encoding.UTF8.GetString(request) != LanDiscoveryProtocol.Request)
                    continue;

                byte[] response = Encoding.UTF8.GetBytes(
                    BuildResponseJson(remoteBuildDirectory, fallbackResponse));
                listener.Send(response, response.Length, sender);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException exception)
        {
            if (ReferenceEquals(_listener, listener))
                LastError = exception.Message;
        }
    }

    private static string BuildResponseJson(
        string remoteBuildDirectory,
        LanDiscoveryResponse fallbackResponse)
    {
        string manifestPath = Path.Combine(
            remoteBuildDirectory,
            "release-manifest.json");
        HotUpdateReleaseManifest manifest = File.Exists(manifestPath)
            ? JsonUtility.FromJson<HotUpdateReleaseManifest>(
                File.ReadAllText(manifestPath))
            : fallbackResponse.ToManifest();
        return JsonUtility.ToJson(new LanDiscoveryResponse
        {
            Scheme = fallbackResponse.Scheme,
            ContentPort = fallbackResponse.ContentPort,
            GamePort = fallbackResponse.GamePort,
            BaselineVersion = manifest.BaselineVersion,
            ContentRevision = manifest.ContentRevision,
            ProtocolVersion = manifest.ProtocolVersion
        });
    }

}
