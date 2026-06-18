using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraFollowService
    {
        Transform CurrentFollowTarget { get; }
        void SetTarget(Transform target);
        void RemoveTarget();
    }
}

