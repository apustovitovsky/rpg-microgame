using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Inventory
{
    [CreateAssetMenu(
        fileName = "InventoryServiceModuleBuilder",
        menuName = "Game/Inventory/Inventory Service Module Builder")]
    public sealed class InventoryServiceModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InventoryService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}