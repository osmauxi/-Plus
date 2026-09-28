using System;
using System.Text;
using UnityEngine;

namespace ProjectGame.Bootstrap
{
    [Serializable]
    public sealed class HotUpdateReleaseManifest
    {
        public const int CurrentSchemaVersion = 1;
        public const int DefaultProtocolVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string BaselineVersion = string.Empty;
        public int ContentRevision;
        public int ProtocolVersion = DefaultProtocolVersion;
        public int ContentPort = LanEndpoint.DefaultContentPort;
        public int GamePort = LanEndpoint.DefaultGamePort;
        public string CatalogHash = string.Empty;
        public string BuildTarget = string.Empty;
        public string BuiltAtUtc = string.Empty;

        public static HotUpdateReleaseManifest CreateFallback(LanEndpoint endpoint)
        {
            return new HotUpdateReleaseManifest
            {
                BaselineVersion = Application.version,
                ContentPort = endpoint.ContentPort,
                GamePort = endpoint.GamePort
            };
        }
    }

    public static class LanRuntimeContext
    {
        private static LanEndpoint _endpoint = LanEndpoint.Fallback;
        private static HotUpdateReleaseManifest _manifest =
            HotUpdateReleaseManifest.CreateFallback(LanEndpoint.Fallback);

        public static LanEndpoint Endpoint => _endpoint;
        public static HotUpdateReleaseManifest Manifest => _manifest;

        public static void Configure(
            LanEndpoint endpoint,
            HotUpdateReleaseManifest manifest)
        {
            _endpoint = endpoint;
            _manifest = manifest ?? HotUpdateReleaseManifest.CreateFallback(endpoint);
        }
    }

    [Serializable]
    public sealed class LanConnectionPayload
    {
        public string PlayerId = string.Empty;
        public string BaselineVersion = string.Empty;
        public int ContentRevision;
        public int ProtocolVersion;

        public static byte[] Create(string playerId)
        {
            HotUpdateReleaseManifest release = LanRuntimeContext.Manifest;
            var payload = new LanConnectionPayload
            {
                PlayerId = playerId,
                BaselineVersion = release.BaselineVersion,
                ContentRevision = release.ContentRevision,
                ProtocolVersion = release.ProtocolVersion
            };
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        }

        public static bool TryParse(byte[] bytes, out LanConnectionPayload payload)
        {
            payload = null;
            if (bytes == null || bytes.Length == 0)
                return false;

            try
            {
                payload = JsonUtility.FromJson<LanConnectionPayload>(
                    Encoding.UTF8.GetString(bytes));
                return payload != null && !string.IsNullOrWhiteSpace(payload.PlayerId);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool IsCompatibleWith(
            HotUpdateReleaseManifest release,
            out string reason)
        {
            if (ProtocolVersion != release.ProtocolVersion)
            {
                reason = "客户端与房主的联机协议版本不同";
                return false;
            }

            if (!string.Equals(
                    BaselineVersion,
                    release.BaselineVersion,
                    StringComparison.Ordinal) ||
                ContentRevision != release.ContentRevision)
            {
                reason = "客户端资源或代码版本与房主不同，请完成更新后重试";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }

    public static class LanDiscoveryProtocol
    {
        public const int Port = 64481;
        public const string Request = "PLUS_LAN_DISCOVERY_V1";
        public const string Response = "PLUS_LAN_SERVER_V1";
    }

    [Serializable]
    public sealed class LanDiscoveryResponse
    {
        public string Magic = LanDiscoveryProtocol.Response;
        public string Scheme = "http";
        public int ContentPort = LanEndpoint.DefaultContentPort;
        public int GamePort = LanEndpoint.DefaultGamePort;
        public string BaselineVersion = string.Empty;
        public int ContentRevision;
        public int ProtocolVersion = HotUpdateReleaseManifest.DefaultProtocolVersion;

        public HotUpdateReleaseManifest ToManifest()
        {
            return new HotUpdateReleaseManifest
            {
                BaselineVersion = BaselineVersion,
                ContentRevision = ContentRevision,
                ProtocolVersion = ProtocolVersion,
                ContentPort = ContentPort,
                GamePort = GamePort
            };
        }
    }
}
