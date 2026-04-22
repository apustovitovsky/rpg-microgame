
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class PickupService : IPickupService
    {
        private readonly HashSet<IPickup> _inProgress = new();

        public bool TryCollect(IPickup pickup, Collider other)
        {
            if (pickup == null || pickup.IsCollected)
                return false;

            if (!_inProgress.Add(pickup))
                return false;

            try
            {
                if (!other.TryGetComponent<IPickupCollector>(out var collector))
                    return false;

                if (!pickup.TryCollect(collector))
                    return false;

                OnCollected(collector.Name);
                return true;
            }
            finally
            {
                _inProgress.Remove(pickup);
            }
        }

        private void OnCollected(string name)
        {
            Debug.Log("Pickup collected by" + name);
        }
    }
}
