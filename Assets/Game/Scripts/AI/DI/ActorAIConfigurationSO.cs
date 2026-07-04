using Game.Actor;
using Game.Core;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Game.AI
{
    [CreateAssetMenu(
        fileName = "ActorAIConfiguration",
        menuName = "Game/AI/Actor AI Configuration")]
    public sealed class ActorAIConfigurationSO : BuildConfigurationSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<NavMeshAgent>();

            builder.RegisterEntryPoint<NavMeshPlanner>(Lifetime.Scoped)
                .As<INavMeshPlanner>();

            builder.RegisterComponentInHierarchy<NavMeshActorInput>();

            builder.RegisterComponentInHierarchy<NavMeshPlannerEndpoint>();

            builder.RegisterComponentInHierarchy<MovementController>();
            builder.RegisterComponentInHierarchy<ActorLookController>();

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