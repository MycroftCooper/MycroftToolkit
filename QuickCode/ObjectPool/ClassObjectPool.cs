using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MycroftToolkit.QuickCode {
    public class ClassObjectPool : PoolBase {
        public override void Init(ObjectPoolData<IPoolObject> poolData) {
            if (!poolData.ParameterCheck()) return;
            PoolData = poolData;

            for (int i = 0; i < PoolData.DefaultSize; i++) {
                AddNewPoolObject();
            }
        }

        public override IPoolObject Take() {
            if (PoolData.UnTakenCount == 0 && !PoolData.IsDynamic) {
                return PoolData.InitPoolObjectHandle();
            }

            ExpandPool();

            var output = PoolData.UnTakenPool.Dequeue();
            PoolData.HasTakenPool.Add(output);
            output.OnTakeHandler();
            return output;
        }

        public override bool Recycle(IPoolObject target) {
            if(target == null)return false;
            bool canRemove = PoolData.HasTakenPool.Remove(target);
            if (!canRemove) {
                return false;
            }
            target.OnRecycleHandler();

            if (PoolData.IsDynamic && !PoolData.NeedExpand && PoolData.Size > PoolData.DefaultSize) {
                target.OnDestroyHandler();
                return true;
            }
            PoolData.UnTakenPool.Enqueue(target);
            return true;
        }
        
        private void ExpandPool() {
            if (!PoolData.IsDynamic || !PoolData.NeedExpand) {
                return;
            }
            int expandCount = PoolData.ExpandCount;
            for (int i = 0; i < expandCount; i++) {
                AddNewPoolObject();
            }
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
