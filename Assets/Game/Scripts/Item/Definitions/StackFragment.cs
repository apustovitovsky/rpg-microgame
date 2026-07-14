using System;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public sealed class StackFragment :
        ItemFragment
    {
        [field: SerializeField, Min(1)]
        public int MaximumCount { get; private set; } = 1;
    }
}