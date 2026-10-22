using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ProjectGame.Bootstrap
{
    public enum BenchmarkProcessRole : byte
    {
        None = 0,
        Host = 1,
        Client = 2,
    }

    /// <summary>
    /// Player 启动时解析一次的压测参数。未显式传入 -benchmark 时不改变正式运行行为。
    /// </summary>
    public sealed class BenchmarkLaunchOptions
    {
        public static readonly int[] DefaultMonsterCounts = { 1024, 2048, 5012 };

        private static BenchmarkLaunchOptions _current;

        public static BenchmarkLaunchOptions Current =>
            _current ??= Parse(Environment.GetCommandLineArgs());

        public bool Enabled { get; private set; }
        public BenchmarkProcessRole Role { get; private set; }
        public string RunId { get; private set; } = string.Empty;
        public int MonsterCount { get; private set; } = DefaultMonsterCounts[0];
        public int RandomSeed { get; private set; } = 12345;
        public int RepeatIndex { get; private set; } = 1;
        public float WarmupSeconds { get; private set; } = 5f;
        public float CaptureSeconds { get; private set; } = 30f;
        public float StageTimeoutSeconds { get; private set; } = 120f;
        public string HostAddress { get; private set; } = "127.0.0.1";
        public ushort GamePort { get; private set; } = LanEndpoint.DefaultGamePort;
        public string ResultRoot { get; private set; } = string.Empty;
        public string ContentRoot { get; private set; } = string.Empty;

        public string RoleName => Role == BenchmarkProcessRole.Host ? "host" : "client";
        public string PlayerId => $"benchmark-{RunId}-{RoleName}";
        public string CaseName => $"monsters-{MonsterCount}";

        public string RoleResultDirectory => Path.Combine(
            ResultRoot,
            "runs",
            RunId,
            CaseName,
            $"repeat-{RepeatIndex:D2}",
            RoleName);

        public static BenchmarkLaunchOptions InitializeFromCommandLine()
        {
            BenchmarkLaunchOptions options = Current;
            if (!options.Enabled)
                return options;

            Application.runInBackground = true;
            int qualityIndex = Array.IndexOf(QualitySettings.names, "Performant");
            if (qualityIndex < 0)
                throw new InvalidOperationException("Benchmark 要求的 Performant 质量档不存在。");
            QualitySettings.SetQualityLevel(qualityIndex, true);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            if (!Application.isBatchMode)
                Screen.SetResolution(1024, 768, FullScreenMode.Windowed);

            BenchmarkFileProtocol.WriteStatus(options, "bootstrap-start", "Benchmark 参数已解析");
            return options;
        }

        private static BenchmarkLaunchOptions Parse(string[] args)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool enabled = false;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index] ?? string.Empty;
                if (string.Equals(argument, "-benchmark", StringComparison.OrdinalIgnoreCase))
                {
                    enabled = true;
                    continue;
                }

                if (!argument.StartsWith("-benchmark-", StringComparison.OrdinalIgnoreCase))
                    continue;

                int separator = argument.IndexOf('=');
                string key;
                string value;
                if (separator >= 0)
                {
                    key = argument.Substring(1, separator - 1);
                    value = argument.Substring(separator + 1);
                }
                else
                {
                    key = argument.Substring(1);
                    value = index + 1 < args.Length ? args[++index] : string.Empty;
                }

                values[key] = value;
            }

            var options = new BenchmarkLaunchOptions { Enabled = enabled };
            if (!enabled)
                return options;

            string role = Require(values, "benchmark-role");
            options.Role = role.Equals("host", StringComparison.OrdinalIgnoreCase)
                ? BenchmarkProcessRole.Host
                : role.Equals("client", StringComparison.OrdinalIgnoreCase)
                    ? BenchmarkProcessRole.Client
                    : throw new ArgumentException($"无效 benchmark role：{role}");

            options.RunId = Require(values, "benchmark-run-id");
            options.MonsterCount = ReadInt(values, "benchmark-monsters", options.MonsterCount, 1, 100000);
            options.RandomSeed = ReadInt(values, "benchmark-seed", options.RandomSeed, int.MinValue, int.MaxValue);
            options.RepeatIndex = ReadInt(values, "benchmark-repeat", options.RepeatIndex, 1, 999);
            options.WarmupSeconds = ReadFloat(values, "benchmark-warmup", options.WarmupSeconds, 0f, 3600f);
            options.CaptureSeconds = ReadFloat(values, "benchmark-duration", options.CaptureSeconds, 1f, 3600f);
            options.StageTimeoutSeconds = ReadFloat(values, "benchmark-timeout", options.StageTimeoutSeconds, 10f, 3600f);
            options.HostAddress = Read(values, "benchmark-host", options.HostAddress).Trim();
            options.GamePort = checked((ushort)ReadInt(
                values,
                "benchmark-port",
                options.GamePort,
                1,
                ushort.MaxValue));

            string defaultRoot = Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(),
                "BenchmarkResults");
            options.ResultRoot = Path.GetFullPath(Read(values, "benchmark-result-root", defaultRoot));

            string contentRoot = Read(values, "benchmark-content-root", string.Empty).Trim();
            options.ContentRoot = string.IsNullOrEmpty(contentRoot)
                ? string.Empty
                : Path.GetFullPath(contentRoot);

            Directory.CreateDirectory(options.RoleResultDirectory);
            return options;
        }

        private static string Require(IReadOnlyDictionary<string, string> values, string key)
        {
            string value = Read(values, key, string.Empty).Trim();
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Benchmark 缺少参数：-{key}");
            return value;
        }

        private static string Read(
            IReadOnlyDictionary<string, string> values,
            string key,
            string fallback) =>
            values.TryGetValue(key, out string value) ? value : fallback;

        private static int ReadInt(
            IReadOnlyDictionary<string, string> values,
            string key,
            int fallback,
            int minimum,
            int maximum)
        {
            string raw = Read(values, key, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ||
                value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(key, raw, $"范围必须是 {minimum}..{maximum}");
            return value;
        }

        private static float ReadFloat(
            IReadOnlyDictionary<string, string> values,
            string key,
            float fallback,
            float minimum,
            float maximum)
        {
            string raw = Read(values, key, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
                float.IsNaN(value) || float.IsInfinity(value) || value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(key, raw, $"范围必须是 {minimum}..{maximum}");
            return value;
        }
    }
}
