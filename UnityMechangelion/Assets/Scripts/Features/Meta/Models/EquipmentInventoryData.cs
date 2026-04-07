#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Models
{
    using System.Collections.Generic;
    using GameFoundationCore.Scripts.Models.Interfaces;
    using HyperCasualGame.Scripts.Core.Enums;

    public sealed class EquipmentInventoryData : ILocalData
    {
        public HashSet<string> OwnedEquipmentIds { get; set; } = new();

        public Dictionary<string, int> EquipmentLevels { get; set; } = new();

        public Dictionary<EquipmentSlotType, string> EquippedEquipmentIds { get; set; } = new();

        public void Init()
        {
            this.OwnedEquipmentIds = new HashSet<string>();
            this.EquipmentLevels = new Dictionary<string, int>();
            this.EquippedEquipmentIds = new Dictionary<EquipmentSlotType, string>();
        }
    }
}
