using Game.World;
using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetable
    {
        WorldId WorldId { get; }
        Transform Root { get; }
        Transform TargetPoint { get; }
        bool IsTargetable { get; }
    }
}