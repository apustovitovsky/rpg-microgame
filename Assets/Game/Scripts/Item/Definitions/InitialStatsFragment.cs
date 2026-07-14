using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public sealed class InitialStatsFragment :
        ItemFragment
    {
        [SerializeField]
        private InitialItemStat[] _stats =
            Array.Empty<InitialItemStat>();

        public IReadOnlyList<InitialItemStat> Stats =>
            _stats;
    }
}