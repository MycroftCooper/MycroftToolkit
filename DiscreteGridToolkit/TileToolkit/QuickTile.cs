using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.MathTool;

namespace MapSystem {
    public interface IQuickTile : IWeightObject {
        public string Name { get; }
        public Vector2Int Size { get; }
        public QuickTileData SetTile(Vector2Int pos, Tilemap map);
    }

    
    public class QuickTile : IQuickTile {
        public Tile Tile;
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }

        public QuickTile(string name, Vector2Int tileSize, Sprite sprite, int weight = 1) {
            Name = name;
            Size = tileSize;
            Weight = weight;
            Tile = ScriptableObject.CreateInstance<Tile>();
            Tile.sprite = sprite;
        }

        public QuickTileData SetTile(Vector2Int pos, Tilemap map) {
            map.SetTile(pos.ToVec3Int(), Tile);
            return new QuickTileData(pos, this);
        }
    }

    
    public class QuickQuickTileRandom : IQuickTile {
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }

        public QuickRandom QRandom;
        public List<IQuickTile> RandomTilesList;
        public QuickQuickTileRandom(string name, Vector2Int tileSize, int weight = 1) {
            Name = name;
            Size = tileSize;
            RandomTilesList = new List<IQuickTile>();
            Weight = weight;
        }

        public void AddTile(IQuickTile quickTile) {
            Weight += quickTile.Weight;
            RandomTilesList.Add(quickTile);
        }
        public QuickTileData SetTile(Vector2Int pos, Tilemap map) {
            return RandomTilesList.GetRandomWeightObject(QRandom).SetTile(pos, map);
        }
    }

    
    public class QuickTileGo : IQuickTile {
        public GameObject Go;
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }
        
        public QuickTileGo(string name, Vector2Int tileSize, GameObject go, int weight = 1) {
            Name = name;
            Size = tileSize;
            this.Go = go;
            Weight = weight;
        }

        public QuickTileData SetTile(Vector2Int pos, Tilemap map) {
            GameObject newGo = Object.Instantiate(Go, map.transform);
            Vector3 localPos = map.CellToLocal(pos.ToVec3Int());
            newGo.transform.localPosition = localPos;
            newGo.name = Name;

            QuickTileGo newQuickTile = new QuickTileGo(Name, Size, newGo);
            return new QuickTileData(pos, newQuickTile);
        }
    }
}