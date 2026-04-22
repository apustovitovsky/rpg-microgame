using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPickupService
    {
        bool TryCollect(IPickup pickup, Collider other);
    }
}