#nullable enable

namespace HyperCasualGame.Scripts.Core.Constants
{
    public static class CombatConstants
    {
        public const float PunchPrepareTime = 0.75f;
        public const float PunchReloadTime = 1f;
        public const float BlockReloadTime = 4f;
        public const float SuperBlockWindow = 0.35f;
        public const float DodgeReloadTime = 1f;
        public const float DodgeDuration = 0.5f;
        public const float BasePunchDamage = 0.1f;
        public const float DamageDefMinPercent = 0.2f;
        public const float EnemyDamageMultiplier = 0.625f;
        public const int LevelStepDownInterval = 5;
        public const float LevelMultiplierStep = 0.05f;
        public const float EnemyHealthMultiplierLow = 0.5f;
        public const float EnemyHealthMultiplierMid = 0.75f;
        public const float EnemyHealthMultiplierHigh = 0.8f;
        public const float EnemyHealthLevelStep = 0.08f;
        public const int BaseLevelUpXp = 100;
        public const int LevelUpXpStep = 25;
    }
}
