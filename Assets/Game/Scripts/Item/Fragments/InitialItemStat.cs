using System;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public struct InitialItemStat
    {
        [field: SerializeField]
        public ItemStat Stat { get; private set; }

        [field: SerializeField]
        public int Value { get; private set; }
    }
}