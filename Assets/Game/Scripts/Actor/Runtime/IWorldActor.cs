using UnityEngine;

namespace Game.Actor
{
    public interface IWorldActor
    {
        Transform Root { get; }
        Transform CameraPivot { get; }
        Transform TargetPoint { get; }
        Transform UiAnchor { get; }
    }
}