#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Models
{
    using HyperCasualGame.Scripts.Core.Enums;

    public sealed class DamageRequest
    {
        public string AttackerId { get; set; } = string.Empty;

        public string TargetId { get; set; } = string.Empty;

        public float BaseDamage { get; set; }

        public float DamageMultiplier { get; set; } = 1f;

        public float CritChance { get; set; }

        public float CritMultiplier { get; set; } = 1.5f;

        public WeaponType WeaponType { get; set; }

        public float PrepareProgress { get; set; } = 1f;

        public bool IsRightHand { get; set; }
    }
}
