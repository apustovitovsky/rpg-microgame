using System;

namespace Game.Inventory
{
    public interface IInventoryOwner
    {
        Guid InstanceId { get; }

        IInventory Inventory { get; }
    }

    public sealed class InventoryOwner : IInventoryOwner
    {
        public InventoryOwner(
            Guid instanceId,
            IInventory inventory)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inventory owner instance id is required.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Inventory = inventory
                ?? throw new ArgumentNullException(nameof(inventory));
        }

        public Guid InstanceId { get; }

        public IInventory Inventory { get; }
    }
}