using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AttributeSystem {
    [Serializable]
    public class AttributesSaveData {
        public string owner;
        public List<AttributeSaveData> attributes = new List<AttributeSaveData>();
    }
    
    [Serializable]
    public class AttributeSaveData {
        public string owner;
        public string name;
        public string baseValue;
        public List<ModifierSaveData> modifiers = new List<ModifierSaveData>();
        public string minValueAttributeName;
        public string maxValueAttributeName;
    }

    [Serializable]
    public class ModifierSaveData {
        public ModifierTypes type;
        public string source;
        public string value;
    }

    public static class AttributeExtensions {
        public static AttributeSaveData ToSaveData(this Attribute attribute) {
            var saveData = new AttributeSaveData {
                owner = attribute.Owner,
                name = attribute.Name,
                baseValue = ConvertToString(attribute.BaseValue),
                modifiers = attribute.Modifiers.Select(m => m.ToSaveData()).ToList(),
                minValueAttributeName = attribute.minValue?.Name,
                maxValueAttributeName = attribute.maxValue?.Name
            };
            return saveData;
        }
        
        public static Attribute FromSaveData(this AttributeSaveData saveData) {
            float baseVal = ParseValue(saveData.baseValue);
            var attr = new Attribute(saveData.name, saveData.owner, baseVal);
            // MinValue 和 MaxValue 暂时留空，需要在外部根据名称找到对应的Attribute实例再注入
            
            foreach (var modData in saveData.modifiers) {
                var modifier = modData.FromSaveData();
                attr.Modifiers.Add(modifier);
            }
            attr.CalculateFinalValue();
            return attr;
        }
        
        public static ModifierSaveData ToSaveData(this AttributeModifier modifier) {
            return new ModifierSaveData {
                type = modifier.Type,
                source = modifier.Source,
                value = ConvertToString(modifier.Value)
            };
        }
        
        public static AttributeModifier FromSaveData(this ModifierSaveData saveData) {
            float val = ParseValue(saveData.value);
            return new AttributeModifier(saveData.type, saveData.source, val);
        }

        public static void LoadSaveData(this AttributeManager mgr, AttributesSaveData saveData) {
            mgr.Owner = saveData.owner;
            mgr.Attributes.Clear();

            // 第一阶段：创建所有属性，但不处理Min/Max和Modifiers，只构造基础属性
            foreach (var asd in saveData.attributes) {
                var attr = asd.FromSaveData();
                mgr.AddAttribute(attr);
            }

            // 第二阶段：为每个属性寻找对应的MinValue和MaxValue属性
            foreach (var asd in saveData.attributes) {
                var attr = mgr.GetAttribute(asd.name);
                if (attr == null) continue;

                bool needCalculate = false;
                if (!string.IsNullOrEmpty(asd.minValueAttributeName)) {
                    var minAttr = mgr.GetAttribute(asd.minValueAttributeName);
                    attr.minValue = minAttr;
                    needCalculate = true;
                }
                if (!string.IsNullOrEmpty(asd.maxValueAttributeName)) {
                    var maxAttr = mgr.GetAttribute(asd.maxValueAttributeName);
                    attr.maxValue = maxAttr;
                    needCalculate = true;
                }

                if (needCalculate) {
                    attr.CalculateFinalValue();
                }
            }
        }

        public static AttributesSaveData ToSaveData(this AttributeManager mgr) {
            var saveData = new AttributesSaveData {
                owner = mgr.Owner,
                attributes = mgr.Attributes.Values.Select(a => a.ToSaveData()).ToList()
            };
            return saveData;
        }
        
        private static string ConvertToString(float value) {
            return Convert.ToSingle(value).ToString(CultureInfo.InvariantCulture);
        }

        private static float ParseValue(string str) {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}