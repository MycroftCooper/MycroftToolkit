using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MycroftToolkit.QuickCode {
    public class MonoObjectPool {
        
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
        
        public bool InitPool(GameObject prefab, int size = 10, string newParentName = null, bool setActive = false) {
            if (size < 1 || prefab == null) {
                Debug.LogError("对象池大小或预制体出错!");
                return false;
            }

            _pool = new Queue<GameObject>();
            Size = size;
            Transform parent = null;
            if (! string.IsNullOrEmpty(newParentName)) {
                parent = new GameObject("newParentName").transform;
            }
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
}