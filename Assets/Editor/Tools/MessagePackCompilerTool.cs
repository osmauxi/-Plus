using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
public class MessagePackCompilerTool
{
    public static void GenerateMPC()
    {
        //获取项目根目录 (也就是 Assets 文件夹的上一级)
        //string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        //string inputPath = Path.Combine(projectRoot, "_HotUpdate.Config.csproj");
        string inputPath = Application.dataPath + "/_HotUpdate/Scripts/Config/Generated";

        //输出路径：把生成的静态解析器文件，也放在 HotFix 目录下参与热更编译
        string outputPath = Application.dataPath + "/_HotUpdate/Scripts/Config/MessagePackGenerated.cs";

        //组装mpc命令行指令                                          
        //-i表示输入目录，-o表示输出文件
        string arguments = $"/c mpc -i \"{inputPath}\" -o \"{outputPath}\" -n ProjectGame.HotFix.Resolvers";

        UnityEngine.Debug.Log($"[MPC] 正在执行命令: cmd.exe {arguments}");

        //在后台静默调用系统终端
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            // 读取输出
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // 0 代表系统层面执行成功
            if (process.ExitCode == 0)
            {
                UnityEngine.Debug.Log("<color=green>[MPC] AOT 静态代码生成成功！无惧 IL2CPP！</color>\n" + output);
                AssetDatabase.Refresh();
            }
            else
            {
                // 打印详细双端日志，让报错无处遁形
                UnityEngine.Debug.LogError($"[MPC] 生成失败\n【错误日志】: {error}\n【输出日志】: {output}");
            }
        }
    }
}
