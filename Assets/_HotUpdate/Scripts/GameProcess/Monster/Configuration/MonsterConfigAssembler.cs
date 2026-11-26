using System;
using System.Collections.Generic;
using ProjectGame.HotFix.Config;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>ConfigManager 可变字典到房间内不可变目录的一次性装配结果。</summary>
    public sealed class MonsterCatalogBundle
    {
        public MonsterAttackCatalog Attacks { get; }
        public MonsterRuntimeCatalog Runtime { get; }
        public MonsterSpawnCatalog Spawns { get; }
        public MonsterViewCatalog Views { get; }

        internal MonsterCatalogBundle(MonsterAttackCatalog attacks,
            MonsterRuntimeCatalog runtime, MonsterSpawnCatalog spawns, MonsterViewCatalog views)
        {
            Attacks = attacks;
            Runtime = runtime;
            Spawns = spawns;
            Views = views;
        }
    }

    /// <summary>
    /// 配表装配只在服务初始化阶段执行：排序建立稳定 ushort 索引、转换秒为 Tick。
    /// Tick 热路径不再读取 ConfigManager 或字符串模块名。
    /// </summary>
    public static class MonsterConfigAssembler
    {
        public static MonsterCatalogBundle FromConfigManager(ConfigManager manager, float tickRate) =>
            Build(
                manager.GetTable<Config_MonsterAttack>(),
                manager.GetTable<Config_MonsterRuntime>(),
                manager.GetTable<Config_MonsterSpawn>(),
                manager.GetTable<Config_MonsterView>(),
                tickRate);

        public static MonsterCatalogBundle Build(
            IReadOnlyDictionary<int, Config_MonsterAttack> attackRows,
            IReadOnlyDictionary<int, Config_MonsterRuntime> runtimeRows,
            IReadOnlyDictionary<int, Config_MonsterSpawn> spawnRows,
            IReadOnlyDictionary<int, Config_MonsterView> viewRows,
            float tickRate)
        {
            var attackIndices = new Dictionary<int, ushort>();
            MonsterAttackCatalog attacks = BuildAttacks(attackRows, attackIndices);
            MonsterRuntimeCatalog runtime = BuildRuntime(runtimeRows, attackIndices, tickRate);
            MonsterSpawnCatalog spawns = BuildSpawns(spawnRows, runtime);
            MonsterViewCatalog views = BuildViews(viewRows, runtime);
            return new MonsterCatalogBundle(attacks, runtime, spawns, views);
        }

        private static MonsterAttackCatalog BuildAttacks(IReadOnlyDictionary<int, Config_MonsterAttack> rows,
            Dictionary<int, ushort> indices)
        {
            int[] keys = SortedKeys(rows);
            var profiles = new MonsterAttackProfile[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterAttack row = rows[keys[i]];
                indices.Add(row.AttackProfileId, (ushort)i);
                profiles[i] = new MonsterAttackProfile(
                    new Vector3(row.OriginOffsetX, row.OriginOffsetY, row.OriginOffsetZ),
                    row.Radius, row.Distance, row.TargetLayerMask);
            }
            return new MonsterAttackCatalog(profiles);
        }

        private static MonsterRuntimeCatalog BuildRuntime(
            IReadOnlyDictionary<int, Config_MonsterRuntime> rows,
            IReadOnlyDictionary<int, ushort> attackIndices, float tickRate)
        {
            int[] keys = SortedKeys(rows);
            var configs = new List<MonsterRuntimeConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterRuntime row = rows[keys[i]];
                if (!row.Enabled) continue;
                configs.Add(new MonsterRuntimeConfig(row.ConfigId, row.MoveSpeed, row.AttackRange,
                    SecondsToTicks(row.WindupSeconds, tickRate),
                    SecondsToTicks(row.RecoverySeconds, tickRate),
                    row.AttackDamage, row.MaxHealth, attackIndices[row.AttackProfileId]));
            }
            return new MonsterRuntimeCatalog(configs.ToArray());
        }

        private static MonsterSpawnCatalog BuildSpawns(IReadOnlyDictionary<int, Config_MonsterSpawn> rows,
            MonsterRuntimeCatalog runtime)
        {
            int[] keys = SortedKeys(rows);
            var items = new List<MonsterSpawnConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterSpawn row = rows[keys[i]];
                if (!runtime.TryGetIndex(row.ConfigId, out ushort configIndex))
                    continue;
                items.Add(new MonsterSpawnConfig(configIndex, row.Cost, row.Weight,
                    row.MinDifficulty, row.MaxDifficulty, row.MaxPerWave));
            }
            return new MonsterSpawnCatalog(items.ToArray());
        }

        private static MonsterViewCatalog BuildViews(IReadOnlyDictionary<int, Config_MonsterView> rows,
            MonsterRuntimeCatalog runtime)
        {
            int[] keys = SortedKeys(rows);
            var items = new List<MonsterViewConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterView row = rows[keys[i]];
                if (!runtime.TryGetIndex(row.ConfigId, out ushort configIndex))
                    continue;
                items.Add(new MonsterViewConfig(configIndex, row.LocalPoolId));
            }
            return new MonsterViewCatalog(items);
        }

        private static uint SecondsToTicks(float seconds, float tickRate) =>
            (uint)Math.Max(1, Math.Ceiling((double)seconds * tickRate - 1e-5d));

        private static int[] SortedKeys<T>(IReadOnlyDictionary<int, T> rows)
        {
            var keys = new int[rows.Count];
            int index = 0;
            foreach (int key in rows.Keys) keys[index++] = key;
            Array.Sort(keys);
            return keys;
        }
    }
}
