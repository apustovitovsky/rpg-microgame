using System;
using Game.CommandSystem;
using Game.Core;
using Game.Interaction;
using Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupScope :
        LifetimeScope
    {
        [SerializeField] private Transform _pickupRoot;

        protected override void Configure(
            IContainerBuilder builder)
        {
            if (_pickupRoot == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PickupScope)} requires a pickup root.");
            }

            builder.RegisterInstance(
                new ModuleRoot(_pickupRoot));

            builder.RegisterComponentInModuleRoot<Targetable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ItemPickupCollectable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<ItemPickupInteractable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Register<InteractCommandHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<Targetable>();
                resolver.Resolve<ICommandReceiver>();
            });
        }
    }
}