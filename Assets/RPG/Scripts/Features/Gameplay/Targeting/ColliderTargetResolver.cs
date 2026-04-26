namespace RPG.Gameplay
{
    using UnityEngine;

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