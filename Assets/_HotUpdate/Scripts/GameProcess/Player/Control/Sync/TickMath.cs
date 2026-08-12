namespace ProjectGame.HotFix.Gameplay.Player.Sync
{
    /// <summary>
    /// 网络模拟 Tick 比较工具。
    /// 使用 uint 自然溢出，要求两次比较的 Tick 距离不超过 int.MaxValue。
    /// </summary>
    public static class TickMath
    {
        public static bool IsNewer(uint a, uint b) => unchecked((int)(a - b)) > 0;
        public static bool IsOlder(uint a, uint b) => unchecked((int)(a - b)) < 0;
        public static bool IsNewerOrEqual(uint a, uint b) => a == b || IsNewer(a, b);
        public static bool IsOlderOrEqual(uint a, uint b) => a == b || IsOlder(a, b);

        public static uint Distance(uint newer, uint older) => unchecked(newer - older);
    }
}