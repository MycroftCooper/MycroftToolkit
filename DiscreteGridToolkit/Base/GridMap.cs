using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MycroftToolkit.DiscreteGridToolkit {
    public class GridMapObjectInfo<T> {
        public IGridMap Map { get; }
        public RectInt PositionInfo { get; }
        public T Obj;

        public GridMapObjectInfo(IGridMap map, RectInt positionInfo)
        {
            this.Map = map;
            PositionInfo = positionInfo;
        }
    }
    public interface IGridMap {
        public Vector3 OriginWorldPos { get; set; }
        public Vector2 CellSize { get; set; }
        public Vector2 CellSpacing { get; set; }
        public bool IsInMap(Vector2Int logicPos);
        public bool HasObject(Vector2Int logicPos);
        public bool CanSetObject(Vector2Int logicPos);
        public Vector3 Logic2World(Vector2Int logicPos);
        public Vector2Int World2Logic(Vector3 worldPos);
        public T GetObject<T>(Vector2Int logicPos);
        public GridMapObjectInfo<T> SetObject<T>(T obj, PointSet points);
        public GridMapObjectInfo<T> SetObject<T>(T obj, RectInt positionInfo);
        public bool Remove(Vector2Int logicPos);
        public bool Remove<T>(T obj);
    }
}
