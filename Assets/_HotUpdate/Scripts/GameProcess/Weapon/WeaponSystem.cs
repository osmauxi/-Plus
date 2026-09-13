using System;

namespace ProjectGame.HotFix.Gameplay.Weapon
{
    /// <summary>
    /// 根据状态判断这一Tick的操作，纯判定脚本
    /// </summary>
    public static class WeaponSystem
    {
        public static WeaponRuntimeState Equip(WeaponDefinition definition) =>
            Equip(definition, definition.Stats, 0, 1);

        public static WeaponRuntimeState Equip(WeaponDefinition definition, in WeaponStatSnapshot stats,
            ushort effectSetId, ushort snapshotVersion) => new WeaponRuntimeState
        {
            WeaponId = definition.WeaponId, StatSnapshotId = stats.Id, EffectSetId = effectSetId,
            SnapshotVersion = snapshotVersion, CurrentAmmo = stats.MagSize, ReserveAmmo = definition.ReserveAmmo,
        };

        public static void Reconfigure(ref WeaponRuntimeState state, WeaponDefinition definition,
            in WeaponStatSnapshot stats, ushort effectSetId, ushort snapshotVersion)
        {
            if (definition == null) 
                throw new ArgumentNullException(nameof(definition));
            WeaponStatSnapshotRepository.Validate(stats);
            if (state.IsEquipped && state.WeaponId != definition.WeaponId)
                throw new InvalidOperationException("不能用 Effect 重配置接口切换武器。");

            if (!state.IsEquipped)
            {
                state = Equip(definition, stats, effectSetId, snapshotVersion);
                return;
            }

            state.StatSnapshotId = stats.Id;
            state.EffectSetId = effectSetId;
            state.SnapshotVersion = snapshotVersion;
            state.CurrentAmmo = Math.Min(state.CurrentAmmo, stats.MagSize);
            state.FireCooldownTicks = Math.Min(state.FireCooldownTicks, stats.FireIntervalTicks);
            Interrupt(ref state);
        }

        /// <summary> 
        /// 返回是否成功打出一枪，只返回判定
        /// </summary>
        public static bool Simulate(ref WeaponRuntimeState state, WeaponDefinition definition,
            bool fireHeld, bool reloadRequested, float deltaTime)
        {
            WeaponStatSnapshot stats = definition != null ? definition.Stats : default;
            return Simulate(ref state, definition, stats, fireHeld, reloadRequested, deltaTime);
        }

        public static bool Simulate(ref WeaponRuntimeState state, WeaponDefinition definition,
            in WeaponStatSnapshot stats, bool fireHeld, bool reloadRequested, float deltaTime)
        {
            //没定义，没装备，Disabled
            if(definition == null || !state.IsEquipped || state.IsDisabled) 
                return false;
            if(deltaTime <= 0 || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            //定义是否与状态是同一把枪
            if(state.WeaponId != definition.WeaponId || state.StatSnapshotId == 0)
                throw new InvalidOperationException("武器快照与已装配定义不一致。");

            if(state.FireCooldownTicks > 0) 
                state.FireCooldownTicks--;

            if(state.IsReloading)
            {
                if(state.ReloadTicksRemaining > 0) 
                    state.ReloadTicksRemaining--;
          
                if(state.ReloadTicksRemaining == 0)
                {
                    int transferred = Math.Min(stats.MagSize - state.CurrentAmmo, state.ReserveAmmo);
                    state.CurrentAmmo += (ushort)transferred;
                    state.ReserveAmmo -= (ushort)transferred;
                    state.Flags &= ~WeaponRuntimeFlags.Reloading;
                }
                //换弹不开枪
                return false;
            }
            //企图换弹，且条件足够换弹，进行换弹
            if((reloadRequested || (definition.AutoReload && fireHeld && state.CurrentAmmo == 0)) &&
                state.CurrentAmmo < stats.MagSize && state.ReserveAmmo > 0)
            {
                state.Flags |= WeaponRuntimeFlags.Reloading;
                state.ReloadTicksRemaining = stats.ReloadTicks > 0
                    ? stats.ReloadTicks : ToTicks(stats.ReloadTime, deltaTime);
                return false;
            }
            // 未按开火、没子弹或仍在冷却时不能开枪；Aim 门槛由玩家状态机统一处理。
            if (!fireHeld || state.CurrentAmmo == 0 || state.FireCooldownTicks > 0) 
                return false;
            //触发时就是默认想开枪，所以所有判定过了就是成功开枪
            state.CurrentAmmo--;
            state.ShotSequence = unchecked(state.ShotSequence + 1);
            state.FireCooldownTicks = stats.FireIntervalTicks > 0
                ? stats.FireIntervalTicks : ToTicks(1f / stats.FireRate, deltaTime);
            return true;
        }

        /// <summary>
        /// 取消当前的换弹行为
        /// </summary>
        public static void Interrupt(ref WeaponRuntimeState state)
        {
            //修改Flags的换弹状态，其他不变
            state.Flags &= ~WeaponRuntimeFlags.Reloading;
            state.ReloadTicksRemaining = 0;
        }
        /// <summary>
        /// 将实际的时间转换为同步友好的Tick
        /// </summary>
        public static ushort ToTicks(float seconds, float deltaTime)
        {
            //Ceiling向上取整保证转换Tick时间等于或高于定义时间，保底1Tick
            double ticks = Math.Ceiling((double)seconds / deltaTime - 0.00001);
            if (ticks > ushort.MaxValue || double.IsNaN(ticks) || ticks < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "武器计时超出网络字段范围。");
            return (ushort)Math.Max(1, ticks);
        }
    }
}
