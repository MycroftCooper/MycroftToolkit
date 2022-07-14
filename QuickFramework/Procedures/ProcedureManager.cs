using System.Collections.Generic;
using UnityEngine;

using MycroftToolkit.QuickCode;

namespace MycroftToolkit.QuickFramework.Procedure {
    public class ProcedureManager : MonoSingleton<ProcedureManager> {
        public Dictionary<string, ProcedureBase> ProcedureDict;
        public string startProcedure;
        public ProcedureBase NowProcedure;

        void Start() {
            ProcedureDict = new Dictionary<string, ProcedureBase>();
            if (string.IsNullOrEmpty(startProcedure)) {
                Debug.LogError($"QuickFramework.Procedure>Error> 未找到初始流程{startProcedure}");
                return;
            }

            ProcedureBase start = QuickReflect.CreateInstance<ProcedureBase>("MycroftToolkit.Runtime.Procedure." + startProcedure);
            start.OnInit();
            ProcedureDict.Add(startProcedure, start);
            start.OnEnter();
            NowProcedure = start;
        }

        public void ChangeProcedure<T>() where T : ProcedureBase, new() {
            string tName = typeof(T).Name;
            ProcedureBase next;
            if (ProcedureDict.ContainsKey(tName))
                next = ProcedureDict[tName];
            else {
                next = new T();
                next.OnInit();
                ProcedureDict.Add(tName, next);
            }

            NowProcedure.OnLeave(false);
            next.OnEnter();
            NowProcedure = next;
        }


        void Update() {
            if (NowProcedure != null)
                NowProcedure.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
        }
        private void FixedUpdate() {
            if (NowProcedure != null)
                NowProcedure.OnFixedUpdate();
        }

        public void Exit() {
            if (NowProcedure != null) {
                NowProcedure.OnLeave(true);
                NowProcedure = null;
            }
            ProcedureDict.ForEach(p => p.Value.OnDestroy());
            ProcedureDict.Clear();
        }
    }
}
