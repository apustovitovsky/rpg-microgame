using UnityEngine;

namespace RPG.Gameplay
{
    public class GoldPickup : IPickup
    {
        public bool IsCollected { get; private set; }
        private readonly int _amount;


        public GoldPickup(int amount)
        {
            _amount = amount;
        }

        public bool TryCollect(IPickupCollector collector)
        {
            if (collector.Inventory == null)
                return false;

            collector.Inventory.AddGold(_amount);

            IsCollected = true;
            OnCollected();

            return true;
        }

        protected virtual void OnCollected()
        {
        }
    }
}
