using System.Collections.Generic;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class PickupService : IPickupService
    {
        private readonly HashSet<IPickup> _inProgress = new();

        public bool TryCollect(IPickup pickup, IPickupCollector collector)
        {
            if (pickup == null || collector == null || pickup.IsCollected)
                return false;

            if (!_inProgress.Add(pickup))
                return false;

            try
            {
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
