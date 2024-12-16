using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AttributeSystem {
    public class AttributeManager : MonoBehaviour {
        public virtual string Owner { get; internal set; }
        internal Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();
        
        protected virtual void Awake() {
            RegisterAttributes(); // 自动注册
        }
        
        protected virtual void RegisterAttributes() {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields) {
                if (field.FieldType != typeof(Attribute)) continue;
                if (field.GetValue(this) is not Attribute attribute) continue;
                attribute.Owner = Owner;
                AddAttribute(attribute);
            }
            
            foreach (var attr in Attributes.Values) {
                // 检查 Min 和 Max 是否存在，并建立关系
                if (Attributes.TryGetValue($"Min{attr.Name}", out var minAttr)) {
                    attr.minValue = minAttr;
                }
                if (Attributes.TryGetValue($"Max{attr.Name}", out var maxAttr)) {
                    attr.maxValue = maxAttr;
                }
                // 重新计算值，确保 Clamp 正确
                attr.CalculateFinalValue();
            }
        }

        internal void AddAttribute(Attribute attribute) {
            if (!Attributes.TryAdd(attribute.Name, attribute)) {
                Debug.LogError($"Duplicate attribute {attribute.Name}");
            }
        }

        public void AddAttribute(string attributeName, float baseValue) {
            if (Attributes.ContainsKey(attributeName)) {
                Debug.LogError($"Duplicate attribute {attributeName}");
                return;
            }
            Attribute a = new Attribute(attributeName, Owner, baseValue);
            AddAttribute(a);
        }
        
        public void AddAttribute(string attributeName, float baseValue, float minValue, float maxValue) {
            if (Attributes.ContainsKey(attributeName)) {
                Debug.LogError($"Duplicate attribute {attributeName}");
                return;
            }
            Attribute aMin = new Attribute($"Min{attributeName}", Owner, minValue);
            Attribute aMax = new Attribute($"Max{attributeName}", Owner, maxValue);
            Attribute a = new Attribute(attributeName, Owner, baseValue, aMin, aMax);
            Attributes.Add(attributeName, a);
            Attributes[aMin.Name] = aMin;
            Attributes[aMax.Name] = aMax;
        }

        public Attribute GetAttribute(string attributeName) {
            if (Attributes != null && Attributes.Count != 0 && Attributes.TryGetValue(attributeName, out var attribute)) {
                return attribute;
            }
            Debug.LogError($"AttributeManager> {Owner} doesn't has attribute {attributeName}!");
            return null;
        }

        public float GetAttributeFinalValue(string attributeName) {
            var attribute = GetAttribute(attributeName);
            return attribute?.FinalValue ?? 0;
        }

        public int GetAttributeRoundedFinalValue(string attributeName) {
            var attribute = GetAttribute(attributeName);
            return attribute.RoundedFinalValue;
        }

        public void AddModifierToAttribute(string attributeName, ModifierTypes type, string source, float value = 0) {
            var attribute = GetAttribute(attributeName);
            if (attribute == null) {
                return;
            }
            AttributeModifier modifier = new AttributeModifier(type, source, value);
            attribute.AddModifier(modifier);
        }
        
        public void RemoveModifierBySource(string source, string attributeName) {
            var attribute = GetAttribute(attributeName);
            attribute?.RemoveModifiersBySource(source);
        }
        
        public void RemoveModifierBySource(string source) {
            if (Attributes == null || Attributes.Count == 0) {
                return;
            }
            foreach (var a in Attributes.Values) {
                a.RemoveModifiersBySource(source);
            }
        }
        
        public Action<AttributeChangedData> OnAttributeChanged;
    }
}