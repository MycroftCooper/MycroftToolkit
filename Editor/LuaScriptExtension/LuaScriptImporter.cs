using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace LuaScriptExtension {
    [ScriptedImporter(1, "lua")]
    public class LuaScriptImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            string scriptText = File.ReadAllText(ctx.assetPath);// 读取文件内容
            TextAsset textAsset = new TextAsset(scriptText);// 创建一个新的TextAsset对象
            ctx.AddObjectToAsset("text", textAsset); // 将TextAsset对象添加到导入的资产上下文
            ctx.SetMainObject(textAsset);
        }
    }
}