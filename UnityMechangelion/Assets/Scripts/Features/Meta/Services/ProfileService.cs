#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Services
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundationCore.Scripts.Signals;
    using HyperCasualGame.Scripts.Core.Constants;
    using HyperCasualGame.Scripts.Features.Meta.Models;
    using HyperCasualGame.Scripts.Features.Meta.Persistence;
    using HyperCasualGame.Scripts.Features.Signals;

    public sealed class ProfileService
    {
        private readonly LocalSaveFacade localSaveFacade;
        private readonly SignalBus signalBus;

        private readonly PlayerProfileData profileData;

        public ProfileService(LocalSaveFacade localSaveFacade, SignalBus signalBus, PlayerProfileData profileData)
        {
            this.localSaveFacade = localSaveFacade;
            this.signalBus = signalBus;
            this.profileData = profileData;
        }

        public int Level => this.profileData.Level;

        public int Experience => this.profileData.Experience;

        public void AddXp(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount == 0)
            {
                return;
            }

            var data = this.profileData;
            var oldLevel = data.Level;
            data.Experience += amount;

            while (data.Experience >= this.GetRequiredXp(data.Level))
            {
                data.Experience -= this.GetRequiredXp(data.Level);
                data.Level++;
            }

            if (data.Level != oldLevel)
            {
                this.signalBus.Fire(new LevelUpSignal
                {
                    OldLevel = oldLevel,
                    NewLevel = data.Level,
                });
            }
        }

        public UniTask SaveAsync(bool force = false)
        {
            return this.localSaveFacade.SaveAsync(this.profileData, force);
        }

        private int GetRequiredXp(int level)
        {
            return CombatConstants.BaseLevelUpXp + ((level - 1) * CombatConstants.LevelUpXpStep);
        }
    }
}