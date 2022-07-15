using System.Collections.Generic;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.MathTool;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapSystem {
    public interface ITile : IWeightObject {
        public string Name { get; }
        public Vector2Int Size { get; }
        public RGTileData SetTile(Vector2Int pos, Tilemap map);
    }

    
    public class Tile : ITile {
        public UnityEngine.Tilemaps.Tile tile;
        public string Name { get => _tileName; }
        private string _tileName;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;

        public int Weight { get; set; }

        public Tile(string name, Vector2Int tileSize, Sprite sprite, int weight = 1) {
            this._tileName = name;
            this._size = tileSize;
            Weight = weight;
            tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sprite;
        }

        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            map.SetTile(pos.ToVec3Int(), tile);
            return new RGTileData(pos, this);
        }
    }

    
    public class TileRandom : ITile {
        public string Name { get => name; }
        private string name;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;
        public int Weight { get; set; }

        public QuickRandom random;
        public List<ITile> RandomTilesList;
        public TileRandom(string name, Vector2Int tileSize, int weight = 1) {
            this.name = name;
            this._size = tileSize;
            RandomTilesList = new List<ITile>();
            Weight = weight;
        }

        public void AddTile(ITile tile) {
            Weight += tile.Weight;
            RandomTilesList.Add(tile);
        }
        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            return RandomTilesList.GetRandomWeightObject(random).SetTile(pos, map);
        }
    }

    
    public class TileGo : ITile, IWeightObject {
        public GameObject go;
        public string Name { get => _tileName; }
        private string _tileName;
        public Vector2Int Size { get => _size; }
        private Vector2Int _size;

        public int Weight { get; set; }
        public TileGo(string name, Vector2Int tileSize, GameObject go, int weight = 1) {
            _tileName = name;
            _size = tileSize;
            this.go = go;
            Weight = weight;
        }

        public RGTileData SetTile(Vector2Int pos, Tilemap map) {
            GameObject newGo = GameObject.Instantiate(go, map.transform);
            Vector3 localPos = map.CellToLocal(pos.ToVec3Int());
            newGo.transform.localPosition = localPos;
            newGo.name = Name;

            TileGo newTile = new TileGo(_tileName, _size, newGo);
            return new RGTileData(pos, newTile);
        }
    }
}