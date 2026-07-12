using System;
using Game.Core;
using Game.Inventory;
using Game.Item;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "InventoryServiceModuleBuilder",
        menuName = "Game/Gameplay/Inventory Service Module Builder")]
    public sealed class InventoryServiceModuleBuilder : ModuleBuilder
    {
        [SerializeField]
        private ItemAssetCatalog _catalog;

        public override void Install(IContainerBuilder builder)
        {
            if (_catalog == null)
            {
                throw new InvalidOperationException(
                    "Item definition catalog is required.");
            }

            builder.RegisterInstance(_catalog)
                .AsImplementedInterfaces();

            builder.Register<InventoryService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<InventoryTransferService>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}