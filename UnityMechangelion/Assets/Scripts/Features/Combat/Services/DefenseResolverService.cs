#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Services
{
    using HyperCasualGame.Scripts.Core.Constants;
    using HyperCasualGame.Scripts.Features.Combat.Models;
    using UnityEngine;

    public sealed class DefenseResolverService
    {
        public DamageResult ResolveDefense(
            DamageResult incomingDamage,
            bool isBlocking,
            bool isDodging,
            float blockStartTime,
            float shieldDefense)
        {
            if (isDodging)
            {
                return DamageResult.CreateDodged();
            }

            if (!isBlocking)
            {
                return incomingDamage;
            }

            shieldDefense = Mathf.Clamp01(shieldDefense);

            var blockTime = Time.time - blockStartTime;
            if (blockTime <= CombatConstants.SuperBlockWindow)
            {
                return new DamageResult
                {
                    RawDamage = incomingDamage.RawDamage,
                    FinalDamage = 0f,
                    IsCritical = incomingDamage.IsCritical,
                    WasBlocked = true,
                    WasSuperBlocked = true,
                    BlockedAmount = incomingDamage.FinalDamage,
                    DefenseReduction = incomingDamage.DefenseReduction,
                };
            }

            var blockedAmount = incomingDamage.FinalDamage * (shieldDefense * 0.5f);
            return new DamageResult
            {
                RawDamage = incomingDamage.RawDamage,
                FinalDamage = Mathf.Max(0f, incomingDamage.FinalDamage - blockedAmount),
                IsCritical = incomingDamage.IsCritical,
                WasBlocked = true,
                BlockedAmount = blockedAmount,
                DefenseReduction = incomingDamage.DefenseReduction,
            };
        }
    }
}
