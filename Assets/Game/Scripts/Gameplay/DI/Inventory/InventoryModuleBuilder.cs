using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Inventory
{
    [CreateAssetMenu(
        fileName = "InventoryModuleBuilder",
        menuName = "Game/Inventory/Inventory Module Builder")]
    public sealed class InventoryModuleBuilder : ModuleBuilder
    {
        [SerializeField, Min(1)]
        private int _capacity = 20;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<Inventory>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .WithParameter("capacity", _capacity);
        }
    }
}