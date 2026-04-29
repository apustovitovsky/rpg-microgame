using UnityEngine;

namespace Etheria.Game.Targeting
{
    public interface ITargetable
    {
        public string DisplayName { get; }
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}

