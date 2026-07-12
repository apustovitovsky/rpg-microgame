using System;
using Game.CommandSystem;
using Game.Core;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Loot
{
    [DisallowMultipleComponent]
    public sealed class LootContainerScope :
        LifetimeScope
    {
        [SerializeField] private Transform _containerRoot;

        protected override void Configure(
            IContainerBuilder builder)
        {
            if (_containerRoot == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(LootContainerScope)} requires a container root.");
            }

            builder.RegisterInstance(
                new ModuleRoot(_containerRoot));

            builder.Register(
                    resolver => resolver
                        .Resolve<LootContainerInstance>()
                        .Definition
                        .CreateInventory(),
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<Targetable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<InventoryOwner>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<LootInteractable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<InventoryOwnerRegistration>(
                Lifetime.Scoped);

            builder.Register<InteractCommandHandler>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<WorldCommandReceiver>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<Targetable>();
                resolver.Resolve<ICommandReceiver>();
            });
        }
    }
}