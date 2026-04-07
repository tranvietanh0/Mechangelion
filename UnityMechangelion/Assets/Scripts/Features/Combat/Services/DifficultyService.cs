#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Services
{
    using System;
    using HyperCasualGame.Scripts.Core.Enums;

    public sealed class DifficultyService
    {
        public int AdjustEnemyLevel(int playerLevel, BattleMode battleMode)
        {
            var offset = battleMode switch
            {
                BattleMode.Ordinary => 0,
                BattleMode.Boss => 2,
                BattleMode.PvP => 1,
                _ => 0,
            };

            return Math.Max(1, playerLevel + offset);
        }

        public float GetRewardMultiplier(BattleMode battleMode)
        {
            return battleMode switch
            {
                BattleMode.Ordinary => 1f,
                BattleMode.Boss => 2f,
                BattleMode.PvP => 1.5f,
                _ => 1f,
            };
        }
    }
}
