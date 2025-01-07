namespace MycroftToolkit.GamePlayQuickFramework.ConditionSystem {
    public enum ConditionType {
        Probability,
    }
    
    public abstract class Condition {
        public abstract ConditionType ConditionType { get; }
    }
}