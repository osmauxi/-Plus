using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Stamina
{
    [Serializable]
    public sealed class PlayerStaminaConfig
    {
        [Header("基础体力")]
        [Tooltip("玩家可持有的最大体力值。调大：可连续冲刺更久，同时从耗尽状态恢复所需的绝对体力更多；调小：连续冲刺时间更短、体力循环更频繁。")]
        [InspectorName("最大体力")]
        [SerializeField, Min(1f)] private float _maxStamina = 100f;

        [Header("冲刺消耗")]
        [Tooltip("冲刺状态下每秒消耗的体力。调大：体力消耗更快、可持续冲刺时间更短；调小：体力消耗更慢、可持续冲刺时间更长。")]
        [InspectorName("每秒冲刺消耗")]
        [SerializeField, Min(0f)] private float _sprintDrainPerSecond = 18f;

        [Header("体力恢复")]
        [Tooltip("恢复阶段每秒补充的体力。调大：体力恢复更快；调小：恢复更慢、再次冲刺需要等待更久。")]
        [InspectorName("每秒恢复量")]
        [SerializeField, Min(0f)] private float _recoveryPerSecond = 25f;

        [Tooltip("停止消耗体力后，开始恢复前需要等待的秒数。调大：恢复启动更晚；调小：恢复启动更早。")]
        [InspectorName("恢复延迟")]
        [SerializeField, Min(0f)] private float _recoveryDelay = 0.8f;

        [Tooltip("体力耗尽后，至少恢复到最大体力的该比例才解除耗尽状态。调大：耗尽惩罚更强、需要恢复更久才能再次冲刺；调小：更早解除耗尽并允许再次冲刺。")]
        [InspectorName("耗尽解除比例")]
        [SerializeField, Range(0f, 1f)] private float _exhaustedRecoveryRatio = 0.2f;

        public float MaxStamina => _maxStamina;
        public float SprintDrainPerSecond => _sprintDrainPerSecond;
        public float RecoveryPerSecond => _recoveryPerSecond;
        public float RecoveryDelay => _recoveryDelay;
        public float ExhaustedRecoveryRatio => _exhaustedRecoveryRatio;

        public void Validate()
        {
            if (_maxStamina <= 0f)
                throw new InvalidOperationException("最大体力必须大于 0。");
        }
    }
}
