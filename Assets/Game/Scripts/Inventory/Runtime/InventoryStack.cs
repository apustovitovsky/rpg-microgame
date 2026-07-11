using System;
using Game.Item;

namespace Game.Inventory
{
    public readonly struct InventoryStack
    {
        internal InventoryStack(
            ItemInstance instance,
            int count)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Count = count;
        }

        public ItemInstance Instance { get; }

        public int Count { get; }
    }
}