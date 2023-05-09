using System;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class ArrayExtensions {
        public static T[][] InitJaggedArray<T>(int rows, int cols) {
            T[][] jaggedArray = new T[rows][];
            for (int i = 0; i < rows; i++) {
                jaggedArray[i] = new T[cols];
            }

            return jaggedArray;
        }

        public static Vector2Int GetSize(this object[,] array) {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            return new Vector2Int(rows, cols);
        }

        public static Vector2Int GetSize(this object[][] jaggedArray) {
            int maxRows = jaggedArray.Length;
            int maxCols = 0;
            for (int i = 0; i < maxRows; i++) {
                if (jaggedArray[i].Length > maxCols) {
                    maxCols = jaggedArray[i].Length;
                }
            }

            return new Vector2Int(maxRows, maxCols);
        }

        public static Vector2Int GetSize<T>(this T[][] jaggedArray) {
            int maxRows = jaggedArray.Length;
            int maxCols = 0;
            for (int i = 0; i < maxRows; i++) {
                if (jaggedArray[i].Length > maxCols) {
                    maxCols = jaggedArray[i].Length;
                }
            }

            return new Vector2Int(maxRows, maxCols);
        }

        public static Vector2Int GetSize<T>(this T[,] array) {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            return new Vector2Int(rows, cols);
        }

        public static T[,] Rotate<T>(this T[,] array, int rotations) {
            var size = array.GetSize();
            int rows = size.x;
            int cols = size.y;

            T[,] rotatedArray = new T[cols, rows];

            for (int r = 0; r < Math.Abs(rotations) % 4; r++) {
                T[,] tempArray = new T[cols, rows];
                for (int i = 0; i < rows; i++) {
                    for (int j = 0; j < cols; j++) {
                        if (rotations > 0) {
                            tempArray[j, rows - 1 - i] = array[i, j]; // 顺时针旋转
                        } else {
                            tempArray[cols - 1 - j, i] = array[i, j]; // 逆时针旋转
                        }
                    }
                }

                rotatedArray = tempArray;
                (rows, cols) = (cols, rows);
            }

            return rotatedArray;
        }

        public static T[][] Rotate<T>(this T[][] array, int rotations) {
            var size = array.GetSize();
            int maxRows = size.x;
            int maxCols = size.y;

            for (int r = 0; r < Math.Abs(rotations) % 4; r++) {
                T[][] tempArray = new T[maxCols][];
                for (int i = 0; i < maxCols; i++) {
                    tempArray[i] = new T[maxRows];
                    for (int j = 0; j < maxRows; j++) {
                        if (rotations > 0) {
                            tempArray[i][j] =
                                (j < array.Length && i < array[j].Length) ? array[j][i] : default; // 顺时针旋转
                        } else {
                            tempArray[maxCols - 1 - i][j] =
                                (j < array.Length && i < array[j].Length) ? array[j][i] : default; // 逆时针旋转
                        }
                    }
                }

                array = tempArray;
                (maxRows, maxCols) = (maxCols, maxRows);
            }

            return array;
        }
    }
}