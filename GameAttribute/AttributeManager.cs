using System.Collections.Generic;
using UnityEngine;

namespace GameAttribute {
    public partial class AttributeManager : MonoBehaviour {
        public string Owner;
        internal Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();

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
            Attributes.Add(attributeName, a);
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

        public void AddModifierToAttribute(string attributeName, ModifierTypes type, string source, float value = 0) {
            var attribute = GetAttribute(attributeName);
            if (attribute == null) {
                return;
            }
            AttributeModifier modifier = new AttributeModifier(type, source, value);
            attribute.AddModifier(modifier);
        }

        public void RemoveModifierBySource(string source) {
            if (Attributes == null || Attributes.Count == 0) {
                return;
            }
            foreach (var a in Attributes.Values) {
                a.RemoveModifiersBySource(source);
            }
        }
    }
}