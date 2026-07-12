using System;
using Game.Core;
using Game.Item;

namespace Game.Inventory
{
    public sealed class InventoryService :
        IInventoryService,
        IRegistryWriter<IInventoryOwner>
    {
        private readonly Registry<IInventoryOwner> _owners =
            new();

        private readonly IItemAssetCatalog _catalog;

        public InventoryService(
            IItemAssetCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Add(
            Guid instanceId,
            IInventoryOwner owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Inventory owner instance id is required.",
                    nameof(instanceId));
            }

            if (owner.InstanceId != instanceId)
            {
                throw new ArgumentException(
                    "Inventory owner instance id does not match " +
                    "the registration id.",
                    nameof(instanceId));
            }

            if (owner.Inventory == null)
            {
                throw new ArgumentException(
                    "Inventory owner must provide an inventory.",
                    nameof(owner));
            }

            _owners.Add(instanceId, owner);
        }

        public bool Remove(
            Guid instanceId,
            IInventoryOwner expectedOwner)
        {
            return _owners.Remove(
                instanceId,
                expectedOwner);
        }

        public bool TryGet(
            Guid ownerInstanceId,
            out IInventory inventory)
        {
            inventory = null;

            if (!_owners.TryGet(
                    ownerInstanceId,
                    out var owner))
            {
                return false;
            }

            inventory = owner.Inventory;
            return inventory != null;
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