using System;
using Game.Core;
using Game.Pickup;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupModuleBuilder",
        menuName = "Game/Gameplay/Pickup Module Builder")]
    public sealed class PickupModuleBuilder : ModuleBuilder
    {
        [SerializeField]
        private PickupAssetCatalog _catalog;

        public override void Install(IContainerBuilder builder)
        {
            if (_catalog == null)
            {
                throw new InvalidOperationException(
                    "Pickup asset catalog is required.");
            }

            builder.RegisterInstance(_catalog)
                .AsImplementedInterfaces();

            builder.Register<PickupSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ItemPickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}