using System;
using Game.Core;
using UnityEngine;

namespace Game.Item
{
    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Game/Item/Item Definition")]
    public sealed class ItemDefinition :
        AssetDefinition<ItemInstance, ItemFragment>
    {
        public bool TryGetMaximumStackCount(
            out int maximumCount)
        {
            maximumCount = 0;

            return TryGetFragment(
                out StackFragment stack) &&
                stack.MaximumCount > 0 &&
                (maximumCount = stack.MaximumCount) > 0;
        }

        public override ItemInstance CreateInstance(
            Guid? instanceId = null)
        {
            var instance = new ItemInstance(
                instanceId ?? Guid.NewGuid(),
                this);

            if (!TryGetFragment(
                    out InitialStatsFragment initialStats))
            {
                return instance;
            }

            if (!TryGetMaximumStackCount(
                    out var maximumCount) ||
                maximumCount != 1)
            {
                throw new InvalidOperationException(
                    $"{nameof(ItemDefinition)} " +
                    $"'{DisplayName}' has " +
                    $"{nameof(InitialStatsFragment)}, so " +
                    $"{nameof(StackFragment)}." +
                    $"{nameof(StackFragment.MaximumCount)} " +
                    "must be 1.");
            }

            foreach (var initialStat in initialStats.Stats)
            {
                if (initialStat.Stat == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(ItemDefinition)} " +
                        $"'{DisplayName}' contains an initial stat " +
                        $"without {nameof(ItemStat)}.");
                }

                instance.SetStatStack(
                    initialStat.Stat,
                    initialStat.Value);
            }

            return instance;
        }
    }
}