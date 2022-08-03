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

        /// <summary>
        /// 只设置颜色不改变alpha
        /// </summary>
        public static void SetColor(this Image image, Color color) {
            Color tempColor = color;
            tempColor.a = image.color.a;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetAlpha(this Image image, float alpha) {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha) {
            Color tempColor = spriteRenderer.color;
            tempColor.a = alpha;
            spriteRenderer.color = tempColor;
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
