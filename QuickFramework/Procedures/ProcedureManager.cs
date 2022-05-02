using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MycroftToolkit.QuickCode;
using Sirenix.OdinInspector;

namespace MycroftToolkit.QuickFramework.Procedure {
    public class ProcedureManager : MonoSingleton<ProcedureManager> {
        [ShowInInspector]
        public Dictionary<string, ProcedureBase> procedureDict;
        public string startProcedure;
        public ProcedureBase nowProcedure;

        void Start() {
            procedureDict = new Dictionary<string, ProcedureBase>();
            if (string.IsNullOrEmpty(startProcedure)) {
                Debug.LogError($"QuickFramework.Procedure>Error> 未找到初始流程{startProcedure}");
                return;
            }
            ProcedureBase start = QuickReflect.Create<ProcedureBase>("MycroftToolkit.Runtime.Procedure." + startProcedure);
            start.OnInit();
            procedureDict.Add(startProcedure, start);
            start.OnEnter();
            nowProcedure = start;
        }

        public void ChangeProcedure<T>() where T : ProcedureBase, new() {
            string tName = typeof(T).Name;
            ProcedureBase next;
            if (procedureDict.ContainsKey(tName))
                next = procedureDict[tName];
            else {
                next = new T();
                next.OnInit();
                procedureDict.Add(tName, next);
            }

            nowProcedure.OnLeave(false);
            next.OnEnter();
            nowProcedure = next;
        }


        void Update() {
            if (nowProcedure != null)
                nowProcedure.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
        }
        private void FixedUpdate() {
            if (nowProcedure != null)
                nowProcedure.OnFixedUpdate();
        }

        public void Exit() {
            if (nowProcedure != null) {
                nowProcedure.OnLeave(true);
                nowProcedure = null;
            }
            procedureDict.ForEach(p => p.Value.OnDestroy());
            procedureDict.Clear();
        }
    }
}
