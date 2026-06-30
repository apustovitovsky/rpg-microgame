using Etheria.Core.DI;
using Etheria.Game.Npc;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    public sealed class NpcScope : LifetimeScope
    {
        [SerializeField] private Transform _npcRoot;
        protected override void Configure(IContainerBuilder builder)
        {
            if (_npcRoot == null)
            {
                Debug.LogError(
                    $"{nameof(NpcScope)} requires assigned NPC root.",
                    this);

                return;
            }

            builder.RegisterInstance(
                new ScopeRoot(_npcRoot));

            builder.Register<INpcState>(
                resolver =>
                {
                    var definition =
                        resolver.Resolve<NpcDefinitionSO>();

                    var registry =
                        resolver.Resolve<INpcStateRegistry>();

                    return registry.GetOrCreate(definition.NpcId);
                },
                Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<NpcAgent>()
                .UnderScopeRoot()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<NpcActorCommandEndpoint>()
                .UnderScopeRoot();

            builder.RegisterComponentInHierarchy<NavMeshAgent>()
                .UnderScopeRoot();

            builder.Register<NpcMotor>(Lifetime.Scoped);

            builder.Register<NpcMovementService>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<NpcPathPlanner>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<NpcRouteFollower>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<NpcTravelController>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<NpcRuntime>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<NpcDialogueSessionService>(Lifetime.Scoped);

            builder.Register<NpcDialogueStarter>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<NpcAwarenessSensor>()
                .UnderScopeRoot();
        }
    }
}