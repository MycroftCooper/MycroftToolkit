using System;
using UnityEngine;

namespace BuffSystem {
    public class Buff {
        public string Name;
        public bool HasOwner => string.IsNullOrEmpty(Owner);
        public string Owner;
        public EffectTypes EffectType;
        public OverlayRules OverlayRules;
        public TriggerRules TriggerRules;
        public RemoveRules RemoveRules;
        
        public Action<Buff> OnAdded;
        public Action<Buff> OnRemoved;

        public void OnAddBuffHandler(string target) {
            if (HasOwner) {
                Debug.LogError($"Buff {Name} is already owned by {Owner}!");
                return;
            }
            Owner = target;
            OnAdded?.Invoke(this);
        }
    }

    public enum EffectTypes { Positive, Neutral, Negative }
    public enum OverlayRules { AddTime, Stack, Replace, Reset, NotAllowed }
    public enum TriggerRules { Manual, ByTimer, ByTicker, ByEvent, OnAdd, OnRemove }
    public enum RemoveRules { Manual, ByTimer, ByTicker, ByEvent }
}