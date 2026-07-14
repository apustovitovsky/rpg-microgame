using System;
using Game.Inventory;
using Game.Item;
using UnityEngine;

namespace Game.Actor
{
    [Serializable]
    public sealed class InventoryFragment :
        ActorFragment
    {
        [Serializable]
        public sealed class InitialItemEntry
        {
            [field: SerializeField]
            public ItemDefinition Definition { get; private set; }

            [field: SerializeField, Min(1)]
            public int Count { get; private set; } = 1;
        }

        [SerializeField, Min(1)]
        private int _capacity = 20;

        [SerializeField]
        private InitialItemEntry[] _initialItems =
            Array.Empty<InitialItemEntry>();

        public InventoryInstance Create()
        {
            var inventory = new InventoryInstance(_capacity);

            foreach (var entry in _initialItems)
            {
                if (entry == null ||
                    entry.Definition == null ||
                    entry.Count <= 0 ||
                    !inventory.TryAdd(
                        entry.Definition,
                        entry.Count))
                {
                    throw new InvalidOperationException(
                        $"{nameof(InventoryFragment)} has invalid " +
                        "initial items.");
                }
            }

            return inventory;
        }
    }
}