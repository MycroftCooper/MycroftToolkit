using System;
using System.Collections.Generic;
using System.IO;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.QuickResource.TextureImportTool;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if UNITY_EDITOR
public class TextureImportToolEditor : OdinEditorWindow {
    [MenuItem("Assets/TextureImportTool/Editor")]
    private static void Open() {
        GetWindow<TextureImportToolEditor>().Show();
        string[] paths = AssetDatabase.FindAssets("TextureImportToolEditor");
        _presetGuid = paths[0];
        _presetPath = AssetDatabase.GUIDToAssetPath(_presetGuid);
        _presetPath = _presetPath.Substring(0,_presetPath.IndexOf("/TextureImportToolEditor", StringComparison.Ordinal));
        _presetPath += "/Preset/";
    }
    #region 预设管理相关
    private static string _presetPath;
    private static string _presetGuid;
    private bool _showDebug;
    [InfoBox("$debugInfo", InfoMessageType.Error), ShowIfGroup("预设管理/_showDebug", Value = true), ReadOnly]
    public string debugInfo;

    [BoxGroup("预设管理"), LabelText("新预设名称")] 
    public string newPreSetName;
    [BoxGroup("预设管理"), Button("新建预设")]
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
        preSet = CreateInstance<TextureImportPreSet>();
        AssetDatabase.CreateAsset(preSet, targetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    #endregion
    
    
    [AssetSelector(Paths = "/Preset/", Filter = "t:TextureImportPreSet"), InlineEditor]
    public TextureImportPreSet preSet; 
    
    
    #region 批处理相关
    [Button("开始批处理")]
    private void StartBatchProcessing() {
        Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        foreach (Texture2D texture in textures) {
            string path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) {
                continue;
            }

            TextureImporterSettings textureSettings = LoadPreset(importer, texture);
            importer.SetTextureSettings(textureSettings);
            AssetDatabase.ImportAsset(path);
        }
    }

    private TextureImporterSettings LoadPreset(TextureImporter importer, Texture2D texture) {
        importer.spriteImportMode = preSet.importMode;
        TextureImporterSettings textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        
        textureSettings.textureType = TextureImporterType.Sprite;
        textureSettings.spriteMeshType = preSet.spriteMeshType;
        textureSettings.wrapMode = preSet.wrapMode;
        textureSettings.filterMode = preSet.filterMode;
        importer.textureCompression = preSet.textureImporterCompression;
            
        textureSettings.spritePixelsPerUnit =  preSet.pixelsPerUnit;
        textureSettings.spriteExtrude = preSet.spriteExtrude;
        textureSettings.spriteGenerateFallbackPhysicsShape = preSet.generatePhysicsShape;
        textureSettings.alphaIsTransparency = preSet.alphaIsTransparency;
        textureSettings.readable = preSet.readWriteEnabled;
        textureSettings.mipmapEnabled = preSet.mipmapEnabled;

        textureSettings.spritePivot = GetSpritePivot(new Vector2Int(texture.width, texture.height) );

        if (preSet.importMode == SpriteImportMode.Multiple) {
            importer.spritesheet = GetSpritesheet(texture);
        }
        return textureSettings;
    }

    private Vector2 GetSpritePivot(Vector2Int textureSize) {
        switch (preSet.pivotMode) {
            case SpriteAlignment.Center:
                return new Vector2(0.5f,0.5f);
            case SpriteAlignment.TopLeft:
                return new Vector2(0,1);
            case SpriteAlignment.TopCenter:
                return new Vector2(0.5f,1);
            case SpriteAlignment.TopRight:
                return new Vector2(1,1);
            case SpriteAlignment.LeftCenter:
                return new Vector2(0,0.5f);
            case SpriteAlignment.RightCenter:
                return new Vector2(1,0.5f);
            case SpriteAlignment.BottomLeft:
                return new Vector2(0,0);
            case SpriteAlignment.BottomCenter:
                return new Vector2(0.5f,0);
            case SpriteAlignment.BottomRight:
                return new Vector2(1,0);
            case SpriteAlignment.Custom:
                if (!preSet.isPixels) {
                    return preSet.pivot;
                }
                return new Vector2(preSet.pivot.x / textureSize.x, preSet.pivot.y / textureSize.y);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private SpriteMetaData[] GetSpritesheet(Texture2D texture2D) {
        Rect[] rects;
        if (preSet.autoSlicing)
        {
            rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(
                texture2D, preSet.pixelsPerUnit, (int)preSet.spriteExtrude/10);
        } else {
            if (preSet.slicingUseSize) {
                rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
                    texture2D, Vector2.zero, preSet.slicingInfo, Vector2.zero);
            } else {
                Vector2 targetSize = new Vector2(
                    (float)texture2D.width/preSet.slicingInfo.x, (float)texture2D.height/preSet.slicingInfo.y);
                rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
                    texture2D, Vector2.zero, targetSize, Vector2.zero);
            }
        }
        
        SpriteMetaData[] spMetaList = new SpriteMetaData[rects.Length];
        for (int i = 0; i < rects.Length; i++) {
            SpriteMetaData metaData = new SpriteMetaData {
                rect = rects[i],
                name = texture2D.name + "_" + i,
                alignment = (int) preSet.pivotMode
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
