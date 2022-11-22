using System.Collections.Generic;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.MathTool;
using UnityEngine;
using UnityEngine.UIElements;

public class ArchimedeanSpiralLerpTest : MonoBehaviour {
    public float a = 1f;
    public float b = 1f;
    public float theta = 360f;
    public float height = 5f;
    public float intervalAngle = 0.01f;
    public float rotateAngle;

    //保存取样点
    List<Vector3> _spiralPointList = new List<Vector3>();

    //绘制曲线
    private LineRenderer _lineRenderer;

    private void Start() {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update() {
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse)) {
            DrawSpiral3();
        }else if (Input.GetMouseButtonDown((int)MouseButton.RightMouse)) {
            DrawSpiral4();
        }else if (Input.GetMouseButtonDown((int)MouseButton.MiddleMouse)) {
            _spiralPointList.Clear();
            _lineRenderer.positionCount = 0;
        }
    }

    private void DrawSpiral() {
        _spiralPointList.Clear();
        Vector3 center = transform.Find("center").position;
        //隔固定角度取样
        float t = 0;
        while (t <= 1) {
            Vector3 targetPos = LerpExtensions.ArchimedeanSpiralLerp(center, height, a, b, 0,theta, t);
            targetPos = targetPos.Rotate(Vector3.zero, Vector3.back, rotateAngle);
            _spiralPointList.Add(targetPos);
            t += intervalAngle;
        }

        _lineRenderer.positionCount = _spiralPointList.Count;
        _lineRenderer.SetPositions(_spiralPointList.ToArray());
    }

    private void DrawSpiral2() {
        _spiralPointList.Clear();
        Vector3 center = transform.Find("center").position;
        Vector3 p1 = transform.Find("p1").position;
        Vector3 p2 = transform.Find("p2").position;
        float t = 0;
        while (t <= 1) {
            Vector3 targetPos = LerpExtensions.ArchimedeanSpiralLerp(center, p1,p2, t);
            targetPos = targetPos.Rotate(Vector3.zero, Vector3.back, rotateAngle);
            _spiralPointList.Add(targetPos);
            t += intervalAngle;
        }
        _lineRenderer.positionCount = _spiralPointList.Count;
        _lineRenderer.SetPositions(_spiralPointList.ToArray());
    }
    
    private void DrawSpiral3() {
        _spiralPointList.Clear();
        Vector3 center = transform.Find("center").position;
        float t = 0;
        while (t <= 1) {
            Vector3 targetPos = LerpExtensions.CircularSpiralLerp(center,1,0, 0,360, t);
            targetPos = targetPos.Rotate(Vector3.zero, Vector3.back, rotateAngle);
            _spiralPointList.Add(targetPos);
            t += intervalAngle;
        }
        _lineRenderer.positionCount = _spiralPointList.Count;
        _lineRenderer.SetPositions(_spiralPointList.ToArray());
    }
    
    private void DrawSpiral4() {
        _spiralPointList.Clear();
        Vector3 center = transform.Find("center").position;
        Vector3 p1 = transform.Find("p1").position;
        Vector3 p2 = transform.Find("p2").position;
        float t = 0;
        while (t <= 1) {
            Vector3 targetPos = LerpExtensions.CircularSpiralLerp(1, p1,p2, t);
            targetPos = targetPos.Rotate(Vector3.zero, Vector3.back, rotateAngle);
            _spiralPointList.Add(targetPos);
            t += intervalAngle;
        }
        _lineRenderer.positionCount = _spiralPointList.Count;
        _lineRenderer.SetPositions(_spiralPointList.ToArray());
    }
}
