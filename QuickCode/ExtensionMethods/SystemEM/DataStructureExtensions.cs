using System;
using System.Collections.Generic;
using System.Linq;

namespace MycroftToolkit.QuickCode {
    public static class DataStructureExtensions {
        /// <summary>
        /// 数组的拷贝Clone
        /// </summary>
        public static T[] Clone<T>(this T[] resArr) where T : ICloneable {
            if (resArr == null) return null;
            T[] cloneArr = new T[resArr.Length];
            for (int i = 0, count = resArr.Length; i < count; ++i) {
                cloneArr[i] = (T)(resArr[i].Clone());
            }
            return cloneArr;
        }

        /// <summary>
        /// 链表List的拷贝Clone
        /// </summary>
        public static List<T> Clone<T>(this List<T> resList) where T : ICloneable {
            if (resList == null) return null;
            List<T> cloneList = new List<T>();
            for (int i = 0, count = resList.Count; i < count; ++i) {
                T res = resList[i];
                T dest = (T)(res.Clone());
                cloneList.Add(dest);
            }
            return cloneList;
        }

        /// <summary>
        /// 字典Dictionary的拷贝Clone
        /// </summary>
        public static Dictionary<T1, T2> Clone<T1, T2>(this Dictionary<T1, T2> resDic) where T2 : ICloneable {
            if (resDic == null) return null;
            Dictionary<T1, T2> cloneDic = new Dictionary<T1, T2>();
            foreach (KeyValuePair<T1, T2> keyValue in resDic) {
                T1 resKey = keyValue.Key;
                T1 desKey = resKey;
                if (resKey is ICloneable resCloneable) {
                    desKey = (T1)(resCloneable.Clone());
                }

                T2 desValue = (T2)(keyValue.Value.Clone());
                cloneDic.Add(desKey, desValue);
            }
            return cloneDic;
        }

        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            Action<KeyValuePair<TKey, TValue>> action) {
            if (action == null || dictionary.Count == 0) return;
            for (int i = 0; i < dictionary.Count; i++) {
                var item = dictionary.ElementAt(i);
                action(item);
            }
        }

        public static void ForLoop(this int i, Action<int> callBack) {
            for (int count = 0; count < i; count++) {
                callBack(count);
            }
        }
        public static void ForLoopInverted(this int i, Action<int> callBack) {
            for (int count = i; count >= 0; count--) {
                callBack(count);
            }
        }
        
    }
}