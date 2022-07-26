using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public class ParabolaMover {
        
    }
    // todo:待补完已知终点的斜抛
    // https://baike.baidu.com/item/%E6%96%9C%E6%8A%9B%E8%BF%90%E5%8A%A8/9905547?fr=aladdin
    public class Parabola {
        private float _initSpeed;
        private float _angle;
        private Vector3 _initSpeedVector;
        
        private float _gravity;
        
        private Vector3 _startPos;
        private Vector3 _endPos;

        private Vector3 _currentPos;
        private Vector3 _currentAngle;
        private Vector3 _currentSpeedVector;

        public Parabola(Vector3 startPos, float initSpeed, float angle, float gravity = 9.8f) {
            _startPos = startPos;
            
            _initSpeed = initSpeed;
            _angle = angle;
            _initSpeedVector = Quaternion.Euler(new Vector3(0, 0, _angle)) * Vector3.right * _initSpeed;
            
            _gravity = gravity;

            _currentPos = startPos;
            _currentAngle = Vector3.zero;
            _currentSpeedVector = _initSpeedVector;
        }

        public (Vector3 pos, Vector3 angle)GetCurrentTransformData(float time) {
            Vector3 deltaPos = new Vector3(
                _initSpeedVector.x * time,
                _initSpeedVector.y * time - (0.5f * _gravity * time * time),
                _initSpeedVector.z * time
                );
            _currentPos = _startPos + deltaPos;
            _currentSpeedVector = _initSpeedVector + new Vector3(0, _gravity * time, 0);
            _currentAngle.z = Mathf.Atan( _currentSpeedVector.y / _initSpeedVector.x) * Mathf.Rad2Deg;
            return (_currentPos, _currentAngle);
        }
    }
}