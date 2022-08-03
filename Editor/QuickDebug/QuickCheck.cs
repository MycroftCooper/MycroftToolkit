using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
public class PrefabChecker : EditorWindow {
    private static string errorInfo = "QuickDebug>PrefabChecker>";
    [MenuItem("QuickDebug/检查预制体")]
    private static void CheckPrefab_All() {
        List<string> listString = new List<string>();
        CollectFiles(Application.dataPath, listString);

        for (int i = 0; i < listString.Count; i++) {
            UpdateProgressBar((float)i / listString.Count);

            string path = listString[i];
            if (!path.EndsWith(".prefab")) continue;
            path = ChangeFilePath(path);
            AssetImporter tmpAssetImport = AssetImporter.GetAtPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(tmpAssetImport.assetPath);

            if (prefab == null) {
                Debug.LogError(errorInfo + "空的预设 ： " + tmpAssetImport.assetPath);
                continue;
            }

            CheckActive(prefab);

            //获取所有的子节点;
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>();
            foreach (var t in transforms) {
                GameObject obj = t.gameObject;
                var components = obj.GetComponents<Component>();

                foreach (var t1 in components) {
                    if (t1 == null) {
                        Debug.LogErrorFormat(obj, errorInfo + "预制体组件丢失! 路径：{0}  对象: {1}", tmpAssetImport.assetPath, obj.name);
                        continue;
                    }
                    CheckReference(t1);
                }
            }
        }
        EditorUtility.ClearProgressBar();
        Debug.Log(errorInfo + "检查完毕!");
    }

    [MenuItem("Assets/检查预制体", false, 38)]
    private static void CheckPrefab_Select() {
        GameObject prefab = Selection.activeObject as GameObject;
        if (!prefab) {
            Debug.LogErrorFormat(prefab, errorInfo + "被检查物体不是预制体!");
            return;
        }

        CheckActive(prefab);

        //获取所有的子节点;
        Transform[] transforms = prefab.GetComponentsInChildren<Transform>();
        foreach (var t in transforms) {
            GameObject obj = t.gameObject;
            var components = obj.GetComponents<Component>();

            foreach (var t1 in components) {
                if (t1 == null) {
                    Debug.LogErrorFormat(obj, errorInfo + "预制体组件丢失! 路径：{0}  对象: {1}", prefab.name, obj.name);
                    continue;
                }
                CheckReference(t1);
            }
        }
    }
    private static void CheckActive(GameObject go) {// 检查是否激活
        if (!go.activeSelf) {
            Debug.LogFormat(go, errorInfo + "未激活: {0}", AssetDatabase.GetAssetPath(go));
        }
    }

    private static void CheckReference(Component component) {// 检查空引用
        var iterator = new SerializedObject(component).GetIterator();
        while (iterator.NextVisible(true)) {
            if (iterator.propertyType == SerializedPropertyType.ObjectReference) {
                if (iterator.objectReferenceValue == null) {
                    Debug.LogWarningFormat(component.gameObject, errorInfo + "引用为空! 引用名: {0}  引用类型: {1}   路径: {2}", iterator.name, component.GetType().Name, AssetDatabase.GetAssetPath(component.gameObject));
                }
            }
        }
    }

    private static void UpdateProgressBar(float progressBar) {// 更新进度条
        EditorUtility.DisplayProgressBar(errorInfo, "检查进度： " + ((int)(progressBar * 100)).ToString() + "%", progressBar);
    }

    private static void CollectFiles(string directory, List<string> outFiles) {// 迭代获取文件路径
        string[] files = Directory.GetFiles(directory);
        outFiles.AddRange(files);

        string[] childDirectories = Directory.GetDirectories(directory);
        if (childDirectories.Length > 0) {
            foreach (var dir in childDirectories) {
                if (string.IsNullOrEmpty(dir)) continue;
                CollectFiles(dir, outFiles);
            }
        }
    }

    //改变路径  
    //这种格式的路径 "C:/Users/XX/Desktop/aaa/New Unity Project/Assets\a.prefab" 改变成 "Assets/a.prefab"
    private static string ChangeFilePath(string path) {
        path = path.Replace("\\", "/");
        path = path.Replace(Application.dataPath + "/", "");
        path = "Assets/" + path;
        return path;
    }
}
#endif
