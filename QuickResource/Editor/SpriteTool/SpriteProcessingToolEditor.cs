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
        
        private void Update() {
            var array = Selection.GetFiltered<Sprite>(SelectionMode.ExcludePrefab | SelectionMode.Editable);
            if (array.Length == 0) return;
            targets.Clear();
            targets.AddRange(array);
        }
        
        [HorizontalGroup("精灵处理器",width:500)]
        [VerticalGroup("精灵处理器/Left"), Title("检查器")]
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("目标预览")]
        [AssetSelector, HideReferenceObjectPicker]
        public List<Sprite> targets = new List<Sprite>();
        
        [VerticalGroup("精灵处理器/Left"),LabelWidth(60), LabelText("结果预览"), PreviewField(256, ObjectFieldAlignment.Left), ReadOnly]
        public List<Texture2D> resultPreview = new List<Texture2D>();

        #region 图片描边
        public enum ELineMode {Out, On, In }
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("描边颜色")]
        public Color outlineColor = Color.white;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("描边模式")]
        public ELineMode lineMode;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("距离模式")]
        public EDistanceType distanceType;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("距离"), PropertyRange(1,64)]
        public int width = 1;
        [TabGroup("精灵处理器/Right","图片描边"), LabelText("自动扩展")]
        public bool autoExtend;

        [TabGroup("精灵处理器/Right", "图片描边"), Button("图片描边", ButtonSizes.Medium)]
        public void GenerateOutlineTexture() {
            if(targets.Count == 0)return;
            resultPreview.Clear();
            foreach (var target in targets) {
                Texture2D result = GetOutlineTexture(target.GetSlicedTexture());
                result.filterMode = FilterMode.Point;
                resultPreview.Add(result);
            }
        }
        public Texture2D GetOutlineTexture(Texture2D targetTexture2D) {
            if (targetTexture2D == null) return null;
            Texture2D  copyTexture = autoExtend ? 
                targetTexture2D.ExtendTexture(width,true) : 
                targetTexture2D.CopyTexture_CPU(Vector2Int.zero, Vector2Int.zero);
            copyTexture.Apply();

            Color[] colors =  copyTexture.GetColors();

            List<Vector2Int> borderlinePoints = copyTexture.GetBorderlinePoints();
            
            int radius = lineMode == ELineMode.On ? width / 2 : width;
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
                            if (pos != targetPoints.Center && copyTexture.GetPixel(pos.x, pos.y).a != 0)
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
                textures[i] = targets[i].texture.CopyTexture_GPU(Vector2Int.zero,Vector2Int.zero);
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
        [TabGroup("精灵处理器/Right","图片拆分"), Button("拆分", ButtonSizes.Medium)]
        public void GenerateIndependentTexture() {
            if (targets.Count == 0)
                return;

            var textures = new Texture2D[targets.Count];
            var maxHeight = float.MinValue;
            for (var i = 0; i < targets.Count; i++) {
                textures[i] = targets[i].GetSlicedTexture();
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
            if(resultPreview.Count == 0) return;
            string defaultName = targets[0].name.Substring(0, targets[0].name.IndexOf("_0", StringComparison.Ordinal));
            string path = EditorUtility.SaveFilePanel("保存图片处理结果", "", defaultName, "png");
            if (string.IsNullOrEmpty(path)) return;
            
            if (resultPreview.Count == 1) {
                File.WriteAllBytes(path, resultPreview[0].EncodeToPNG());
            }else {
                path = path.Remove(path.Length - 4, 4);
                for (int i = 0; i < resultPreview.Count; i++) {
                    File.WriteAllBytes(path +"_"+ i +".png", resultPreview[i].EncodeToPNG());
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"图片处理结果保存在{path}");
        }
    }
#endif
}