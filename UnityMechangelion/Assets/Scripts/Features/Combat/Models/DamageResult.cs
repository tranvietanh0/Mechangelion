#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Models
{
    public sealed class DamageResult
    {
        public float RawDamage { get; set; }

        public float FinalDamage { get; set; }

        public bool IsCritical { get; set; }

        public bool WasBlocked { get; set; }

        public bool WasSuperBlocked { get; set; }

        public bool WasDodged { get; set; }

        public float BlockedAmount { get; set; }

        public float DefenseReduction { get; set; }

        public static DamageResult CreateDodged()
        {
            return new DamageResult
            {
                WasDodged = true,
                FinalDamage = 0f,
            };
        }
    }
}
