using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace MycroftToolkit.QuickResource.SpriteImportTool {
#if UNITY_EDITOR
    public class TextureProcessingToolEditor : OdinEditorWindow
    {
        [MenuItem("Assets/QuickResource/Sprite/ProcessingTool")]
        public static void CreateWindow() {
            EditorWindow window = GetWindow<TextureProcessingToolEditor>("精灵处理工具");
            window.minSize = new Vector2(1000, 600);
            window.Focus();
            window.Show();
        }
        
        [HorizontalGroup("精灵处理器")]
        [VerticalGroup("精灵处理器/Left"), Title("检查器")]
        [AssetSelector, LabelText("目标图片")]
        public Sprite targetSprite;
        
        
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("目标预览"), PreviewField(256, ObjectFieldAlignment.Left), ReadOnly]
        public List<Sprite> targetPreview;
        
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("结果预览"), PreviewField(256, ObjectFieldAlignment.Left), ReadOnly]
        public List<Sprite> resultPreview;


        #region 图片描边相关
        [TabGroup("精灵处理器/Right", "图片描边")]
        [LabelText("描边颜色")] public Color outlineColor;
        
        [TabGroup("精灵处理器/Right", "图片描边")]
        [Range(1, 64), LabelText("描边大小")]
        public int outlineSize;

        [TabGroup("精灵处理器/Right", "图片描边"), LabelText("是否扩展边缘")]
        public bool canExtendedEdge;

        [TabGroup("精灵处理器/Right", "图片描边")]
        [Button("刷新预览")]
        private void UpdatePreview_OutlinePicture() {
            
        }
        
        [TabGroup("精灵处理器/Right", "图片描边")]
        [Button("生成描边图")]
        private void GenerateOutlinePicture(Sprite target) {
            if (target == null) {
                return;
            }

            (Vector2Int extendSize, Vector2Int offset) = (Vector2Int.zero, Vector2Int.zero);
            if (canExtendedEdge) {
                (extendSize, offset) = ScanEdgeToExtend(target);
            }

            var spriteTexture = target.texture;
            var texture = CopyTexture(spriteTexture, extendSize, offset);
            
            texture.Apply();
            
            var originPath = AssetDatabase.GetAssetPath(targetSprite);
            var newPath = originPath.Remove(originPath.Length - 5) + "_outline.png";
            File.WriteAllBytes(newPath, texture.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log($"描边图保存在 ‘{newPath}’", AssetDatabase.LoadAssetAtPath<Texture2D>(newPath));
        }

        private (Vector2Int extendSize, Vector2Int offset) ScanEdgeToExtend(Sprite target) {
            int up = 0, down = 0, left = 0, right = 0;
            Texture2D spriteTexture = target.texture;
            Rect spriteRect = target.rect;
            for (int x = (int)spriteRect.xMin; x <= spriteRect.xMax; x++) {
                if(up != 0 && down != 0)break;
                if (down == 0 && spriteTexture.GetPixel(x, (int)spriteRect.yMin).a != 0) {
                    down = outlineSize;
                }
                if(up == 0 && spriteTexture.GetPixel(x, (int)spriteRect.yMax).a != 0) {
                    up = outlineSize;
                }
            }
            for (int y = (int)spriteRect.yMin; y <= spriteRect.yMax; y++) {
                if(left != 0 && right != 0)break;
                if (left == 0 && spriteTexture.GetPixel(y, (int)spriteRect.xMin).a != 0) {
                    left = outlineSize;
                }
                if(right == 0 && spriteTexture.GetPixel(y, (int)spriteRect.xMax).a != 0) {
                    right = outlineSize;
                }
            }

            return (new Vector2Int(left + right, up + down), new Vector2Int(left,down));
        }
        
        private static Texture2D CopyTexture(Texture2D source, Vector2Int extendSize, Vector2Int offset) {
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
        #endregion
        

        #region 图片拼接相关
        [TabGroup("精灵处理器/Right", "图片拼接"), AssetSelector, LabelText("需要生成的sprite"), HideReferenceObjectPicker]
        public List<Sprite> combinationSprites = new List<Sprite>();

        [TabGroup("精灵处理器/Right","图片拼接"), Button("合并为新图集", ButtonSizes.Medium)]
        public void GenerateCombinedTexture() {
            if (combinationSprites.Count == 0)
            {
                return;
            }

            var textures = new Texture2D[combinationSprites.Count];
            var maxHeight = float.MinValue;
            var wholeWidth = 0f;
            for (var i = 0; i < combinationSprites.Count; i++)
            {
                textures[i] = CopyTexture(combinationSprites[i].texture, Vector2Int.zero,Vector2Int.zero);
                if (combinationSprites[i].rect.height > maxHeight)
                {
                    maxHeight = combinationSprites[i].rect.height;
                }

                wholeWidth += combinationSprites[i].rect.width;
            }

            var targetTex = new Texture2D((int)wholeWidth, (int)maxHeight);
            var currentX = 0;
            for (var i = 0; i < textures.Length; i++)
            {
                var currentSpriteRect = combinationSprites[i].rect;
                var pixels = textures[i].GetPixels((int)currentSpriteRect.x, (int)currentSpriteRect.y,
                    (int)currentSpriteRect.width, (int)currentSpriteRect.height);
                targetTex.SetPixels(currentX, 0, (int)currentSpriteRect.width, (int)currentSpriteRect.height, pixels);
                currentX += (int)currentSpriteRect.width;
            }

            targetTex.Apply();
            var path = EditorUtility.SaveFilePanel("保存新图集", "", "texture", "png");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllBytes(path, targetTex.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log($"新贴图保存在{path}");
        }
        

        #endregion


        #region 图片拆分相关
        [TabGroup("精灵处理器/Right","图片拆分"), ShowInInspector, NonSerialized, HideReferenceObjectPicker, LabelText("需要生成的sprite")]
        public List<Sprite> IndependentSprites = new List<Sprite>();

        [TabGroup("精灵处理器/Right","图片拆分"), ShowInInspector, NonSerialized, HideReferenceObjectPicker, LabelText("名字前缀")]
        public string IndependentNamePrefix;

        public TextureProcessingToolEditor(Sprite targetSprite) {
            this.targetSprite = targetSprite;
        }

        [TabGroup("精灵处理器/Right","图片拆分"), Button("生成贴图", ButtonSizes.Medium)]
        public void GenerateIndependentTexture() {
            if (IndependentSprites.Count == 0 || string.IsNullOrEmpty(IndependentNamePrefix))
            {
                return;
            }

            var textures = new Texture2D[IndependentSprites.Count];
            var maxHeight = float.MinValue;
            for (var i = 0; i < IndependentSprites.Count; i++)
            {
                var currentTexture = CopyTexture(IndependentSprites[i].texture,Vector2Int.zero,Vector2Int.zero);
                var rect = IndependentSprites[i].rect;
                textures[i] = new Texture2D((int)rect.width, (int)rect.height);
                for (int x = 0; x < rect.width; x++)
                {
                    for (int y = 0; y < rect.height; y++)
                    {
                        textures[i].SetPixel(x, y, currentTexture.GetPixel(x + (int)rect.x, y + (int)rect.y));
                    }
                }

                textures[i].Apply();
                if (IndependentSprites[i].rect.height > maxHeight)
                {
                    maxHeight = IndependentSprites[i].rect.height;
                }
            }

            var path = EditorUtility.SaveFolderPanel("保存新图集", "", "");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            for (var i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];
                File.WriteAllBytes($"{path}/{IndependentNamePrefix}_{i}.png", texture.EncodeToPNG());
            }

            AssetDatabase.Refresh();
            Debug.Log($"新贴图保存在{path}");
        }
        #endregion
    }
#endif
}