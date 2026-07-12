using System;
using Game.Item;

namespace Game.Inventory
{
    public interface IInventoryService
    {
        bool TryGet(
            Guid ownerInstanceId,
            out IInventory inventory);

        bool CanAdd(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount);

        bool TryAdd(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount);

        bool TryRemove(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount);

        int GetCount(
            Guid ownerInstanceId,
            ItemDefinition definition);

        bool TryGetCount(
            Guid ownerInstanceId,
            string definitionId,
            out int count);

        bool HasItems(
            Guid ownerInstanceId,
            string definitionId,
            int amount);
    }
}
