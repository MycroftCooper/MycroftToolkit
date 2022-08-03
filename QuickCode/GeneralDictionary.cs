using System.Collections.Generic;

namespace  MycroftToolkit.QuickCode {
    public class GeneralDictionary<TKey> where TKey : notnull {
        public Dictionary<TKey, object> Dict;
        public int Count => Dict.Count;
        public GeneralDictionary() => Dict = new Dictionary<TKey, object>();
        public void Add(TKey key, object value) => Dict.Add(key, value);
        public TValue Get<TValue>(TKey key) {
            if (Dict.ContainsKey(key))
                return (TValue)Dict[key];
            return default;
        }
        public bool Set(TKey key, object value) {
            if (!Dict.ContainsKey(key)) return false;
            Dict[key] = value;
            return true;
        }
        public bool ContainsKey(TKey key) => Dict.ContainsKey(key);
        public bool ContainsValue(object value) => Dict.ContainsValue(value);
        public bool Remove(TKey key) => Dict.Remove(key);
        public void Clear() => Dict.Clear();
    }
}
