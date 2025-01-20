using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Least Recently Used，最近最少使用缓存是一种常见的缓存淘汰算法。其基本原理是，当缓存满时，淘汰最久未被使用的项

namespace MycroftToolkit.QuickCode.Structure {
    public class LRUCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        private int _size; // 缓存容量
        private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _cache; // 存储缓存数据
        private readonly LinkedList<KeyValuePair<TKey, TValue>> _order; // 存储访问顺序，最近访问的在前面
        
        public int Size => _size;
        public int Count => _cache.Count;
        
        public IEnumerable<TKey> Keys => _order.Select(node => node.Key);
        public IEnumerable<TValue> Values => _order.Select(node => node.Value);

        public LRUCache(int size) {
            if (size < 1) throw new ArgumentException("Size must be greater than 0.");
            _size = size;
            _cache = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(size);
            _order = new LinkedList<KeyValuePair<TKey, TValue>>();
        }
        
        public void Resize(int newSize) {
            if (newSize < 1) throw new ArgumentException("Size must be greater than 0.");
            while (_cache.Count > newSize) {
                // 移除最久未使用的条目
                var lastNode = _order.Last;
                _cache.Remove(lastNode.Value.Key);
                _order.RemoveLast();
            }
            _size = newSize;
        }

        public bool ContainsKey(TKey key) => _cache.ContainsKey(key);
        
        /// <summary>
        /// 获取缓存中的值
        /// </summary>
        public TValue Get(TKey key) {
            if (!_cache.TryGetValue(key, out var node)) return default;
            // 将该节点移动到队列的前面
            _order.Remove(node);
            _order.AddFirst(node);
            return node.Value.Value;
        }
        
        public KeyValuePair<TKey, TValue>? GetLeastRecentlyUsed() {
            if (_order.Count == 0) return null;
            var lastNode = _order.Last;
            return lastNode.Value;
        }

        /// <summary>
        /// 添加或更新缓存中的值
        /// </summary>
        public void Put(TKey key, TValue value) {
            if (_cache.TryGetValue(key, out var node)) {
                node.Value = new KeyValuePair<TKey, TValue>(key, value);// 更新节点的值
                _order.Remove(node);
                _order.AddFirst(node);
            } else {
                if (_cache.Count >= _size) { // 如果缓存满了，移除最久未使用的条目
                    var lastNode = _order.Last;
                    _cache.Remove(lastNode.Value.Key);
                    _order.RemoveLast();
                }
                // 插入新条目
                var newNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
                _cache[key] = newNode;
                _order.AddFirst(newNode);
            }
        }
        
        public bool Remove(TKey key) {
            if (!_cache.TryGetValue(key, out var node)) return false;
            _order.Remove(node);
            _cache.Remove(key);
            return true;
        }
        
        public void Clear() {
            _cache.Clear();
            _order.Clear();
        }

        public override string ToString() 
            => string.Join(" | ", _order.Select(node => $"{node.Key}:{node.Value}"));
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            foreach (var item in _order) {
                yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }
    }
}