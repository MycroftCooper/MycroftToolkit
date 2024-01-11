using System;
using System.Collections.Generic;
using System.Linq;

namespace RGScript.Util.QuickCode {
    public class ValueSortedDictionary<TKey, TValue> 
        where TKey : IComparable<TKey> 
        where TValue : IComparable<TValue>  {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly SortedSet<KeyValuePair<TKey, TValue>> _sortedSet;

        public ValueSortedDictionary() : this(Comparer<TValue>.Default) { }

        public ValueSortedDictionary(IComparer<TValue> valueComparer) {
            _dictionary = new Dictionary<TKey, TValue>();
            _sortedSet = new SortedSet<KeyValuePair<TKey, TValue>>(new ValueComparer(valueComparer));
        }

        public void Add(TKey key, TValue value) {
            var entry = new KeyValuePair<TKey, TValue>(key, value);
            _dictionary.Add(key, value);
            _sortedSet.Add(entry);
        }

        public bool Remove(TKey key) {
            if (!_dictionary.TryGetValue(key, out var value)) {
                return false;
            }

            var entry = new KeyValuePair<TKey, TValue>(key, value);

            _dictionary.Remove(key);
            return _sortedSet.Remove(entry);
        }
        
        public bool TryGetValue(TKey key, out TValue value) {
            return _dictionary.TryGetValue(key, out value);
        }
        
        public IEnumerable<TKey> GetKeysForValue(TValue v) {
            foreach (var kvp in _dictionary) {
                if (kvp.Value.Equals(v)) {
                    yield return kvp.Key;
                }
            }
        }

        public KeyValuePair<TKey, TValue> GetEntryAtRank(int i) {
            if (i < 0 || i >= _sortedSet.Count) {
                throw new ArgumentOutOfRangeException(nameof(i), "Rank is out of range.");
            }
            return _sortedSet.ElementAt(i);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetEntriesSortedByValue() {
            return _sortedSet;
        }
        public IEnumerable<KeyValuePair<TKey, TValue>> GetEntriesSortedByValueDescending() {
            return _sortedSet.Reverse();
        }

        private class ValueComparer : IComparer<KeyValuePair<TKey, TValue>> {
            private readonly IComparer<TValue> _baseComparer;

            public ValueComparer(IComparer<TValue> baseComparer) {
                _baseComparer = baseComparer ?? throw new ArgumentNullException(nameof(baseComparer));
            }

            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) {
                int result = _baseComparer.Compare(x.Value, y.Value);
                return result == 0 ? x.Key.CompareTo(y.Key) : result;
            }
        }
    }
}
