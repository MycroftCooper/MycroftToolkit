using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode.FrameTask {
    public class FrameTaskScheduler<T> : MonoBehaviour where T : FrameTask {
        private readonly PriorityQueue<T> _requestQueue = new();
        public int TaskCount => _requestQueue.Count;
        
        protected bool ContainsTask(T t) {
            return _requestQueue.Contains(t);
        }

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

        public bool isPause;
        public bool IsExecutingInFrame { get; private set; }
        
        public float miniFrameRate = 20f;
        private float _currentFrameRate;
        private float _currentTimeSlicing;
        public bool needSmoothFrameRate;
        public float smoothFactor = 0.3f; // 平滑系数，调整平滑效果的强度
        private float _smoothedFrameRate; // 平滑后的帧率
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
            IsExecutingInFrame = true;
            float timeSpent = 0f;
            while (_requestQueue.Count > 0 && timeSpent < _currentTimeSlicing) {
                var task = _requestQueue.Peek(); // 获取优先级最高的任务
                
                float startTime = Time.realtimeSinceStartup;
                task.ExecuteByScheduler(); // 执行任务
                if (task.IsCompleted) {
                    _requestQueue.Remove(task); // 移除已完成的任务
                }
                float endTime = Time.realtimeSinceStartup;
                float taskTime = endTime - startTime; // 记录执行时间
                timeSpent += taskTime;
                
                if (timeSpent >= _currentTimeSlicing) {
                    yield return null; // 暂停一帧
                    if(isPause)break;
                    CalculateTimeSlicing();
                }
            }
            IsExecutingInFrame = false;
        }
        
        private void CalculateTimeSlicing() {// 计算当前帧率和时间切片
            float currentFrameRate = 1f / Time.deltaTime;

            if (needSmoothFrameRate) {
                // 使用指数加权移动平均来平滑当前帧率
                if (_smoothedFrameRate == 0f) {
                    _smoothedFrameRate = currentFrameRate; // 如果是第一次计算，则直接使用当前帧率
                } else {
                    _smoothedFrameRate = smoothFactor * currentFrameRate + (1 - smoothFactor) * _smoothedFrameRate;
                }
                currentFrameRate = _smoothedFrameRate;
            }
            
            float dFrameRate = Mathf.Max(0, currentFrameRate - miniFrameRate);
            if (dFrameRate == 0) {
                _currentTimeSlicing = 0f;
            } else {
                _currentTimeSlicing = 1f / dFrameRate;
            }
        }
    }
}