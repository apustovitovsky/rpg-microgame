using System;
using Game.World;

namespace Game.Inventory
{
    public interface IInventoryOwner
    {
        WorldId WorldId { get; }

        IInventory Inventory { get; }
    }

    public sealed class InventoryOwner : IInventoryOwner
    {
        public InventoryOwner(
            WorldId worldId,
            IInventory inventory)
        {
            if (worldId.IsEmpty)
                throw new ArgumentException(
                    "World id is required.",
                    nameof(worldId));

            WorldId = worldId;

            Inventory = inventory
                ?? throw new ArgumentNullException(nameof(inventory));
        }

        public WorldId WorldId { get; }

        public IInventory Inventory { get; }
    }
}