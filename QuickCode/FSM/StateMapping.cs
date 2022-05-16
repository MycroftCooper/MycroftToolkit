using System;
using System.Collections;

namespace MycroftToolkit.QuickTool.FSM {
    internal class StateMapping<TState, TDriver> where TState : struct, IConvertible, IComparable
        where TDriver : class, new() {
        public TState state;

        public bool hasEnterRoutine;
        public Action EnterCall = StateMachineRunner.DoNothing;
        public Func<IEnumerator> EnterRoutine = StateMachineRunner.DoNothingCoroutine;

        public bool hasExitRoutine;
        public Action ExitCall = StateMachineRunner.DoNothing;
        public Func<IEnumerator> ExitRoutine = StateMachineRunner.DoNothingCoroutine;

        public Action Finally = StateMachineRunner.DoNothing;

        private Func<TState> stateProviderCallback;
        private StateMachine<TState, TDriver> fsm;

        public StateMapping(StateMachine<TState, TDriver> fsm, TState state, Func<TState> stateProvider) {
            this.fsm = fsm;
            this.state = state;
            stateProviderCallback = stateProvider;
        }
    }
}
