using System;
using Game.Core;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorServiceModuleBuilder",
        menuName = "Game/Gameplay/Actor Service Module Builder")]
    public sealed class ActorServiceModuleBuilder : ModuleBuilder
    {
        [SerializeField]
        private ActorDefinitionCatalog _catalog;

        public override void Install(IContainerBuilder builder)
        {
            if (_catalog == null)
            {
                throw new InvalidOperationException(
                    "Actor definition catalog is required.");
            }

            builder.RegisterInstance(_catalog)
                .AsImplementedInterfaces();

            builder.Register<InstanceIndex<IPossessable>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<InstanceIndex<ITargetProvider>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorFactory>(Lifetime.Singleton);

            builder.Register<ActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}