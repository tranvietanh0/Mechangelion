#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Services
{
    using System.Collections.Generic;
    using HyperCasualGame.Scripts.Features.Combat.Models;

    public sealed class CombatRegistry
    {
        private readonly Dictionary<string, CombatStats> combatants = new();

        public void Register(string combatantId, CombatStats combatStats)
        {
            this.combatants[combatantId] = combatStats;
        }

        public bool Unregister(string combatantId)
        {
            return this.combatants.Remove(combatantId);
        }

        public CombatStats? Get(string combatantId)
        {
            return this.combatants.TryGetValue(combatantId, out var combatStats) ? combatStats : null;
        }

        public void Clear()
        {
            this.combatants.Clear();
        }
    }
}
