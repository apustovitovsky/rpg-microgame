using System;

namespace Game.Inventory
{
    public sealed class InventoryTransferService :
        IInventoryTransferService
    {
        public InventoryTransferResult TryTransfer(
            IInventory source,
            IInventory destination,
            Guid instanceId,
            int count)
        {
            if (source == null ||
                destination == null ||
                source == destination ||
                instanceId == Guid.Empty ||
                count <= 0)
            {
                return InventoryTransferResult.InvalidRequest;
            }

            if (!TryFindEntry(
                    source,
                    instanceId,
                    out var sourceEntry))
            {
                return InventoryTransferResult.SourceStackNotFound;
            }

            if (count > sourceEntry.Count)
                return InventoryTransferResult.InsufficientAmount;

            var requestedStack = new InventoryStack(
                sourceEntry.Instance,
                count);

            if (!destination.CanInsert(requestedStack))
                return InventoryTransferResult.DestinationFull;

            if (!source.TryExtract(
                    instanceId,
                    count,
                    out var extractedStack))
            {
                return InventoryTransferResult.SourceStackNotFound;
            }

            if (!destination.TryInsert(extractedStack))
            {
                throw new InvalidOperationException(
                    "Inventory accepted a stack in CanInsert " +
                    "but rejected it in TryInsert.");
            }

            return InventoryTransferResult.Succeeded;
        }

        private static bool TryFindEntry(
            IInventory inventory,
            Guid instanceId,
            out InventoryEntry entry)
        {
            foreach (var candidate in inventory.Entries)
            {
                if (candidate.Instance.InstanceId != instanceId)
                    continue;

                entry = candidate;
                return true;
            }

            entry = null;
            return false;
        }
    }
}