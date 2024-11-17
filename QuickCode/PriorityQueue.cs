using System;
using System.Collections;
using System.Collections.Generic;

namespace MycroftToolkit.QuickCode {
    public class PriorityQueue<T> : IEnumerable<T> where T : IComparable<T> {
        private readonly List<T> _data = new();
        private readonly HashSet<T> _set = new();
        public int Count => _data.Count;
        public bool IsEmpty => _data.Count == 0;

        private readonly IComparer<T> _comparer;

        public PriorityQueue(IComparer<T> comparer = null) {
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public bool Contains(T item) => _set.Contains(item);

        public bool Enqueue(T item) {
            if (!_set.Add(item)) {
                return false;
            }
            _data.Add(item);
            HeapifyUp(_data.Count - 1);
            return true;
        }

        public T Dequeue() {
            if (IsEmpty) throw new InvalidOperationException("Queue is empty.");

            int lastIndex = _data.Count - 1;
            T frontItem = _data[0];
            _data[0] = _data[lastIndex];
            _data.RemoveAt(lastIndex);

            if (_data.Count > 0) {
                HeapifyDown(0);
            }

            _set.Remove(frontItem);
            return frontItem;
        }

        public bool Remove(T item) {
            if (!_set.Remove(item)) {
                return false;
            }

            int index = _data.IndexOf(item);
            int lastIndex = _data.Count - 1;

            if (index == lastIndex) {
                _data.RemoveAt(lastIndex);
            } else {
                _data[index] = _data[lastIndex];
                _data.RemoveAt(lastIndex);
                if (index > 0 && Compare(_data[index], _data[(index - 1) / 2]) < 0) {
                    HeapifyUp(index);
                } else {
                    HeapifyDown(index);
                }
            }
            return true;
        }

        public void Clear() {
            _data.Clear();
            _set.Clear();
        }

        public T Peek() {
            if (IsEmpty) throw new InvalidOperationException("Queue is empty.");
            return _data[0];
        }
        
        private int Compare(T x, T y) {
            return _comparer.Compare(x, y);
        }
        
        private void HeapifyUp(int index) {
            T item = _data[index];
            int childIndex = index;
            while (childIndex > 0) {
                int parentIndex = (childIndex - 1) / 2;
                if (Compare(item, _data[parentIndex]) >= 0) break;

                // 将父节点向下移动
                _data[childIndex] = _data[parentIndex];
                childIndex = parentIndex;
            }
            _data[childIndex] = item; // 最后将元素放到合适的位置
        }

        private void HeapifyDown(int index) {
            int lastIndex = _data.Count - 1;
            T item = _data[index];
            int parentIndex = index;

            while (true) {
                int leftChildIndex = 2 * parentIndex + 1;
                if (leftChildIndex > lastIndex) break;

                int rightChildIndex = leftChildIndex + 1;
                int minChildIndex = leftChildIndex;
                if (rightChildIndex <= lastIndex && Compare(_data[rightChildIndex], _data[leftChildIndex]) < 0) {
                    minChildIndex = rightChildIndex;
                }

                if (Compare(item, _data[minChildIndex]) <= 0) break;

                // 将子节点向上移动
                _data[parentIndex] = _data[minChildIndex];
                parentIndex = minChildIndex;
            }
            _data[parentIndex] = item; // 最后将元素放到合适的位置
        }
        
        public IEnumerator<T> GetEnumerator() {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}