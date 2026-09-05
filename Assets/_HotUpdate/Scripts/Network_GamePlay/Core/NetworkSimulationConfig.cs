using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Network
{
    /// <summary>
    /// 整个 Gameplay 网络会话共享的最小模拟配置。
    /// 只有已经确认属于所有网络 Gameplay 系统的参数才应放在这里。
    /// </summary>
    [Serializable]
    public sealed class NetworkSimulationConfig
    {
        [Tooltip("Gameplay 网络模拟每秒推进的 Tick 数，必须与 NGO TickRate 一致。")]
        [InspectorName("网络模拟 Tick 率")]
        [SerializeField, Range(20, 120)] private int _tickRate = 30;

        public int TickRate => _tickRate;

        public NetworkSimulationConfig()
        {
        }

        public NetworkSimulationConfig(int tickRate)
        {
            _tickRate = tickRate;
            Validate();
        }

        public void Validate()
        {
            if (_tickRate < 20 || _tickRate > 120)
            {
                throw new InvalidOperationException(
                    $"Gameplay 网络 TickRate 必须处于 20~120，当前值为 {_tickRate}。");
            }
        }
    }
}
