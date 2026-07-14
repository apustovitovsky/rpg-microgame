using System;
using Game.Item;
using UnityEngine;

namespace Game.Pickup
{
    [Serializable]
    public sealed class ItemPickupFragment :
        PickupFragment
    {
        [field: SerializeField]
        public ItemDefinition Item { get; private set; }

        [field: SerializeField, Min(1)]
        public int Count { get; private set; } = 1;
    }
}