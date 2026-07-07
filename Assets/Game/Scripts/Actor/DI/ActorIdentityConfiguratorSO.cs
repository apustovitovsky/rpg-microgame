using Game.Core;
using Game.Pickup;
using UnityEngine;
using VContainer;


namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorIdentityConfigurator",
        menuName = "Game/Actor/Actor Identity Configurator")]
    public sealed class ActorIdentityConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(
            IContainerBuilder builder)
        {
            builder.Register<ActorIdentity>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorView>()
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