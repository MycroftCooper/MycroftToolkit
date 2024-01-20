using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Object = UnityEngine.Object;

namespace EditorProjectExtension.ReferenceFinder {
    public class QuickReferenceFinderCtrl {
        public Object TargetObject;
        public List<Object> References = new List<Object>();
        public int ReferencesCount => References.Count;

        public void FindObjectReferences(Object targetObj) {
            References.Clear();
            TargetObject = targetObj;

            try {
                EditorUtility.DisplayProgressBar("Searching", "Generating file paths", 0.0f);

                var targetPath = AssetDatabase.GetAssetPath(targetObj);
                var guid = AssetDatabase.AssetPathToGUID(targetPath);
                var internalID = "";
                if (targetObj is Sprite) {
                    internalID = GetSpriteInternalID(targetObj);
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
            } catch (Exception ex) {
                Debug.LogError($"Error while searching for references: {ex.Message}");
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void LoadFoundObjects(List<string> pathList) {
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

        #region Sprite处理相关
        private string GetSpriteInternalID(Object toFind) {
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

        private string GetSpriteInternalIDInSpriteAtlas(Object toFind) {
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
        #endregion
    }
}