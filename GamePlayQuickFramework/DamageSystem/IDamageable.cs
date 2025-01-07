namespace MycroftToolkit.GamePlayQuickFramework.DamageSystem {
    public interface IDamageable {
        public bool IsDamageable { get; }
        public void TakeDamage(DamageData damageData);
    }
}