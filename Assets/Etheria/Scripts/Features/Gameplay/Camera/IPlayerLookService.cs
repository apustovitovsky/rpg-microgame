using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public interface IPlayerLookService
    {
        Transform CurrentPivot { get; }
        Vector3 Forward { get; }
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetTarget(Transform actorRoot, Transform cameraPivot);
        void RemoveTarget();
    }
}

