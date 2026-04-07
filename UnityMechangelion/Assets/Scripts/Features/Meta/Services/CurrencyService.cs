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

    public sealed class CurrencyService
    {
        private readonly LocalSaveFacade localSaveFacade;
        private readonly SignalBus signalBus;

        private readonly CurrencyWalletData walletData;

        public CurrencyService(LocalSaveFacade localSaveFacade, SignalBus signalBus, CurrencyWalletData walletData)
        {
            this.localSaveFacade = localSaveFacade;
            this.signalBus = signalBus;
            this.walletData = walletData;
        }

        public int Coins => this.walletData.Coins;

        public int Cores => this.walletData.Cores;

        public void AddCoins(int amount)
        {
            this.ChangeCurrency(CurrencyType.Coins, amount);
        }

        public void AddCores(int amount)
        {
            this.ChangeCurrency(CurrencyType.Cores, amount);
        }

        public bool TrySpendCoins(int amount)
        {
            return this.TrySpend(CurrencyType.Coins, amount);
        }

        public bool TrySpendCores(int amount)
        {
            return this.TrySpend(CurrencyType.Cores, amount);
        }

        public UniTask SaveAsync(bool force = false)
        {
            return this.localSaveFacade.SaveAsync(this.walletData, force);
        }

        private void ChangeCurrency(CurrencyType currencyType, int delta)
        {
            if (delta < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delta));
            }

            if (delta == 0)
            {
                return;
            }

            var oldAmount = this.GetAmount(currencyType);
            var newAmount = oldAmount + delta;
            this.SetAmount(currencyType, newAmount);
            this.signalBus.Fire(new CurrencyChangedSignal
            {
                CurrencyType = currencyType,
                OldAmount = oldAmount,
                NewAmount = newAmount,
            });
        }

        private bool TrySpend(CurrencyType currencyType, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var oldAmount = this.GetAmount(currencyType);
            if (oldAmount < amount)
            {
                return false;
            }

            var newAmount = oldAmount - amount;
            this.SetAmount(currencyType, newAmount);
            this.signalBus.Fire(new CurrencyChangedSignal
            {
                CurrencyType = currencyType,
                OldAmount = oldAmount,
                NewAmount = newAmount,
            });

            return true;
        }

        private int GetAmount(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.Coins => this.walletData.Coins,
                CurrencyType.Cores => this.walletData.Cores,
                _ => throw new ArgumentOutOfRangeException(nameof(currencyType)),
            };
        }

        private void SetAmount(CurrencyType currencyType, int value)
        {
            switch (currencyType)
            {
                case CurrencyType.Coins:
                    this.walletData.Coins = value;
                    break;
                case CurrencyType.Cores:
                    this.walletData.Cores = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currencyType));
            }
        }
    }
}
