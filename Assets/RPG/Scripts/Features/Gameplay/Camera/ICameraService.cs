using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICameraService
    {
        void SetHandler(IPlayerInputHandler handler);
        void RemoveHandler(IPlayerInputHandler handler);
        void SetTarget(Transform target);
        void RemoveTarget();
    }
}
