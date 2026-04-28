using UnityEngine;

namespace Etheria.Features.Camera
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetTarget(Transform cameraPivot);
        void RemoveTarget();
    }
}

