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

        /// <summary>
        /// 获取Hierarchy视图中的对象的路劲
        /// </summary>
        public static void GetPathInHierarchy(this Transform tran, ref string path) {
            while (true) {
                path = string.IsNullOrEmpty(path) ? tran.name : $"{tran.name}/{path}";
                if (tran.parent != null) {
                    tran = tran.parent;
                    continue;
                }

                break;
            }
        }

        public static void PrintSystemInfo() {
            string systemInfo =
                $"OS:{SystemInfo.operatingSystem}-{SystemInfo.processorType}-{SystemInfo.processorCount}\n" +
                $"MemorySize:{SystemInfo.systemMemorySize}\n" +
                $"Graphics:{SystemInfo.graphicsDeviceName} " +
                $"-vendor:{SystemInfo.graphicsDeviceVendor} " +
                $"-memorySize:{SystemInfo.graphicsMemorySize} -deviceVersion:{SystemInfo.graphicsDeviceVersion}";
            Debug.Log(systemInfo);
        }
    }
}
