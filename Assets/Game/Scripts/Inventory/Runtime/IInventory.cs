using System;
using System.Collections.Generic;
using Game.Item;

namespace Game.Inventory
{
    public interface IInventory
    {
        int Capacity { get; }

        IReadOnlyList<InventoryEntry> Entries { get; }

        bool CanAdd(
            ItemDefinition definition,
            int amount);

        bool TryAdd(
            ItemDefinition definition,
            int amount);

        bool TryRemove(
            ItemDefinition definition,
            int amount);

        bool TryExtract(
            Guid instanceId,
            int count,
            out InventoryStack stack);

        bool CanInsert(
            InventoryStack stack);

        bool TryInsert(
            InventoryStack stack);

        int GetCount(ItemDefinition definition);
    }
}