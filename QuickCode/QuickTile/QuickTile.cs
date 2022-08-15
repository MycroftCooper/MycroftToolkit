using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.MathTool;

namespace MycroftToolkit.QuickCode.QuickTile {
    public interface IQuickTile : IWeightObject {
        public string Name { get; }
        public Vector2Int Size { get; }
        public QuickTileData GetTileData(Vector2Int pos, QuickTileMap map);
    }


    public class QuickTile : IQuickTile {
        public Tile TheTile;
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }

        public QuickTile(string name, Vector2Int tileSize, Sprite sprite, int weight = 1) {
            Name = name;
            Size = tileSize;
            Weight = weight;
            TheTile = ScriptableObject.CreateInstance<Tile>();
            TheTile.sprite = sprite;
        }

        public QuickTileData GetTileData(Vector2Int pos, QuickTileMap map) {
            map.TheTilemap.SetTile(pos.ToVec3Int(), TheTile);
            return new QuickTileData(pos, this);
        }
    }


    public class QuickTileRandom : IQuickTile {
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }

        public QuickRandom QRandom;
        public List<IQuickTile> RandomTilesList;

        public QuickTileRandom(string name, Vector2Int tileSize, int weight = 1) {
            Name = name;
            Size = tileSize;
            RandomTilesList = new List<IQuickTile>();
            Weight = weight;
        }

        public void AddTile(IQuickTile quickTile) {
            Weight += quickTile.Weight;
            RandomTilesList.Add(quickTile);
        }

        public QuickTileData GetTileData(Vector2Int pos, QuickTileMap map) {
            return RandomTilesList.GetRandomWeightObject(QRandom).GetTileData(pos, map);
        }
    }


    public class QuickTileGo : IQuickTile {
        public enum EPivot { Top, Bottom, Left, Right, Center, BottomLeft, BottomRight, TopRight, TopLeft }
        public EPivot Pivot;
        
        public GameObject Go;
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }

        public QuickTileGo(string name, Vector2Int tileSize, GameObject go,EPivot pivot = EPivot.BottomLeft, int weight = 1) {
            Pivot = pivot;
            Name = name;
            Size = tileSize;
            Go = go;
            Weight = weight;
        }

        public QuickTileData GetTileData(Vector2Int pos, QuickTileMap map) {
            if (Go == null) return null;
            
            Vector3 targetPos = Pivot switch {
                EPivot.Top=> new Vector3(pos.x + Size.x / 2f, pos.y + Size.y),
                EPivot.Center=> new Vector3(pos.x + Size.x / 2f, pos.y + Size.y / 2f),
                EPivot.Left=> new Vector3(pos.x, pos.y + Size.y / 2f),
                EPivot.Right=> new Vector3(pos.x + Size.x, pos.y + Size.y / 2f),
                EPivot.Bottom=> new Vector3(pos.x + Size.x / 2f, pos.y),
                EPivot.TopLeft=> new Vector3(pos.x, pos.y + Size.y),
                EPivot.TopRight=> new Vector3(pos.x + Size.x, pos.y + Size.y),
                EPivot.BottomLeft=> new Vector3(pos.x, pos.y),
                EPivot.BottomRight=> new Vector3(pos.x + Size.x, pos.y),
                _ => throw new ArgumentOutOfRangeException(nameof(Pivot), Pivot, "不存在的锚点")
            };
            
            GameObject newGo = Object.Instantiate(Go, map.TheTilemap.transform);
            newGo.name = Name;
            Vector3 localPos = map.TheTilemap.CellToLocal(targetPos.ToVec3Int());
            newGo.transform.localPosition = localPos;
            
            QuickTileGo newQuickTile = new QuickTileGo(Name, Size, newGo);
            return new QuickTileData(pos, newQuickTile);
        }

        public void SetSprite(Sprite sprite, bool isFlip = false) {
            if (Go == null) return;
            Go.GetComponent<SpriteRenderer>().sprite = sprite;
            if (isFlip) Go.GetComponent<SpriteRenderer>().flipX = true;
        }
        
        public void SetSpriteLayer(string layerName, int layerID) {
            if (Go == null) return;
            SpriteRenderer sr = Go.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = layerName;
            sr.sortingOrder = layerID;
        }
    }

    // todo:完成轻量级规则瓦片的功能
    public abstract class QuickTileRule : IQuickTile {
        public string Name { get; }
        public Vector2Int Size { get; }
        public int Weight { get; set; }
        public bool CanUpdate = true;
        
        public List<QuickTileRule> RuleTilesList;
        
        public QuickTileRule(string name, Vector2Int tileSize) {
            Name = name;
            Size = tileSize;
            RuleTilesList = new List<QuickTileRule>();
            Weight = 1;
        }

        public QuickTileData GetTileData(Vector2Int pos, QuickTileMap  map) {
            return null;
        }

        public abstract bool CheckRule(Dictionary<Vector2Int, IQuickTile> neighbors);
        public abstract void UpdateNeighbors();
    }
}