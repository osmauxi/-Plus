using System;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player.State
{
    /// <summary>
    /// 轻量数据驱动 HFSM：只根据输入事实和可回滚运行状态求出 Life / Reaction / Combat / Locomotion 
    /// 不持有隐藏状态，也不调用 Animator、网络或输入设备 
    /// </summary>
    public sealed class PlayerStateMachine
    {
        // 将设计秒数转换为固定 Tick；状态机自身不读取 Time.time 或 Animator 时间 
        private readonly PlayerActionConfig _config;

        /// <summary>注入动作时间规则并立即校验，避免模拟开始后出现零时长状态 </summary>
        public PlayerStateMachine(PlayerActionConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _config.Validate();
        }

        /// <summary>
        /// 推进一个固定 Tick，并按 Dead → Hit → Reload → Fire → Locomotion 的优先级解析四个状态分支 
        /// controlState 和 actionState 都由调用方显式传入，确保预测回滚恢复后不会残留对象内部的隐藏状态 
        /// deltaTime 必须是固定模拟步长；canSprint 已包含体力耗尽等外部规则 
        /// </summary>
        public void Simulate(
            ref PlayerControlState controlState,
            ref PlayerActionRuntimeState actionState,
            in PlayerStateInput input,
            bool canSprint,
            float deltaTime)
        {
            // Reload 是渲染帧边沿，使用累计序号跨越多个网络 Tick 可靠传递 
            // 先记录已消费值，即使玩家已死亡也不会在复活后补执行死亡期间按下的旧请求 
            bool hasNewReloadRequest = input.ReloadRequestSequence != actionState.LastReloadRequestSequence;

            if (hasNewReloadRequest)
                actionState.LastReloadRequestSequence = input.ReloadRequestSequence;

            // 顶层生命状态具有最高优先级：Dead 不允许任何下层动作继续计时或恢复 
            if (controlState.IsDead)
            {
                ClearInterruptibleActions(ref controlState, ref actionState);
                return;
            }

            // 冷却先递减再判定射击，因此 cooldown=1 的当前 Tick 会降到 0 并允许产生下一发 
            if (actionState.FireCooldownTicks > 0)
                actionState.FireCooldownTicks--;

            // Reaction 高于 Combat：受击会取消换弹，并阻止本 Tick 继续进入射击与移动解析 
            if (actionState.HitTicksRemaining > 0)
            {
                controlState.ReactionMode = PlayerReactionMode.HitReaction;
                controlState.CombatMode = PlayerCombatMode.Ready;
                actionState.ReloadTicksRemaining = 0;
                actionState.HitTicksRemaining--;
                return;
            }

            controlState.ReactionMode = PlayerReactionMode.Normal;

            // 已经进入 Reloading 时优先继续消耗剩余 Tick，不能被按住 Fire 抢占 
            if (controlState.CombatMode == PlayerCombatMode.Reloading)
            {
                if (actionState.ReloadTicksRemaining > 0)
                {
                    actionState.ReloadTicksRemaining--;
                    ResolveLocomotion(ref controlState, input, false);
                    return;
                }

                controlState.CombatMode = PlayerCombatMode.Ready;
            }

            if (hasNewReloadRequest)
            {
                controlState.CombatMode = PlayerCombatMode.Reloading;
                ushort reloadTicks = _config.ResolveReloadTicks(deltaTime);
                // 当前 Tick 已经处于 Reloading，因此只保存后续仍需占用的 Tick，避免总时长多一 Tick 
                actionState.ReloadTicksRemaining = (ushort)(reloadTicks - 1);
                ResolveLocomotion(ref controlState, input, false);
                return;
            }

            if (input.FireHeld)
            {
                controlState.CombatMode = PlayerCombatMode.Firing;

                if (actionState.FireCooldownTicks == 0)
                {
                    // Sequence 允许 uint 自然回绕；消费者必须使用 TickMath.IsNewer，而不能直接用 > 比较 
                    actionState.ShotSequence = unchecked(actionState.ShotSequence + 1u);
                    actionState.FireCooldownTicks = _config.ResolveFireIntervalTicks(deltaTime);
                }
            }
            else
            {
                controlState.CombatMode = PlayerCombatMode.Ready;
            }

            ResolveLocomotion(ref controlState, input, canSprint);
        }

        /// <summary>
        /// 切换顶层生命状态 进入 Dead 会立即清理可中断动作；重新 Alive 从 Normal/Ready/Free 开始 
        /// ShotSequence、HitSequence 与已消费 Reload 序号不会清零，避免表现层把旧事件重新播放 
        /// </summary>
        public void SetLifeState(
            ref PlayerControlState controlState,
            ref PlayerActionRuntimeState actionState,
            PlayerLifeState lifeState)
        {
            if (controlState.LifeState == lifeState)
                return;

            controlState.LifeState = lifeState;

            if (lifeState == PlayerLifeState.Dead)
            {
                controlState.LocomotionMode = PlayerLocomotionMode.Free;
                ClearInterruptibleActions(ref controlState, ref actionState);
                return;
            }

            controlState.ReactionMode = PlayerReactionMode.Normal;
            controlState.CombatMode = PlayerCombatMode.Ready;
            controlState.LocomotionMode = PlayerLocomotionMode.Free;
        }

        /// <summary>
        /// 应用一次已由权威 Gameplay 判定成立的受击事件 
        /// 重复调用会递增 HitSequence 并刷新持续 Tick；Dead 时拒绝，不产生表现事件 
        /// </summary>
        public bool ApplyHit(
            ref PlayerControlState controlState,
            ref PlayerActionRuntimeState actionState,
            float deltaTime)
        {
            if (controlState.IsDead)
                return false;

            actionState.HitSequence = unchecked(actionState.HitSequence + 1u);
            actionState.HitTicksRemaining = _config.ResolveHitTicks(deltaTime);
            actionState.ReloadTicksRemaining = 0;
            controlState.ReactionMode = PlayerReactionMode.HitReaction;
            controlState.CombatMode = PlayerCombatMode.Ready;
            return true;
        }

        /// <summary>
        /// 解析移动分支 Aim 优先于 Sprint，使按住瞄准时不会因 SprintHeld 同时存在而进入冲刺 
        /// canSprint 由体力规则提供；Reloading 调用时会显式传 false 
        /// </summary>
        private static void ResolveLocomotion(
            ref PlayerControlState controlState,
            in PlayerStateInput input,
            bool canSprint)
        {
            if (input.AimHeld)
                controlState.LocomotionMode = PlayerLocomotionMode.Aim;
            else if (input.SprintHeld && input.HasMoveInput && canSprint)
                controlState.LocomotionMode = PlayerLocomotionMode.Sprint;
            else
                controlState.LocomotionMode = PlayerLocomotionMode.Free;
        }

        /// <summary>
        /// 清理会占用操作的短时状态和计时器 
        /// 不清理事件序号或 LastReloadRequestSequence，因为它们是跨回滚/重绑定的去重依据 
        /// </summary>
        private static void ClearInterruptibleActions(
            ref PlayerControlState controlState,
            ref PlayerActionRuntimeState actionState)
        {
            controlState.ReactionMode = PlayerReactionMode.Normal;
            controlState.CombatMode = PlayerCombatMode.Ready;
            actionState.HitTicksRemaining = 0;
            actionState.ReloadTicksRemaining = 0;
            actionState.FireCooldownTicks = 0;
        }
    }
}
