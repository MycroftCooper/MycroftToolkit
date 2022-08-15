using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.DiscreteGridToolkit.Square;
using MycroftToolkit.MathTool;


namespace MycroftToolkit.QuickCode.QuickTile {
    public class QuickTileData {
        public Vector2Int Pos;
        public IQuickTile QuickTile;
        public RectInt TileRect;
        public Vector2Int SpaceSize;
        public QuickTileData(Vector2Int pos, IQuickTile quickTile) {
            Pos = pos;
            QuickTile = quickTile;
            TileRect = new RectInt(pos, quickTile.Size);
        }
    }
    
    
    public class QuickTileMap {
        public Vector2Int MapSize;
        public QuickTileData[,] LogicMap;
        public Tilemap TheTilemap;
        
        public QuickTileMap(Vector2Int size, Tilemap map) {
            MapSize = size;
            TheTilemap = map;
            LogicMap = new QuickTileData[size.x, size.y];
        }
        
        public bool IsInMap(int x, int y)
            => x < MapSize.x && x >= 0 && y < MapSize.y && y >= 0;
        public bool IsInMap(Vector2Int pos)
            => IsInMap(pos.x, pos.y);
        
        public bool IsSafe(int x, int y) {
            if (!IsInMap(x, y)) return false;
            return LogicMap[x, y] == null;
        }
        
        public bool IsSafe(int x, int y, Vector2Int size, Vector2Int range) {
            if (!IsInMap(x, y) ||
                !IsInMap(x + size.x - 1, y + size.y - 1) ||
                x + size.x - 1 > range.x || y + size.y - 1 > range.y) {
                return false;
            }

            if (LogicMap[x, y] != null ||
                LogicMap[x + size.x - 1, y + size.y - 1] != null ||
                LogicMap[x + size.x - 1, y] != null) {
                return false;
            }
            
            return true;
        }

        public QuickTileData SetTile(Vector2Int pos, IQuickTile quickTile, bool isCover = true) {
            if (!isCover) {
                RemoveTiles(pos, pos + quickTile.Size - Vector2Int.one);
            }
            QuickTileData td = quickTile.GetTileData(pos, this);
            for (int x = pos.x; x < pos.x + quickTile.Size.x; x++) {
                for (int y = pos.y; y < pos.y + quickTile.Size.y; y++) {
                    if (x < 0 || x >= LogicMap.GetLength(0) || y < 0 || y >= LogicMap.GetLength(1))
                        Debug.LogError($"MyTile>Error>瓦片地图越界:({x},{y})");
                    LogicMap[x, y] = td;
                }
            }
            return td;
        }
        
        public IQuickTile GetTile(Vector2Int pos) {
            if (LogicMap[pos.x, pos.y] == null) return null;
            return LogicMap[pos.x, pos.y].QuickTile;
        }
        
        public T GetTile<T>(Vector2Int pos) where T : IQuickTile {
            if (LogicMap[pos.x, pos.y] == null) return default;
            if (LogicMap[pos.x, pos.y].QuickTile is T) return (T)LogicMap[pos.x, pos.y].QuickTile;
            return default;
        }

        public Dictionary<Vector2Int, IQuickTile> GetTileNeighbors(Vector2Int pos, int radius, EDistanceType distanceType = EDistanceType.D8) {
            if (!IsInMap(pos)) return null;

            Dictionary<Vector2Int, IQuickTile> output = new Dictionary<Vector2Int, IQuickTile>();
            PointSetRadius neighborsPos = new PointSetRadius(pos,radius,distanceType);
            IQuickTile thisTile = GetTile(pos);
            neighborsPos.ForEach((p) => {
                if(!IsInMap(p))return;
                IQuickTile tile = GetTile(p);
                if(tile != thisTile && output.ContainsValue(tile))
                    output.Add(p, tile);
            });
            return output;
        }

        public QuickTileData GetTileData(Vector2Int pos)
            => LogicMap[pos.x, pos.y];

        public void FillTile(RectInt rect, IQuickTile quickTile, Vector2Int spacing, bool isCover = false) {
            int y = rect.min.y;
            while (y <= rect.max.y - quickTile.Size.y + 1) {
                int x = rect.min.x;
                while (x <= rect.max.x - quickTile.Size.x + 1) {
                    if (!IsInMap(new Vector2Int(x - 1, y - 1) + quickTile.Size)) break;
                    if (!isCover && !IsSafe(x, y)) {
                        x = LogicMap[x, y].Pos.x + LogicMap[x, y].QuickTile.Size.x;
                        continue;
                    }
                    SetTile(new Vector2Int(x, y), quickTile, false);
                    x += quickTile.Size.x + spacing.x;
                }
                y += quickTile.Size.y + spacing.y;
            }
        }

        public void FillRingTile_Rule(RectInt rect, Dictionary<int, QuickTile> tiles, int width) {
            Vector2Int start = rect.min;
            Vector2Int end = rect.max;
            SetTile(start, tiles[1]);
            SetTile(new Vector2Int(end.x, start.y), tiles[3]);
            SetTile(new Vector2Int(start.x, end.y), tiles[7]);
            SetTile(end, tiles[9]);

            for (int x = start.x + width; x <= end.x - width; x += width) {
                SetTile(new Vector2Int(x, start.y), tiles[2]);
                SetTile(new Vector2Int(x, end.y), tiles[8]);
            }
            for (int y = start.y + width; y <= end.y - width; y += width) {
                SetTile(new Vector2Int(start.x, y), tiles[4]);
                SetTile(new Vector2Int(end.x, y), tiles[6]);
            }
        }
        
        public void FillRingTile(RectInt rect,  IQuickTile tiles, int width) {
            for (int x = rect.xMin; x <= rect.xMax; x += width) {
                SetTile(new Vector2Int(x, rect.yMin), tiles);
                SetTile(new Vector2Int(x, rect.yMax), tiles);
            }
            for (int y = rect.yMin; y <= rect.yMax; y += width) {
                SetTile(new Vector2Int(rect.xMin, y), tiles);
                SetTile(new Vector2Int(rect.xMax, y), tiles);
            }
        }
        
        public void FillDifferentSizesTile(RectInt rect, Dictionary<Vector2Int, QuickTileRandom> tiles, QuickRandom random) {
            Vector2Int start = rect.min;
            Vector2Int end = rect.max;
            
            Dictionary<Vector2Int, int> sizeDict = new Dictionary<Vector2Int, int>();
            tiles.ForEach((kv) => {
                sizeDict.Add(kv.Key, kv.Value.Weight);
                kv.Value.QRandom = random;
            });
            
            for (int y = start.y; y < end.y; y++) {
                for(int x = start.x;x < end.x;){
                    if (!IsInMap(x, y)) {
                        break;
                    }
                    
                    if (!IsSafe(x, y)) {
                        x = LogicMap[x, y].Pos.x +LogicMap[x, y].QuickTile.Size.x;
                        continue;
                    }

                    Dictionary<Vector2Int, int> tileSizeRandomDict = new Dictionary<Vector2Int, int>();

                    int x1 = x, y1 = y;
                    sizeDict.ForEach((pair) => {
                        if (IsSafe(x1,y1, pair.Key, end))
                            tileSizeRandomDict.Add(pair.Key, pair.Value);
                    });

                    if (tileSizeRandomDict.Count == 0) {
                        break;
                    }
                    
                    Vector2Int targetTileSize = tileSizeRandomDict.GetRandomWeightObject(random);
                    SetTile(new Vector2Int(x, y), tiles[targetTileSize]);
                    
                    x += targetTileSize.x;
                }
            }
        }
        
        public void FillDifferentSizesTile_BigFirst(RectInt rect, Dictionary<Vector2Int, QuickTileRandom> tiles) {
            Vector2Int start = rect.min;
            Vector2Int end = rect.max;
            for (int y = start.y; y < end.y; y++) {
                int x = start.x;
                while (x < end.x) {
                    if (!IsSafe(x, y)) {
                        x = LogicMap[x, y].Pos.x + LogicMap[x, y].QuickTile.Size.x;
                        continue;
                    }
                    Vector2Int maxSize = Vector2Int.zero;
                    foreach (Vector2Int s in tiles.Keys) {
                        if (IsSafe(x, y, s, end) && s.sqrMagnitude > maxSize.sqrMagnitude)
                            maxSize = s;
                    }
                    if (maxSize == Vector2Int.zero) break;
                    SetTile(new Vector2Int(x, y), tiles[maxSize]);
                    x += maxSize.x;
                }
            }
        }

        public void RemoveTile(int x, int y) => RemoveTile(new Vector2Int(x, y));
        public void RemoveTile(Vector2Int pos) {
            if (!IsInMap(pos) || LogicMap[pos.x, pos.y] == null) return;
            Vector2Int start = GetTileData(pos).Pos;
            Vector2Int size = GetTileData(pos).QuickTile.Size;
            for (int x = start.x; x < (start + size).x; x++) {
                for (int y = start.y; y < (start + size).y; y++) {
                    if (TheTilemap.HasTile(pos.ToVec3Int()))
                        TheTilemap.SetTile(pos.ToVec3Int(), null);
                    if (IsInMap(pos))
                        LogicMap[x, y] = null;
                }
            }
        }
        
        public void RemoveTiles(Vector2Int pos1, Vector2Int pos2) {
            for (int i = pos1.x; i <= pos2.x; i++) {
                for (int j = pos1.y; j <= pos2.y; j++) {
                    RemoveTile(i, j);
                }
            }
        }
    }
}