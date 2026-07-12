using System;
using System.Collections.Generic;
using Game.Inventory;
using Game.Item;
using Game.World;
using UnityEngine;

namespace Game.Loot
{
    [CreateAssetMenu(
        fileName = "LootContainerDefinition",
        menuName = "Game/Loot/Loot Container Definition")]
    public sealed class LootContainerDefinition :
        WorldDefinition<LootContainerInstance>
    {
        [Serializable]
        public sealed class InitialStack
        {
            [field: SerializeField]
            public ItemDefinition Item { get; private set; }

            [field: SerializeField, Min(1)]
            public int Count { get; private set; } = 1;
        }

        [field: SerializeField, Min(1)]
        public int Capacity { get; private set; } = 20;

        [SerializeField]
        private InitialStack[] _initialContents =
            Array.Empty<InitialStack>();

        public IReadOnlyList<InitialStack> InitialContents =>
            _initialContents;

        public override LootContainerInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new LootContainerInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }

        public Inventory.Inventory CreateInventory()
        {
            var inventory = new Inventory.Inventory(Capacity);

            foreach (var stack in _initialContents)
            {
                if (stack == null ||
                    stack.Item == null ||
                    stack.Count <= 0 ||
                    !inventory.TryAdd(
                        stack.Item,
                        stack.Count))
                {
                    throw new InvalidOperationException(
                        $"Loot container '{name}' " +
                        "has invalid initial contents.");
                }
            }

            return inventory;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Capacity < 1)
                Capacity = 1;

            _initialContents ??= Array.Empty<InitialStack>();
        }
    }
}