using UnityEngine;

namespace Etheria.Gameplay
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetTarget(Transform cameraPivot);
        void RemoveTarget();
    }
}
