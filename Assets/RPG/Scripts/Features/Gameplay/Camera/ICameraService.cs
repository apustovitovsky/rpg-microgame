using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetTarget(Transform cameraPivot);
        void RemoveTarget();
    }
}
