using System;

namespace Game.Inventory
{
    public interface IInventoryTransferService
    {
        InventoryTransferResult TryTransfer(
            IInventory source,
            IInventory destination,
            Guid instanceId,
            int count);
    }
}