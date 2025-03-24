using System;
using System.Collections;
using System.Collections.Generic;

namespace MycroftToolkit.QuickCode {
    public class BiDictionary<TKey, TValue> : IEnumerable {
        private readonly Dictionary<TKey, TValue> keyToValue = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TValue, TKey> valueToKey = new Dictionary<TValue, TKey>();
        
        public void Add(TKey key, TValue value) {
            if (keyToValue.ContainsKey(key))
                throw new ArgumentException("相同的键已存在", nameof(key));
            if (valueToKey.ContainsKey(value))
                throw new ArgumentException("相同的值已存在", nameof(value));

            keyToValue.Add(key, value);
            valueToKey.Add(value, key);
        }
        
        public bool RemoveByKey(TKey key) {
            if (!keyToValue.Remove(key, out var value)) return false;
            valueToKey.Remove(value);
            return true;
        }
        
        public bool RemoveByValue(TValue value) {
            if (!valueToKey.Remove(value, out var key)) return false;
            keyToValue.Remove(key);
            return true;
        }
        
        public TValue GetValue(TKey key) {
            return keyToValue[key];
        }
        
        public TKey GetKey(TValue value) {
            return valueToKey[value];
        }
        
        public bool TryGetValueByKey(TKey key, out TValue value) {
            return keyToValue.TryGetValue(key, out value);
        }
        
        public bool TryGetKeyByValue(TValue value, out TKey key) {
            return valueToKey.TryGetValue(value, out key);
        }
        
        public void Clear() {
            keyToValue.Clear();
            valueToKey.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => keyToValue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}