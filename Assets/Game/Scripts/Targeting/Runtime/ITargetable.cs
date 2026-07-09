using Game.World;
using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetable
    {
        WorldInfo Info { get; }
        WorldId WorldId { get; }
        Transform TargetPoint { get; }
        Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}