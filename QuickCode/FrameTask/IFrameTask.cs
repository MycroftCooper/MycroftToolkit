using System;
using UnityEngine;

namespace MycroftToolkit.QuickCode.FrameTask {
    public abstract class FrameTask : IComparable<FrameTask> {
        public float Priority { get; set; }
        public bool IsCompleted { get; private set; }
        protected abstract void Execute();
        public Action<FrameTask> Callback;

        protected FrameTask(float priority = 0, Action<FrameTask> callback = null) {
            Priority = priority;
            Callback = callback;
        }

        public void ExecuteByScheduler() {
            if (IsCompleted) {
                Debug.LogError($"FrameTask is already completed!");
                return;
            }
            Execute();
        }

        protected void SetCompleted() {
            if (IsCompleted) {
                Debug.LogError("FrameTask is already completed!");
                return;
            }
            IsCompleted = true;
            Callback?.Invoke(this);
        }

        public virtual void Reset() {
            IsCompleted = false;
        }

        public int CompareTo(FrameTask other) {
            return other == null ? 1 : Priority.CompareTo(other.Priority); // 升序，小值排前
        }
    }
}