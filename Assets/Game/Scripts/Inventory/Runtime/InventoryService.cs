using System;
using Game.Core;
using Game.Item;

namespace Game.Inventory
{
    public sealed class InventoryService :
        IInventoryService,
        IRegistryWriter<IInventory>
    {
        private readonly Registry<IInventory> _inventories =
            new();

        private readonly IItemAssetCatalog _catalog;

        public InventoryService(
            IItemAssetCatalog catalog)
        {
            _catalog = catalog
                ?? throw new ArgumentNullException(nameof(catalog));
        }

        public void Add(
            Guid instanceId,
            IInventory inventory)
        {
            _inventories.Add(
                instanceId,
                inventory);
        }

        public bool Remove(
            Guid instanceId,
            IInventory expectedInventory)
        {
            return _inventories.Remove(
                instanceId,
                expectedInventory);
        }

        public bool TryGet(
            Guid ownerInstanceId,
            out IInventory inventory)
        {
            return _inventories.TryGet(
                ownerInstanceId,
                out inventory);
        }

        public bool CanAdd(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerInstanceId, out var inventory) &&
                   inventory.CanAdd(definition, amount);
        }

        public bool TryAdd(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerInstanceId, out var inventory) &&
                   inventory.TryAdd(definition, amount);
        }

        public bool TryRemove(
            Guid ownerInstanceId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerInstanceId, out var inventory) &&
                   inventory.TryRemove(definition, amount);
        }

        public int GetCount(
            Guid ownerInstanceId,
            ItemDefinition definition)
        {
            return TryGet(ownerInstanceId, out var inventory)
                ? inventory.GetCount(definition)
                : 0;
        }

        public bool TryGetCount(
            Guid ownerInstanceId,
            string definitionId,
            out int count)
        {
            count = 0;

            if (!TryGet(ownerInstanceId, out var inventory) ||
                !_catalog.TryGet(
                    definitionId,
                    out var definition))
            {
                return false;
            }

            count = inventory.GetCount(definition);
            return true;
        }

        public bool HasItems(
            Guid ownerInstanceId,
            string definitionId,
            int amount)
        {
            return amount > 0 &&
                   TryGetCount(
                       ownerInstanceId,
                       definitionId,
                       out var count) &&
                   count >= amount;
        }
    }
}