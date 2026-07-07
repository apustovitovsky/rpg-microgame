using Game.Actor;
using Game.Core;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Game.AI
{
    [CreateAssetMenu(
        fileName = "ActorAIConfigurator",
        menuName = "Game/AI/Actor AI Configurator")]
    public sealed class ActorAIConfiguratorSO : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInScope<NavMeshAgent>();

            builder.RegisterEntryPoint<NavMeshPlanner>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<NavMeshActorInput>();

            builder.RegisterComponentInScope<NavMeshTravelEndpoint>()

                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(container =>
            {
                var input = container.Resolve<NavMeshActorInput>();
                var movement = container.Resolve<MovementController>();
                var look = container.Resolve<ActorLookController>();

                movement.Bind(input);
                look.Bind(input);
            });
        }
    }
}