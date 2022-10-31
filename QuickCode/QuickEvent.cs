using System;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public class QuickEvent : Singleton<QuickEvent> {
        public delegate void EventHandle<in T>(T message) where T : struct;

        private readonly Dictionary<Type, Delegate> _delegateDict = new();

        public void SubscribeListener<T>(EventHandle<T> handle) where T : struct {
            if (handle == null) {
                Debug.LogError("QuickEvent>Error>新增监听器为Null");
                return;
            }
            var type = typeof(T);
            if (_delegateDict.TryGetValue(type, out var delegates)) {
                _delegateDict[type] = Delegate.Combine(delegates, handle);
            }
            else {
                _delegateDict[type] = handle;
            }
        }

        public void UnsubscribeListener<T>(EventHandle<T> handle) where T : struct {
            var type = typeof(T);
            if (!_delegateDict.TryGetValue(type, out var delegates)) {
                Debug.LogError($"QuickEvent>Error>{type}事件未订阅监听器");
                return;
            }

            _delegateDict[type] = Delegate.Remove(delegates, handle);
            if (_delegateDict[type] == null) {
                _delegateDict.Remove(type);
            }
        }

        public void DispatchMessage<T>(T message) where T : struct {
            if (!_delegateDict.TryGetValue(typeof(T), out var delegates)) {
                return;
            }

            var handle = delegates as EventHandle<T>;
            handle?.Invoke(message);
        }
    }
}