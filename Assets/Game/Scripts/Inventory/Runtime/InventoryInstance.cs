using System;
using System.Collections.Generic;
using Game.Item;

namespace Game.Inventory
{
    public sealed class InventoryInstance
    {
        private readonly List<InventoryEntry> _entries = new();

        public InventoryInstance(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
        }

        public int Capacity { get; }

        public IReadOnlyList<InventoryEntry> Entries => _entries;

        public bool CanAdd(
            ItemDefinition definition,
            int amount)
        {
            if (definition == null ||
                amount <= 0 ||
                !definition.TryGetMaximumStackCount(
                    out var maximumCount))
            {
                return false;
            }

            long availableSpace =
                (long)(Capacity - _entries.Count) *
                maximumCount;

            foreach (var entry in _entries)
            {
                if (entry.Definition == definition)
                    availableSpace += entry.AvailableSpace;
            }

            return availableSpace >= amount;
        }

        public bool TryAdd(
            ItemDefinition definition,
            int amount)
        {
            if (!CanAdd(definition, amount) ||
                !definition.TryGetMaximumStackCount(
                    out var maximumCount))
            {
                return false;
            }

            var remaining = amount;

            foreach (var entry in _entries)
            {
                if (entry.Definition != definition)
                    continue;

                remaining -= entry.Add(remaining);

                if (remaining == 0)
                    return true;
            }

            while (remaining > 0)
            {
                var stackSize = Math.Min(
                    remaining,
                    maximumCount);

                _entries.Add(
                    new InventoryEntry(
                        definition.CreateInstance(),
                        stackSize));

                remaining -= stackSize;
            }

            return true;
        }

        public bool TryRemove(
            ItemDefinition definition,
            int amount)
        {
            if (definition == null ||
                amount <= 0 ||
                GetCount(definition) < amount)
            {
                return false;
            }

            var remaining = amount;

            for (var index = _entries.Count - 1;
                 index >= 0 && remaining > 0;
                 index--)
            {
                var entry = _entries[index];

                if (entry.Definition != definition)
                    continue;

                remaining -= entry.Remove(remaining);

                if (entry.Count == 0)
                    _entries.RemoveAt(index);
            }

            return true;
        }

        public bool TryExtract(
            Guid instanceId,
            int count,
            out ItemStack stack)
        {
            stack = default;

            if (instanceId == Guid.Empty || count <= 0)
                return false;

            for (var index = 0; index < _entries.Count; index++)
            {
                var entry = _entries[index];

                if (entry.Instance.InstanceId != instanceId)
                    continue;

                if (count > entry.Count)
                    return false;

                if (count == entry.Count)
                {
                    _entries.RemoveAt(index);

                    stack = new ItemStack(
                        entry.Instance,
                        count);

                    return true;
                }

                entry.Remove(count);

                stack = new ItemStack(
                    entry.Definition.CreateInstance(),
                    count);

                return true;
            }

            return false;
        }

        public bool CanInsert(
            ItemStack stack)
        {
            return stack.Instance != null &&
                   stack.Count > 0 &&
                   stack.Instance.Definition.TryGetMaximumStackCount(
                       out var maximumCount) &&
                   stack.Count <= maximumCount &&
                   _entries.Count < Capacity;
        }

        public bool TryInsert(
            ItemStack stack)
        {
            if (!CanInsert(stack))
                return false;

            _entries.Add(
                new InventoryEntry(
                    stack.Instance,
                    stack.Count));

            return true;
        }

        public int GetCount(ItemDefinition definition)
        {
            if (definition == null)
                return 0;

            var total = 0;

            foreach (var entry in _entries)
            {
                if (entry.Definition == definition)
                    total += entry.Count;
            }

            return total;
        }
    }
}