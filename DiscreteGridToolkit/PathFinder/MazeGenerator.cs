using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


namespace PathFinding {
    public class MazeGenerator {

        // 生成迷宫，返回迷宫网格
    public bool[,] GenerateMaze(int width, int height, int seed)
    {
        System.Random rng = new System.Random(seed); // 设置随机种子
        bool[,] mazeGrid = new bool[width, height]; // 迷宫格子
        bool[,] visitedCells = new bool[width, height]; // 已访问的格子（使用bool数组提高效率）
        List<Vector2Int> walls = new List<Vector2Int>(); // 墙壁列表

        // 初始格子设置为已访问
        Vector2Int startCell = new Vector2Int(rng.Next(0, width), rng.Next(0, height));
        visitedCells[startCell.x, startCell.y] = true;

        // 添加周围的墙壁
        AddWallsAroundCell(startCell, width, height, visitedCells, walls);

        while (walls.Count > 0)
        {
            // 随机选择一堵墙
            int randomWallIndex = rng.Next(0, walls.Count);
            Vector2Int wall = walls[randomWallIndex];
            walls.RemoveAt(randomWallIndex);

            // 获取墙壁两侧的单元格
            Vector2Int cell1 = wall;
            Vector2Int cell2 = new Vector2Int(wall.x + (wall.x % 2 == 0 ? 1 : -1), wall.y + (wall.y % 2 == 0 ? 1 : -1));

            // 如果其中一个格子已访问，且另一个未访问，则打通这堵墙
            if (IsValidCell(cell2, width, height) && !visitedCells[cell2.x, cell2.y])
            {
                mazeGrid[cell1.x, cell1.y] = true; // 打通墙壁
                visitedCells[cell2.x, cell2.y] = true;
                AddWallsAroundCell(cell2, width, height, visitedCells, walls);
            }
        }

        return mazeGrid; // 返回生成的迷宫网格
    }

    // 判断坐标是否合法
    bool IsValidCell(Vector2Int cell, int width, int height)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    // 添加围绕某个格子的墙壁
    void AddWallsAroundCell(Vector2Int cell, int width, int height, bool[,] visitedCells, List<Vector2Int> walls)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, -1), // 上
            new Vector2Int(1, 0), // 右
            new Vector2Int(0, 1), // 下
            new Vector2Int(-1, 0) // 左
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsValidCell(neighbor, width, height) && !visitedCells[neighbor.x, neighbor.y])
            {
                // 添加墙壁
                walls.Add(neighbor);
            }
        }
    }
    }
}