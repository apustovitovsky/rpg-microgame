using System;
using System.Collections.Generic;
using Game.Core;
using Game.Item;
using UnityEngine;

namespace Game.Loot
{
    [CreateAssetMenu(
        fileName = "LootContainerDefinition",
        menuName = "Game/Loot/Loot Container Definition")]
    public sealed class LootContainerDefinition : Definition
    {
        [Serializable]
        public sealed class InitialStack
        {
            [field: SerializeField]
            public ItemDefinition Item { get; private set; }

            [field: SerializeField, Min(1)]
            public int Count { get; private set; } = 1;
        }

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        [field: SerializeField, Min(1)]
        public int Capacity { get; private set; } = 20;

        [SerializeField]
        private InitialStack[] _initialContents =
            Array.Empty<InitialStack>();

        public IReadOnlyList<InitialStack> InitialContents =>
            _initialContents;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Capacity < 1)
                Capacity = 1;

            _initialContents ??= Array.Empty<InitialStack>();
        }
    }
}