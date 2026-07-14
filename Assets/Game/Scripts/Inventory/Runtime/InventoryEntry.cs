using System;
using Game.Item;

namespace Game.Inventory
{
    public sealed class InventoryEntry
    {
        private readonly int _maximumCount;

        internal InventoryEntry(
            ItemInstance instance,
            int count)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            _maximumCount = GetMaximumCount(
                instance.Definition);

            if (count <= 0 || count > _maximumCount)
                throw new ArgumentOutOfRangeException(nameof(count));

            Count = count;
        }

        public ItemInstance Instance { get; }

        public ItemDefinition Definition => Instance.Definition;

        public int Count { get; private set; }

        public int AvailableSpace =>
            _maximumCount - Count;

        internal int Add(int amount)
        {
            if (amount <= 0)
                return 0;

            var added = Math.Min(
                amount,
                AvailableSpace);

            Count += added;
            return added;
        }

        internal int Remove(int amount)
        {
            if (amount <= 0)
                return 0;

            var removed = Math.Min(
                amount,
                Count);

            Count -= removed;
            return removed;
        }

        private static int GetMaximumCount(
            ItemDefinition definition)
        {
            if (definition == null ||
                !definition.TryGetFragment(
                    out StackFragment stack))
            {
                throw new InvalidOperationException(
                    $"{nameof(ItemDefinition)} requires " +
                    $"{nameof(StackFragment)}.");
            }

            return stack.MaximumCount;
        }
    }
}