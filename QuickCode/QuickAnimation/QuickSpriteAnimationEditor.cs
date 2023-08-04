#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MycroftToolkit.QuickCode.QuickAnimation {
    public partial class QuickSpriteAnimation {
        [Header("QuickLoadTexture")]
        [OnValueChanged(nameof(OnTextureChange))]
        [BoxGroup("Resources")]
        public Texture texture;
        [BoxGroup("Resources")]
        public bool isInRange;
        [ShowIf(nameof(isInRange))]
        [BoxGroup("Resources")]
        public Vector2Int range;
        private void OnTextureChange() {
            if (texture == null) return;
            string path = AssetDatabase.GetAssetPath(texture);
            List<Sprite> spList = new List<Sprite>();

            foreach (var item in AssetDatabase.LoadAllAssetsAtPath(path)) {
                if (item is Sprite sprite) {
                    spList.Add(sprite);
                }
            }
            sprites = isInRange ? spList.GetRange(range.x, range.y - range.x + 1).ToArray() : spList.ToArray();
        }
        
        private bool _isAdded;
        private float _lastFrameTime;

        [Button]
        private void EditorPlay() {
            if (Application.isPlaying) {
                return;
            }

            if (_isAdded) {
                RemoveEditorUpdate();
            } else {
                AddToEditorUpdate();
            }
        }

        private void AddToEditorUpdate() {
            _isAdded = true;
            EditorApplication.update += EditorUpdate;
            _lastFrameTime = Time.realtimeSinceStartup;
        }

        [OnInspectorDispose]
        private void OnInspectorDispose() {
            RemoveEditorUpdate();
        }

        private void RemoveEditorUpdate() {
            if (!_isAdded) return;
            _isAdded = false;
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate() {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;
            
            _currentFrameDuration += deltaTime;

            if (_currentFrameDuration < 1 / frameRate) {
                return;
            }

            NextFrame();
        }
    }
}
#endif