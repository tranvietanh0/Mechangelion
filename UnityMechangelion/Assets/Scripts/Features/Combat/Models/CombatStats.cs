#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Models
{
    using UnityEngine;

    public class CombatStats
    {
        public float MaxHealth { get; set; } = 1f;

        public float CurrentHealth { get; set; } = 1f;

        public float BaseDamage { get; set; } = 0.1f;

        public float Defense { get; set; }

        public float ShieldDefense { get; set; }

        public float CritChance { get; set; } = 0.05f;

        public float CritMultiplier { get; set; } = 1.5f;

        public float DamageMultiplier { get; set; } = 1f;

        public bool IsDead => this.CurrentHealth <= 0f;

        public float HealthPercent => this.MaxHealth <= 0f ? 0f : this.CurrentHealth / this.MaxHealth;

        public void TakeDamage(float amount)
        {
            this.CurrentHealth = Mathf.Max(0f, this.CurrentHealth - amount);
        }

        public void Heal(float amount)
        {
            this.CurrentHealth = Mathf.Min(this.MaxHealth, this.CurrentHealth + amount);
        }

        public void Reset()
        {
            this.CurrentHealth = this.MaxHealth;
        }
    }
}
