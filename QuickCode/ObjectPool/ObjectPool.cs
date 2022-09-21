using System;
using System.Collections.Generic;
using System.Threading;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MycroftToolkit.QuickCode {
    public interface IPoolObject {
        public void OnTakeHandler();
        public void OnRecycleHandler();

        public void OnDestroyHandler();
    }

    public class ObjectPoolData<TIPoolObject> where TIPoolObject : IPoolObject {
        public Queue<TIPoolObject> UnTakenPool;
        public List<TIPoolObject> HasTakenPool;

        public readonly int DefaultSize;
        public int Size =>UnTakenPool.Count + HasTakenPool.Count;
        public int UnTakenCount => UnTakenPool.Count;
        public int HasTakenCount => HasTakenPool.Count;

        /// <summary>
        /// 是否为动态池
        /// </summary>
        public bool IsDynamic;

        /// <summary>
        /// 动态池阈值(百分比)
        /// 可用对象比小于此值时扩展
        /// </summary>
        public float ExpandThresholdRate;

        /// <summary>
        /// 动态池增长率(百分比)
        /// </summary>
        public float ExpandRate;

        /// <summary>
        /// 动态池增长速率(每帧初始化个数)
        /// 0为立刻全部初始化
        /// </summary>
        public int OnceMaxExpandCount;

        public int ExpandThreshold => (int)(Size * ExpandThresholdRate);
        public bool NeedExpand => UnTakenCount <= ExpandThreshold;

        public int ExpandCount {
            get {
                if (OnceMaxExpandCount == 0) {
                    return (int)(Size * ExpandRate);
                }
                return Math.Min((int)(Size * ExpandRate), OnceMaxExpandCount);
            }
        }
        
        public delegate TIPoolObject InitPoolObject();

        public readonly InitPoolObject InitPoolObjectHandle;

        public ObjectPoolData(int size, InitPoolObject initPoolObjectHandle,
            bool isDynamic = false, float expandThresholdRate = 1, float expandRate = 0.5f, int onceMaxExpandCount = 0) {
            InitPoolObjectHandle = initPoolObjectHandle;
            
            DefaultSize = size;
            IsDynamic = isDynamic;
            ExpandThresholdRate = expandThresholdRate;
            ExpandRate = expandRate;
            OnceMaxExpandCount = onceMaxExpandCount;

            UnTakenPool = new Queue<TIPoolObject>();
            HasTakenPool = new List<TIPoolObject>();
        }

        public bool ParameterCheck() {
            bool output = true;
            if (DefaultSize <= 0) {
                Debug.LogError($"ObjectPool>InitError>对象池大小非法:{DefaultSize}");
                output = false;   
            }

            if (InitPoolObjectHandle == null) {
                Debug.LogError($"ObjectPool>InitError>对象生成函数指针为空");
                output = false;
            }
            
            if (ExpandThresholdRate <= 0 || ExpandThresholdRate > 1) {
                Debug.LogError($"ObjectPool>InitError>对象池阈值非法:{ExpandThresholdRate},应当0<对象池阈值=<1");
                output = false;
            }

            if (ExpandRate < 0) {
                Debug.LogError($"ObjectPool>InitError>对象池增长率非法:{ExpandRate}");
                output = false;
            }
            
            if (OnceMaxExpandCount < 0) {
                Debug.LogError($"ObjectPool>InitError>对象池增长速度非法:{OnceMaxExpandCount}");
                output = false;
            }
            return output;
        }
    }

    public abstract class PoolBase {
        protected ObjectPoolData<IPoolObject> PoolData;
        public abstract void Init(ObjectPoolData<IPoolObject> poolData);
        public abstract IPoolObject Take();
        public abstract bool Recycle(IPoolObject target);
        
        public List<IPoolObject> Take(int count) {
            List<IPoolObject> output = new List<IPoolObject>(count);
            for (int i = 0; i < count; i++) {
                output.Add(Take());
            }
            return output;
        }
        public List<IPoolObject> GetAllTaken() {
            List<IPoolObject> output = new List<IPoolObject>();
            for (int i = PoolData.HasTakenCount; i >= 0; i--) {
                if (PoolData.HasTakenPool[i] == null) {
                    PoolData.HasTakenPool.RemoveAt(i);
                    continue;
                }
                output.Add(PoolData.HasTakenPool[i]);
            }

            return output;
        }
        
        public void RecycleAll() {
            PoolData.HasTakenPool.ForEach((x)=>Recycle(x));
            PoolData.HasTakenPool.Clear();
        }

        public void CleanPool() {
            RecycleAll();
            PoolData.UnTakenPool.ForEach((x) => x.OnDestroyHandler());
            PoolData.UnTakenPool.Clear();
        }

        protected void AddNewPoolObject() {
            PoolData.UnTakenPool.Enqueue(PoolData.InitPoolObjectHandle());
        }
    }

    
    public class ObjectPool : PoolBase {
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
