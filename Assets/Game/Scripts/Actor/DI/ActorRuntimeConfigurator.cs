using Game.Core;
using Game.Pickup;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorRuntimeConfigurator",
        menuName = "Game/Actor/Actor Runtime Configurator")]
    public sealed class ActorRuntimeConfigurator : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInModuleRoot<WorldActor>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorInteractor>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorLookController>();

            builder.RegisterComponentInModuleRoot<MovementController>();

            builder.RegisterComponentInModuleRoot<TargetingController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<DialogueInteractable>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorTarget>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Register<DebugActorViewPickupEffectHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<PickupEffectHandlerProvider>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}