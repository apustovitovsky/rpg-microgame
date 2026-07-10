using System;
using Game.World;

namespace Game.Inventory
{
    public interface IInventoryService
    {
        bool TryGet(
            WorldId ownerId,
            out IInventory inventory);

        bool CanAdd(
            WorldId ownerId,
            ItemDefinition definition,
            int amount);

        bool TryAdd(
            WorldId ownerId,
            ItemDefinition definition,
            int amount);

        bool TryRemove(
            WorldId ownerId,
            ItemDefinition definition,
            int amount);

        int GetCount(
            WorldId ownerId,
            ItemDefinition definition);
    }

    public interface IInventoryRegistrationService
    {
        IDisposable Register(IInventoryOwner owner);
    }

    public sealed class InventoryService :
        IInventoryService,
        IInventoryRegistrationService
    {
        private readonly WorldIndex<IInventoryOwner> _owners = new();

        public IDisposable Register(IInventoryOwner owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (owner.Inventory == null)
                throw new ArgumentException(
                    "Inventory owner must provide an inventory.",
                    nameof(owner));

            return _owners.Register(
                owner.WorldId,
                owner);
        }

        public bool TryGet(
            WorldId ownerId,
            out IInventory inventory)
        {
            inventory = null;

            if (!_owners.TryGet(ownerId, out var owner))
                return false;

            inventory = owner.Inventory;
            return inventory != null;
        }

        public bool CanAdd(
            WorldId ownerId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerId, out var inventory) &&
                   inventory.CanAdd(definition, amount);
        }

        public bool TryAdd(
            WorldId ownerId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerId, out var inventory) &&
                   inventory.TryAdd(definition, amount);
        }

        public bool TryRemove(
            WorldId ownerId,
            ItemDefinition definition,
            int amount)
        {
            return TryGet(ownerId, out var inventory) &&
                   inventory.TryRemove(definition, amount);
        }

        public int GetCount(
            WorldId ownerId,
            ItemDefinition definition)
        {
            return TryGet(ownerId, out var inventory)
                ? inventory.GetCount(definition)
                : 0;
        }
    }
}