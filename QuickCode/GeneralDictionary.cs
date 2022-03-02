using System.Collections.Generic;

namespace  MycroftToolkit.QuickCode {
    public class GeneralDictionary<Tkey> where Tkey : notnull {
        public Dictionary<Tkey, object> _dict;
        public int Count { get => _dict.Count; }
        public GeneralDictionary() => _dict = new Dictionary<Tkey, object>();
        public void Add(Tkey key, object value) => _dict.Add(key, value);
        public Tvalue Get<Tvalue>(Tkey key) {
            if (_dict.ContainsKey(key))
                return (Tvalue)_dict[key];
            return default;
        }
        public bool Set(Tkey key, object value) {
            if (_dict.ContainsKey(key)) {
                _dict[key] = value;
                return true;
            }
            return false;
        }
        public bool ContainsKey(Tkey key) => _dict.ContainsKey(key);
        public bool ContainsValue(object value) => _dict.ContainsValue(value);
        public bool Remove(Tkey key) => _dict.Remove(key);
        public void Clear() => _dict.Clear();
    }
}
