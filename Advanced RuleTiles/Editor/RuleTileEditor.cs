using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AdvancedRuleTiles;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using RuleTile = UnityEngine.AdvancedRuleTiles.RuleTile;

namespace UnityEditor.AdvancedRuleTiles {
    [CustomEditor(typeof(RuleTile))]
    [CanEditMultipleObjects]
    internal class RuleTileEditor : Editor {
        static GUIStyle grey = new GUIStyle();
        private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
        private const string s_Arrow0 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPzZExDoQwDATzE4oU4QXXcgUFj+YxtETwgpMwXuFcwMFSRMVKKwzZcWzhiMg91jtg34XIntkre5EaT7yjjhI9pOD5Mw5k2X/DdUwFr3cQ7Pu23E/BiwXyWSOxrNqx+ewnsayam5OLBtbOGPUM/r93YZL4/dhpR/amwByGFBz170gNChA6w5bQQMqramBTgJ+Z3A58WuWejPCaHQAAAABJRU5ErkJggg==";
        private const string s_Arrow1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPxYzBDYAgEATpxYcd+PVr0fZ2siZrjmMhFz6STIiDs8XMlpEyi5RkO/d66TcgJUB43JfNBqRkSEYDnYjhbKD5GIUkDqRDwoH3+NgTAw+bL/aoOP4DOgH+iwECEt+IlFmkzGHlAYKAWF9R8zUnAAAAAElFTkSuQmCC";
        private const string s_Arrow2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAC0SURBVDhPjVE5EsIwDMxPKFKYF9CagoJH8xhaMskLmEGsjOSRkBzYmU2s9a58TUQUmCH1BWEHweuKP+D8tphrWcAHuIGrjPnPNY8X2+DzEWE+FzrdrkNyg2YGNNfRGlyOaZDJOxBrDhgOowaYW8UW0Vau5ZkFmXbbDr+CzOHKmLinAXMEePyZ9dZkZR+s5QX2O8DY3zZ/sgYcdDqeEVp8516o0QQV1qeMwg6C91toYoLoo+kNt/tpKQEVvFQAAAAASUVORK5CYII=";
        private const string s_Arrow3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAB2SURBVDhPzY1LCoAwEEPnLi48gW5d6p31bH5SMhp0Cq0g+CCLxrzRPqMZ2pRqKG4IqzJc7JepTlbRZXYpWTg4RZE1XAso8VHFKNhQuTjKtZvHUNCEMogO4K3BhvMn9wP4EzoPZ3n0AGTW5fiBVzLAAYTP32C2Ay3agtu9V/9PAAAAAElFTkSuQmCC";
        private const string s_Arrow5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPnY3BCYBADASvFx924NevRdvbyoLBmNuDJQMDGjNxAFhK1DyUQ9fvobCdO+j7+sOKj/uSB+xYHZAxl7IR1wNTXJeVcaAVU+614uWfCT9mVUhknMlxDokd15BYsQrJFHeUQ0+MB5ErsPi/6hO1AAAAAElFTkSuQmCC";
        private const string s_Arrow6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACaSURBVDhPxZExEkAwEEVzE4UiTqClUDi0w2hlOIEZsV82xCZmQuPPfFn8t1mirLWf7S5flQOXjd64vCuEKWTKVt+6AayH3tIa7yLg6Qh2FcKFB72jBgJeziA1CMHzeaNHjkfwnAK86f3KUafU2ClHIJSzs/8HHLv09M3SaMCxS7ljw/IYJWzQABOQZ66x4h614ahTCL/WT7BSO51b5Z5hSx88AAAAAElFTkSuQmCC";
        private const string s_Arrow7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABQSURBVDhPYxh8QNle/T8U/4MKEQdAmsz2eICx6W530gygr2aQBmSMphkZYxqErAEXxusKfAYQ7XyyNMIAsgEkaYQBkAFkaYQBsjXSGDAwAAD193z4luKPrAAAAABJRU5ErkJggg==";
        private const string s_Arrow8 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPxZE9DoAwCIW9iUOHegJXHRw8tIdx1egJTMSHAeMPaHSR5KVQ+KCkCRF91mdz4VDEWVzXTBgg5U1N5wahjHzXS3iFFVRxAygNVaZxJ6VHGIl2D6oUXP0ijlJuTp724FnID1Lq7uw2QM5+thoKth0N+GGyA7IA3+yM77Ag1e2zkey5gCdAg/h8csy+/89v7E+YkgUntOWeVt2SfAAAAABJRU5ErkJggg==";
        private const string s_MirrorX = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG1JREFUOE+lj9ENwCAIRB2IFdyRfRiuDSaXAF4MrR9P5eRhHGb2Gxp2oaEjIovTXSrAnPNx6hlgyCZ7o6omOdYOldGIZhAziEmOTSfigLV0RYAB9y9f/7kO8L3WUaQyhCgz0dmCL9CwCw172HgBeyG6oloC8fAAAAAASUVORK5CYII=";
        private const string s_MirrorY = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG9JREFUOE+djckNACEMAykoLdAjHbPyw1IOJ0L7mAejjFlm9hspyd77Kk+kBAjPOXcakJIh6QaKyOE0EB5dSPJAiUmOiL8PMVGxugsP/0OOib8vsY8yYwy6gRyC8CB5QIWgCMKBLgRSkikEUr5h6wOPWfMoCYILdgAAAABJRU5ErkJggg==";
        private const string s_Rotated = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAHdJREFUOE+djssNwCAMQxmIFdgx+2S4Vj4YxWlQgcOT8nuG5u5C732Sd3lfLlmPMR4QhXgrTQaimUlA3EtD+CJlBuQ7aUAUMjEAv9gWCQNEPhHJUkYfZ1kEpcxDzioRzGIlr0Qwi0r+Q5rTgM+AAVcygHgt7+HtBZs/2QVWP8ahAAAAAElFTkSuQmCC";
        #region Advanced
        private const string circle1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFSAj+Hmf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAAX8aQSTpzLc0AAAAASUVORK5CYII=";
        private const string circle2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFTg9g3V/+iYbI1EGYBPI14D8CkiaAAh04eAZpL9TFFoUxzPFKcwugEA2grX1TiIQSkAAAAASUVORK5CYII=";
        private const string circle3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFSgUXbzf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAA3Sqvwf/BxaMAAAAASUVORK5CYII=";
        private const string circle4 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFTgXlDHf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAAvYvOVenLju0AAAAASUVORK5CYII=";


        private const string circle5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFTgQanAf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAAyce1sVPCVtQAAAAASUVORK5CYII=";
        private const string circle6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFTA8ojdf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAAOpGpOV3hibcAAAAASUVORK5CYII=";
        private const string circle7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAMklEQVR42mNgGFTgiKXlf3RMtkaiDMCnEa8B+BQRNICQ6UNAM8l+pii0KY5nilMY3QAAvUynvQdvQVEAAAAASUVORK5CYII=";
        //     ----------------------------------------------------------------------
        private const string x1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAPElEQVR42mNgGB6AmSHtPwgTEsOqkY/hJhjDFGMTI6gZphidT7TtJGnE5wKyNJLtZJJcQFFoUxTPQwcAAPZlZgeDaYACAAAAAElFTkSuQmCC";
        private const string x2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAPElEQVR42mNgGB5gzkyp/yBMSAyrxts3VMEYphibGEHNMMXofKJtJ0kjPheQpZFsJ5PkAopCm6J4HjoAANz3nkCCfAG6AAAAAElFTkSuQmCC";
        private const string x3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAUUlEQVR42mNgGB5AW8boDQgTEsOqcYLZ3v8grC1j/AYiZowkhscAZM0wA1D5BG1H1YDuEiL8TaKNhG0m0cmYYYA3wIwJhLYxYdvRFWETG8IAAJaaed90uTinAAAAAElFTkSuQmCC";
        private const string x4 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAPElEQVR42mNgGB5gpmXcfxAmJIZV472gDjCGKcYmRlAzTDE6n2jbSdKIzwVkaSTbySS5gKLQpiiehw4AAFXolryf9K/0AAAAAElFTkSuQmCC";
        private const string x5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAPElEQVR42mNgGB5gWSj/fxAmJIZV44NSATCGKcYmRlAzTDE6n2jbSdKIzwVkaSTbySS5gKLQpiiehw4AANpPh+HPPMQxAAAAAElFTkSuQmCC";
        private const string x6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAATklEQVR42mNgGB5A0VrxDQgTEsOq0eKo3X8QhinGJkZQM0wxCt+KBNvRDSLa31TRSLSTFfD5+ZDtf0VLIgMMI7QJaYbZjjWerYj09+AHAJzfcOZDjC/eAAAAAElFTkSuQmCC";
        #endregion
        private static Texture2D[] s_Arrows;
        public static Texture2D[] arrows {
            get {
                if (s_Arrows == null) {
                    s_Arrows = new Texture2D[10];
                    s_Arrows[0] = Base64ToTexture(s_Arrow0);
                    s_Arrows[1] = Base64ToTexture(s_Arrow1);
                    s_Arrows[2] = Base64ToTexture(s_Arrow2);
                    s_Arrows[3] = Base64ToTexture(s_Arrow3);
                    s_Arrows[5] = Base64ToTexture(s_Arrow5);
                    s_Arrows[6] = Base64ToTexture(s_Arrow6);
                    s_Arrows[7] = Base64ToTexture(s_Arrow7);
                    s_Arrows[8] = Base64ToTexture(s_Arrow8);
                    s_Arrows[9] = Base64ToTexture(s_XIconString);
                }
                return s_Arrows;
            }
        }
        private static Texture2D[] t_icons;
        public static Texture2D[] icons {
            get {
                if (t_icons == null) {
                    t_icons = new Texture2D[13];
                    /*t_icons[0] = Base64ToTexture("");
                    t_icons[1] = Base64ToTexture("");
                    t_icons[2] = Base64ToTexture("");*/
                    t_icons[0] = Base64ToTexture(circle1);
                    t_icons[1] = Base64ToTexture(circle2);
                    t_icons[2] = Base64ToTexture(circle3);
                    t_icons[3] = Base64ToTexture(circle4);
                    t_icons[4] = Base64ToTexture(circle5);
                    t_icons[5] = Base64ToTexture(circle6);
                    t_icons[6] = Base64ToTexture(circle7);
                    t_icons[7] = Base64ToTexture(x1);
                    t_icons[8] = Base64ToTexture(x2);
                    t_icons[9] = Base64ToTexture(x3);
                    t_icons[10] = Base64ToTexture(x4);
                    t_icons[11] = Base64ToTexture(x5);
                    t_icons[12] = Base64ToTexture(x6);
                }
                return t_icons;
            }
        }


        private static Texture2D[] s_AutoTransforms;
        public static Texture2D[] autoTransforms {
            get {
                if (s_AutoTransforms == null) {
                    s_AutoTransforms = new Texture2D[3];
                    s_AutoTransforms[0] = Base64ToTexture(s_Rotated);
                    s_AutoTransforms[1] = Base64ToTexture(s_MirrorX);
                    s_AutoTransforms[2] = Base64ToTexture(s_MirrorY);
                }
                return s_AutoTransforms;
            }
        }

        private ReorderableList m_ReorderableList;
        public RuleTile tile { get { return (target as RuleTile); } }
        private Rect m_ListRect;

        public const float k_DefaultElementHeight = 48f;
        public const float k_PaddingBetweenRules = 30f; // was 13
        public const float k_SingleLineHeight = 16f;
        public const float k_LabelWidth = 53f;


        public void OnEnable() {
            grey.normal.textColor = Color.grey;
            if (tile.NotZero == 8) {
                NewRuleTile();
            }

            if (tile.m_TilingRules == null)
                tile.m_TilingRules = new List<RuleTile.TilingRule>();

            m_ReorderableList = new ReorderableList(tile.m_TilingRules, typeof(RuleTile.TilingRule), true, true, true, true);
            m_ReorderableList.drawHeaderCallback = OnDrawHeader;
            m_ReorderableList.drawElementCallback = OnDrawElement;
            m_ReorderableList.elementHeightCallback = GetElementHeight;
            m_ReorderableList.onReorderCallback = ListUpdated;
        }

        private void NewRuleTile() {
            tile.NotZero = 5;
            tile.DefaultNumOfThese = 0;
            tile.TheseNumberOfTiles = new byte[7];
            //tile.TheseSprites = new TileBase[7][];
            tile.DefaultNumOfNotThese = 0;
            tile.NotTheseNumberOfTiles = new byte[6];
            //tile.NotTheseSprites = new TileBase[6][];
        }

        private void ListUpdated(ReorderableList list) {
            SaveTile();
        }

        private float GetElementHeight(int index) {
            if (tile.m_TilingRules != null && tile.m_TilingRules.Count > 0) {
                switch (tile.m_TilingRules[index].m_Output) {
                    case RuleTile.TilingRule.OutputSprite.Random:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 3) + k_PaddingBetweenRules;
                    case RuleTile.TilingRule.OutputSprite.Animation:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 2) + k_PaddingBetweenRules;
                }
            }
            return k_DefaultElementHeight + k_PaddingBetweenRules;
        }

        private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused) {
            RuleTile.TilingRule rule = tile.m_TilingRules[index];

            float yPos = rect.yMin + 2f;
            float height = rect.height - k_PaddingBetweenRules;
            float matrixWidth = k_DefaultElementHeight;

            Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2f - 20f, height);
            Rect matrixRect = new Rect(rect.xMax - matrixWidth * 2f - 10f, yPos, matrixWidth, k_DefaultElementHeight);
            Rect spriteRect = new Rect(rect.xMax - matrixWidth - 5f, yPos, matrixWidth, k_DefaultElementHeight);

            EditorGUI.BeginChangeCheck();
            RuleInspectorOnGUI(inspectorRect, rule);
            RuleMatrixOnGUI(matrixRect, rule, tile);
            SpriteOnGUI(spriteRect, rule);
            if (EditorGUI.EndChangeCheck())
                SaveTile();
        }

        private void SaveTile() {
            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(tile);
            SceneView.RepaintAll();
        }

        private void OnDrawHeader(Rect rect) {
            GUI.Label(rect, "Tiling Rules");
        }

        public override void OnInspectorGUI() {
            tile.m_DefaultSprite = EditorGUILayout.ObjectField("Default Sprite", tile.m_DefaultSprite, typeof(Sprite), false) as Sprite;
            tile.m_DefaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default Collider", tile.m_DefaultColliderType);

            EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
            DoAdvanceRuleLayout();
            EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();

            if (m_ReorderableList != null && tile.m_TilingRules != null)
                m_ReorderableList.DoLayoutList();
        }



        private void DoAdvanceRuleLayout() {

            tile.These = EditorGUILayout.Toggle("Show Inclusive", tile.These);
            if (tile.These) {
                EditorGUILayout.LabelField("# of inclusive rules");
                tile.DefaultNumOfThese = (byte)EditorGUILayout.IntSlider(tile.DefaultNumOfThese, 0, 7);
                for (byte i = 0; i < tile.DefaultNumOfThese; i++) {
                    Rect guiPOS = EditorGUILayout.BeginHorizontal();
                    GUI.DrawTexture(new Rect(0, guiPOS.y, 15, guiPOS.height), icons[i]);
                    EditorGUILayout.LabelField("Tiles");
                    EditorGUI.BeginChangeCheck();
                    tile.TheseNumberOfTiles[i] = (byte)EditorGUI.DelayedIntField(new Rect(60, guiPOS.y, 30, guiPOS.height), tile.TheseNumberOfTiles[i]);
                    if (EditorGUI.EndChangeCheck() || tile.firstExists) {
                        ResizeArr(i, 0);
                        tile.firstExists = false;
                    }
                    tile.RuleNames[i] = (tile.RuleNames[i] == "" || tile.RuleNames[i] == "Click to set rule name" || tile.RuleNames[i] == null) ? EditorGUILayout.TextField("Click to set rule name", grey) : EditorGUILayout.TextField(tile.RuleNames[i], GUILayout.MaxWidth(Math.Max(100, GUI.skin.textField.CalcSize(new GUIContent(tile.RuleNames[i])).x)));
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    for (byte q = 0; q < tile.TheseNumberOfTiles[i]; q++) {
                        #region This is bad
                        switch (i) {
                            case 0:
                                tile.t1[q] = EditorGUILayout.ObjectField(tile.t1[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 1:
                                tile.t2[q] = EditorGUILayout.ObjectField(tile.t2[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 2:
                                tile.t3[q] = EditorGUILayout.ObjectField(tile.t3[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 3:
                                tile.t4[q] = EditorGUILayout.ObjectField(tile.t4[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 4:
                                tile.t5[q] = EditorGUILayout.ObjectField(tile.t5[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 5:
                                tile.t6[q] = EditorGUILayout.ObjectField(tile.t6[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 6:
                                tile.t7[q] = EditorGUILayout.ObjectField(tile.t7[q], typeof(TileBase), false) as TileBase;
                                break;
                        }
                        #endregion
                    }
                }
            }
            EditorGUILayout.Space(); EditorGUILayout.Space();
            tile.NotThese = EditorGUILayout.Toggle("Show Exclusive", tile.NotThese);
            if (tile.NotThese) {
                EditorGUILayout.LabelField("# of exclusive rules");
                tile.DefaultNumOfNotThese = (byte)EditorGUILayout.IntSlider(tile.DefaultNumOfNotThese, 0, 6);
                for (byte i = 0; i < tile.DefaultNumOfNotThese; i++) {
                    Rect guiPOS = EditorGUILayout.BeginHorizontal();
                    GUI.DrawTexture(new Rect(0, guiPOS.y, 15, guiPOS.height), icons[i + 7]);
                    EditorGUILayout.LabelField("Tiles");
                    EditorGUI.BeginChangeCheck();
                    tile.NotTheseNumberOfTiles[i] = (byte)EditorGUI.DelayedIntField(new Rect(60, guiPOS.y, 30, guiPOS.height), tile.NotTheseNumberOfTiles[i]);
                    if (EditorGUI.EndChangeCheck() || tile.firstExists2) {
                        ResizeArr(i, 1);
                        tile.firstExists2 = false;
                    }
                    tile.RuleNames[i + 7] = (tile.RuleNames[i + 7] == "" || tile.RuleNames[i + 7] == "Click to set rule name" || tile.RuleNames[i] == null) ? EditorGUILayout.TextField("Click to set rule name", grey) : EditorGUILayout.TextField(tile.RuleNames[i + 7], GUILayout.MaxWidth(Math.Max(100, GUI.skin.textField.CalcSize(new GUIContent(tile.RuleNames[i + 7])).x)));
                    EditorGUILayout.EndHorizontal();
                    for (byte q = 0; q < tile.NotTheseNumberOfTiles[i]; q++) {
                        #region This is bad
                        switch (i) {
                            case 0:
                                tile.n1[q] = EditorGUILayout.ObjectField(tile.n1[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 1:
                                tile.n2[q] = EditorGUILayout.ObjectField(tile.n2[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 2:
                                tile.n3[q] = EditorGUILayout.ObjectField(tile.n3[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 3:
                                tile.n4[q] = EditorGUILayout.ObjectField(tile.n4[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 4:
                                tile.n5[q] = EditorGUILayout.ObjectField(tile.n5[q], typeof(TileBase), false) as TileBase;
                                break;
                            case 5:
                                tile.n6[q] = EditorGUILayout.ObjectField(tile.n6[q], typeof(TileBase), false) as TileBase;
                                break;
                        }
                        #endregion
                    }
                }
            }
        }

        public enum Num { t1, t2, t3, t4, t5, t6, t7 }
        public enum Nom { n1, n2, n3, n4, n5, n6 }

        private void ResizeArr(byte i, byte v) {
            #region This is bad
            switch (v) {
                case 0:
                    switch (i) {
                        case 0:
                            Array.Resize(ref tile.t1, tile.TheseNumberOfTiles[i]);
                            break;
                        case 1:
                            Array.Resize(ref tile.t2, tile.TheseNumberOfTiles[i]);
                            break;
                        case 2:
                            Array.Resize(ref tile.t3, tile.TheseNumberOfTiles[i]);
                            break;
                        case 3:
                            Array.Resize(ref tile.t4, tile.TheseNumberOfTiles[i]);
                            break;
                        case 4:
                            Array.Resize(ref tile.t5, tile.TheseNumberOfTiles[i]);
                            break;
                        case 5:
                            Array.Resize(ref tile.t6, tile.TheseNumberOfTiles[i]);
                            break;
                        case 6:
                            Array.Resize(ref tile.t7, tile.TheseNumberOfTiles[i]);
                            break;
                    }
                    break;
                case 1:
                    switch (i) {
                        case 0:
                            Array.Resize(ref tile.n1, tile.NotTheseNumberOfTiles[i]);
                            break;
                        case 1:
                            Array.Resize(ref tile.n2, tile.NotTheseNumberOfTiles[i]);
                            break;
                        case 2:
                            Array.Resize(ref tile.n3, tile.NotTheseNumberOfTiles[i]);
                            break;
                        case 3:
                            Array.Resize(ref tile.n4, tile.NotTheseNumberOfTiles[i]);
                            break;
                        case 4:
                            Array.Resize(ref tile.n5, tile.NotTheseNumberOfTiles[i]);
                            break;
                        case 5:
                            Array.Resize(ref tile.n6, tile.NotTheseNumberOfTiles[i]);
                            break;
                    }
                    break;
            }
            #endregion
        }


        internal static void RuleMatrixOnGUI(Rect rect, RuleTile.TilingRule tilingRule, RuleTile tile) {
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
            int index = 0;
            float w = rect.width / 3f;
            float h = rect.height / 3f;

            for (int y = 0; y <= 3; y++) {
                float top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }
            for (int x = 0; x <= 3; x++) {
                float left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }
            Handles.color = Color.white;

            for (int y = 0; y <= 2; y++) {
                for (int x = 0; x <= 2; x++) {
                    Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
                    if (x != 1 || y != 1) {
                        try {
                            switch (tilingRule.m_Neighbors[index]) {
                                case RuleTile.TilingRule.Neighbor.This:
                                    GUI.DrawTexture(r, arrows[y * 3 + x]);
                                    break;
                                case RuleTile.TilingRule.Neighbor.NotThis:
                                    GUI.DrawTexture(r, arrows[9]);
                                    break;
                                case RuleTile.TilingRule.Neighbor.DontCare:
                                    break;
                                default:
                                    GUI.DrawTexture(r, icons[(int)tilingRule.m_Neighbors[index] - 3]);
                                    //Debug.Log("index: " + index);
                                    //Debug.Log((int)tilingRule.m_Neighbors[index] - 3);
                                    //GUI.DrawTexture(r, icons[(int)tilingRule.m_Neighbors[index] - 3]);//- 3]);
                                    break;
                                    index++;
                            }
                        } catch (System.IndexOutOfRangeException e) { }
                        if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) {
                            int change = 1;
                            if (Event.current.button == 0) {
                                change = -1;
                                tilingRule.m_Neighbors[index] = (RuleTile.TilingRule.Neighbor)/*(((int)tilingRule.m_Neighbors[index] + change) % 3); //*/ FindNextIcon((int)tilingRule.m_Neighbors[index], tile.DefaultNumOfThese, tile.DefaultNumOfNotThese);
                                GUI.changed = true;
                                Event.current.Use();
                            }
                        }

                        index++;
                    } else {
                        switch (tilingRule.m_RuleTransform) {
                            case RuleTile.TilingRule.Transform.Rotated:
                                GUI.DrawTexture(r, autoTransforms[0]);
                                break;
                            case RuleTile.TilingRule.Transform.MirrorX:
                                GUI.DrawTexture(r, autoTransforms[1]);
                                break;
                            case RuleTile.TilingRule.Transform.MirrorY:
                                GUI.DrawTexture(r, autoTransforms[2]);
                                break;
                        }

                        if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) {
                            tilingRule.m_RuleTransform = (RuleTile.TilingRule.Transform)(((int)tilingRule.m_RuleTransform + 1) % 4);
                            GUI.changed = true;
                            Event.current.Use();
                        }
                    }
                }
            }
        }

        private static void OnSelect(object userdata) {
            MenuItemData data = (MenuItemData)userdata;
            data.m_Rule.m_RuleTransform = data.m_NewValue;
        }

        private class MenuItemData {
            public RuleTile.TilingRule m_Rule;
            public RuleTile.TilingRule.Transform m_NewValue;

            public MenuItemData(RuleTile.TilingRule mRule, RuleTile.TilingRule.Transform mNewValue) {
                this.m_Rule = mRule;
                this.m_NewValue = mNewValue;
            }
        }

        ///*
        internal static void SpriteOnGUI(Rect rect, RuleTile.TilingRule tilingRule) {
            tilingRule.m_Sprites[0] = EditorGUI.ObjectField(new Rect(rect.xMax - rect.height, rect.yMin, rect.height, rect.height), tilingRule.m_Sprites[0], typeof(Sprite), false) as Sprite;
        }

        internal static void RuleInspectorOnGUI(Rect rect, RuleTile.TilingRule tilingRule) {
            float y = rect.yMin;
            EditorGUI.BeginChangeCheck();
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Rule");
            tilingRule.m_RuleTransform = (RuleTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RuleTransform);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Collider");
            tilingRule.m_ColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_ColliderType);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Output");
            tilingRule.m_Output = (RuleTile.TilingRule.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Output);
            y += k_SingleLineHeight;

            if (tilingRule.m_Output == RuleTile.TilingRule.OutputSprite.Animation) {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Speed");
                tilingRule.m_AnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_AnimationSpeed);
                y += k_SingleLineHeight;
            }
            if (tilingRule.m_Output == RuleTile.TilingRule.OutputSprite.Random) {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Noise");
                tilingRule.m_PerlinScale = EditorGUI.Slider(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_PerlinScale, 0.001f, 0.999f);
                y += k_SingleLineHeight;

                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Shuffle");
                tilingRule.m_RandomTransform = (RuleTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RandomTransform);
                y += k_SingleLineHeight;
            }

            if (tilingRule.m_Output == RuleTile.TilingRule.OutputSprite.RandomTile) {
                tilingRule.m_OutputRandomTile = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_OutputRandomTile, typeof(RandomTile), false) as RandomTile;
                if (tilingRule.m_OutputRandomTile != null && tilingRule.m_OutputRandomTile.distribute != null && tilingRule.m_OutputRandomTile.distribute.Count > 0) {
                    tilingRule.m_Sprites[0] = tilingRule.m_OutputRandomTile.distribute[0].sprite;
                }
            }

            if (tilingRule.m_Output == RuleTile.TilingRule.OutputSprite.RandomTile2) {
                tilingRule.m_OutputRandomTile2 = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_OutputRandomTile2, typeof(RandomTile2), false) as RandomTile2;
                if (tilingRule.m_OutputRandomTile2 != null && tilingRule.m_OutputRandomTile2.distribute != null && tilingRule.m_OutputRandomTile2.distribute.Count > 0) {
                    tilingRule.m_Sprites[0] = null;
                }
            }

            if (tilingRule.m_Output != RuleTile.TilingRule.OutputSprite.Single && tilingRule.m_Output != RuleTile.TilingRule.OutputSprite.RandomTile && tilingRule.m_Output != RuleTile.TilingRule.OutputSprite.RandomTile2) {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Size");
                EditorGUI.BeginChangeCheck();
                int newLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites.Length);
                if (EditorGUI.EndChangeCheck())
                    Array.Resize(ref tilingRule.m_Sprites, Math.Max(newLength, 1));
                y += k_SingleLineHeight;

                for (int i = 0; i < tilingRule.m_Sprites.Length; i++) {
                    tilingRule.m_Sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites[i], typeof(Sprite), false) as Sprite;
                    y += k_SingleLineHeight;
                }
            }


        }
        //*/
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            if (tile.m_DefaultSprite != null) {
                Type t = GetType("UnityEditor.SpriteUtility");
                if (t != null) {
                    MethodInfo method = t.GetMethod("RenderStaticPreview", new Type[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
                    if (method != null) {
                        object ret = method.Invoke("RenderStaticPreview", new object[] { tile.m_DefaultSprite, Color.white, width, height });
                        if (ret is Texture2D)
                            return ret as Texture2D;
                    }
                }
            }
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private static Type GetType(string TypeName) {
            var type = Type.GetType(TypeName);
            if (type != null)
                return type;

            if (TypeName.Contains(".")) {
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies) {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null) {
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        private static Texture2D Base64ToTexture(string base64) {
            Texture2D t = new Texture2D(1, 1);
            t.hideFlags = HideFlags.HideAndDontSave;
            t.LoadImage(System.Convert.FromBase64String(base64));
            return t;
        }

        private static int FindNextIcon(int current, byte NumThese, byte NumNotThese) { //0-15  say there's 5 & 4   it's i13    14 < 14
            current++;
            if (current > 15)
                current = 0;
            if (current < 3 + NumThese)
                return current;
            if (current > 9 && current < 10 + NumNotThese)
                return current;
            return FindNextIcon(current, NumThese, NumNotThese);
        }

    }
}
