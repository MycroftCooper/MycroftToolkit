using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameAttribute {
    public class Attribute {
        public readonly string Owner;
        public readonly string Name;
        public readonly float BaseValue;
        
        public float FinalValue { get; private set; }
        public int RoundedFinalValue { get; private set; }
        public bool HasClamp => MinValue != null || MaxValue != null;
        public Attribute MaxValue;
        public Attribute MinValue;
        
        public bool IsLocked => Modifiers.Any(m => m.Type == ModifierTypes.Locked);
        internal List<AttributeModifier> Modifiers = new List<AttributeModifier>();
        public Func<AttributeModifier, bool, bool> CanModifierChange;
        public Action<AttributeChangedData> OnValueChanged;
        
        public Attribute(string name, string owner = null, float baseValue = 0, Attribute min = null, Attribute max = null) {
            Owner = owner;
            Name = name;
            BaseValue = baseValue;
            MaxValue = max;
            MinValue = min;
            if (string.IsNullOrEmpty(owner)) {
                Debug.LogWarning("[Attribute] owner is empty!");
            }
            if (string.IsNullOrEmpty(name)) {
                Debug.LogWarning("[Attribute] name is empty!");
            }

            CalculateFinalValue();
        }

        public void AddModifier(AttributeModifier modifier) {
            if (IsLocked) {
                return;
            }
            bool canAdd = CanModifierChange == null || CanModifierChange.Invoke(modifier, true);
            if (!canAdd) {
                return;
            }
            Modifiers.Add(modifier);
            
            float oldValue = FinalValue;
            CalculateFinalValue();
            AttributeChangedData data = new AttributeChangedData {
                OldValue = oldValue,
                NewValue = FinalValue,
                Modifiers = new List<AttributeModifier> { modifier },
                Attribute = this,
                IsAddModifier = true
            };
            OnValueChanged?.Invoke(data);
        }

        public void RemoveModifier(AttributeModifier modifier) {
            if (Modifiers.Any(m => m.Type == ModifierTypes.Locked && m != modifier)) {
                return;
            }
            bool canRemove = CanModifierChange == null || CanModifierChange.Invoke(modifier, false);
            if (!canRemove) {
                return;
            }
            Modifiers.Remove(modifier);
            
            float oldValue = FinalValue;
            CalculateFinalValue();
            AttributeChangedData data = new AttributeChangedData {
                OldValue = oldValue,
                NewValue = FinalValue,
                Modifiers = new List<AttributeModifier> { modifier },
                Attribute = this,
                IsAddModifier = false
            };
            OnValueChanged?.Invoke(data);
        }

        public void RemoveModifiersBySource(string source) {
            float oldValue = FinalValue;
            if (Modifiers.Any(m => m.Type == ModifierTypes.Locked && m.Source != source)) {
                return;
            }
            var targets = Modifiers.FindAll(m => m.Source == source);
            Modifiers.RemoveAll(m => m.Source == source);
            CalculateFinalValue();
            AttributeChangedData data = new AttributeChangedData {
                OldValue = oldValue,
                NewValue = FinalValue,
                Modifiers = targets,
                Attribute = this,
                IsAddModifier = false
            };
            OnValueChanged?.Invoke(data);
        }

        internal void CalculateFinalValue() {
            // 检查Reset类型的Modifier
            if (Modifiers.Any(m => m.Type == ModifierTypes.Reset)) {
                FinalValue = BaseValue;
                Modifiers.Clear();
                return;
            }

            // 检查Fixed类型的Modifier
            var fixedModifier = Modifiers.FindLast(m => m.Type == ModifierTypes.Fixed);
            if (fixedModifier != null) {
                FinalValue = fixedModifier.Value;
                return;
            }
            
            FinalValue = CalculateFinalValue(BaseValue, Modifiers);
            RoundedFinalValue = Mathf.RoundToInt(FinalValue);// 四舍五入
        }

        private float CalculateFinalValue(float baseVal, List<AttributeModifier> modifiers) {
            float sumAdd = 0f;
            float productMul = 1f;

            foreach (var mod in modifiers) {
                switch (mod.Type) {
                    case ModifierTypes.Add:
                        sumAdd += Convert.ToSingle(mod.Value);
                        break;
                    case ModifierTypes.Multiply:
                        productMul *= Convert.ToSingle(mod.Value);
                        break;
                }
            }
            float final = (baseVal + sumAdd) * productMul;

            if (!HasClamp) return final;
            float minVal = MinValue != null ? Convert.ToSingle(MinValue.FinalValue) : float.MinValue;
            float maxVal = MaxValue != null ? Convert.ToSingle(MaxValue.FinalValue) : float.MaxValue;
            final = Mathf.Clamp(final, minVal, maxVal);
            return final;
        }

        public override string ToString() {
            var modifiersStr = new StringBuilder();
            foreach (var modifier in Modifiers) {
                modifiersStr.Append(modifier + "\n");
            }
            return $"Attribute> Owner:{Owner} Name:{Name} BaseValue:{BaseValue} FinalValue:{FinalValue}\n{modifiersStr}";
        }
    }
    
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
}