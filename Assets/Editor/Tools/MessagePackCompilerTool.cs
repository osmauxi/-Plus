using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Text;

public class MessagePackCompilerTool
{
    public static void GenerateMPC()
    {
        // 获取项目根目录（Assets 的上一级）
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        // 输入目录：生成的 Config C# 脚本所在位置
        string inputPath = Path.Combine(Application.dataPath, "_HotUpdate", "Scripts", "Config", "Generated");

        // 输出文件：生成的静态解析器，放在 HotFix 目录下参与热更编译
        string outputPath = Path.Combine(Application.dataPath, "_HotUpdate", "Scripts", "Config", "MessagePackGenerated.cs");

        // 清理旧的生成文件，避免残留过时代码导致编译报错
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
            UnityEngine.Debug.Log($"[清理] 已删除旧的 MessagePackGenerated.cs，准备重新生成");
        }

        // mpc 的可执行文件路径（dotnet 全局工具）
#if UNITY_EDITOR_WIN
        string mpcPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".dotnet", "tools", "mpc.exe");
#else
        string mpcPath = "mpc";
#endif

        // 组装 mpc 命令行参数
        // -i 指定输入目录，-o 指定输出文件，-n 指定命名空间
        string arguments = $"-i \"{inputPath}\" -o \"{outputPath}\" -n ProjectGame.HotFix.Resolvers";

        UnityEngine.Debug.Log($"[MPC] 执行命令: {mpcPath} {arguments}");

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = mpcPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
            WorkingDirectory = projectRoot
        };

        using (Process process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                UnityEngine.Debug.Log("<color=green>[MPC] AOT 静态代码生成成功！无惧 IL2CPP！</color>\n" + output);
                AssetDatabase.Refresh();
            }
            else
            {
                UnityEngine.Debug.LogError($"[MPC] 生成失败 (ExitCode: {process.ExitCode})\n【错误日志】: {error}\n【输出日志】: {output}");
            }
        }
    }
}