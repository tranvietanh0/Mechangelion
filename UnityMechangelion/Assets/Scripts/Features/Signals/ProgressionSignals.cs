#nullable enable

namespace HyperCasualGame.Scripts.Features.Signals
{
    using HyperCasualGame.Scripts.Core.Enums;

    public sealed class LevelUpSignal
    {
        public int OldLevel { get; set; }

        public int NewLevel { get; set; }
    }

    public sealed class CurrencyChangedSignal
    {
        public CurrencyType CurrencyType { get; set; }

        public int OldAmount { get; set; }

        public int NewAmount { get; set; }
    }

    public sealed class EquipmentChangedSignal
    {
        public string EquipmentId { get; set; } = string.Empty;

        public EquipmentSlotType SlotType { get; set; }
    }

    public sealed class RewardReceivedSignal
    {
        public int Coins { get; set; }

        public int Cores { get; set; }

        public int Xp { get; set; }
    }
}
