#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameAttribute.Editor {
    public class AttributeManagerDebugWindow : OdinEditorWindow {
        private const string WindowTitle = "Attribute Manager Debug";

        [MenuItem("QuickGameFramework/GamePlay/AttributeManagerDebug")]
        public static void OpenWindow() {
            var window = GetWindow<AttributeManagerDebugWindow>(true, WindowTitle, true);
            window.Show();
        }

        [ShowInInspector, HorizontalGroup("row0"), PropertyOrder(0)]
        public AttributeManager SelectedManager { get; private set; }
        [ShowInInspector, HorizontalGroup("row0"), PropertyOrder(1)]
        private bool _isLocked;

        [ShowInInspector, PropertyOrder(2)]
        private AttributeManagerDebug _mgrDebug;

        [Obsolete("Rename this to OnImGUI()", true)]
        protected override void OnGUI() {
            if (_isLocked) {
                return;
            }

            GameObject selectedGameObject = Selection.activeGameObject;
            AttributeManager newMgr = null;
            if (selectedGameObject != null) {
                newMgr = selectedGameObject.GetComponent<AttributeManager>();
            }
            
            if (selectedGameObject == null || newMgr == null) {
                _mgrDebug = null;
                SelectedManager = null;
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("Please select a GameObject with AttributeManager.", MessageType.Info);
                return;
            }
            if (newMgr == SelectedManager) {
                return;
            }
            SelectedManager = newMgr;
            _mgrDebug = new AttributeManagerDebug(SelectedManager);
        }
    }

    public class AttributeManagerDebug {
        public AttributeManager SelectedManager;
        [Title("Attribute Manager Debug", bold: true), PropertyOrder(0)]
        [ShowInInspector, ReadOnly]
        private string DebugOwner => SelectedManager?.Owner;
        private Dictionary<string, Attribute> Attributes => SelectedManager?.Attributes;

        [ShowInInspector, TableList(IsReadOnly = true), PropertyOrder(1)]
        private List<AttributeDebugInfo> _debugAttributes = new List<AttributeDebugInfo>();

        public AttributeManagerDebug(AttributeManager selectedManager) {
            SelectedManager = selectedManager;
            RefreshDebugAttributes();
        }

        private void RefreshDebugAttributes() {
            if (Attributes == null || Attributes.Count == 0) {
                _debugAttributes.Clear();
            }
            else {
                _debugAttributes = Attributes.Values.Select(a => new AttributeDebugInfo(a, this)).ToList();
            }
        }
        
        [Button("Add Attribute"), PropertyOrder(2)]
        private void DebugAddAttribute(string attributeName, float baseValue, float minValue = 1, float maxValue = -1) {
            if (string.IsNullOrEmpty(attributeName)) {
                Debug.LogError("[AttributeManager Debug] Attribute name is empty!");
                return;
            }

            bool debugHasMinMax = maxValue >= minValue;
            if (debugHasMinMax) {
                SelectedManager.AddAttribute(attributeName, baseValue, minValue, maxValue);
            }
            else {
                SelectedManager.AddAttribute(attributeName, baseValue);
            }

            RefreshDebugAttributes();
        }

        internal void DebugRemoveAttribute(string attributeName) {
            if (Attributes == null || Attributes.Count == 0) {
                Debug.LogWarning("[AttributeManager] No attributes to remove.");
                return;
            }

            // 检测该属性是主属性，还是min/max属性
            bool isMinAttr = attributeName.StartsWith("Min");
            bool isMaxAttr = attributeName.StartsWith("Max");

            // 如果是Min或Max属性，对应的主属性名是去掉前缀后的名字
            string mainAttrName = attributeName;
            if (isMinAttr) {
                mainAttrName = attributeName.Substring(3); // 移除"Min"
            }else if (isMaxAttr) {
                mainAttrName = attributeName.Substring(3); // 移除"Max"
            }

            // 如果是主属性，则其Min和Max属性的名字为：
            string minAttrName = "Min" + attributeName;
            string maxAttrName = "Max" + attributeName;

            if (!isMinAttr && !isMaxAttr) {
                // 主属性
                // 先尝试删除Min和Max属性（如果存在）
                if (Attributes.ContainsKey(minAttrName)) {
                    Attributes.Remove(minAttrName);
                    Debug.Log(
                        $"[AttributeManager] Removed attribute {minAttrName} since its main attribute {attributeName} was removed.");
                }
                if (Attributes.ContainsKey(maxAttrName)) {
                    Attributes.Remove(maxAttrName);
                    Debug.Log(
                        $"[AttributeManager] Removed attribute {maxAttrName} since its main attribute {attributeName} was removed.");
                }

                // 最后删除主属性本身
                if (!Attributes.Remove(attributeName)) {
                    Debug.LogError($"[AttributeManager] Unable to remove {attributeName}, not found.");
                }else {
                    Debug.Log($"[AttributeManager] Removed attribute {attributeName}");
                }
            }else {
                // 是Min或Max属性
                // 需要找到对应的主属性，如果主属性存在，则清除其MinValue或MaxValue引用
                if (Attributes.TryGetValue(mainAttrName, out Attribute mainAttr) && mainAttr != null) {
                    if (isMinAttr && mainAttr.MinValue != null && mainAttr.MinValue.Name == attributeName) {
                        // 清除主属性的MinValue
                        mainAttr.MinValue = null;
                        Debug.Log(
                            $"[AttributeManager] Cleared MinValue of {mainAttrName} since {attributeName} was removed.");
                    }

                    if (isMaxAttr && mainAttr.MaxValue != null && mainAttr.MaxValue.Name == attributeName) {
                        // 清除主属性的MaxValue
                        mainAttr.MaxValue = null;
                        Debug.Log(
                            $"[AttributeManager] Cleared MaxValue of {mainAttrName} since {attributeName} was removed.");
                    }

                    // Recalculate mainAttr final value in case clamping changed
                    mainAttr.CalculateFinalValue();
                }

                // 删除该Min/Max属性本身
                if (Attributes.Remove(attributeName)) {
                    Debug.Log($"[AttributeManager] Removed attribute {attributeName}");
                }else {
                    Debug.LogError($"[AttributeManager] Unable to remove {attributeName}, not found.");
                }
            }

            RefreshDebugAttributes();
        }

        [Button("Remove Modifier By Source"), PropertyOrder(4)]
        private void DebugRemoveModifierBySource(string source) {
            SelectedManager.RemoveModifierBySource(source);
            RefreshDebugAttributes();
        }

        [BoxGroup("SaveData"), HorizontalGroup("SaveData/Row1"), PropertyOrder(5)]
        [Button("Export Save Data")]
        public void ExportSaveData() {
            var saveData = SelectedManager.ToSaveData();
            // 将数据转为JSON并打印出来（实际可改为保存到文件）
            string json = JsonUtility.ToJson(saveData, true);
            Debug.Log($"[AttributeManager] Exported Save Data:\n{json}");
        }

        [BoxGroup("SaveData"), HorizontalGroup("SaveData/Row1"), PropertyOrder(5)]
        [Button("Import Save Data (From Clipboard)")]
        public void ImportSaveDataFromClipboard() {
            string json = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(json)) {
                Debug.LogError("[AttributeManager] Clipboard is empty or invalid!");
                return;
            }

            try {
                var saveData = JsonUtility.FromJson<AttributesSaveData>(json);
                if (saveData != null) {
                    SelectedManager.LoadSaveData(saveData);
                    Debug.Log("[AttributeManager] Successfully imported save data from clipboard.");
                }
                else {
                    Debug.LogError("[AttributeManager] Failed to parse save data from clipboard.");
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[AttributeManager] Exception while importing: {ex}");
            }

            RefreshDebugAttributes();
        }
    }

    public class AttributeDebugInfo {
        private readonly Attribute _attribute;
        private readonly AttributeManagerDebug _debugMgr;

        public AttributeDebugInfo(Attribute attribute, AttributeManagerDebug debugMgr) {
            _attribute = attribute;
            _debugMgr = debugMgr;
            RefreshDebugModifiers();
        }

        [TableColumnWidth(80)]
        [ShowInInspector, ReadOnly]
        public string Name => _attribute.Name;

        [ShowInInspector, ReadOnly, TableColumnWidth(80)]
        public float BaseValue => _attribute.BaseValue;

        [ShowInInspector, ReadOnly, TableColumnWidth(80)]
        public float FinalValue => _attribute.FinalValue;

        [ShowInInspector, ReadOnly, LabelText("Min"), TableColumnWidth(80)]
        public float Min => _attribute.MinValue?.FinalValue ?? float.MinValue;

        [ShowInInspector, ReadOnly, LabelText("Max"), TableColumnWidth(80)]
        public float Max => _attribute.MinValue?.FinalValue ?? float.MaxValue;

        [TableList(AlwaysExpanded = true, IsReadOnly = true), PropertyOrder(1)]
        [ShowInInspector, LabelText("Modifiers"), TableColumnWidth(250)]
        public List<ModifierDebugInfo> Modifiers;

        public void RefreshDebugModifiers() {
            Modifiers = _attribute.Modifiers.Select(m => new ModifierDebugInfo(m, _attribute, this)).ToList();
        }

        [HorizontalGroup("Actions"), PropertyOrder(2), TableColumnWidth(250)]
        [Button("AddModifier")]
        public void DebugAddModifier(ModifierTypes type, string source, float value = 0) {
            _debugMgr.SelectedManager.AddModifierToAttribute(Name, type, source, value);
            RefreshDebugModifiers();
        }

        [HorizontalGroup("Actions"), PropertyOrder(2)]
        [Button("Remove Attribute")]
        public void RemoveAttribute() {
            _debugMgr.DebugRemoveAttribute(Name);
        }
    }

    public class ModifierDebugInfo {
        private readonly AttributeModifier _modifier;
        private readonly Attribute _attribute;
        private readonly AttributeDebugInfo _attributeDebugInfo;

        public ModifierDebugInfo(AttributeModifier modifier, Attribute attribute,
            AttributeDebugInfo attributeDebugInfo) {
            _modifier = modifier;
            _attribute = attribute;
            _attributeDebugInfo = attributeDebugInfo;
        }

        [ShowInInspector, ReadOnly] public ModifierTypes Type => _modifier.Type;
        [ShowInInspector, ReadOnly] public string Source => _modifier.Source;
        [ShowInInspector, ReadOnly] public float Value => _modifier.Value;

        [Button("Remove Modifier")]
        public void RemoveModifier() {
            _attribute.RemoveModifier(_modifier);
            _attributeDebugInfo.RefreshDebugModifiers();
        }
    }
}
#endif