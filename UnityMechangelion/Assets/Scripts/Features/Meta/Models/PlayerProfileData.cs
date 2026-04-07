#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Models
{
    using GameFoundationCore.Scripts.Models.Interfaces;

    public sealed class PlayerProfileData : ILocalData
    {
        public int Level { get; set; }

        public int Experience { get; set; }

        public void Init()
        {
            this.Level = 1;
            this.Experience = 0;
        }
    }
}
