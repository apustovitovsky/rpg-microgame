using UnityEngine;

namespace Game.Actor
{
    public interface IActorView
    {
        string ActorId { get; }

        Transform Root { get; }
        Transform CameraPivot { get; }
        Transform TargetPoint { get; }
        Transform UiAnchor { get; }

        bool TryGet<T>(out T capability)
            where T : class;

        T Get<T>()
            where T : class;
    }
}