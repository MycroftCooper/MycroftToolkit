using System.Collections.Generic;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.DiscreteGridToolkit.Square;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MycroftToolkit.QuickCode {
    public static class TextureExtension {
        /// <summary>
        /// 只设置颜色不改变alpha
        /// </summary>
        public static void SetColor(this Image image, Color color) {
            Color tempColor = color;
            tempColor.a = image.color.a;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetAlpha(this Image image, float alpha) {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha) {
            Color tempColor = spriteRenderer.color;
            tempColor.a = alpha;
            spriteRenderer.color = tempColor;
        }
        
        /// <summary>
        /// uint转换为Color
        /// </summary>
        public static Color ParseColor(uint input) {
            return new Color(
                ((input >> 16) & 0xff) / 255.0f,
                ((input >> 8) & 0xff) / 255.0f,
                (input & 0xff) / 255.0f,
                ((input >> 24) & 0xff) / 255.0f
            );
        }

        /// <summary>
        /// uint转换为Color, 但alpha值为1
        /// </summary>
        public static Color ParseSolidColor(uint input) {
            return new Color(
                ((input >> 16) & 0xff) / 255.0f,
                ((input >> 8) & 0xff) / 255.0f,
                (input & 0xff) / 255.0f,
                1.0f
            );
        }

        /// <summary>
        /// uint(为BGR格式)转换为Color, 但alpha值为1
        /// </summary>
        public static Color ParseSolidColor_BGR(uint input) {
            return new Color(
                (input & 0xff) / 255.0f,
                ((input >> 8) & 0xff) / 255.0f,
                ((input >> 16) & 0xff) / 255.0f,
                1.0f
            );
        }


        public static Vector2Int GetSize(this Texture target) => new Vector2Int(target.width, target.height);
        public static RectInt GetRectInt(this Texture target) => new RectInt(Vector2Int.zero, target.GetSize());

        public static Texture2D GetSlicedTexture(this Sprite sprite) {
            Texture2D source = sprite.texture;
            RectInt rect = new RectInt(sprite.rect.position.ToVec2Int(), sprite.rect.size.ToVec2Int());
            Texture2D output = new Texture2D(rect.width, rect.height);
            for (int x = 0; x < rect.width; x++) {
                for (int y = 0; y < rect.height; y++) {
                    Color targetColor = source.GetPixel(rect.x + x, rect.y + y);
                    output.SetPixel(x, y, targetColor);
                }
            }

            output.Apply();
            return output;
        }

        public static Texture2D ExtendTexture(this Texture2D target, int extendWidth, bool scanPixels = false) {
            Vector2Int extendSize, offset;
            if (scanPixels) {
                int up = 0, down = 0, left = 0, right = 0;
                for (int x = 0; x < target.width; x++) {
                    if (up + down == extendWidth * 2) break;
                    if (up == 0 && target.GetPixel(x, target.height - 1).a != 0) up = extendWidth;
                    if (down == 0 && target.GetPixel(x, 0).a != 0) down = extendWidth;
                }

                for (int y = 0; y < target.height; y++) {
                    if (left + right == extendWidth * 2) break;
                    if (left == 0 && target.GetPixel(0, y).a != 0) left = extendWidth;
                    if (right == 0 && target.GetPixel(target.width - 1, 0).a != 0) right = extendWidth;
                }

                extendSize = new Vector2Int(left + right, up + down);
                offset = new Vector2Int(left, down);
            } else {
                extendSize = Vector2Int.one * extendWidth * 2;
                offset = Vector2Int.one * extendWidth;
            }

            return CopyTexture_CPU(target, extendSize, offset);
        }

        public static Texture2D CopyTexture_CPU(this Texture2D source, Vector2Int extendSize, Vector2Int offset) {
            Texture2D output = new Texture2D(source.width + extendSize.x, source.height + extendSize.y);
            for (int y = 0; y < output.height; y++) {
                for (int x = 0; x < output.width; x++) {
                    if (x < offset.x || x > source.width || y < offset.y || y > source.height) {
                        output.SetPixel(x,y,Color.clear);
                    } else {
                        Color targetColor = source.GetPixel(x-offset.x,y-offset.y);
                        output.SetPixel(x,y,targetColor);
                    }
                }
            }
            output.Apply();
            return output;
        }

        public static Texture2D CopyTexture_GPU(this Texture2D source, Vector2Int extendSize, Vector2Int offset) {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width + extendSize.x, source.height + extendSize.y);
            readableText.filterMode = FilterMode.Point;
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), offset.x, offset.y);
            readableText.Apply();

            if (extendSize != Vector2Int.zero) {
                PointSet cleanPointSet = new PointSetRect(readableText.GetRectInt()) - 
                                         new PointSetRect(new RectInt(offset, source.GetSize()));
                cleanPointSet.ForEach((point) => {
                    readableText.SetPixel(point.x,point.y,Color.clear);
                });
            }
            readableText.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Color[] GetColors(this Texture2D target) {
            Color[] output = new Color[target.width * target.height];
            int index = 0;
            for (var y = 0; y < target.height; y++) {
                for (var x = 0; x < target.width; x++, index++) {
                    output[index] = target.GetPixel(x, y);
                }
            }
            return output;
        }

        public static List<Vector2Int> GetBorderlinePoints(this Texture2D target) {
            List<Vector2Int> output = new List<Vector2Int>();
            for (int x = 0; x < target.width; x++) {
                for (int y = 0; y < target.height; y++) {
                    if (target.GetPixel(x, y).a == 0) continue;

                    Vector2Int targetPoint = new Vector2Int(x, y);
                    Vector2Int[] neighbors = targetPoint.GetNeighborsD4();
                    foreach (var neighbor in neighbors) {
                        if (neighbor.x < 0 || neighbor.x >= target.width ||
                            neighbor.y < 0 || neighbor.y >= target.height) {
                            continue;
                        }

                        if (target.GetPixel(neighbor.x, neighbor.y).a == 0) {
                            output.Add(new Vector2Int(x,y));
                        }
                    }
                }
            }

            return output;
        }
        
        public static void SetReadable(this Texture2D texture, bool readable) {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null) {
                importer.isReadable = readable;
                importer.SaveAndReimport();
            } else {
                Debug.LogError("Failed to get TextureImporter.");
            }
        }
    }
}
