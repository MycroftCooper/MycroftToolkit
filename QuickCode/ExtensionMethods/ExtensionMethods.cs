using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MycroftToolkit.QuickCode {
    public static class ExtensionMethods {
        public static void RemoveComponent<TComponent>(this GameObject obj, bool immediate = false) {
            TComponent component = obj.GetComponent<TComponent>();
            if (component != null) {
                if (immediate) {
                    UnityEngine.Object.DestroyImmediate(component as UnityEngine.Object, true);
                } else {
                    UnityEngine.Object.Destroy(component as UnityEngine.Object);
                }
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
            tr.localScale = Vector3.zero;
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

        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action) {
            if (action == null || dictionary.Count == 0) return;
            for (int i = 0; i < dictionary.Count; i++) {
                var item = dictionary.ElementAt(i);
                action(item);
            }
        }
    }
}
