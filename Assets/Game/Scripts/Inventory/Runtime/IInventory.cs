using System.Collections.Generic;

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

        int GetCount(ItemDefinition definition);
    }
}