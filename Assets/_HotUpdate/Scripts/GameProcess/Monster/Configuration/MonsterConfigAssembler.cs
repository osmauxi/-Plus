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
    /// 配表装配只在服务初始化阶段执行：排序建立稳定 ushort 索引、转换秒为 Tick，并校验跨表外键。
    /// Tick 热路径不再读取 ConfigManager 或字符串模块名。
    /// </summary>
    public static class MonsterConfigAssembler
    {
        public static MonsterCatalogBundle FromConfigManager(ConfigManager manager, float tickRate)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            return Build(
                RequireTable(manager.GetTable<Config_MonsterAttack>()),
                RequireTable(manager.GetTable<Config_MonsterRuntime>()),
                RequireTable(manager.GetTable<Config_MonsterSpawn>()),
                RequireTable(manager.GetTable<Config_MonsterView>()),
                tickRate);
        }

        public static MonsterCatalogBundle Build(
            IReadOnlyDictionary<int, Config_MonsterAttack> attackRows,
            IReadOnlyDictionary<int, Config_MonsterRuntime> runtimeRows,
            IReadOnlyDictionary<int, Config_MonsterSpawn> spawnRows,
            IReadOnlyDictionary<int, Config_MonsterView> viewRows,
            float tickRate)
        {
            if (attackRows == null) throw new ArgumentNullException(nameof(attackRows));
            if (runtimeRows == null) throw new ArgumentNullException(nameof(runtimeRows));
            if (spawnRows == null) throw new ArgumentNullException(nameof(spawnRows));
            if (viewRows == null) throw new ArgumentNullException(nameof(viewRows));
            if (!MonsterMath.IsPositiveFinite(tickRate))
                throw new ArgumentOutOfRangeException(nameof(tickRate));

            var attackIndices = new Dictionary<int, ushort>();
            MonsterAttackCatalog attacks = BuildAttacks(attackRows, attackIndices);
            var enabledIds = new HashSet<int>();
            MonsterRuntimeCatalog runtime = BuildRuntime(runtimeRows, attackIndices, tickRate, enabledIds);
            MonsterSpawnCatalog spawns = BuildSpawns(spawnRows, runtime, runtimeRows, enabledIds);
            MonsterViewCatalog views = BuildViews(viewRows, runtime, runtimeRows, enabledIds);
            EnsureCompleteCoverage(enabledIds, spawnRows, viewRows);
            return new MonsterCatalogBundle(attacks, runtime, spawns, views);
        }

        private static TTable RequireTable<TTable>(TTable table) where TTable : class => table ??
            throw new InvalidOperationException($"Monster 依赖配置表 {typeof(TTable).GenericTypeArguments[1].Name} 尚未加载。");

        private static MonsterAttackCatalog BuildAttacks(IReadOnlyDictionary<int, Config_MonsterAttack> rows,
            Dictionary<int, ushort> indices)
        {
            int[] keys = SortedKeys(rows);
            if (keys.Length > ushort.MaxValue + 1)
                throw new InvalidOperationException("MonsterAttack 超过 ushort 索引容量。");
            var profiles = new MonsterAttackProfile[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterAttack row = RequireRow(rows[keys[i]], "MonsterAttack", keys[i]);
                EnsureKey("MonsterAttack", keys[i], row.AttackProfileId);
                if (row.TargetLayerMask == 0)
                    throw new InvalidOperationException($"MonsterAttack[{keys[i]}].TargetLayerMask 不能为空。");
                indices.Add(row.AttackProfileId, checked((ushort)i));
                profiles[i] = new MonsterAttackProfile(
                    new Vector3(row.OriginOffsetX, row.OriginOffsetY, row.OriginOffsetZ),
                    row.Radius, row.Distance, row.TargetLayerMask);
            }
            return new MonsterAttackCatalog(profiles);
        }

        private static MonsterRuntimeCatalog BuildRuntime(
            IReadOnlyDictionary<int, Config_MonsterRuntime> rows,
            IReadOnlyDictionary<int, ushort> attackIndices, float tickRate, HashSet<int> enabledIds)
        {
            int[] keys = SortedKeys(rows);
            var configs = new List<MonsterRuntimeConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterRuntime row = RequireRow(rows[keys[i]], "MonsterRuntime", keys[i]);
                EnsureKey("MonsterRuntime", keys[i], row.ConfigId);
                if (!row.Enabled) continue;
                if (string.IsNullOrWhiteSpace(row.Name))
                    throw new InvalidOperationException($"MonsterRuntime[{keys[i]}].Name 不能为空。");
                if (!attackIndices.TryGetValue(row.AttackProfileId, out ushort attackIndex))
                    throw new InvalidOperationException(
                        $"MonsterRuntime[{keys[i]}] 引用了不存在的 AttackProfileId={row.AttackProfileId}。");
                configs.Add(new MonsterRuntimeConfig(row.ConfigId, row.MoveSpeed, row.AttackRange,
                    SecondsToTicks(row.WindupSeconds, tickRate, false, "MonsterRuntime.WindupSeconds"),
                    SecondsToTicks(row.RecoverySeconds, tickRate, false, "MonsterRuntime.RecoverySeconds"),
                    row.AttackDamage, row.MaxHealth, attackIndex));
                enabledIds.Add(row.ConfigId);
            }
            if (configs.Count == 0) throw new InvalidOperationException("MonsterRuntime 没有启用的配置。");
            return new MonsterRuntimeCatalog(configs);
        }

        private static MonsterSpawnCatalog BuildSpawns(IReadOnlyDictionary<int, Config_MonsterSpawn> rows,
            MonsterRuntimeCatalog runtime, IReadOnlyDictionary<int, Config_MonsterRuntime> runtimeRows,
            HashSet<int> enabledIds)
        {
            int[] keys = SortedKeys(rows);
            var items = new List<MonsterSpawnConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterSpawn row = RequireRow(rows[keys[i]], "MonsterSpawn", keys[i]);
                EnsureKey("MonsterSpawn", keys[i], row.ConfigId);
                if (!enabledIds.Contains(row.ConfigId))
                {
                    EnsureDisabledRuntimeReference("MonsterSpawn", row.ConfigId, runtimeRows);
                    continue;
                }
                if (!runtime.TryGetIndex(row.ConfigId, out ushort configIndex))
                    throw new InvalidOperationException($"MonsterSpawn[{keys[i]}] 无法解析 Runtime 索引。");
                items.Add(new MonsterSpawnConfig(configIndex, row.Cost, row.Weight,
                    row.MinDifficulty, row.MaxDifficulty, row.MaxPerWave));
            }
            return new MonsterSpawnCatalog(items, runtime);
        }

        private static MonsterViewCatalog BuildViews(IReadOnlyDictionary<int, Config_MonsterView> rows,
            MonsterRuntimeCatalog runtime, IReadOnlyDictionary<int, Config_MonsterRuntime> runtimeRows,
            HashSet<int> enabledIds)
        {
            int[] keys = SortedKeys(rows);
            var items = new List<MonsterViewConfig>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                Config_MonsterView row = RequireRow(rows[keys[i]], "MonsterView", keys[i]);
                EnsureKey("MonsterView", keys[i], row.ConfigId);
                if (!enabledIds.Contains(row.ConfigId))
                {
                    EnsureDisabledRuntimeReference("MonsterView", row.ConfigId, runtimeRows);
                    continue;
                }
                if (!runtime.TryGetIndex(row.ConfigId, out ushort configIndex))
                    throw new InvalidOperationException($"MonsterView[{keys[i]}] 无法解析 Runtime 索引。");
                items.Add(new MonsterViewConfig(configIndex, row.LocalPoolId));
            }
            return new MonsterViewCatalog(items, runtime);
        }

        private static void EnsureCompleteCoverage(HashSet<int> enabledIds,
            IReadOnlyDictionary<int, Config_MonsterSpawn> spawnRows,
            IReadOnlyDictionary<int, Config_MonsterView> viewRows)
        {
            foreach (int configId in enabledIds)
            {
                if (!spawnRows.ContainsKey(configId))
                    throw new InvalidOperationException($"启用的 MonsterRuntime[{configId}] 缺少 MonsterSpawn 行。");
                if (!viewRows.ContainsKey(configId))
                    throw new InvalidOperationException($"启用的 MonsterRuntime[{configId}] 缺少 MonsterView 行。");
            }
        }

        private static void EnsureDisabledRuntimeReference(string table, int configId,
            IReadOnlyDictionary<int, Config_MonsterRuntime> runtimeRows)
        {
            if (!runtimeRows.TryGetValue(configId, out Config_MonsterRuntime runtime) || runtime == null)
                throw new InvalidOperationException($"{table}[{configId}] 引用了不存在的 MonsterRuntime。 ");
            if (runtime.Enabled)
                throw new InvalidOperationException($"{table}[{configId}] 未能解析已启用的 MonsterRuntime。 ");
        }

        private static uint SecondsToTicks(float seconds, float tickRate, bool allowZero, string field)
        {
            if (!MonsterMath.IsNonNegativeFinite(seconds) || (!allowZero && seconds <= 0))
                throw new InvalidOperationException($"{field} 必须是{(allowZero ? "非负" : "正")}有限秒数。");
            if (seconds == 0) return 0;
            double rawTicks = (double)seconds * tickRate;
            double nearestTick = Math.Round(rawTicks);
            double tolerance = 1e-6d * Math.Max(1d, Math.Abs(rawTicks));
            if (Math.Abs(rawTicks - nearestTick) <= tolerance)
                rawTicks = nearestTick;
            double ticks = Math.Ceiling(rawTicks);
            if (ticks > int.MaxValue)
                throw new InvalidOperationException($"{field} 转换后超过 Tick 上限。");
            return checked((uint)Math.Max(1, ticks));
        }

        private static int[] SortedKeys<T>(IReadOnlyDictionary<int, T> rows)
        {
            var keys = new int[rows.Count];
            int index = 0;
            foreach (int key in rows.Keys) keys[index++] = key;
            Array.Sort(keys);
            return keys;
        }

        private static T RequireRow<T>(T row, string table, int key) where T : class => row ??
            throw new InvalidOperationException($"{table}[{key}] 为空。");

        private static void EnsureKey(string table, int key, int fieldId)
        {
            if (key != fieldId)
                throw new InvalidOperationException($"{table} 字典键 {key} 与首列 ID {fieldId} 不一致。");
        }
    }
}
