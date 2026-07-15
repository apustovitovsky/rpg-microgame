using System;

namespace Game.Item
{
    public readonly struct ItemStack
    {
        public ItemStack(
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