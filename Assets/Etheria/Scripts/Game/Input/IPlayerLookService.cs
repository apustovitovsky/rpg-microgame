using Etheria.Game.Player;
using UnityEngine;

namespace Etheria.Game.Camera
{
    // Maintains player-driven look orientation for the current avatar and applies it to the look rig.
    // It should not own gameplay facing policy long-term.
    public interface IPlayerLookService
    {
        Transform CurrentPivot { get; }
        Vector3 Forward { get; }

        void SetTarget(Transform actorRoot, Transform cameraPivot);
        void RemoveTarget();
        void SetYawFromWorldDirection(Vector3 direction);
    }
}
