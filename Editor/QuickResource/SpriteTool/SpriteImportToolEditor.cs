using System;
using System.Collections.Generic;
using System.IO;
using MycroftToolkit.DiscreteGridToolkit;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MycroftToolkit.QuickResource.SpriteImportTool {
#if UNITY_EDITOR
    public class SpriteImportToolEditor : OdinEditorWindow {
        [MenuItem("Assets/QuickResource/Sprite/ImportTool")]
        private static void Open() {
            EditorWindow window = GetWindow<SpriteImportToolEditor>("贴图导入工具");
            window.minSize = new Vector2(800, 500);
            window.Focus();
            window.Show();

            string[] paths = AssetDatabase.FindAssets("SpriteImportToolEditor");
            _presetGuid = paths[0];
            _presetPath = AssetDatabase.GUIDToAssetPath(_presetGuid);
            _presetPath = _presetPath.Substring(0,
                _presetPath.IndexOf("/SpriteImportToolEditor", StringComparison.Ordinal));
            _presetPath += "/Preset/";
        }

        [HorizontalGroup("Split", width: 200)]
        [VerticalGroup("Split/Left"), Title("检查器")]
        [AssetSelector, HideReferenceObjectPicker]
        public List<Texture2D> targets = new List<Texture2D>();

        private void Update() {
            var array = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
            if (array.Length == 0) return;
            targets.Clear();
            targets.AddRange(array);
        }


        [VerticalGroup("Split/Right"), Title("编辑器"), PropertyOrder(1)]

        #region 预设管理相关

        [VerticalGroup("Split/Right"), LabelText("新预设名称")]
        public string newPreSetName;

        [VerticalGroup("Split/Right"), InfoBox("$debugInfo", InfoMessageType.Error)]
        [ShowIf("_showDebug", Value = true), ReadOnly, PropertyOrder(2)]
        public string debugInfo;
#pragma warning disable CS0414
        private bool _showDebug;
#pragma warning restore CS0414
        private static string _presetPath;
        private static string _presetGuid;


        [VerticalGroup("Split/Right"), Button("新建预设"), PropertyOrder(3)]
        private void NewPreset() {
            if (string.IsNullOrWhiteSpace(newPreSetName)) {
                debugInfo = "Error>新预设名称不合法!";
                _showDebug = true;
                return;
            }

            string targetPath = _presetPath + newPreSetName + ".asset";
            if (File.Exists(targetPath)) {
                debugInfo = "Error>存在同名预设,无法新建!";
                _showDebug = true;
                return;
            }

            _showDebug = false;
            preSet = CreateInstance<SpriteImportPreSet>();
            AssetDatabase.CreateAsset(preSet, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion


        [VerticalGroup("Split/Right"), AssetSelector(Paths = "/Preset/", Filter = "t:TextureImportPreSet"),
         InlineEditor]
        [PropertyOrder(4)]
        public SpriteImportPreSet preSet;


        #region 批处理相关

        [Button("开始批量导入")]
        private void StartBatchImport() {
            List<Texture2D> textures = targets;
            foreach (Texture2D texture in textures) {
                string path = AssetDatabase.GetAssetPath(texture);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) {
                    continue;
                }

                TextureImporterSettings textureSettings = LoadPreset(importer, texture);
                importer.SetTextureSettings(textureSettings);
                importer.SaveAndReimport();
                EditorUtility.SetDirty(importer);
                AssetDatabase.SaveAssetIfDirty(importer);
                AssetDatabase.ImportAsset(path);
                
                if (preSet.importMode != SpriteImportMode.Multiple) continue;
                importer.spritesheet = GetSpritesheet(texture);
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        private TextureImporterSettings LoadPreset(TextureImporter importer, Texture2D texture) {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = preSet.importMode;
            importer.textureCompression = preSet.textureImporterCompression;
            importer.filterMode = preSet.filterMode;
            importer.mipmapEnabled = preSet.mipmapEnabled;
            importer.spritePixelsPerUnit = preSet.pixelsPerUnit;

            TextureImporterSettings textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);

            textureSettings.spriteMeshType = preSet.spriteMeshType;
            textureSettings.wrapMode = preSet.wrapMode;

            textureSettings.spriteExtrude = preSet.spriteExtrude;
            textureSettings.spriteGenerateFallbackPhysicsShape = preSet.generatePhysicsShape;
            textureSettings.alphaIsTransparency = preSet.alphaIsTransparency;
            textureSettings.readable = preSet.readWriteEnabled;

            textureSettings.spritePivot = GetSpritePivot(new Vector2Int(texture.width, texture.height));

            return textureSettings;
        }

        private Vector2 GetSpritePivot(Vector2Int textureSize) {
            switch (preSet.pivotMode) {
                case SpriteAlignment.Center:
                    return new Vector2(0.5f, 0.5f);
                case SpriteAlignment.TopLeft:
                    return new Vector2(0, 1);
                case SpriteAlignment.TopCenter:
                    return new Vector2(0.5f, 1);
                case SpriteAlignment.TopRight:
                    return new Vector2(1, 1);
                case SpriteAlignment.LeftCenter:
                    return new Vector2(0, 0.5f);
                case SpriteAlignment.RightCenter:
                    return new Vector2(1, 0.5f);
                case SpriteAlignment.BottomLeft:
                    return new Vector2(0, 0);
                case SpriteAlignment.BottomCenter:
                    return new Vector2(0.5f, 0);
                case SpriteAlignment.BottomRight:
                    return new Vector2(1, 0);
                case SpriteAlignment.Custom:
                    return !preSet.isPixels ? 
                        preSet.pivot : new Vector2(preSet.pivot.x / textureSize.x, preSet.pivot.y / textureSize.y);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private SpriteMetaData[] GetSpritesheet(Texture2D texture2D) {
            Rect[] rects;
            if (preSet.autoSlicing) {
                rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(
                    texture2D, preSet.pixelsPerUnit, (int)preSet.spriteExtrude / 10);
            } else {
                if (preSet.slicingUseSize) {
                    rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
                        texture2D, Vector2.zero, preSet.slicingInfo, Vector2.zero);
                } else {
                    Vector2 targetSize = new Vector2(
                        (float)texture2D.width / preSet.slicingInfo.x, (float)texture2D.height / preSet.slicingInfo.y);
                    rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
                        texture2D, Vector2.zero, targetSize, Vector2.zero);
                }
            }

            SpriteMetaData[] spMetaList = new SpriteMetaData[rects.Length];
            for (int i = 0; i < rects.Length; i++) {
                SpriteMetaData metaData = new SpriteMetaData {
                    rect = rects[i],
                    name = texture2D.name + "_" + i,
                    alignment = (int)preSet.pivotMode
                };
                if (metaData.alignment == (int)SpriteAlignment.Custom) {
                    metaData.pivot = GetSpritePivot(metaData.rect.size.ToVec2Int());
                }

                spMetaList[i] = metaData;
            }

            return spMetaList;
        }

        #endregion
    }
#endif
}
