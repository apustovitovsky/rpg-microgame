using UnityEngine;

namespace RPG.Gameplay
{
    public class GoldPickupEffect : IPickupEffect
    {
        private readonly int _amount;


        public GoldPickupEffect(int amount)
        {
            _amount = amount;
        }

        public bool TryApply(IPickupCollector collector)
        {
            if (!collector.TryGet<IInventory>(out var inventory))
                return false;

            inventory.AddGold(_amount);

            return true;
        }
    }
}
