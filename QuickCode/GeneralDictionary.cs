using System.Collections.Generic;
using UnityEngine;

namespace  MycroftToolkit.QuickCode {
    public class GeneralDictionary<TKey> where TKey : notnull {
        public Dictionary<TKey, object> Dict;
        public int Count => Dict.Count;
        public GeneralDictionary() => Dict = new Dictionary<TKey, object>();
        public void Add(TKey key, object value) => Dict.Add(key, value);
        public TValue Get<TValue>(TKey key) {
            if (Dict.TryGetValue(key, out var output)) {
                return (TValue)output;
            }
            Debug.LogError($"字典中未找到键{key}");
            return default;
        }
        public bool Set(TKey key, object value, bool forceAdd = false) {
            if (!Dict.ContainsKey(key)) {
                if (!forceAdd) {
                    return false;
                }
                Add(key, value);
            }
            Dict[key] = value;
            return true;
        }
        public bool ContainsKey(TKey key) => Dict.ContainsKey(key);
        public bool ContainsValue(object value) => Dict.ContainsValue(value);
        public bool Remove(TKey key) => Dict.Remove(key);
        public void Clear() => Dict.Clear();
    }
}
