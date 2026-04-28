using UnityEngine;

namespace Etheria.Features.Camera
{
    public interface IPlayerLookService
    {
        Transform CurrentActorRoot { get; }
        Transform CurrentPivot { get; }
        Vector3 Forward { get; }
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetTarget(Transform actorRoot, Transform cameraPivot);
        void RemoveTarget();
    }
}

