using System;
using System.Collections.Generic;
using UnityEngine;

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
        public int Size => UnTakenPool.Count + HasTakenPool.Count;
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
            bool isDynamic = false, float expandThresholdRate = 1, float expandRate = 0.5f,
            int onceMaxExpandCount = 0) {
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
            PoolData.HasTakenPool.ForEach((x) => Recycle(x));
            PoolData.HasTakenPool.Clear();
        }

        public void CleanPool() {
            RecycleAll();
            foreach (var unTakeObject in PoolData.UnTakenPool) {
                unTakeObject.OnDestroyHandler();
            }

            PoolData.UnTakenPool.Clear();
        }

        protected void AddNewPoolObject() {
            PoolData.UnTakenPool.Enqueue(PoolData.InitPoolObjectHandle());
        }
    }
}