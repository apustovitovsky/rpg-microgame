using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetTarget(Transform actorRoot, Transform lookTarget);
        void RemoveTarget();
    }
}
