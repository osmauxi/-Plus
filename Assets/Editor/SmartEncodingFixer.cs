using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class SmartEncodingFixer : EditorWindow 
{
    [MenuItem("Tools/智能修复：仅将 GBK 转为 UTF-8")]
    public static void SmartConvert()
    {
        string assetsPath = Application.dataPath;
        string[] allFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);

        // 936 是 GBK/GB2312
        Encoding gbk = Encoding.GetEncoding(936);
        Encoding utf8Bom = new UTF8Encoding(true);

        int convertedCount = 0;
        int skippedCount = 0;

        foreach (string filePath in allFiles)
        {
            if (filePath.Contains("PackageCache")) continue;

            byte[] fileBytes = File.ReadAllBytes(filePath);

            // 1. 核心判断：如果已经是 UTF-8 了，绝对不要碰！
            if (IsLikelyUTF8(fileBytes))
            {
                skippedCount++;
                continue;
            }

            try
            {
                // 2. 只有检测出不是 UTF-8，才按 GBK 读
                string content = File.ReadAllText(filePath, gbk);

                // 3. 存为 UTF-8 BOM
                File.WriteAllText(filePath, content, utf8Bom);
                convertedCount++;
                Debug.Log($"已修复 GBK 文件: {Path.GetFileName(filePath)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"文件处理出错: {filePath}\n{e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"智能转换完成！\n跳过(原UTF-8): {skippedCount} 个\n修复(原GBK): {convertedCount} 个");
    }

    // --- 核心算法：检测字节流是否符合 UTF-8 规律 ---
    private static bool IsLikelyUTF8(byte[] data)
    {
        // 1. 先看有没有 BOM (EF BB BF)
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            return true;

        // 2. 扫描字节流，看是否符合 UTF-8 编码规则
        int i = 0;
        while (i < data.Length)
        {
            byte b = data[i];
            if (b < 0x80) // ASCII (0xxxxxxx)，单字节
            {
                i++;
                continue;
            }

            int numBytes = 0;
            if ((b & 0xE0) == 0xC0) numBytes = 2;      // 110xxxxx
            else if ((b & 0xF0) == 0xE0) numBytes = 3; // 1110xxxx
            else if ((b & 0xF8) == 0xF0) numBytes = 4; // 11110xxx
            else return false; // 不符合 UTF-8 规则，认为是 GBK

            i++;
            for (int j = 0; j < numBytes - 1; j++)
            {
                if (i >= data.Length) return false;
                // 后续字节必须是 10xxxxxx
                if ((data[i] & 0xC0) != 0x80) return false;
                i++;
            }
        }
        return true; // 全程符合 UTF-8 规则
    }
}