using System;
using Game.Core;

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

    public interface IInventoryRegistrationService
    {
        IDisposable Register(IInventoryOwner owner);
    }

    public sealed class InventoryService :
        IInventoryService,
        IInventoryRegistrationService
    {
        private readonly InstanceIndex<IInventoryOwner> _owners =
            new();

        private readonly IItemDefinitionCatalog _definitions;

        public InventoryService(
            IItemDefinitionCatalog definitions)
        {
            _definitions = definitions;
        }

        public IDisposable Register(IInventoryOwner owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (owner.Inventory == null)
            {
                throw new ArgumentException(
                    "Inventory owner must provide an inventory.",
                    nameof(owner));
            }

            return _owners.Register(
                owner.InstanceId,
                owner);
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
                !_definitions.TryGet(
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