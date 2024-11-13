using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class MazeGenerator {

        public bool[,] GenerateMaze(Vector2Int mazeSize, int seed, Vector2Int startPoint, Vector2Int endPoint) {
            // Initialize the maze: false means wall, true means path.
            bool[,] maze = new bool[mazeSize.x, mazeSize.y];

            // Set up random generator.
            System.Random random = new System.Random(seed);

            // List to store unvisited cells for the algorithm.
            List<Vector2Int> stack = new List<Vector2Int>();

            // Start from the given starting point.
            Vector2Int currentCell = startPoint;
            maze[currentCell.x, currentCell.y] = true; // Mark as path.
            stack.Add(currentCell);

            // Directions for neighbors (left, right, up, down).
            Vector2Int[] directions = {
                new Vector2Int(-2, 0), // Left
                new Vector2Int(2, 0), // Right
                new Vector2Int(0, 2), // Up
                new Vector2Int(0, -2) // Down
            };

            // Algorithm: Randomized DFS
            while (stack.Count > 0) {
                // Get current cell.
                currentCell = stack[^1];
                stack.RemoveAt(stack.Count - 1);

                // Shuffle directions for randomness.
                Shuffle(directions, random);

                bool foundUnvisitedNeighbor = false;
                foreach (var direction in directions) {
                    Vector2Int neighbor = currentCell + direction;

                    // Check if neighbor is within bounds and is a wall.
                    if (IsInBounds(neighbor, mazeSize) && !maze[neighbor.x, neighbor.y]) {
                        // Carve a path between current cell and neighbor.
                        Vector2Int wall = currentCell + direction / 2;
                        maze[wall.x, wall.y] = true; // Remove wall.
                        maze[neighbor.x, neighbor.y] = true; // Mark neighbor as path.

                        // Add neighbor to stack for further exploration.
                        stack.Add(neighbor);
                        foundUnvisitedNeighbor = true;
                    }
                }

                // If no unvisited neighbors, backtrack (handled by removing from stack).
            }

            // Ensure the endpoint is reachable by setting it to true if not already.
            if (!maze[endPoint.x, endPoint.y]) {
                maze[endPoint.x, endPoint.y] = true;
            }

            return maze;
        }

        // Check if a position is within bounds of the maze.
        private bool IsInBounds(Vector2Int pos, Vector2Int mazeSize) {
            return pos.x > 0 && pos.y > 0 && pos.x < mazeSize.x - 1 && pos.y < mazeSize.y - 1;
        }

        // Shuffle array using Fisher-Yates algorithm.
        private void Shuffle(Vector2Int[] array, System.Random random) {
            for (int i = array.Length - 1; i > 0; i--) {
                int j = random.Next(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

    }
}