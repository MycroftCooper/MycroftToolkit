using System;
using UnityEngine;


namespace MycroftToolkit.MathTool {
    public class ParabolaMover : MonoBehaviour {
        public Parabola ParabolaData;
        public bool IsMoving { get; set; }
        public float time = 0;

        private void FixedUpdate() {
            if (ParabolaData == null || IsMoving == false) {
                return;
            }

            time += Time.fixedDeltaTime;
            (Vector3 pos, Vector3 a) = ParabolaData.GetCurrentTransformData(time);
            transform.position = pos;
            transform.eulerAngles = a;
        }

        public void Reset() {
            IsMoving = false;
            time = 0f;
            transform.position = ParabolaData.StartPos;
        }
    }
}