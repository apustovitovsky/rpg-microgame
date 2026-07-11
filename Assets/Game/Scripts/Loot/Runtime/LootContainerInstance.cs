using System;
using Game.Inventory;
using Game.World;

namespace Game.Loot
{
    public sealed class LootContainerInstance :
        IWorldInstance,
        IInventoryOwner
    {
        public LootContainerInstance(
            LootContainerDefinition definition)
            : this(Guid.NewGuid(), definition)
        {
        }

        public LootContainerInstance(
            Guid instanceId,
            LootContainerDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot container instance id cannot be empty.",
                    nameof(instanceId));
            }

            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));

            InstanceId = instanceId;

            var inventory = new Game.Inventory.Inventory(
                Definition.Capacity);

            foreach (var stack in Definition.InitialContents)
            {
                if (stack == null ||
                    stack.Item == null ||
                    stack.Count <= 0 ||
                    !inventory.TryAdd(
                        stack.Item,
                        stack.Count))
                {
                    throw new InvalidOperationException(
                        $"Loot container '{Definition.name}' " +
                        "has invalid initial contents.");
                }
            }

            Inventory = inventory;
        }

        public Guid InstanceId { get; }

        public LootContainerDefinition Definition { get; }

        public IInventory Inventory { get; }
    }
}