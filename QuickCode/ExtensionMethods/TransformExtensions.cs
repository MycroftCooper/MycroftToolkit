using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class TransformExtensions {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
            T t = go.GetComponent<T>();
            if (null == t) {
                t = go.AddComponent<T>();
            }
            return t;
        }

        public static void RemoveComponent<TComponent>(this GameObject obj, bool immediate = false) {
            TComponent component = obj.GetComponent<TComponent>();
            if (component == null) return;
            if (immediate) {
                Object.DestroyImmediate(component as Object, true);
            } else {
                Object.Destroy(component as Object);
            }
        }

        public static void LocalRest(this Transform tr) {
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }
        
        public static void Reset(this Transform tr) {
            tr.position = Vector3.zero;
            tr.rotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }
        
        public static Transform FindParent(this Transform tr, string name) {
            Transform parent = tr.parent;
            while (parent != null && parent.name != name)
                parent = parent.parent;
            return parent;
        }
        
        public static T GetGetComponentInAllParents<T>(this GameObject go) where T:MonoBehaviour{
            Transform parent = go.transform.parent;
            while (parent != null ) {
                T output = parent.GetComponent<T>();
                if (output != null) return output;
                parent = parent.parent;
            }
            return null;
        }

        public static List<Transform> FindAllNameContains(this Transform tr, string name) {
            List<Transform> output = new List<Transform>();
            for (int i = 0; i < tr.childCount; i++) {
                Transform child = tr.GetChild(i);
                output.AddRange(child.FindAllNameContains(name));
                if (child.name.Contains(name)) {
                    output.Add(child);
                }
            }
            return output;
        }

        public static string GetPathInScenes(this Transform transform) {
            string path = transform.name;
            while (transform.parent != null) {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
        
        public static string GetPathInScenes(this GameObject gameObject) {
            var transform = gameObject.transform;
            string path = transform.name;
            while (transform.parent != null) {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
        
        public static void DestroyAllChildren(this Transform tr) {
            if (tr.childCount <= 0) {
                return;
            }
            for (int i = tr.childCount - 1; i >= 0; i--) {
                Object.Destroy(tr.GetChild(i).gameObject);
            }
        }
    }
}
