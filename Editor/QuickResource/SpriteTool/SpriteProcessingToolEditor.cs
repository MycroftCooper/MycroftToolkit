using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MycroftToolkit.QuickResource.SpriteImportTool {
#if UNITY_EDITOR
    public class SpriteProcessingToolEditor : OdinEditorWindow
    {
        [MenuItem("Assets/QuickResource/Sprite/ProcessingTool")]
        public static void CreateWindow() {
            EditorWindow window = GetWindow<SpriteProcessingToolEditor>("精灵处理工具");
            window.minSize = new Vector2(1000, 600);
            window.Focus();
            window.Show();
        }
        
        [HorizontalGroup("精灵处理器",width:500)]
        [VerticalGroup("精灵处理器/Left"), Title("检查器")]
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("目标预览"), AssetSelector]
        public List<Sprite> targets = new List<Sprite>();
        
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("结果预览"), PreviewField(256, ObjectFieldAlignment.Left), ReadOnly]
        public List<Texture2D> resultPreview = new List<Texture2D>();
        
        
        #region 图片拼接相关
        [TabGroup("精灵处理器/Right","图片拼接"), Button("合并", ButtonSizes.Medium)]
        public void GenerateCombinedTexture() {
            if (targets.Count == 0) return;

            var textures = new Texture2D[targets.Count];
            var maxHeight = float.MinValue;
            var wholeWidth = 0f;
            for (var i = 0; i < targets.Count; i++) {
                textures[i] = CopyTexture(targets[i].texture, Vector2Int.zero,Vector2Int.zero);
                if (targets[i].rect.height > maxHeight) {
                    maxHeight = targets[i].rect.height;
                }

                wholeWidth += targets[i].rect.width;
            }

            var targetTex = new Texture2D((int)wholeWidth, (int)maxHeight);
            var currentX = 0;
            for (var i = 0; i < textures.Length; i++) {
                var currentSpriteRect = targets[i].rect;
                var pixels = textures[i].GetPixels((int)currentSpriteRect.x, (int)currentSpriteRect.y,
                    (int)currentSpriteRect.width, (int)currentSpriteRect.height);
                targetTex.SetPixels(currentX, 0, (int)currentSpriteRect.width, (int)currentSpriteRect.height, pixels);
                currentX += (int)currentSpriteRect.width;
            }
            targetTex.Apply();

            resultPreview = new List<Texture2D> { targetTex };
        }
        

        #endregion


        #region 图片拆分相关
        [TabGroup("精灵处理器/Right","图片拆分"), ShowInInspector, NonSerialized, HideReferenceObjectPicker, LabelText("名字前缀")]
        public string IndependentNamePrefix;

        [TabGroup("精灵处理器/Right","图片拆分"), Button("拆分", ButtonSizes.Medium)]
        public void GenerateIndependentTexture() {
            if (targets.Count == 0 || string.IsNullOrEmpty(IndependentNamePrefix))
                return;

            var textures = new Texture2D[targets.Count];
            var maxHeight = float.MinValue;
            for (var i = 0; i < targets.Count; i++) {
                var currentTexture = CopyTexture(targets[i].texture,Vector2Int.zero,Vector2Int.zero);
                var rect = targets[i].rect;
                textures[i] = new Texture2D((int)rect.width, (int)rect.height);
                for (int x = 0; x < rect.width; x++) {
                    for (int y = 0; y < rect.height; y++) {
                        textures[i].SetPixel(x, y, currentTexture.GetPixel(x + (int)rect.x, y + (int)rect.y));
                    }
                }

                textures[i].Apply();
                if (targets[i].rect.height > maxHeight) {
                    maxHeight = targets[i].rect.height;
                }
            }

            resultPreview = new List<Texture2D>(textures);
        }
        #endregion
        
        
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
        
        [VerticalGroup("精灵处理器/Left"), LabelText("保存结果"), Button]
        private void SaveTexture() {
            var path = EditorUtility.SaveFilePanel("保存图片处理结果", "", targets[0].name+"_", "png");
            if (string.IsNullOrEmpty(path) || resultPreview.Count == 0) return;
            if (resultPreview.Count == 1) {
                File.WriteAllBytes(path, resultPreview[0].EncodeToPNG());
            }else {
                for (int i = 0; i < resultPreview.Count; i++) {
                    File.WriteAllBytes(path+"_"+i, resultPreview[i].EncodeToPNG());
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"图片处理结果保存在{path}");
        }
    }
#endif
}