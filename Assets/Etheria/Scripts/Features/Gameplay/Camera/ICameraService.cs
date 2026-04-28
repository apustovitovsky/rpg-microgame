using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetTarget(Transform cameraPivot);
        void RemoveTarget();
    }
}

