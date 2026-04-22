using System.Collections.Generic;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class PickupService : IPickupService
    {
        private readonly HashSet<IPickupInstance> _inProgress = new();

        public bool TryCollect(IPickupInstance pickup, IPickupCollector collector)
        {
            if (pickup == null || collector == null || pickup.IsCollected)
                return false;

            if (!_inProgress.Add(pickup))
                return false;

            try
            {
                if (!pickup.TryCollect(collector))
                    return false;

                OnCollected(pickup, collector);
                return true;
            }
            finally
            {
                _inProgress.Remove(pickup);
            }
        }

        private void OnCollected(IPickupInstance pickup, IPickupCollector collector)
        {
            Debug.Log($"Pickup '{pickup.DisplayName}' collected by {collector.DisplayName}");
        }
    }
}
