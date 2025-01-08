namespace MycroftToolkit.GamePlayQuickFramework.DamageSystem {
    public interface IDamageable {
        public bool IsDamageable { get; }
        public void TakeDamage(DamageData damageData);
    }
    
    public interface IDamageTrigger {
        public void TriggerDamage(IDamageable target);
    }
}