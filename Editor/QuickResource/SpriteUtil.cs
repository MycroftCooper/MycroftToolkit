using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEditorInternal;
using UnityEngine;

public static class SpriteUtil {

    public static SpriteMetaData[] SliceTexture(Texture2D texture2D, SpriteAlignment spAlign, int pixelSize = 32) {
        return SliceTexture(texture2D, spAlign, Vector2.one * pixelSize);
    }
    public static SpriteMetaData[] SliceTexture(Texture2D texture2D, SpriteAlignment spAlign, Vector2 size) {
        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture2D, Vector2.zero, size, Vector2.zero);
        List<SpriteMetaData> spMetaList = new List<SpriteMetaData>();
        for (int i = 0; i < rects.Length; i++) {
            SpriteMetaData metaData = new SpriteMetaData();
            metaData.rect = rects[i];
            metaData.name = texture2D.name + "_" + i;
            metaData.alignment = (int) spAlign;
            spMetaList.Add(metaData);
        }
        return spMetaList.ToArray();
    }
    
    [MenuItem("Assets/SpriteUtil/SpriteSlicer/SliceCharacter_32x32", false, 11)]
    public static void SliceCharacter_32x32() {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        foreach (var texture in textures) {
            string path = AssetDatabase.GetAssetPath(texture);
            ProcessTexture(texture);
            SliceTexture(texture, SpriteAlignment.BottomCenter);
        }
    }
    
    [MenuItem("Assets/SpriteUtil/PixelSpriteImport", false, 1)]
    public static void PixelTextureImport() {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        foreach (var texture in textures) {
            string path = AssetDatabase.GetAssetPath(texture);
            ProcessTextureSetting(texture);
        }
    }

    static void ProcessTextureSetting(Texture2D texture) {
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 16;
        
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 1;
        
        importer.SetTextureSettings(textureSettings);
        AssetDatabase.ImportAsset(path);
    }

    public static void ProcessTexture(Texture2D texture, Vector2 size = default(Vector2)) {
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;


        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 16;

        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 1;

        importer.SetTextureSettings(textureSettings);
        if (size == default(Vector2)) {
            size = new Vector2(32, 32);
        }
        var metaDatas = SliceTexture(texture, SpriteAlignment.BottomCenter, size);

        importer.spritesheet = metaDatas;
        AssetDatabase.ImportAsset(path);
    }
}