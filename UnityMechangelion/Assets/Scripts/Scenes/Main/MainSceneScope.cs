namespace HyperCasualGame.Scripts.Scenes.Main
{
    using System.Linq;
    using GameFoundationCore.Scripts.DI.VContainer;
    using HyperCasualGame.Scripts.Features.Combat.Services;
    using HyperCasualGame.Scripts.StateMachines.Game;
    using HyperCasualGame.Scripts.StateMachines.Game.Interfaces;
    using UniT.Extensions;
    using VContainer;

    public class MainSceneScope : SceneScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<DamageCalculatorService>(Lifetime.Scoped);
            builder.Register<CooldownService>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
            builder.Register<DefenseResolverService>(Lifetime.Scoped);
            builder.Register<DifficultyService>(Lifetime.Scoped);
            builder.Register<CombatRegistry>(Lifetime.Scoped);
            builder.Register<GameStateMachine>(Lifetime.Singleton)
                .WithParameter(container => typeof(IGameState).GetDerivedTypes().Select(type => (IGameState)container.Instantiate(type)).ToList())
                .AsInterfacesAndSelf();
        }
    }
}
