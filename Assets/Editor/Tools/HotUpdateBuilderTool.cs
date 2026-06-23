using UnityEditor;
using UnityEngine;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;

public class HotUpdateBuilderTool
{
    public static void BuildAndCopyHotUpdateDlls()
    {
        Debug.Log("[HotUpdateBuilderTool] 开始执行 HybridCLR 编译...");

        //获取目标平台 (PC/Android/iOS 等)
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        //直接在代码里触发HybridCLR的底层编译指令
        CompileDllCommand.CompileDll(target);

        //用HybridCLR官方API，获取编译后的DLL存放目录
        string hotfixDllDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);

        //目标存放目录
        string destDir = Application.dataPath + "/_HotUpdate/DLLs";

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        //拿到你在 HybridCLR 设置界面里配置的所有【热更程序集】列表
        var hotUpdateDlls = SettingsUtil.HotUpdateAssemblyFilesExcludePreserved;

        //遍历列表，执行自动拷贝和后缀重命名
        foreach (var dllName in hotUpdateDlls)
        {
            string sourcePath = Path.Combine(hotfixDllDir, dllName);
            //自动追加 .bytes 后缀
            string destPath = Path.Combine(destDir, dllName + ".bytes");

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, true); // true表示强行覆盖旧文件
                Debug.Log($"[DLL同步] 成功更新热更程序集: {dllName}.bytes");
            }
            else
            {
                Debug.LogError($"[DLL同步失败] 找不到编译后的源文件: {sourcePath}");
            }
        }

        AssetDatabase.Refresh();

        Debug.Log("<color=cyan><b>[HotUpdateBuilderTool] DLL更新完毕</b></color>");
    }
}