using Game.Core;
using Game.Targeting;
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
            builder.RegisterComponentInModuleRoot<ActorTransform>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<Targetable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorLookController>();

            builder.RegisterComponentInModuleRoot<MovementController>();

            builder.RegisterComponentInModuleRoot<ActorTargetController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorDialogue>()
                .AsImplementedInterfaces();
        }
    }
}