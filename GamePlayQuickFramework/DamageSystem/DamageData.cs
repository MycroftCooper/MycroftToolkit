using System.Text;
using UnityEngine;

namespace MycroftToolkit.GamePlayQuickFramework.DamageSystem {
    public enum DamageType { Physical, Fire }
    public class DamageData {
        public GameObject Source; // 伤害来源
        public IDamageTrigger Trigger;
        public IDamageable Target;
        
        public float BaseDamage;  // 基础伤害值
        public float FinalDamage; // 计算后的最终伤害值
        
        public DamageType Type;   // 伤害类型
        public bool IsCritical;   // 是否暴击
        public Vector3 HitPoint;  // 命中点

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("DamageData:");
            sb.AppendLine($"  Source: {Source?.name ?? "null"}");
            sb.AppendLine($"  Trigger: {Trigger?.GetType().Name ?? "null"}");
            sb.AppendLine($"  Target: {Target?.GetType().Name ?? "null"}");
            sb.AppendLine($"  BaseDamage: {BaseDamage}");
            sb.AppendLine($"  FinalDamage: {FinalDamage}");
            sb.AppendLine($"  DamageType: {Type}");
            sb.AppendLine($"  IsCritical: {IsCritical}");
            sb.AppendLine($"  HitPoint: {HitPoint}");
            return sb.ToString();
        }
    }
}