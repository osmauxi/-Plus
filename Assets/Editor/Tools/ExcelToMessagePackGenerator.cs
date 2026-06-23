using ExcelDataReader;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

//继承EditorWindow表示脚本只生存在Unity编辑器进程中，打包时会被剔除。
public class ExcelToMessagePackGenerator : EditorWindow
{
    private string excelFolderPath;
    private string csOutputFolderPath;
    private string bytesOutputFolderPath;

    private List<string> validClassNames = new List<string>(10);

    //类型解析字典，提前录入各数据类型的字符串转换逻辑，方便后续扩展和维护。策划在Excel里填什么类型，这里就要有对应的解析函数。
    private readonly Dictionary<string, Func<string, object>> TypeParsers = new Dictionary<string, Func<string, object>>()
    {
        { "int", (val) => int.Parse(val) },
        { "float", (val) => float.Parse(val) },
        { "string", (val) => val },
        { "bool", (val) => val.ToLower() == "true" || val == "1" },
        
        //整数数组 (在 Excel 里填 "101,102,103")
        { "int[]", (val) => {
            string[] parts = val.Split(',');
            int[] arr = new int[parts.Length];
            for(int i=0; i<parts.Length; i++) arr[i] = int.Parse(parts[i]);
            return arr;
        }},
        
        //字符串数组 (在 Excel 里填 "LifeSteal,FireDamage")
        { "string[]", (val) => val.Split(',') },

        { "Vector2", (val) => {
        string[] p = val.Split(',');
        return new Vector2(float.Parse(p[0]), float.Parse(p[1]));
        }},

        { "Vector3", (val) => {
        string[] p = val.Split(',');
        return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }}

        // TODO: 以后如果要加 Vector3 或者 Enum 枚举，直接在这里加代码
    };

    [MenuItem("Tools/数据管线")]
    public static void ShowWindow()
    {
        //它会去编辑器里找有没有打开的“数据管线引擎”的面板，如果有，就把焦点切过去；
        //如果没有，就在内存里new一个该类的实例并弹出一个新窗口。
        //OnGUI方法就是这个窗口的核心绘制函数，Unity会在窗口需要重绘时自动调用它。
        GetWindow<ExcelToMessagePackGenerator>("数据管线引擎");
    }

    private void OnEnable()
    {
        excelFolderPath = Application.dataPath + "/DesignData/Excels/";
        csOutputFolderPath = Application.dataPath + "/_HotUpdate/Scripts/Config/Generated/";
        bytesOutputFolderPath = Application.dataPath + "/AddressableResources/Config/"; // Addressables 预留目录
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUI.color = Color.cyan;
        GUILayout.Label("一：定义数据结构", EditorStyles.boldLabel);
        GUI.color = Color.white;

        if (GUILayout.Button("1. 读表生成C#脚本", GUILayout.Height(40)))
        {
            ProcessExcels(generateCS: true, exportBytes: false);
        }

        GUILayout.Space(15);
        GUI.color = Color.green;
        GUILayout.Label("二：数据打包与AOT防闪退", EditorStyles.boldLabel);
        GUI.color = Color.white;

        if (GUILayout.Button("2. 导出二进制数据(.bytes)", GUILayout.Height(40)))
        {
            ProcessExcels(generateCS: false, exportBytes: true);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("3. 生成 AOT 静态解析代码 (MPC)", GUILayout.Height(40)))
        {
            MessagePackCompilerTool.GenerateMPC();
        }

        GUILayout.Space(15);
        GUI.color = Color.yellow;
        GUILayout.Label("阶段三：热更注入", EditorStyles.boldLabel);
        GUI.color = Color.white;

        if (GUILayout.Button("4. 一键编译并同步热更 DLL", GUILayout.Height(50)))
        {
            HotUpdateBuilderTool.BuildAndCopyHotUpdateDlls();
        }

        GUILayout.Space(20);
        GUIStyle helpStyle = new GUIStyle(EditorStyles.helpBox);
        helpStyle.fontSize = 12;
        helpStyle.richText = true;
        GUILayout.Label("<b>日常工作流提示：</b>\n" +
            "• 修改了Excel表头/新增表：按 <b>1 -> 2 -> 3 -> 4</b> 完整执行。\n" +
            "• 仅修改了Excel里的数值：按 <b>2</b> 即可。\n" +
            "• 仅修改了业务 C# 代码：按 <b>4</b> 即可。", helpStyle);
    }

    private void ProcessExcels(bool generateCS, bool exportBytes)
    {
        //ExcelDataReader需要这个才能正确处理 Excel 文件的编码问题，尤其是中文路径或内容
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //获取指定目录下所有的.xlsx文件
        string[] excelFiles = Directory.GetFiles(excelFolderPath, "*.xlsx");

        foreach (string file in excelFiles)
        {
            //剔除临时文件（Excel打开时会生成一个同目录下的临时文件，名字以 ~$ 开头）
            if (Path.GetFileName(file).StartsWith("~$")) 
                continue;

            //所有的文件流操作都放在using里，确保用完就关。
            //ReadWrite防锁死，允许Excel打开状态下读取数据，避免了文件被占用无法访问的问题。
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把Excel表格数据解析成C#里可以操作的数据结构。IExcelDataReader统一封装了 .xlsx/.xls 格式的读取逻辑，无需关心底层格式差异
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    //将文件流转换成内存里的DataSet，Excel 结构 转 .NET 数据结构
                    var result = reader.AsDataSet();
                    //DataTable代表一张内存数据表，对应 Excel 里单个工作表，TableName为表名，Rows/Columns为行列数据集合。
                    foreach (DataTable table in result.Tables)
                    {
                        //过滤掉以 # 开头的表，这些通常是Excel的隐藏工作表或者临时数据表，不是我们要处理的正式配置表。
                        if (table.TableName.StartsWith("#")) 
                            continue;

                        string className = "Config_" + table.TableName.Split('|')[0];
                        if (!validClassNames.Contains(className))
                        {
                            validClassNames.Add(className);
                        }

                        if (generateCS) 
                            ExtractSchemaAndGenerateCSharp(table.TableName, table);
                        if (exportBytes)
                            PackDataToBinary(table.TableName, table);
                    }
                }
            }
        }

        if (generateCS)
        {
            GenerateConfigRegisterScript(validClassNames);
            AssetDatabase.Refresh(); //强行触发编译
            Debug.Log("<color=yellow>脚本生成完毕，请等待编译结束</color>");
        }
    }

    //简易字符串拼接，生成一个带有 MessagePack 标签的 C# 类，属性和类型根据 Excel 表头自动推断。这个类就是后续反射实例化对象的模板。
    private void ExtractSchemaAndGenerateCSharp(string rawTableName, DataTable table)
    {
        //行数不够3行，说明不是标准表，这是自己的约定：
        //第一行中文注释，第二行英文变量名，第三行数据类型，如果不满足这个结构，就直接跳过这个表，不生成代码。
        if (table.Rows.Count < 3) 
            return;

        //命名约定，表名如果包含 | 符号，| 前面部分作为类名，后面部分作为继承关系。例如 "Weapon|Config_Base" 会生成一个 class Config_Weapon : Config_Base
        string[] nameParts = rawTableName.Split('|');
        string className = "Config_" + nameParts[0];
        string inheritance = nameParts.Length > 1 ? $" : {nameParts[1]}" : "";

        StringBuilder csBuilder = new StringBuilder();

        //引入 MessagePack 的命名空间
        csBuilder.AppendLine("using System;");
        csBuilder.AppendLine("using MessagePack;");
        csBuilder.AppendLine("using System.Collections.Generic;");
        csBuilder.AppendLine("");

        //[MessagePackObject]允许这个类被 MessagePack 序列化器识别和处理，
        //这样我们后续在打包数据时就能直接把这个类的实例转换成二进制格式，满足高性能序列化的需求。
        csBuilder.AppendLine("[MessagePackObject]");
        csBuilder.AppendLine($"public class {className}{inheritance}");
        csBuilder.AppendLine("{");

        //遍历所有的列
        for (int col = 0; col < table.Columns.Count; col++)
        {
            string desc = table.Rows[0][col].ToString();       // 第1行 中文注释
            string varName = table.Rows[1][col].ToString();    // 第2行 英文变量名
            string typeName = table.Rows[2][col].ToString();   // 第3行 数据类型 (int, string, float)

            //如果这一列没填变量名，说明是留空的废列，跳过
            if (string.IsNullOrWhiteSpace(varName)) continue;

            //写入注释
            csBuilder.AppendLine($"    /// <summary> {desc} </summary>");

            //写入 MessagePack 的序列化索引标签 [Key(0)], [Key(1)]... 这是 MessagePack 的核心要求
            csBuilder.AppendLine($"    [Key({col})]");

            //写入字段声明
            csBuilder.AppendLine($"    public {typeName} {varName};");
            csBuilder.AppendLine("");
        }

        csBuilder.AppendLine("}");

        //将拼接好的字符串保存为 .cs 文件
        if (!Directory.Exists(csOutputFolderPath)) 
            Directory.CreateDirectory(csOutputFolderPath);

        File.WriteAllText(csOutputFolderPath + className + ".cs", csBuilder.ToString(), Encoding.UTF8);
        Debug.Log($"自动生成代码成功: {className}.cs");
    }

    //PackDataToBinary与ExtractSchemaAndGenerateCSharp关系类似脚本跟ScriptableObejct
    //PackDataToBinary负责生成脚本，定义规则，而ExtractSchemaAndGenerateCSharp负责读取数据生成脚本实例并压缩为二进制流保存
    /// <summary>
    /// 利用反射实例化对象，并打包为 MessagePack 二进制
    /// </summary>
    private void PackDataToBinary(string rawTableName, DataTable table)
    {
        //只获取类名部分，去掉可能的继承关系描述（|后面部分）
        string className = "Config_" + rawTableName.Split('|')[0];
        //通过反射找到第一步生成的那个类
        //注意：如果生成的类放进了特定的namespace（比如 ProjectGame.HotFix），这里也要加上命名空间
        //当前脚本被_HotUpdate.Config程序集管辖，所以要加入程序集名称
        Type configType = Type.GetType(className + ", _HotUpdate.Config"); 

        if (configType == null)
        {
            Debug.LogError($"找不到类 {className}，可能是Unity编译未完成或程序集名称不对");
            return;
        }

        //创建一个泛型字典实例，类型为Dictionary<int, configType>
        Type dictType = typeof(Dictionary<,>).MakeGenericType(typeof(int), configType);
        IDictionary dataDict = Activator.CreateInstance(dictType) as IDictionary;

        //从第4行开始遍历真实数据
        for (int row = 3; row < table.Rows.Count; row++)
        {
            //实例化一个该类的空对象 (例如 new Config_Weapon())
            object configInstance = Activator.CreateInstance(configType);
            int keyId = 0;

            for (int col = 0; col < table.Columns.Count; col++)
            {
                //从Excel表中拿出这个标签的具体数值填进去
                string varName = table.Rows[1][col].ToString();
                string typeName = table.Rows[2][col].ToString(); //提取第3行填写的类型字符串
                string cellValue = table.Rows[row][col].ToString();

                if (string.IsNullOrWhiteSpace(varName) || string.IsNullOrWhiteSpace(cellValue)) 
                    continue;

                //使用Excel表中读出的字段名反射获取字段，并将字符串转换为目标类型并赋值
                FieldInfo field = configType.GetField(varName);
                if (field != null)
                {
                    try
                    {
                        //获取这个字段在C#里真实的类型
                        Type fieldType = field.FieldType; 
                        if (fieldType.IsEnum)
                        {
                            //枚举如果要写在解析器里面的话，每种枚举都要单独写一个解析函数
                            //所以直接在这里用反射调用Enum.Parse来解析枚举类型，要求Excel表里直接填枚举的字符串值（例如 "FireDamage"）
                            object parsedValue = Enum.Parse(fieldType, cellValue);
                            field.SetValue(configInstance, parsedValue);
                        }
                        else if(TypeParsers.ContainsKey(typeName))
                        {
                            object parsedValue = TypeParsers[typeName].Invoke(cellValue);
                            field.SetValue(configInstance, parsedValue);
                        }
                        else
                        {
                            //走到这里的，说明是极其复杂的嵌套结构体，结构体在Excel表中以Json格式存在，比如 {"x":1,"y":2}
                            //因为fieldType获取到了结构体，创建了结构体实例，所以表中直接{}填结构体具体数据就行了。
                            object parsedValue = JsonUtility.FromJson(cellValue, fieldType);

                            if (parsedValue != null)
                            {
                                field.SetValue(configInstance, parsedValue);
                            }
                            else
                            {
                                Debug.LogWarning($"未能通过 JSON 解析复杂类型: {typeName}，单元格数据: {cellValue}");
                            }
                        }
                        //强制约定第一列必须是 int 类型的 ID
                        if (col == 0)
                            keyId = Convert.ToInt32(cellValue);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"解析错误! 表:{className} 行:{row + 1} 列:{varName} 数据:{cellValue}. 错误:{e.Message}");
                    }
                }
            }

            //将拼装好的对象塞进字典
            dataDict.Add(keyId, configInstance);
            Debug.Log($"实例化数据生成成功: {className}.bytes");
        }

        //调用MessagePack的极速序列化，把整个字典压缩成 byte[]
        byte[] bytes = MessagePackSerializer.Serialize(dictType, dataDict);

        //存入本地，加上.bytes后缀供Addressables读取
        string outPath = bytesOutputFolderPath + className + ".bytes";
        File.WriteAllBytes(outPath, bytes);
    }

    /// <summary>
    /// 自动生成 ConfigRegister.cs 静态路由脚本
    /// </summary>
    private void GenerateConfigRegisterScript(List<string> classNames)
    {
        StringBuilder csBuilder = new StringBuilder();

        csBuilder.AppendLine("// ====================================================");
        csBuilder.AppendLine("// 本文件由工具自动生成，请勿手动修改！");
        csBuilder.AppendLine("// ====================================================");
        csBuilder.AppendLine("using System;");
        csBuilder.AppendLine("using MessagePack;");
        csBuilder.AppendLine("using System.Collections.Generic;");
        csBuilder.AppendLine("using UnityEngine;");
        csBuilder.AppendLine("");
        csBuilder.AppendLine("namespace ProjectGame.HotFix.Config");

        csBuilder.AppendLine("{");
        csBuilder.AppendLine("    public static class ConfigRegister");
        csBuilder.AppendLine("    {");
        csBuilder.AppendLine("        public static void ParseAndRegister(string addressableName, byte[] bytes)");
        csBuilder.AppendLine("        {");
        csBuilder.AppendLine("            switch (addressableName)");
        csBuilder.AppendLine("            {");

        //遍历所有收集到的类名，生成对应的 case 分支
        foreach (string className in classNames)
        {
            csBuilder.AppendLine($"                case \"{className}\":");
            csBuilder.AppendLine($"                    var dict_{className} = MessagePackSerializer.Deserialize<Dictionary<int, {className}>>(bytes);");
            csBuilder.AppendLine($"                    ConfigManager.Instance.RegisterTable(dict_{className});");
            csBuilder.AppendLine($"                    break;");
        }

        csBuilder.AppendLine("                default:");
        csBuilder.AppendLine("                    Debug.LogWarning($\"[ConfigRegister] 未知的配置表名: {addressableName}，检查Address标签是否打错\");");
        csBuilder.AppendLine("                    break;");
        csBuilder.AppendLine("            }");
        csBuilder.AppendLine("        }");
        csBuilder.AppendLine("    }");
        csBuilder.AppendLine("}");

        //输出路径：和配置表实体类放在同一个目录下
        if (!Directory.Exists(csOutputFolderPath))
            Directory.CreateDirectory(csOutputFolderPath);

        File.WriteAllText(csOutputFolderPath + "ConfigRegister.cs", csBuilder.ToString(), Encoding.UTF8);
        Debug.Log("<color=green>自动生成路由脚本成功: ConfigRegister.cs</color>");
    }
}