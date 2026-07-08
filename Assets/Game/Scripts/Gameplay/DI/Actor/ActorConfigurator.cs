using Game.Core;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorConfigurator",
        menuName = "Game/Gameplay/Actor Configurator")]
    public sealed class ActorConfigurator : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldRegistry<IWorldActor>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IActorAnchors>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IActorInputBinder>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<ITargetProvider>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IActorDialogueEndpoint>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IActorTravelEndpoint>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IPickupEffectHandlerProvider>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorWorldObjectFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}