using UnityEngine;

namespace MycroftToolkit.QuickShader {
    [ExecuteInEditMode]
    public class SpriteOutline : MonoBehaviour {
        public Color color = Color.white;
        public bool showOutline = true;
        private SpriteRenderer _spriteRenderer;
        private static readonly int Outline = Shader.PropertyToID("_Outline");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        void OnEnable() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            showOutline = true;
            UpdateOutline();
        }

        void OnDisable() {
            showOutline = false;
            UpdateOutline();
        }

        void Update() {
            UpdateOutline();
        }

        void UpdateOutline() {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            _spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(Outline, showOutline ? 1f : 0);
            mpb.SetColor(OutlineColor, color);
            _spriteRenderer.SetPropertyBlock(mpb);
        }
    }
}