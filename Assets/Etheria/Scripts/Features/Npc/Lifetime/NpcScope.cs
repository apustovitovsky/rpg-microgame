using System;
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

            builder.RegisterComponentInHierarchy<NpcAgent>()
                .UnderScopeRoot()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<NavMeshAgent>()
                .UnderScopeRoot();

            builder.Register<NpcMotor>(Lifetime.Scoped);
            builder.Register<NpcTaskScheduler>(Lifetime.Scoped);


            builder.Register<NpcInteractionService>(Lifetime.Scoped);
        }
    }
}