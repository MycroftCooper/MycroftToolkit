using System;

// Warning! 
// 这是有点脆弱的事件模式实现。建议不要在状态机之外使用它们
// 
namespace MycroftToolkit.QuickTool.FSM {
    public class StateEvent {
        private readonly Func<int> _getStateInt;
        private readonly Func<bool> _isInvokeAllowed;
        private readonly Action[] _routingTable;

        public StateEvent(Func<bool> isInvokeAllowed, Func<int> stateProvider, int capacity) {
            _isInvokeAllowed = isInvokeAllowed;
            _getStateInt = stateProvider;
            _routingTable = new Action[capacity];
        }

        internal void AddListener(int stateInt, Action listener) {
            _routingTable[stateInt] = listener;
        }

        public void Invoke() {
            if (_isInvokeAllowed != null && !_isInvokeAllowed()) {
                return;
            }

            Action call = _routingTable[_getStateInt()];
            call?.Invoke();
        }
    }

    public class StateEvent<T> {
        private readonly Func<int> _getStateInt;
        private readonly Func<bool> _isInvokeAllowed;
        private readonly Action<T>[] _routingTable;

        public StateEvent(Func<bool> isInvokeAllowed, Func<int> stateProvider, int capacity) {
            _isInvokeAllowed = isInvokeAllowed;
            _getStateInt = stateProvider;
            _routingTable = new Action<T>[capacity];
        }

        internal void AddListener(int stateInt, Action<T> listener) {
            _routingTable[stateInt] = listener;
        }

        public void Invoke(T param) {
            if (_isInvokeAllowed != null && !_isInvokeAllowed()) {
                return;
            }

            Action<T> call = _routingTable[_getStateInt()];
            call?.Invoke(param);
        }
    }

    public class StateEvent<T1, T2> {
        private readonly Func<int> _getStateInt;
        private readonly Func<bool> _isInvokeAllowed;
        private readonly Action<T1, T2>[] _routingTable;

        public StateEvent(Func<bool> isInvokeAllowed, Func<int> stateProvider, int capacity) {
            _isInvokeAllowed = isInvokeAllowed;
            _getStateInt = stateProvider;
            _routingTable = new Action<T1, T2>[capacity];
        }

        internal void AddListener(int stateInt, Action<T1, T2> listener) {
            _routingTable[stateInt] = listener;
        }

        public void Invoke(T1 param1, T2 param2) {
            if (_isInvokeAllowed != null && !_isInvokeAllowed()) {
                return;
            }

            Action<T1, T2> call = _routingTable[_getStateInt()];
            call?.Invoke(param1, param2);
        }
    }
}
