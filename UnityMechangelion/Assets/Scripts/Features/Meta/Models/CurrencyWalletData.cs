#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Models
{
    using GameFoundationCore.Scripts.Models.Interfaces;

    public sealed class CurrencyWalletData : ILocalData
    {
        public int Coins { get; set; }

        public int Cores { get; set; }

        public void Init()
        {
            this.Coins = 0;
            this.Cores = 0;
        }
    }
}
