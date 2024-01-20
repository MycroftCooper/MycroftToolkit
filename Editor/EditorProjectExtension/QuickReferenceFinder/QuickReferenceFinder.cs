using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Object = UnityEngine.Object;

namespace EditorProjectExtension.ReferenceFinder {
    public class QuickReferenceFinder : EditorWindow {
        #region UI相关
        [MenuItem("Assets/Find References", false)]
        private static void FindObjectReferences() {
            var window = GetWindow<QuickReferenceFinder>(true, "Find References", true);
            window.FindObjectReferences(Selection.activeObject);
        }

        private static readonly List<Object> References = new List<Object>();
        private Object _findReferencesAfterLayout;
        private Vector2 _scrollPosition = Vector2.zero;

        private void OnGUI() {
            GUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Found: " + References.Count);
            if (GUILayout.Button("Clear", EditorStyles.miniButton)) {
                References.Clear();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            for (var i = References.Count - 1; i >= 0; --i) {
                LayoutItem(References[i]);
            }

            EditorGUILayout.EndScrollView();

            if (_findReferencesAfterLayout == null) {
                return;
            }

            FindObjectReferences(_findReferencesAfterLayout);

            _findReferencesAfterLayout = null;
        }

        private void LayoutItem(Object obj) {
            var style = EditorStyles.miniButtonLeft;
            style.alignment = TextAnchor.MiddleLeft;

            if (obj == null) {
                return;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(obj.name, style)) {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            // Use "right arrow" unicode character 
            if (GUILayout.Button("\u25B6", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20))) {
                _findReferencesAfterLayout = obj;
            }

            GUILayout.EndHorizontal();
        }
        #endregion

        private void FindObjectReferences(Object toFind) {
            References.Clear();
            EditorUtility.DisplayProgressBar("Searching", "Generating file paths", 0.0f);
            FindObjectReferencesInFiles(toFind);
            EditorUtility.ClearProgressBar();
        }

        public static void FindObjectReferencesInFiles(Object toFind) {
            var targetPath = AssetDatabase.GetAssetPath(toFind);
            var guid = AssetDatabase.AssetPathToGUID(targetPath);
            var internalID = "";
            if (toFind is Sprite) {
                internalID = GetSpriteInternalID(toFind);
            }
            
            var rootPath = Path.GetDirectoryName(Application.dataPath);
            List<string> result;

            if (string.IsNullOrEmpty(internalID)) {
                result = RipsGrepHelper.Search(guid, rootPath);
            } else {
                var internalIDText = $"fileID: {internalID}, guid: {guid}";
                result = RipsGrepHelper.Search(internalIDText, rootPath);
            }
            LoadFoundObjects(result);
        }

        private static void LoadFoundObjects(List<string> pathList) {
            for (int i = 0; i < pathList.Count; i++) {
                var text = $"Searching dependencies ({i + 1}/{pathList.Count})";
                EditorUtility.DisplayProgressBar("Searching", text, (i + 1f) / pathList.Count);
                
                string path = pathList[i];
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj == null) {
                    Debug.LogError($"QuickReferenceFinder>Path[{path}] object load fail!");
                    continue;
                }
                References.Add(obj);
            }
        }

        private static string GetSpriteInternalID(Object toFind) {
            var targetPath = AssetDatabase.GetAssetPath(toFind);
            var targetMetaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(targetPath);

            var streamReader = new StreamReader(targetMetaPath, Encoding.UTF8);
            var yamlStream = new YamlStream();
            yamlStream.Load(streamReader);
            streamReader.Close();

            var internalID = "";
            var rootNode = yamlStream.Documents[0].RootNode;
            var internalIDToNameTable =
                (YamlSequenceNode)rootNode["TextureImporter"]["internalIDToNameTable"];
            foreach (var internalIDToNameTableNode in internalIDToNameTable.Children) {
                var second = internalIDToNameTableNode["second"];
                if (second.ToString() != toFind.name) {
                    continue;
                }

                internalID = internalIDToNameTableNode["first"]["213"].ToString();
                break;
            }

            if (string.IsNullOrEmpty(internalID)) {
                internalID = GetSpriteInternalIDInSpriteAtlas(toFind);
            }

            return internalID;
        }

        private static string GetSpriteInternalIDInSpriteAtlas(Object toFind) {
            var targetPath = AssetDatabase.GetAssetPath(toFind);
            var guid = AssetDatabase.AssetPathToGUID(targetPath);
            var internalID = "";

            var paths = Directory.GetFiles("Assets/RGTexture/sprite_atlas", "*.spriteatlas",
                SearchOption.AllDirectories);
            foreach (var path in paths) {
                var fileText = File.ReadAllText(path);
                var ret = fileText.IndexOf(guid, StringComparison.Ordinal);
                if (ret == -1) {
                    continue;
                }

                var streamReader = new StreamReader(path, Encoding.UTF8);
                var yamlStream = new YamlStream();
                yamlStream.Load(streamReader);
                streamReader.Close();

                var spriteAtlasNode = yamlStream.Documents[0].RootNode["SpriteAtlas"];
                var packedSpriteNamesToIndex = (YamlSequenceNode)spriteAtlasNode["m_PackedSpriteNamesToIndex"];

                var i = 0;
                var spriteIndex = -1;
                foreach (var packedSpriteName in packedSpriteNamesToIndex) {
                    if (packedSpriteName.ToString() == toFind.name) {
                        spriteIndex = i;
                        break;
                    }
                    ++i;
                }

                if (spriteIndex < 0) {
                    continue;
                }
                var packedSprites = (YamlSequenceNode)spriteAtlasNode["m_PackedSprites"];
                internalID = packedSprites[spriteIndex]["fileID"].ToString();
                break;
            }

            return internalID;
        }
    }
}