#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Services
{
    using HyperCasualGame.Scripts.Core.Constants;
    using HyperCasualGame.Scripts.Features.Combat.Models;
    using UnityEngine;

    public sealed class DamageCalculatorService
    {
        public DamageResult Calculate(DamageRequest request, CombatStats targetStats)
        {
            var damage = request.BaseDamage * request.DamageMultiplier;
            damage *= Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(request.PrepareProgress));

            var result = new DamageResult
            {
                RawDamage = damage,
            };

            if (Random.value < request.CritChance)
            {
                damage *= request.CritMultiplier;
                result.IsCritical = true;
            }

            var defenseReduction = targetStats.Defense * 0.7f;
            damage -= defenseReduction;
            result.DefenseReduction = defenseReduction;

            var minDamage = result.RawDamage * CombatConstants.DamageDefMinPercent;
            result.FinalDamage = Mathf.Max(damage, minDamage);
            return result;
        }

        public float CalculateEnemyDamageForLevel(float baseDamage, int level)
        {
            var levelsToAccount = (level - 1) - (((level - 1) / CombatConstants.LevelStepDownInterval) * 2);
            var multiplier = CombatConstants.EnemyDamageMultiplier + (levelsToAccount * CombatConstants.LevelMultiplierStep);
            return baseDamage * multiplier;
        }

        public float CalculateEnemyHealthForLevel(float baseHealth, int level, int playerLevel)
        {
            var levelsToAccount = (level - 1) - (((level - 1) / CombatConstants.LevelStepDownInterval) * 2);
            var multiplier = this.GetHealthMultiplierForPlayerLevel(playerLevel) + (levelsToAccount * CombatConstants.EnemyHealthLevelStep);
            return baseHealth * multiplier;
        }

        private float GetHealthMultiplierForPlayerLevel(int playerLevel)
        {
            if (playerLevel >= 8)
            {
                return CombatConstants.EnemyHealthMultiplierHigh;
            }

            if (playerLevel >= 6)
            {
                return CombatConstants.EnemyHealthMultiplierMid;
            }

            return CombatConstants.EnemyHealthMultiplierLow;
        }
    }
}
