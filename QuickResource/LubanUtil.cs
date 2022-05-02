using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public static class LubanUtil {
    [MenuItem("QuickResources/LubanUtil/ImportExcel")]
    public static void ImportExcel() {
        Process proc = new Process();//new 一个Process对象
        string targetDir = string.Format(@"Luban/");//文件目录

        proc.StartInfo.WorkingDirectory = targetDir;
        proc.StartInfo.FileName = "gen_code_json.bat";//文件名字


        proc.Start();
        proc.WaitForExit();
    }
}
