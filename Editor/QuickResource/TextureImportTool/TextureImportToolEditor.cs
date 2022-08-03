using System.Collections;
using System.Collections.Generic;
using MycroftToolkit.QuickResource.TextureImportTool;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class TextureImportToolEditor : OdinEditorWindow
{
    [AssetSelector(Paths = "/Preset/", Filter = "t:TextureImportPreSet"), InlineEditor]
    public TextureImportPreSet preSet; 
    
    [MenuItem("Assets/TextureImportTool/Editor")]
    private static void Open() {
        GetWindow<TextureImportToolEditor>().Show();
    }
    
    [BoxGroup("预设管理"), Button("新建预设")]
    private void NewPreset() {
        preSet = CreateInstance<TextureImportPreSet>();
    }
    [BoxGroup("预设管理"), Button("保存预设")]
    private void SavePreset() {
        AssetDatabase.CreateAsset(preSet, $"/Preset/{preSet.preSetName}");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    [BoxGroup("预设管理"), Button("加载预设")]
    private void LoadPreset() {
        
    }
    
    [Button("开始批处理")]
    private void StartBatchProcessing() {
        
    }
}
#endif
