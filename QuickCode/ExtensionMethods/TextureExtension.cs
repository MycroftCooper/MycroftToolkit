using System.Collections.Generic;
using MycroftToolkit.DiscreteGridToolkit.Square;
using UnityEngine;
using UnityEngine.UI;

namespace MycroftToolkit.QuickCode
{
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
        
        public static Texture2D ExtendTexture(this Texture2D target, int extendWidth, bool scanPixels = false) {
            Vector2Int extendSize, offset;
            if (scanPixels) {
                int up = 0, down = 0, left = 0, right = 0;
                for (int x = 0; x < target.width; x++) {
                    if(up + down == extendWidth*2)
                        break;
                    if (up == 0 && target.GetPixel(x, target.height - 1).a != 0)
                        up = extendWidth;
                    if (down == 0 && target.GetPixel(x, 0).a != 0)
                        down = extendWidth;
                }
                for (int y = 0; y < target.height; y++) {
                    if(left + right == extendWidth*2)
                        break;
                    if (left == 0 && target.GetPixel(0, y).a != 0)
                        left = extendWidth;
                    if (right == 0 && target.GetPixel(target.width-1, 0).a != 0)
                        right = extendWidth;
                }

                extendSize = new Vector2Int(left + right, up + down);
                offset= new Vector2Int(left,down);
                if (extendSize == Vector2Int.zero) return target;
            }else {
                extendSize = Vector2Int.one*extendWidth*2;
                offset = Vector2Int.one * extendWidth;
            }

            return CopyTexture(target, extendSize, offset);
        }
        
        private static Texture2D CopyTexture(this Texture2D source, Vector2Int extendSize, Vector2Int offset) {
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
            Texture2D readableText = new Texture2D(source.width+extendSize.x, source.height+extendSize.y);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), offset.x, offset.y);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
        
        public static List<Vector2Int> GetBorderlinePoints(this Texture2D target) {
            List<Vector2Int> output = new List<Vector2Int>();
            for (int x = 0; x < target.width; x++) {
                for (int y = 0; y < target.height; y++) {
                    if(target.GetPixel(x,y).a == 0)continue;
                    
                    Vector2Int targetPoint = new Vector2Int(x, y);
                    Vector2Int[] neighbors = targetPoint.GetNeighborsD4();
                    foreach (var neighbor in neighbors) {
                        if (neighbor.x < 0 || neighbor.x >= target.width || 
                            neighbor.y < 0 || neighbor.y >= target.width) {
                            continue;
                        }
                        if (target.GetPixel(neighbor.x, neighbor.y).a == 0) {
                            output.Add(neighbor);
                        }
                    }
                }
            }

            return output;
        }
    }
}