using System;

namespace Game.Inventory
{
    public interface IInventoryOwner
    {
        Guid InstanceId { get; }

        IInventory Inventory { get; }
    }
}