using Game.Control;
using Game.Core;
using Game.Targeting;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorConfigurator",
        menuName = "Game/Actor/Actor Configurator")]
    public sealed class ActorModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInModuleRoot<PossessionEndpoint>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<Targetable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ActorLookController>();

            builder.RegisterComponentInModuleRoot<MovementController>();

            builder.RegisterComponentInModuleRoot<ActorTargetController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<DialogueParticipant>()
                .AsImplementedInterfaces();
        }
    }
}