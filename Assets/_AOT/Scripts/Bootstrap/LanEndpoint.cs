namespace ProjectGame.Bootstrap
{
    /// <summary>
    /// 一组局域网资源与游戏服务器地址。
    /// </summary>
    public readonly struct LanEndpoint
    {
        public const int DefaultContentPort = 64482;
        public const int DefaultGamePort = 7777;

        public static readonly LanEndpoint Fallback = new LanEndpoint(
            "http",
            "192.168.137.1",
            DefaultContentPort,
            DefaultGamePort);

        public readonly string Scheme;
        public readonly string Host;
        public readonly int ContentPort;
        public readonly int GamePort;

        public LanEndpoint(
            string scheme,
            string host,
            int contentPort,
            int gamePort)
        {
            Scheme = scheme;
            Host = host;
            ContentPort = contentPort;
            GamePort = gamePort;
        }

        public string ContentOrigin => $"{Scheme}://{Host}:{ContentPort}";

        public override string ToString()
        {
            return $"Content={ContentOrigin}, Game={Host}:{GamePort}";
        }
    }
}
