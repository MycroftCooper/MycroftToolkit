using UnityEditor;
using UnityEngine;

namespace EditorProjectExtension.ReferenceFinder {
    [CreateAssetMenu(fileName = "QuickReferenceFinderConfig", menuName = "EditorProjectExtension/ReferenceFinderConfig")]
    public class QuickReferenceFinderConfig : ScriptableObject {
        public Object ripGrepDirectory;
        public int rgSearchTimeOutLimit = 10000;

        public string GetRipGrepPath() {
            if (ripGrepDirectory == null) {
                Debug.LogError("RipGrepDirectory reference is missing!");
                return null;
            }
            string path = AssetDatabase.GetAssetPath(ripGrepDirectory);
            path = path.Replace("Assets/", "");
            return path;
        }
    }
}