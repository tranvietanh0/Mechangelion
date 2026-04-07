#nullable enable

namespace HyperCasualGame.Scripts.Features.Signals
{
    using HyperCasualGame.Scripts.Core.Enums;

    public sealed class PlayerAttackSignal
    {
        public bool IsRightHand { get; set; }

        public float PrepareProgress { get; set; }
    }

    public sealed class PlayerBlockSignal
    {
        public bool IsSuperBlock { get; set; }
    }

    public sealed class PlayerDodgeSignal
    {
        public bool IsLeft { get; set; }
    }

    public sealed class EnemyAttackSignal
    {
        public string EnemyId { get; set; } = string.Empty;

        public int AttackType { get; set; }
    }

    public sealed class DamageDealtSignal
    {
        public string AttackerId { get; set; } = string.Empty;

        public string TargetId { get; set; } = string.Empty;

        public float Damage { get; set; }

        public bool IsCritical { get; set; }

        public bool WasBlocked { get; set; }
    }

    public sealed class EntityDefeatedSignal
    {
        public string EntityId { get; set; } = string.Empty;

        public bool IsPlayer { get; set; }
    }

    public sealed class BattleStartedSignal
    {
        public string EnemyId { get; set; } = string.Empty;

        public EnemyType EnemyType { get; set; }

        public BattleMode BattleMode { get; set; }
    }

    public sealed class BattleEndedSignal
    {
        public bool Victory { get; set; }

        public float Duration { get; set; }
    }
}
