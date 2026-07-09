using Game.Core;
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
            builder.RegisterComponentInModuleRoot<ActorView>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorLookController>();

            builder.RegisterComponentInModuleRoot<MovementController>();

            builder.RegisterComponentInModuleRoot<ActorTargetController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<DialogueInteractable>()
                .AsImplementedInterfaces();

            // builder.RegisterComponentInModuleRoot<ActorTarget>()
            //     .AsSelf()
            //     .AsImplementedInterfaces();
        }
    }
}