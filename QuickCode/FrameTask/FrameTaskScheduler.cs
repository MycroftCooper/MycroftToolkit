using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode.FrameTask {
    public class FrameTaskScheduler<T> : MonoBehaviour where T : FrameTask {
        #region 任务操作
        private readonly PriorityQueue<T> _requestQueue = new();
        public IEnumerable<T> Tasks => _requestQueue;
        public int TaskCount => _requestQueue.Count;
        protected bool ContainsTask(T t) => _requestQueue.Contains(t);

        protected void AddTask(T t) {
            if (!_requestQueue.Enqueue(t)) {
                Debug.LogError("Already have task!");
            }
        }

        protected void CancelTask(T t) {
            if (!_requestQueue.Remove(t)) {
                Debug.LogError("Task not found!");
            }
        }

        protected void CancelAllTask() {
            _requestQueue.Clear();
        }

        protected void ChangeTaskPriority(T t, int newPriority) {
            if (!_requestQueue.Remove(t)) {
                Debug.LogError("Task not found to change priority.");
                return;
            }
            t.Priority = newPriority;
            _requestQueue.Enqueue(t);
        }

        protected void ImmediateExecuteTaskToComplete(T t) {
            if (!_requestQueue.Contains(t)) {
                Debug.LogError("Task not found for immediate execution.");
                return;
            }
            while (!t.IsCompleted) {
                t.ExecuteByScheduler();
            }
            _requestQueue.Remove(t);
        }

        protected void ImmediateExecuteAllTaskToComplete() {
            foreach (var t in _requestQueue) {
                while (!t.IsCompleted) {
                    t.ExecuteByScheduler();
                }
            }
            _requestQueue.Clear();
        }

        private void ExecuteQueueTask() {
            var task = _requestQueue.Peek(); // 获取优先级最高的任务
            task.ExecuteByScheduler(); // 执行任务
            if (task.IsCompleted) {
                _requestQueue.Remove(task); // 移除已完成的任务
            }
        }
        #endregion

        #region 任务调度
        public bool isPause;
        public bool IsExecutingInFrame { get; private set; }
        
        public int minExecuteInFrame = 1;
        public float minFrameRate = 20f;
        private float _currentFrameRate;
        private float _currentTimeSlicing;
        protected virtual void Update() {
            if (isPause || IsExecutingInFrame || _requestQueue.Count <= 0) return;
            CalculateTimeSlicing();
            StartCoroutine(ExecuteTasksForCurrentFrame());
        }
        
        protected void OnDisable() {
            IsExecutingInFrame = false;
        }
        
        protected virtual void OnDestroy() {
            CancelAllTask();
        }

        private IEnumerator ExecuteTasksForCurrentFrame() {
            if(_currentTimeSlicing == 0 && minExecuteInFrame <= 0)yield break;
            
            IsExecutingInFrame = true;
            
            int taskCount = 0;
            float timeSpent = 0f;
            
            while (_requestQueue.Count > 0 && (timeSpent < _currentTimeSlicing || taskCount < minExecuteInFrame)) {
                float startTime = Time.realtimeSinceStartup;
                ExecuteQueueTask(); // 执行任务
                float endTime = Time.realtimeSinceStartup;
                
                float taskTime = endTime - startTime;
                timeSpent += taskTime;
                taskCount++;

                // 如果时间片已用尽，暂停一帧
                if (timeSpent >= _currentTimeSlicing) {
                    yield return null; // 暂停一帧
                    if (isPause) break; // 如果已暂停，退出
                    CalculateTimeSlicing(); // 重新计算时间切片
                    timeSpent = 0f; // 重置时间统计
                    taskCount = 0; // 重置任务计数
                }
            }
            IsExecutingInFrame = false;
        }
        
        private void CalculateTimeSlicing() {// 计算当前帧率和时间切片
            float currentFrameRate = 1f / Time.deltaTime;
            float dFrameRate = Mathf.Max(0, currentFrameRate - minFrameRate);
            if (dFrameRate == 0) {
                _currentTimeSlicing = 0f;
            } else {
                _currentTimeSlicing = 1f / dFrameRate;
            }
        }
        #endregion
    }
}