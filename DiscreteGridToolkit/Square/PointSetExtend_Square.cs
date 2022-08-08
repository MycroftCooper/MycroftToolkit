using UnityEngine;
using System;

namespace MycroftToolkit.DiscreteGridToolkit.Square {
    public class PointSetLine : PointSet {
        private Vector2Int _startPos, _endPos;
        public Vector2Int StartPos {
            get => _startPos;
            set {
                if (value == _startPos) return;
                _startPos = value;
                UpdatePointSet();
            }
        }
        public Vector2Int EndPos {
            get => _endPos;
            set {
                if (value == _endPos) return;
                _endPos = value;
                UpdatePointSet();
            }
        }
        public PointSetLine(Vector2Int startPos, Vector2Int endPos) : base(EPointSetType.Line) {
            this._startPos = startPos;
            this._endPos = endPos;
        }
        protected override void UpdatePointSet() {
            Points.Clear();
            if (_startPos == _endPos) {
                Points.Add(_startPos);
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
                Points.Add(new Vector2Int(x, y));
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

    public class PointSetRect : PointSet {
        private RectInt _rect;
        public RectInt Rect {
            get => _rect;
            set {
                if (_rect.Equals(value)) return;
                _rect = value;
                UpdatePointSet();
            }
        }
        public PointSetRect(RectInt rect) : base(EPointSetType.Rect) {
            _rect = rect;
            UpdatePointSet();
        }
        public PointSetRect(Vector2Int pos1, Vector2Int pos2) : base(EPointSetType.Rect) {
            int xStart = Math.Min(pos1.x, pos2.x);
            int xEnd = Math.Max(pos1.x, pos2.x);
            int yStart = Math.Min(pos1.y, pos2.y);
            int yEnd = Math.Max(pos1.y, pos2.y);
            _rect = new RectInt();
            _rect.SetMinMax(new Vector2Int(xStart, yStart), new Vector2Int(xEnd, yEnd));
            UpdatePointSet();
        }
        protected sealed override void UpdatePointSet() {
            Points.Clear();
            foreach (Vector2Int point in _rect.allPositionsWithin) {
                Points.Add(point);
            }
        }
        public bool IsInRectPointSet(PointSetRect pointSet) {
            if (_rect.xMin > pointSet.Rect.xMin && _rect.xMax < pointSet.Rect.xMax &&
                _rect.yMin > pointSet.Rect.yMin && _rect.yMax < pointSet.Rect.yMax)
                return true;
            return false;
        }
    }

    public class PointSetRadius : PointSet {
        public enum ERadiusPointSetType { RadiusD4, RadiusD8, RadiusEuler }
        public int Radius {
            get => _radius;
            set {
                if (_radius == value) return;
                _radius = value;
                UpdatePointSet();
            }
        }
        
        public ERadiusPointSetType RadiusType {
            get => _radiusType;
            set {
                if (_radiusType == value) return;
                _radiusType = value;
                UpdatePointSet();
            }
        }
        
        public Vector2Int Center {
            get => _center;
            set {
                if (value == _center) return;
                _center = value;
                UpdatePointSet();
            }
        }

        private int _radius;
        private ERadiusPointSetType _radiusType;
        private Vector2Int _center;

        public PointSetRadius(Vector2Int center, int radius, ERadiusPointSetType type) : base(EPointSetType.Radius) {
            _center = center;
            _radius = radius;
            _radiusType = type;
            UpdatePointSet();
        }
        protected sealed override void UpdatePointSet() {
            if (Radius < 0) {
                Debug.LogError("离散网格工具>D4半径计算器>Error>半径不可为负");
                return;
            }

            Points.Clear();

            if (Radius == 0) {
                Points.Add(_center);
                return;
            }

            switch (_radiusType) {
                case ERadiusPointSetType.RadiusEuler:
                    for (int x = -_radius; x <= _radius; x++) {
                        for (int y = -_radius; y <= _radius; y++) {
                            Vector2Int p = new Vector2Int(Center.x + x, Center.y + y);
                            if (new Vector2Int(0, 0).GetDistance_Euler(new Vector2Int(x, y)) <= Radius)
                                Points.Add(p);
                        }
                    }
                    break;

                case ERadiusPointSetType.RadiusD4:
                    for (int x = -Radius; x <= Radius; x++) {
                        for (int y = -Radius; y <= Radius; y++) {
                            Vector2Int p = new Vector2Int(Center.x + x, Center.y + y);
                            if ((new Vector2Int(0, 0).GetDistance_D4(new Vector2Int(x, y)) <= Radius))
                                Points.Add(p);
                        }
                    }
                    break;

                case ERadiusPointSetType.RadiusD8:
                    for (int x = -Radius; x <= Radius; x++) {
                        for (int y = -Radius; y <= Radius; y++) {
                            Vector2Int p = new Vector2Int(Center.x + x, Center.y + y);
                            Points.Add(p);
                        }
                    }
                    break;
            }
        }
    }
}
