using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace ProjectGame.Bootstrap
{
    [Serializable]
    public sealed class BenchmarkProcessStatus
    {
        public int schemaVersion = 1;
        public string runId;
        public string role;
        public string stage;
        public string detail;
        public string utcTimestamp;
        public int processId;
        public int monsterCount;
        public bool failed;
        public bool completed;
    }

    [Serializable]
    public sealed class BenchmarkCaptureCommand
    {
        public int schemaVersion = 1;
        public string runId;
        public long startUtcTicks;
        public long stopUtcTicks;
    }

    public static class BenchmarkFileProtocol
    {
        public const string StatusFileName = "status.json";
        public const string CaptureCommandFileName = "capture-command.json";

        public static void WriteStatus(
            BenchmarkLaunchOptions options,
            string stage,
            string detail,
            bool failed = false,
            bool completed = false)
        {
            if (options == null || !options.Enabled)
                return;

            var status = new BenchmarkProcessStatus
            {
                runId = options.RunId,
                role = options.RoleName,
                stage = stage,
                detail = detail ?? string.Empty,
                utcTimestamp = DateTime.UtcNow.ToString("O"),
                processId = Process.GetCurrentProcess().Id,
                monsterCount = options.MonsterCount,
                failed = failed,
                completed = completed,
            };
            WriteJsonAtomically(
                Path.Combine(options.RoleResultDirectory, StatusFileName),
                JsonUtility.ToJson(status, true));
        }

        public static bool TryReadCaptureCommand(
            BenchmarkLaunchOptions options,
            out BenchmarkCaptureCommand command)
        {
            command = null;
            if (options == null || !options.Enabled)
                return false;

            string path = Path.Combine(
                Directory.GetParent(options.RoleResultDirectory)?.FullName ?? options.RoleResultDirectory,
                CaptureCommandFileName);
            if (!File.Exists(path))
                return false;

            try
            {
                command = JsonUtility.FromJson<BenchmarkCaptureCommand>(File.ReadAllText(path));
                return command != null &&
                       command.runId == options.RunId &&
                       command.startUtcTicks > 0 &&
                       command.stopUtcTicks > command.startUtcTicks;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static void WriteJsonAtomically(string path, string contents)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, contents);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temporaryPath, path);
        }
    }
}
