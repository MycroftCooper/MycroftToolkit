using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
public class TextureProcessingToolEditor : OdinEditorWindow {
    [MenuItem("Assets/QuickResource/Texture/ProcessingTool")]
    public static void CreateWindow() {
        EditorWindow window = GetWindow<TextureProcessingToolEditor>("贴图处理工具");
        window.minSize = new Vector2(600, 400);
        window.Focus();
        window.Show();
    }
    

    #region 图片描边相关
    [AssetSelector, TabGroup("图片描边"), LabelText("目标图片")]
    public Sprite targetSprite;
    [TabGroup("图片描边"), LabelText("描边颜色")]
    public Color outlineColor;
    [TabGroup("图片描边"), Range(1,64), LabelText("描边大小")]
    public int outlineSize;
    
    [TabGroup("图片描边"), LabelText("结果预览"), PreviewField(128, ObjectFieldAlignment.Center), ReadOnly]
    public Sprite resultPreview;
    
    [TabGroup("图片描边"), Button("生成描边图", ButtonSizes.Medium)]
    public void GenerateOutlinePicture() {
        if (targetSprite == null) {
            return;
        }
        var spriteTexture = targetSprite.texture;
        var newTexture = DuplicateTexture(spriteTexture);
        var spriteRect = targetSprite.rect;
        var texture = new Texture2D((int)spriteRect.width + 1, (int)spriteRect.height + 1);
        for (var i = 0; i < spriteRect.width + 1; i++) {
            for (var j = 0; j < spriteRect.height + 1; j++) {
                if (i == 0 || j == 0) {
                    texture.SetPixel(i, j, Color.clear);
                    continue;
                }
                var originPixel = newTexture.GetPixel((int) spriteRect.x + i - 1, (int) spriteRect.y + j - 1);
                texture.SetPixel(i, j, originPixel);
            }
        }
        texture.Apply();
        var colors = new Color[texture.width * texture.height];
        for (var i = 0; i < texture.width; i++) {
            for (var j = 0; j < texture.height; j++) {
                var left = i > 0 ? texture.GetPixel(i - 1, j).a : -1;
                var right = i < texture.width - 1 ? texture.GetPixel(i + 1, j).a : -1;
                var up = j > 0 ? texture.GetPixel(i, j - 1).a : -1;
                var down = j < texture.height - 1 ? texture.GetPixel(i, j + 1).a : -1;
                var currentColor = texture.GetPixel(i, j);
                var outline = Mathf.RoundToInt(Mathf.Max(Mathf.Max(left, up), Mathf.Max(right, down)) - currentColor.a);
                colors[i + j * texture.width] = Color.Lerp(currentColor, outlineColor, outline);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        var originPath = AssetDatabase.GetAssetPath(targetSprite);
        var newPath = originPath.Remove(originPath.Length - 5) + "_outline.png";
        File.WriteAllBytes(newPath, texture.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log($"描边图保存在 ‘{newPath}’", AssetDatabase.LoadAssetAtPath<Texture2D>(newPath));
    }
    
    #endregion

    
    private static Texture2D DuplicateTexture(Texture2D source) {
        var renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        var readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    [AssetSelector, TabGroup("图片拼接"), LabelText("需要生成的sprite"), HideReferenceObjectPicker]
    public List<Sprite> combinationSprites = new List<Sprite>();
    [TabGroup("图片拼接"), Button("合并为新图集", ButtonSizes.Medium)]
    public void GenerateCombinedTexture() {
        if (combinationSprites.Count == 0) {
            return;
        }
        var textures = new Texture2D[combinationSprites.Count];
        var maxHeight = float.MinValue;
        var wholeWidth = 0f;
        for (var i = 0; i < combinationSprites.Count; i++) {
            textures[i] = DuplicateTexture(combinationSprites[i].texture);
            if (combinationSprites[i].rect.height > maxHeight) {
                maxHeight = combinationSprites[i].rect.height;
            }
            wholeWidth += combinationSprites[i].rect.width;
        }
        var targetTex = new Texture2D((int) wholeWidth, (int) maxHeight);
        var currentX = 0;
        for (var i = 0; i < textures.Length; i++) {
            var currentSpriteRect = combinationSprites[i].rect;
            var pixels = textures[i].GetPixels((int)currentSpriteRect.x, (int)currentSpriteRect.y,
                (int)currentSpriteRect.width, (int)currentSpriteRect.height);
            targetTex.SetPixels(currentX, 0, (int)currentSpriteRect.width, (int)currentSpriteRect.height, pixels);
            currentX += (int)currentSpriteRect.width;
        }
        targetTex.Apply();
        var path = EditorUtility.SaveFilePanel("保存新图集", "", "texture", "png");
        if (string.IsNullOrEmpty(path)) {
            return;
        }
        File.WriteAllBytes(path, targetTex.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log($"新贴图保存在{path}");
    }

    [TabGroup("图片拆分"), ShowInInspector, NonSerialized, HideReferenceObjectPicker, LabelText("需要生成的sprite")]
    public List<Sprite> IndependentSprites = new List<Sprite>();

    [TabGroup("图片拆分"), ShowInInspector, NonSerialized, HideReferenceObjectPicker, LabelText("名字前缀")]
    public string IndependentNamePrefix;

    public TextureProcessingToolEditor(Sprite targetSprite) {
        this.targetSprite = targetSprite;
    }

    [TabGroup("图片拆分"), Button("生成贴图", ButtonSizes.Medium)]
    public void GenerateIndependentTexture() {
        if (IndependentSprites.Count == 0 || string.IsNullOrEmpty(IndependentNamePrefix)) {
            return;
        }
        var textures = new Texture2D[IndependentSprites.Count];
        var maxHeight = float.MinValue;
        for (var i = 0; i < IndependentSprites.Count; i++) {
            var currentTexture = DuplicateTexture(IndependentSprites[i].texture);
            var rect = IndependentSprites[i].rect;
            textures[i] = new Texture2D((int)rect.width, (int)rect.height);
            for (int x = 0; x < rect.width; x++) {
                for (int y = 0; y < rect.height; y++) {
                    textures[i].SetPixel(x, y, currentTexture.GetPixel(x + (int)rect.x, y + (int)rect.y));
                }
            }
            textures[i].Apply();
            if (IndependentSprites[i].rect.height > maxHeight) {
                maxHeight = IndependentSprites[i].rect.height;
            }
        }
        
        var path = EditorUtility.SaveFolderPanel("保存新图集", "", "");
        if (string.IsNullOrEmpty(path)) {
            return;
        }
        for (var i = 0; i < textures.Length; i++) {
            var texture = textures[i];
            File.WriteAllBytes($"{path}/{IndependentNamePrefix}_{i}.png", texture.EncodeToPNG());
        }

        AssetDatabase.Refresh();
        Debug.Log($"新贴图保存在{path}");
    }
}
#endif