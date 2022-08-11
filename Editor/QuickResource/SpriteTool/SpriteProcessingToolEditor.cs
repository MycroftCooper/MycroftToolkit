using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using MycroftToolkit.DiscreteGridToolkit.Square;
using MycroftToolkit.QuickCode;

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

        #region 图片描边
        public enum ELineMode {Out, On, In }
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("描边颜色")]
        public Color outlineColor;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("描边模式")]
        public ELineMode lineMode;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("距离模式")]
        public EDistanceType distanceType;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("距离"), Range(1,64)]
        public int width;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("自动扩展")]
        public bool autoExtend;

        [TabGroup("精灵处理器/Right", "图片描边"), Button("图片描边", ButtonSizes.Medium)]
        public void GenerateOutlineTexture() {
            if(targets.Count == 0)return;
            resultPreview.Clear();
            foreach (var target in targets) {
                Texture2D result = GetOutlineTexture(target.texture);
                result.filterMode = FilterMode.Point;
                resultPreview.Add(result);
            }
        }
        public Texture2D GetOutlineTexture(Texture2D targetTexture2D) {
            if (targetTexture2D == null) return null;
            Texture2D  copyTexture = autoExtend ? 
                targetTexture2D.ExtendTexture(width,true) : 
                targetTexture2D.CopyTexture(Vector2Int.zero, Vector2Int.zero);
            copyTexture.Apply();
            
            Color[] colors =  copyTexture.GetColors();
            List<Vector2Int> borderlinePoints = copyTexture.GetBorderlinePoints();
            
            int radius = lineMode == ELineMode.On ? Math.Max(1, width / 2) : width;
            foreach (Vector2Int point in borderlinePoints) {
                PointSetRadius targetPoints = new PointSetRadius(point, radius, distanceType);
                targetPoints.ForEach((pos) => {
                    int targetIndex = pos.x + pos.y * copyTexture.width;
                    if (targetIndex >= colors.Length || targetIndex < 0) return;
                    switch (lineMode) {
                        case ELineMode.Out:
                            if (copyTexture.GetPixel(pos.x, pos.y).a == 0)
                                colors[targetIndex] = outlineColor;
                            break;
                        case ELineMode.On:
                            colors[targetIndex] = outlineColor;
                            break;
                        case ELineMode.In:
                            if (copyTexture.GetPixel(pos.x, pos.y).a != 0)
                                colors[targetIndex] = outlineColor;
                            break;
                    }
                });
            }
            copyTexture.SetPixels(colors);
            copyTexture.Apply();

            return copyTexture;
        }
        #endregion
        
        
        #region 图片拼接相关
        [TabGroup("精灵处理器/Right","图片拼接"), Button("合并", ButtonSizes.Medium)]
        public void GenerateCombinedTexture() {
            if (targets.Count == 0) return;

            var textures = new Texture2D[targets.Count];
            var maxHeight = float.MinValue;
            var wholeWidth = 0f;
            for (var i = 0; i < targets.Count; i++) {
                textures[i] = targets[i].texture.CopyTexture(Vector2Int.zero,Vector2Int.zero);
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
            resultPreview.Clear();
            resultPreview.Add(targetTex);
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
                var currentTexture = targets[i].texture.CopyTexture(Vector2Int.zero,Vector2Int.zero);
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
            resultPreview.Clear();
            resultPreview = new List<Texture2D>(textures);
        }
        #endregion

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