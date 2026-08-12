namespace ProjectGame.HotFix.Core.NetworkEvents
{
    public sealed class NetEventBusConfig
    {
        public string RequestMessageName { get; set; } = "ProjectGame.NetEvent.Request";
        public string BroadcastMessageName { get; set; } = "ProjectGame.NetEvent.Broadcast";

        public int InitialWriterCapacity { get; set; } = 256;
        public int MaxWriterCapacity { get; set; } = 4096;

        public bool InvokeHostRequestLocally { get; set; } = true;
        public bool InvokeHostBroadcastLocally { get; set; } = true;
    }
}