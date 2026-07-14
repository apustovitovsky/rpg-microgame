using System;

namespace Game.Inventory
{
    public interface IInventoryTransferService
    {
        InventoryTransferResult TryTransfer(
            InventoryInstance source,
            InventoryInstance destination,
            Guid instanceId,
            int count);
    }
}