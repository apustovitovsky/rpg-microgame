using System;
using Game.Actor;
using Game.Core;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavMeshPlannerEndpoint :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private NavMeshActorInput _input;
        [SerializeField] private MovementController _movement;
        [SerializeField] private ActorLookController _look;

        public INavMeshPlanner Planner { get; private set; }

        public void Install(
            IContainerBuilder builder)
        {
            if (_agent == null ||
                _input == null ||
                _movement == null ||
                _look == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(NavMeshPlannerEndpoint)} requires " +
                    $"{nameof(NavMeshAgent)}, " +
                    $"{nameof(NavMeshActorInput)}, " +
                    $"{nameof(MovementController)} and " +
                    $"{nameof(ActorLookController)}.");
            }

            builder.RegisterComponent(this);

            builder.RegisterComponent(_agent);

            builder.RegisterEntryPoint<NavMeshPlanner>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponent(_input);

            builder.RegisterBuildCallback(_ =>
            {
                _movement.Bind(_input);
                _look.Bind(_input);
            });
        }

        [Inject]
        public void Construct(INavMeshPlanner planner)
        {
            Planner = planner
                ?? throw new ArgumentNullException(nameof(planner));
        }
    }
}