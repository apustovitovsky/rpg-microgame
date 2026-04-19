using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICameraService
    {
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetTarget(Transform target);
        void RemoveTarget();
    }
}
