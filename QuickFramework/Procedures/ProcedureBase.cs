using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickFramework.Procedure {
    public abstract class ProcedureBase {
        /// <summary>
        /// 流程初始化时调用
        /// </summary>
        protected abstract internal void OnInit();

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        protected abstract internal void OnEnter();

        /// <summary>
        /// 状态轮询时调用
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        protected abstract internal void OnUpdate(float elapseSeconds, float realElapseSeconds);

        protected abstract internal void OnFixedUpdate();
        /// <summary>
        /// 离开状态时调用
        /// </summary>
        /// <param name="isShutdown">是否是关闭状态机时触发</param>
        protected abstract internal void OnLeave(bool isShutdown);

        /// <summary>
        /// 状态销毁时调用
        /// </summary>
        protected abstract internal void OnDestroy();
    }
}
