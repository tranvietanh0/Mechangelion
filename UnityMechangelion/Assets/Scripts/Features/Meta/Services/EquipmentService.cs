#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Services
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundationCore.Scripts.Signals;
    using HyperCasualGame.Scripts.Core.Enums;
    using HyperCasualGame.Scripts.Features.Meta.Models;
    using HyperCasualGame.Scripts.Features.Meta.Persistence;
    using HyperCasualGame.Scripts.Features.Signals;

    public sealed class EquipmentService
    {
        private readonly LocalSaveFacade localSaveFacade;
        private readonly SignalBus signalBus;

        private readonly EquipmentInventoryData inventoryData;

        public EquipmentService(LocalSaveFacade localSaveFacade, SignalBus signalBus, EquipmentInventoryData inventoryData)
        {
            this.localSaveFacade = localSaveFacade;
            this.signalBus = signalBus;
            this.inventoryData = inventoryData;
        }

        private EquipmentInventoryData Data => this.inventoryData;

        public bool IsOwned(string equipmentId)
        {
            return this.Data.OwnedEquipmentIds.Contains(equipmentId);
        }

        public int GetLevel(string equipmentId)
        {
            return this.Data.EquipmentLevels.TryGetValue(equipmentId, out var level) ? level : 0;
        }

        public string GetEquipped(EquipmentSlotType slotType)
        {
            return this.Data.EquippedEquipmentIds.TryGetValue(slotType, out var equipmentId) ? equipmentId : string.Empty;
        }

        public void Unlock(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(equipmentId));
            }

            if (!this.Data.OwnedEquipmentIds.Add(equipmentId))
            {
                return;
            }

            this.Data.EquipmentLevels.TryAdd(equipmentId, 1);
            this.signalBus.Fire(new EquipmentChangedSignal
            {
                EquipmentId = equipmentId,
            });
        }

        public void SetLevel(string equipmentId, int level)
        {
            if (!this.IsOwned(equipmentId))
            {
                throw new InvalidOperationException($"Equipment '{equipmentId}' is not owned.");
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            this.Data.EquipmentLevels[equipmentId] = level;
            this.signalBus.Fire(new EquipmentChangedSignal
            {
                EquipmentId = equipmentId,
            });
        }

        public void Equip(EquipmentSlotType slotType, string equipmentId)
        {
            if (!this.IsOwned(equipmentId))
            {
                throw new InvalidOperationException($"Equipment '{equipmentId}' is not owned.");
            }

            this.Data.EquippedEquipmentIds[slotType] = equipmentId;
            this.signalBus.Fire(new EquipmentChangedSignal
            {
                EquipmentId = equipmentId,
                SlotType = slotType,
            });
        }

        public void Unequip(EquipmentSlotType slotType)
        {
            if (!this.Data.EquippedEquipmentIds.Remove(slotType, out var equipmentId))
            {
                return;
            }

            this.signalBus.Fire(new EquipmentChangedSignal
            {
                EquipmentId = equipmentId,
                SlotType = slotType,
            });
        }

        public UniTask SaveAsync(bool force = false)
        {
            return this.localSaveFacade.SaveAsync(this.Data, force);
        }
    }
}
