using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AttributeSystem {
    public enum ModifierTypes { 
        Add, // 加法
        Multiply, // 乘法
        Fixed, // 固定为某个值
        Locked, // 锁定属性不可改变
        Reset // 重置为BaseValue并移除其他modifier
    }

    public class AttributeModifier {
        public readonly ModifierTypes Type;
        public readonly float Value;
        public readonly string Source;

        public AttributeModifier(ModifierTypes type, string source, float value = 0) {
            Type = type;
            Value = value;
            Source = source;
            if (string.IsNullOrEmpty(source)) {
                Debug.LogWarning("[AttributeModifier] source is empty!");
            }
        }
        public override string ToString() => $"AttributeModifier> (Type:{Type}, Value:{Value}, Source:{Source})";
    }
    
    public enum ModifierChangeTypes { Add, Remove, Modify }
    public class AttributeChangedInfo {
        public float OldValue;
        public float NewValue;
        public Attribute Attribute;
        public Dictionary<AttributeModifier, ModifierChangeTypes> ChangedModifiers;
        
        public override string ToString() {
            var modifiersStr = new StringBuilder();
            foreach (var modifier in ChangedModifiers) {
                modifiersStr.Append($"{modifier.Key}[{modifier.Value}]\n");
            }
            return $"AttributeChangedData>{Attribute.Owner}>{Attribute.Name}Changed> OldValue:{OldValue}, NewValue:{NewValue}\n" +
                   $"Modifiers:[\n{modifiersStr}\n], \nAttribute:{Attribute}";
        }
    }
}