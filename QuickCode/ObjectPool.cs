using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public class ObjectPool<T> where T : class, new() {
        private Queue<T> _pool;
        public int Size;
        public int CanUseCount => _pool.Count;
        public int UsingCount;
        public bool InitPool(int size = 10) {
            if (size < 1) {
                Debug.LogError("对象池大小出错!");
                return false;
            }
            _pool = new Queue<T>();
            Size = size;
            for (int i = 0; i < Size; i++) {
                _pool.Enqueue(new T());
            }
            UsingCount = 0;
            return true;
        }

        public T GetObject(bool createIfPoolEmpty = true) {
            if (_pool.Count != 0) { // 池子没空
                var output = _pool.Dequeue();
                UsingCount++;
                return output;
            }

            if (!createIfPoolEmpty) return null; // 池子空了
            UsingCount++;
            return new T();
        }

        public bool Recycle(T obj) {
            if (obj == null) return false;
            UsingCount--;
            if (_pool.Count >= Size) {
                return true;
            }
            _pool.Enqueue(obj);
            return true;
        }

        public void CleanPool() {
            while (_pool.Peek() != null) {
                _pool.Dequeue();
            }
            UsingCount = 0;
        }
    }

    public class GameObjectPool {
        private Queue<GameObject> _pool;
        public int Size;
        public int CanUseCount => _pool.Count;
        public int UsingCount;
        public GameObject Prefab;
        public Transform Parent;
        public bool InitPool(GameObject prefab, int size = 10, Transform parent = null, bool setActive = false) {
            if (size < 1 || prefab == null) {
                Debug.LogError("对象池大小或预制体出错!");
                return false;
            }

            _pool = new Queue<GameObject>();
            Size = size;
            Parent = parent;
            Prefab = prefab;

            for (int i = 0; i < Size; i++) {
                _pool.Enqueue(CreateNewGo(setActive));
            }
            UsingCount = 0;

            return true;
        }
        private GameObject CreateNewGo(bool setActive) {
            GameObject newObject = Object.Instantiate(Prefab, Parent, true);
            newObject.SetActive(setActive);
            return newObject;
        }

        public GameObject GetObject(Transform parent = null, bool setActive = true, bool createIfPoolEmpty = true) {
            GameObject output = null;
            if (_pool.Count != 0) { // 池子没空
                output = _pool.Dequeue();
                UsingCount++;
                output.gameObject.SetActive(setActive);
            } else if (createIfPoolEmpty) { // 池子空了
                UsingCount++;
                output = CreateNewGo(setActive);
            }
            if (output != null) output.transform.parent = parent;
            return output;
        }

        public bool Recycle(GameObject obj, bool setActive = false) {
            if (obj == null) return false;
            UsingCount--;
            if (_pool.Count >= Size) {
                Object.Destroy(obj);
                return true;
            }
            obj.gameObject.SetActive(setActive);
            _pool.Enqueue(obj);
            if (Parent != null) obj.transform.parent = Parent;
            return true;
        }

        public void CleanPool() {
            while (_pool.Peek() != null) {
                GameObject go = _pool.Dequeue();
                Object.Destroy(go, 0f);
            }
            UsingCount = 0;
        }
    }

    public class ComponentPool<T> where T : Behaviour, new() {
        public GameObject Parent;
        private Queue<T> _pool;
        public int Size;
        public int CanUseCount => _pool.Count;
        public int UsingCount;

        public bool InitPool(GameObject parent, int size = 10, bool setEnabled = false) {
            if (size < 1 || parent == null) {
                Debug.LogError("对象池大小或父物体出错!");
                return false;
            }
            _pool = new Queue<T>();
            Size = size;
            Parent = parent;

            for (int i = 0; i < Size; i++) {
                _pool.Enqueue(AddNewComponentToParent(setEnabled));
            }
            UsingCount = 0;

            return true;
        }
        private T AddNewComponentToParent(bool setEnabled = false) {
            T output = Parent.AddComponent<T>();
            if (output == null) {
                Debug.LogError("脚本出错!");
                return null;
            }
            output.enabled = setEnabled;
            return output;
        }

        public T GetObject(bool createIfPoolEmpty = true, bool setEnabled = true) {
            T output;
            if (_pool.Count != 0) { // 池子没空
                output = _pool.Dequeue();
                UsingCount++;
                output.enabled = setEnabled;
                return output;
            }
            if (createIfPoolEmpty) { // 池子空了
                UsingCount++;
                output = AddNewComponentToParent();
                return output;
            }
            return null;
        }

        public bool Recycle(T obj, bool setEnabled = false) {
            if (obj == null) return false;
            UsingCount--;
            if (_pool.Count >= Size) {
                Object.Destroy(obj);
                return true;
            }
            obj.enabled = setEnabled;
            _pool.Enqueue(obj);
            return true;
        }

        public void CleanPool() {
            while (_pool.Peek() != null) {
                T obj = _pool.Dequeue();
                Object.Destroy(obj);
            }
            UsingCount = 0;
        }
    }
}
