using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class HealthPickup : Pickup
    {
        [SerializeField] private float healAmount = 25f;

        protected override bool TryCollect(Collider other)
        {
            var receiver = other.GetComponentInParent<ActorPickupReceiver>();
            if (receiver == null)
                return false;

            return receiver.TryReceiveHealing(healAmount);
        }
    }
}
