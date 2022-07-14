using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapSystem {
    public class RGTileData {
        public Vector2Int pos;
        public IRGTile tile;
        public RectInt rect;
        public Vector2Int spaceSize;
        public RGTileData(Vector2Int pos, IRGTile tile) {
            this.pos = pos;
            this.tile = tile;
            rect = new RectInt(pos, tile.Size);
        }
    }
    
    
    public class RGTileMap {
        public Vector2Int mapSize;
        public RGTileData[,] logicMap;
        public Tilemap tilemap;
        public RGTileMap(Vector2Int size, Tilemap map) {
            mapSize = size;
            this.tilemap = map;
            logicMap = new RGTileData[size.x, size.y];
        }
        public bool IsInMap(int x, int y)
            => x < mapSize.x && x >= 0 && y < mapSize.y && y >= 0;
        public bool IsInMap(Vector2Int pos)
            => IsInMap(pos.x, pos.y);
        public bool IsSafe(int x, int y) {
            if (!IsInMap(x, y)) return false;
            return logicMap[x, y] == null;
        }
        public bool IsSafe(int x, int y, Vector2Int size, Vector2Int range) {
            if (!IsInMap(x, y) ||
                !IsInMap(x + size.x - 1, y + size.y - 1) ||
                x + size.x - 1 > range.x || y + size.y - 1 > range.y)
                return false;
            if (logicMap[x, y] != null ||
                logicMap[x + size.x - 1, y + size.y - 1] != null ||
                logicMap[x + size.x - 1, y] != null) return false;
            return true;
        }

        public RGTileData SetTile(Vector2Int pos, IRGTile tile, bool isCover = true) {
            if (!isCover) {
                RemoveTiles(pos, pos + tile.Size - Vector2Int.one);
            }
            RGTileData td = tile.SetTile(pos, tilemap);
            for (int x = pos.x; x < pos.x + tile.Size.x; x++) {
                for (int y = pos.y; y < pos.y + tile.Size.y; y++) {
                    if (x < 0 || x >= logicMap.GetLength(0) || y < 0 || y >= logicMap.GetLength(1))
                        Debug.LogError($"MyTile>Error>瓦片地图越界:({x},{y})");
                    logicMap[x, y] = td;
                }
            }
            return td;
        }
        public IRGTile GetTile(Vector2Int pos) {
            if (logicMap[pos.x, pos.y] == null) return null;
            return logicMap[pos.x, pos.y].tile;
        }
        public T GetTile<T>(Vector2Int pos) where T : IRGTile {
            if (logicMap[pos.x, pos.y] == null) return default;
            if (logicMap[pos.x, pos.y].tile is T) return (T)logicMap[pos.x, pos.y].tile;
            return default;
        }
        public RGTileData GetTileData(Vector2Int pos)
            => logicMap[pos.x, pos.y];

        public void FillTile(Vector2Int start, Vector2Int end, IRGTile tile, Vector2Int spacing, bool isCover = false) {
            int x, y = start.y;
            while (y <= end.y - tile.Size.y + 1) {
                x = start.x;
                while (x <= end.x - tile.Size.x + 1) {
                    if (!IsInMap(new Vector2Int(x - 1, y - 1) + tile.Size)) break;
                    if (!isCover && !IsSafe(x, y)) {
                        x = logicMap[x, y].pos.x + logicMap[x, y].tile.Size.x;
                        continue;
                    }
                    SetTile(new Vector2Int(x, y), tile, false);
                    x += tile.Size.x + spacing.x;
                }
                y += tile.Size.y + spacing.y;
            }
        }
        public void FillRingTile(Vector2Int start, Vector2Int end, Dictionary<int, RGTile> tiles, int width) {
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
        public void FillDifferentSizesTile(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, RGTile_Random> tiles, RGRandom random) {
            int x;
            for (int y = start.y; y < end.y; y++) {
                x = start.x;
                while (x < end.x) {
                    if (!IsInMap(x, y)) break;
                    if (!IsSafe(x, y)) {
                        x = logicMap[x, y].pos.x + logicMap[x, y].tile.Size.x;
                        continue;
                    }

                    List<Vector2Int> tileSizeRandomList = new List<Vector2Int>(tiles.Keys);
                    for (int i = tileSizeRandomList.Count - 1; i >= 0; i--) {
                        if (!IsSafe(x, y, tileSizeRandomList[i], end))
                            tileSizeRandomList.Remove(tileSizeRandomList[i]);
                    }
                    if (tileSizeRandomList.Count == 0) break;
                    // Todo: 改成加权随机
                    Vector2Int targetTileSize = tileSizeRandomList.GetRandomObject(random);
                    // Debug.Log($"当前位置:{x},{y} 目标大小:{targetTileSize}");
                    SetTile(new Vector2Int(x, y), tiles[targetTileSize]);
                    x += targetTileSize.x;
                }
            }
        }
        public void FillDifferentSizesTile_BigFirst(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, RGTile_Random> tiles) {
            int x;
            for (int y = start.y; y < end.y; y++) {
                x = start.x;
                while (x < end.x) {
                    if (!IsSafe(x, y)) {
                        x = logicMap[x, y].pos.x + logicMap[x, y].tile.Size.x;
                        continue;
                    }
                    Vector2Int maxSize = Vector2Int.zero;
                    foreach (Vector2Int s in tiles.Keys) {
                        if (IsSafe(x, y, s, end) && s.sqrMagnitude > maxSize.sqrMagnitude)
                            maxSize = s;
                    }
                    if (maxSize == Vector2Int.zero) break;
                    // Debug.Log($"当前位置:{x},{y} 目标大小:{targetTileSize}");
                    SetTile(new Vector2Int(x, y), tiles[maxSize]);
                    x += maxSize.x;
                }
            }
        }

        public void RemoveTile(int x, int y) => RemoveTile(new Vector2Int(x, y));
        public void RemoveTile(Vector2Int pos) {
            if (!IsInMap(pos) || logicMap[pos.x, pos.y] == null) return;
            Vector2Int start = GetTileData(pos).pos;
            Vector2Int size = GetTileData(pos).tile.Size;
            for (int x = start.x; x < (start + size).x; x++) {
                for (int y = start.y; y < (start + size).y; y++) {
                    if (tilemap.HasTile(pos.Vec3Int()))
                        tilemap.SetTile(pos.Vec3Int(), null);
                    if (IsInMap(pos))
                        logicMap[x, y] = null;
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
        
        #region GO专用
        public void SetTileSprite(Vector2Int pos, Sprite sprite, bool isFlip = false) {
            RGTile_GO tile = logicMap[pos.x, pos.y].tile as RGTile_GO;
            if (tile == null) return;
            tile.go.GetComponent<SpriteRenderer>().sprite = sprite;
            if (isFlip) tile.go.GetComponent<SpriteRenderer>().flipX = true;
        }
        
        public void SetTileSpriteLayer(Vector2Int pos, string layerName, int layerID) {
            RGTile_GO tile = logicMap[pos.x, pos.y].tile as RGTile_GO;
            if (tile == null) return;
            SpriteRenderer sr = tile.go.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = layerName;
            sr.sortingOrder = layerID;
        }
        
        public enum EPivot { Top, Bottom, Left, Right, Center, BottomLeft, BottomRight, TopRight, TopLeft }
        
        public void SetTile_Pivot(Vector2Int pos, RGTile_GO tile, EPivot pivot) {
            RGTileData td = SetTile(pos, tile);
            Transform target = (td.tile as RGTile_GO)?.go.transform;
            if (!target) return;
            switch (pivot) {
                case EPivot.Top:
                    target.localPosition = new Vector3(pos.x + tile.Size.x / 2f, pos.y + tile.Size.y);
                    break;
                case EPivot.Center:
                    target.localPosition = new Vector3(pos.x + tile.Size.x / 2f, pos.y + tile.Size.y / 2f);
                    break;
                case EPivot.Left:
                    target.localPosition = new Vector3(pos.x, pos.y + tile.Size.y / 2f);
                    break;
                case EPivot.Right:
                    target.localPosition = new Vector3(pos.x + tile.Size.x, pos.y + tile.Size.y / 2f);
                    break;
                case EPivot.Bottom:
                    target.localPosition = new Vector3(pos.x + tile.Size.x / 2f, pos.y);
                    break;
                case EPivot.TopLeft:
                    target.localPosition = new Vector3(pos.x, pos.y + tile.Size.y);
                    break;
                case EPivot.TopRight:
                    target.localPosition = new Vector3(pos.x + tile.Size.x, pos.y + tile.Size.y);
                    break;
                case EPivot.BottomLeft:
                    target.localPosition = new Vector3(pos.x, pos.y);
                    break;
                case EPivot.BottomRight:
                    target.localPosition = new Vector3(pos.x + tile.Size.x, pos.y);
                    break;
            }
        }
        #endregion
    }
}