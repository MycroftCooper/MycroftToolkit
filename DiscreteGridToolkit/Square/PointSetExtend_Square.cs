using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MycroftToolkit.DiscreteGridToolkit.Square {
    public class PS_Line : PointSet {
        private Vector2Int _startPos, _endPos;
        public Vector2Int startPos {
            get => _startPos;
            set {
                if (value == _startPos) return;
                _startPos = value;
                updatePointSet();
            }
        }
        public Vector2Int endPos {
            get => _endPos;
            set {
                if (value == _endPos) return;
                _endPos = value;
                updatePointSet();
            }
        }
        public PS_Line(Vector2Int startPos, Vector2Int endPos) : base(EPointSetType.line) {
            this._startPos = startPos;
            this._endPos = endPos;
        }
        protected override void updatePointSet() {
            _points.Clear();
            if (_startPos == _endPos) {
                _points.Add(_startPos);
                return;
            }
            int w = _endPos.x - _startPos.x;
            int h = _endPos.y - _startPos.y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest)) {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int x = _startPos.x;
            int y = _startPos.y;
            for (int i = 0; i <= longest; i++) {
                _points.Add(new Vector2Int(x, y));
                numerator += shortest;
                if (!(numerator < longest)) {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                } else {
                    x += dx2;
                    y += dy2;
                }
            }
        }
    }

    public class PS_Rect : PointSet {
        private RectInt _rect;
        public RectInt rect {
            get => _rect;
            set {
                if (_rect.Equals(value)) return;
                _rect = value;
                updatePointSet();
            }
        }
        public PS_Rect(RectInt rect) : base(EPointSetType.rect) {
            _rect = rect;
            updatePointSet();
        }
        public PS_Rect(Vector2Int pos1, Vector2Int pos2) : base(EPointSetType.rect) {
            int xStart = Math.Min(pos1.x, pos2.x);
            int xEnd = Math.Max(pos1.x, pos2.x);
            int yStart = Math.Min(pos1.y, pos2.y);
            int yEnd = Math.Max(pos1.y, pos2.y);
            _rect = new RectInt();
            _rect.SetMinMax(new Vector2Int(xStart, yStart), new Vector2Int(xEnd, yEnd));
            updatePointSet();
        }
        protected override void updatePointSet() {
            _points.Clear();
            foreach (Vector2Int point in _rect.allPositionsWithin) {
                _points.Add(point);
            }
        }
        public bool IsInRectPointSet(PS_Rect pointSet) {
            if (_rect.xMin > pointSet.rect.xMin && _rect.xMax < pointSet.rect.xMax &&
                _rect.yMin > pointSet.rect.yMin && _rect.yMax < pointSet.rect.yMax)
                return true;
            return false;
        }
    }

    public class PS_Radius : PointSet {
        public enum ERadiusPointSetType { radius_d4, radius_d8, radius_euler }
        public int radius {
            get => _radius;
            set {
                if (_radius == value) return;
                _radius = value;
                updatePointSet();
            }
        }
        public ERadiusPointSetType radiusType {
            get => _radiusType;
            set {
                if (_radiusType == value) return;
                _radiusType = value;
                updatePointSet();
            }
        }
        public Vector2Int center {
            get => _center;
            set {
                if (value == _center) return;
                _center = value;
                updatePointSet();
            }
        }

        private int _radius;
        private ERadiusPointSetType _radiusType;
        private Vector2Int _center;

        public PS_Radius(Vector2Int center, int radius, ERadiusPointSetType type) : base(EPointSetType.radius) {
            _center = center;
            _radius = radius;
            _radiusType = type;
            updatePointSet();
        }
        protected override void updatePointSet() {
            if (radius < 0) {
                Debug.LogError("离散网格工具>D4半径计算器>Error>半径不可为负");
                return;
            }

            _points.Clear();

            if (radius == 0) {
                _points.Add(_center);
                return;
            }

            switch (_radiusType) {
                case ERadiusPointSetType.radius_euler:
                    for (int x = -_radius; x <= _radius; x++) {
                        for (int y = -_radius; y <= _radius; y++) {
                            Vector2Int p = new Vector2Int(center.x + x, center.y + y);
                            if (new Vector2Int(0, 0).GetDistance_Euler(new Vector2Int(x, y)) <= radius)
                                _points.Add(p);
                        }
                    }
                    break;

                case ERadiusPointSetType.radius_d4:
                    for (int x = -radius; x <= radius; x++) {
                        for (int y = -radius; y <= radius; y++) {
                            Vector2Int p = new Vector2Int(center.x + x, center.y + y);
                            if ((new Vector2Int(0, 0).GetDistance_D4(new Vector2Int(x, y)) <= radius))
                                _points.Add(p);
                        }
                    }
                    break;

                case ERadiusPointSetType.radius_d8:
                    for (int x = -radius; x <= radius; x++) {
                        for (int y = -radius; y <= radius; y++) {
                            Vector2Int p = new Vector2Int(center.x + x, center.y + y);
                            _points.Add(p);
                        }
                    }
                    break;
            }
        }
    }
}
