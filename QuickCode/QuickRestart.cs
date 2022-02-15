using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class QuickRestart {
    [MenuItem("Tools/快速重启")]
    public static void RestartUnity() {
        EditorApplication.OpenProject(Application.dataPath.Replace("Assets", string.Empty));
    }
}
#endif
