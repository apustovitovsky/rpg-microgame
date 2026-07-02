using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetable
    {
        Transform Root { get; }
        Transform TargetPoint { get; }
        bool IsTargetable { get; }
    }
}