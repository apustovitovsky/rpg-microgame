using UnityEngine;

namespace Game.Actor
{
    public interface IActorTransform
    {
        Transform Root { get; }
        Transform CameraPivot { get; }
    }
}