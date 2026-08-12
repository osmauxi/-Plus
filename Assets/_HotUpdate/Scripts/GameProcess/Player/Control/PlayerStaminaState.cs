using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.Stamina
{
    /// <summary>
    /// 可同步的玩家体力状态。
    /// </summary>
    public struct PlayerStaminaState
    {
        public float Current;
        public float RecoveryDelayRemaining;
        public bool IsExhausted;

        public float Normalized(float maxStamina)
        {
            return maxStamina <= 0f ? 0f : Mathf.Clamp01(Current / maxStamina);
        }
    }

    /// <summary>
    /// 纯体力计算逻辑。
    /// Sprint、Dash 等系统只描述自己的消耗方式，
    /// 不直接拥有体力值。
    /// </summary>
    public sealed class PlayerStaminaLogic
    {
        private readonly PlayerStaminaConfig _config;

        public PlayerStaminaLogic(PlayerStaminaConfig config)
        {
            _config = config;
            _config.Validate();
        }

        public PlayerStaminaState CreateInitialState()
        {
            return new PlayerStaminaState
            {
                Current = _config.MaxStamina,
                RecoveryDelayRemaining = 0f,
                IsExhausted = false,
            };
        }

        public bool CanSprint(in PlayerStaminaState state)
        {
            return !state.IsExhausted && state.Current > 0f;
        }

        /// <summary>
        /// 每 Tick 更新体力。
        /// 当前只有 Sprint 持续消耗，未来 Dash 使用 TrySpend 即可。
        /// </summary>
        public void Tick(ref PlayerStaminaState state, bool isSprinting, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            if (isSprinting)
            {
                Drain(ref state, _config.SprintDrainPerSecond * deltaTime);
                return;
            }

            Recover(ref state, deltaTime);
        }

        /// <summary>
        /// 一次性消耗。
        /// 以后 Dash / Skill 等特殊动作直接复用。
        /// </summary>
        public bool TrySpend(ref PlayerStaminaState state, float amount)
        {
            if (amount <= 0f)
                return true;

            if (state.IsExhausted || state.Current < amount)
                return false;

            Drain(ref state, amount);
            return true;
        }

        private void Drain(ref PlayerStaminaState state, float amount)
        {
            state.Current = Mathf.Max(0f, state.Current - amount);
            state.RecoveryDelayRemaining = _config.RecoveryDelay;

            if (state.Current <= 0f)
                state.IsExhausted = true;
        }

        private void Recover(ref PlayerStaminaState state, float deltaTime)
        {
            if (state.RecoveryDelayRemaining > 0f)
            {
                state.RecoveryDelayRemaining = Mathf.Max(0f, state.RecoveryDelayRemaining - deltaTime);
                return;
            }

            if (state.Current < _config.MaxStamina)
                state.Current = Mathf.Min(_config.MaxStamina, state.Current + _config.RecoveryPerSecond * deltaTime);

            if (!state.IsExhausted)
                return;

            float recoveryThreshold = _config.MaxStamina * _config.ExhaustedRecoveryRatio;

            if (state.Current >= recoveryThreshold)
                state.IsExhausted = false;
        }
    }
}