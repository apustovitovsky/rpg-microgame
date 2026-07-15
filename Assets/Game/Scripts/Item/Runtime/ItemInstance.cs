using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Item
{
    public sealed class ItemInstance :
        IFragmentProvider
    {
        private readonly Dictionary<ItemStat, int> _statStacks =
            new();

        public ItemInstance(
            Guid instanceId,
            ItemDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Item instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public ItemDefinition Definition { get; }

        public bool TryGetFragment<TFragment>(
            out TFragment fragment)
            where TFragment : class
        {
            return Definition.TryGetFragment(
                out fragment);
        }

        public int GetStatStack(ItemStat stat)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            return _statStacks.TryGetValue(
                stat,
                out var value)
                ? value
                : 0;
        }

        public void SetStatStack(
            ItemStat stat,
            int value)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            if (value == 0)
            {
                _statStacks.Remove(stat);
                return;
            }

            _statStacks[stat] = value;
        }

        public void AddStatStack(
            ItemStat stat,
            int amount)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            SetStatStack(
                stat,
                GetStatStack(stat) + amount);
        }

        public bool TryRemoveStatStack(
            ItemStat stat,
            int amount)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount));
            }

            var current = GetStatStack(stat);

            if (current < amount)
                return false;

            SetStatStack(stat, current - amount);
            return true;
        }
    }
}