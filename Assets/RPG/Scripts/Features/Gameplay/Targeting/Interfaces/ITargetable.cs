using UnityEngine;

namespace RPG.Gameplay
{
    public interface ITargetable
    {
        public string DisplayName { get; }
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}
