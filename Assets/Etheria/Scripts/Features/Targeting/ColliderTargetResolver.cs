using Etheria.Features.Actor;
using UnityEngine;


namespace Etheria.Features.Targeting
{

    public sealed class ColliderTargetResolver
    {
        public bool TryResolve(RaycastHit hit, out ITargetable targetable)
        {
            if (hit.collider.TryGetComponent<ActorHitbox>(out var hitbox) &&
                hitbox.Targetable != null &&
                hitbox.Targetable.IsTargetable)
            {
                targetable = hitbox.Targetable;
                return true;
            }

            targetable = null;
            return false;
        }
    }
}
