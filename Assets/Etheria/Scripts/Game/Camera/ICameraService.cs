using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraService
    {
        Transform CurrentTarget { get; }
        void SetTarget(Transform cameraPivot);
        void RemoveTarget();
    }
}

