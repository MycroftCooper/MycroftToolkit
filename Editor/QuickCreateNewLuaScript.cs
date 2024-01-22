using UnityEngine;
using UnityEditor;
using System.IO;

namespace EditorProjectExtension {
    public class QuickCreateNewLuaScript : MonoBehaviour {
        private const string Content = ""; 
        
        [MenuItem("Assets/Create/Lua Script", false, 80)]
        public static void CreateNewLuaScript() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);// 获取当前选择的路径
            if (path == "") {
                path = "Assets";
            } else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string fullPath = AssetDatabase.GenerateUniqueAssetPath(path + "/NewLuaScript.lua");
            File.WriteAllText(fullPath, Content);
            AssetDatabase.Refresh();// 刷新AssetDatabase，使新文件在Unity编辑器中显示
            
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
            Selection.activeObject = obj;
        }
    }
}