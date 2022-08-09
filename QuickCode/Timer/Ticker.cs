using System;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnassignedField.Global

namespace MycroftToolkit.QuickCode {
    public abstract class Ticker {
        /// <summary>
        /// 当前执行次数
        /// </summary>
        public int NowExecuteTime;
        
        /// <summary>
        /// 目标执行次数
        /// </summary>
        public int TargetExecuteTime;
        
        /// <summary>
        /// 剩余执行次数
        /// </summary>
        public int RemainingExecuteTime => IsLoop ? -1 : TargetExecuteTime - NowExecuteTime;

        public bool IsPause { get; protected set; }
        public bool IsFinish { get; protected set; }
        public bool IsLoop;

        public Action OnTick;
        public Action OnPause;
        public Action OnResume;
        public Action OnCancel;
        public Action OnFinish;

        public abstract void Start();
        public abstract void DoTick();
        public abstract void Pause();
        public abstract void Resume();
        public abstract void Cancel();
    }

    /// <summary>
    /// 自动计数执行器
    /// </summary>
    public class TickerAuto : Ticker {
        private float _interval;
        /// <summary>
        /// 执行间隔
        /// </summary>
        public float Interval {
            get => _interval;
            set {
                if (value == _interval) return;
                _interval = value;
                if (_timer == null) return;
                _timer.Cancel();
                _timer = Timer.Register(_interval, DoTick);
                _timer.isLooped = true;
            }
        }
        /// <summary>
        /// 当前计数
        /// </summary>
        public float NowTicks => _timer == null ? -1 : _timer.GetTimeElapsed() + NowExecuteTime * Interval;

        /// <summary>
        /// 目标计数
        /// </summary>
        public float TargetTicks => IsLoop ? -1 : TargetExecuteTime * Interval;

        /// <summary>
        /// 剩余计数
        /// </summary>
        public float RemainingTicks {
            get {
                if (_timer == null) return TargetTicks;
                if (IsLoop) return -1;
                return _timer.GetTimeRemaining() + RemainingExecuteTime * Interval;
            }
        }

        private Timer _timer;

        /// <summary>
        /// 循环自动计数执行器
        /// </summary>
        /// <param name="interval">执行间隔(秒)</param>
        public TickerAuto(float interval) {
            IsLoop = true;
            IsPause = true;
            IsFinish = false;
            this.Interval = interval;
            TargetExecuteTime = -1;
        }
        /// <summary>
        /// 自动计数执行器
        /// </summary>
        /// <param name="targetExecuteTime">目标执行数(次数)</param>
        /// <param name="interval">执行间隔(秒)</param>
        public TickerAuto(int targetExecuteTime, float interval) {
            IsLoop = false;
            IsPause = true;
            IsFinish = false;
            this.Interval = interval;
            this.TargetExecuteTime = targetExecuteTime;
        }

        public override void Start() {
            IsPause = false;
            IsFinish = false;
            NowExecuteTime = 0;

            _timer = Timer.Register(Interval, DoTick);
            _timer.isLooped = true;
        }

        public override void DoTick() {
            if (IsPause || IsFinish) return;
            OnTick?.Invoke();
            NowExecuteTime++;
            if (IsLoop || NowExecuteTime != TargetExecuteTime) return;
            OnFinish?.Invoke();
            _timer.Cancel();
            IsPause = true;
            IsFinish = true;
        }

        public override void Pause() {
            if (IsFinish || IsPause) return;
            IsPause = true;
            _timer.Pause();
            OnPause?.Invoke();
        }

        public override void Resume() {
            if (IsFinish || !IsPause) return;
            IsPause = false;
            _timer.Resume();
            OnResume?.Invoke();
        }

        public override void Cancel() {
            _timer.Cancel();
            IsPause = true;
            IsFinish = true;
        }
    }

    /// <summary>
    /// 手动计数执行器
    /// </summary>
    public class TickerManual : Ticker {
        /// <summary>
        /// 执行间隔
        /// </summary>
        public int Interval { get; set; }

        private int _ticks;

        /// <summary>
        /// 当前计数
        /// </summary>
        public int NowTime => _ticks + NowExecuteTime * Interval;

        /// <summary>
        /// 目标计数
        /// </summary>
        public float TargetTime => IsLoop ? -1 : TargetExecuteTime * Interval;

        /// <summary>
        /// 剩余计数
        /// </summary>
        public float RemainingTime {
            get {
                if (IsLoop) return -1;
                return _ticks + RemainingExecuteTime * Interval;
            }
        }

        /// <summary>
        /// 循环手动计数执行器
        /// </summary>
        /// <param name="interval">执行间隔(次数)</param>
        public TickerManual(int interval) {
            IsLoop = true;
            IsPause = true;
            IsFinish = false;
            Interval = interval;
            TargetExecuteTime = -1;
        }

        /// <summary>
        /// 手动计数执行器
        /// </summary>
        /// <param name="targetExecuteTime">目标执行数(次数)</param>
        /// <param name="interval">执行间隔(次数)</param>
        public TickerManual(int targetExecuteTime, int interval) {
            IsLoop = false;
            IsPause = true;
            IsFinish = false;
            Interval = interval;
            this.TargetExecuteTime = targetExecuteTime;
        }

        public override void Start() {
            IsPause = false;
            IsFinish = false;
            _ticks = 0;
            NowExecuteTime = 0;
        }
        public override void DoTick() {
            if (IsPause || IsFinish) return;
            _ticks++;
            if (_ticks != Interval) return;
            OnTick?.Invoke();
            NowExecuteTime++;
            _ticks = 0;
            if (IsLoop || NowExecuteTime != TargetExecuteTime) return;
            OnFinish?.Invoke();
            IsPause = true;
            IsFinish = true;
        }
        public override void Pause() {
            if (IsFinish || IsPause) return;
            IsPause = true;
            OnPause?.Invoke();
        }
        public override void Resume() {
            if (IsFinish || !IsPause) return;
            IsPause = false;
            OnResume?.Invoke();
        }
        public override void Cancel() {
            IsPause = true;
            IsFinish = true;
        }
    }
}
