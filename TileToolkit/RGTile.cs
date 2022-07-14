using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapSystem {
    public interface IRGTile : IWeightObject {
        public string Name { get; }
        public Vector2Int Size { get; }
        public RGTileData SetTile(Vector2Int pos, Tilemap map);
    }

    
    public class RGTile : IRGTile {
        public Tile tile;
        public string Name { get => _tileName; }
        private string _tileName;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;

        public int Weight { get; set; }

        public RGTile(string name, Vector2Int tileSize, Sprite sprite, int weight = 1) {
            this._tileName = name;
            this._size = tileSize;
            Weight = weight;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
        }

        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            map.SetTile(pos.Vec3Int(), tile);
            return new RGTileData(pos, this);
        }
    }

    
    public class RGTile_Random : IRGTile {
        public string Name { get => name; }
        private string name;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;
        public int Weight { get; set; }

        public RGRandom random;
        public List<IRGTile> RandomTilesList;
        public RGTile_Random(string name, Vector2Int tileSize, int weight = 1) {
            this.name = name;
            this._size = tileSize;
            RandomTilesList = new List<IRGTile>();
            Weight = weight;
        }

        public void AddTile(IRGTile tile) {
            Weight += tile.Weight;
            RandomTilesList.Add(tile);
        }
        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            return RandomTilesList.GetRandomWeightObject(random).SetTile(pos, map);
        }
    }

    
    public class RGTile_GO : IRGTile, IWeightObject {
        public GameObject go;
        public string Name { get => _tileName; }
        private string _tileName;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;

        public int Weight { get; set; }
        public RGTile_GO(string name, Vector2Int tileSize, GameObject go, int weight = 1) {
            _tileName = name;
            _size = tileSize;
            this.go = go;
            Weight = weight;
        }

        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            GameObject newGo = GameObject.Instantiate(go, map.transform);
            Vector3 localPos = map.CellToLocal(pos.Vec3Int());
            newGo.transform.localPosition = localPos;
            newGo.name = Name;

            RGTile_GO newTile = new RGTile_GO(_tileName, _size, newGo);
            return new RGTileData(pos, newTile);
        }
    }
}