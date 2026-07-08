using UnityEngine;

namespace Game.Actor
{
    public interface IActorView
    {
        Transform Root { get; }

        Transform CameraPivot { get; }

        Transform TargetPoint { get; }

        Transform UiAnchor { get; }
    }
}