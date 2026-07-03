using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetable
    {
        string TargetId { get; }
        Transform Root { get; }
        Transform TargetPoint { get; }
        bool IsTargetable { get; }
    }
}