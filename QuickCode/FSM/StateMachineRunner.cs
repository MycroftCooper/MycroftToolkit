
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickTool.FSM {
    public class StateMachineRunner : MonoBehaviour {
        private List<IStateMachine<StateDriverRunner>> _stateMachineList = new ();

        /// <summary>
        /// 创建一个管理Mono的状态机
        /// </summary>
        /// <typeparam name="TState">列出不同状态转换的枚举</typeparam>
        /// <param name="component">目标组件</param>
        /// <returns>状态机</returns>
        public StateMachine<TState> Initialize<TState>(MonoBehaviour component) where TState : struct, IConvertible, IComparable {
            var fsm = new StateMachine<TState>(component);

            _stateMachineList.Add(fsm);

            return fsm;
        }

        /// <summary>
        /// 创建一个管理Mono的状态机并设置初始状态
        /// </summary>
        /// <typeparam name="TState">列出不同状态转换的枚举</typeparam>
        /// <param name="component">目标组件</param>
        /// <param name="startState">初始状态</param>
        /// <returns>状态机</returns>
        public StateMachine<TState> Initialize<TState>(MonoBehaviour component, TState startState) where TState : struct, IConvertible, IComparable {
            var fsm = Initialize<TState>(component);

            fsm.ChangeState(startState);

            return fsm;
        }

        void FixedUpdate() {
            foreach (var fsm in _stateMachineList) {
                if (!fsm.IsInTransition && fsm.Component.enabled) {
                    fsm.Driver.FixedUpdate.Invoke();
                }
            }
        }

        void Update()
        {
            foreach (var fsm in _stateMachineList)
            {
                if (!fsm.IsInTransition && fsm.Component.enabled) {
                    fsm.Driver.Update.Invoke();
                }
            }
        }

        void LateUpdate() {
            for (int i = 0; i < _stateMachineList.Count; i++) {
                var fsm = _stateMachineList[i];
                if (!fsm.IsInTransition && fsm.Component.enabled) {
                    fsm.Driver.LateUpdate.Invoke();
                }
            }
        }

        public static void DoNothing() {
        }

        public static IEnumerator DoNothingCoroutine() {
            yield break;
        }
    }
}
