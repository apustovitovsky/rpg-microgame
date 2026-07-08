using Game.Core;
using Game.Pickup;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorRuntimeConfigurator",
        menuName = "Game/Actor/Actor Runtime Configurator")]
    public sealed class ActorRuntimeConfigurator : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInScope<WorldActor>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorLookController>();

            builder.RegisterComponentInScope<MovementController>();

            builder.RegisterComponentInScope<TargetingController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<DialogueInteractable>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorTarget>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Register<DebugActorViewPickupEffectHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<PickupEffectHandlerProvider>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}