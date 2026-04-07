namespace HyperCasualGame.Scripts.Scenes
{
    using GameFoundationCore.Scripts;
    using GameFoundationCore.Scripts.DI.VContainer;
    using GameFoundationCore.Scripts.Signals;
    using HyperCasualGame.Scripts.Features.Meta.Persistence;
    using HyperCasualGame.Scripts.Features.Meta.Services;
    using HyperCasualGame.Scripts.Features.Signals;
    using UITemplate.Scripts;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;

    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterGameFoundation(this.transform);
            builder.RegisterUITemplate();
            builder.DeclareSignal<PlayerAttackSignal>();
            builder.DeclareSignal<PlayerBlockSignal>();
            builder.DeclareSignal<PlayerDodgeSignal>();
            builder.DeclareSignal<EnemyAttackSignal>();
            builder.DeclareSignal<DamageDealtSignal>();
            builder.DeclareSignal<EntityDefeatedSignal>();
            builder.DeclareSignal<BattleStartedSignal>();
            builder.DeclareSignal<BattleEndedSignal>();
            builder.DeclareSignal<LevelUpSignal>();
            builder.DeclareSignal<CurrencyChangedSignal>();
            builder.DeclareSignal<EquipmentChangedSignal>();
            builder.DeclareSignal<RewardReceivedSignal>();
            builder.Register<LocalSaveFacade>(Lifetime.Singleton);
            builder.Register<ProfileService>(Lifetime.Singleton).AsSelf();
            builder.Register<CurrencyService>(Lifetime.Singleton).AsSelf();
            builder.Register<EquipmentService>(Lifetime.Singleton).AsSelf();
        }
    }
}
