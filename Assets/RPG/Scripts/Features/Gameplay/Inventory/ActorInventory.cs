using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class ActorInventory : IInventory
    {
        private int _amount;
        public int CurrentGold => _amount;

        public ActorInventory(int amount)
        {
            _amount = amount;
        }
        public void AddGold(int amount)
        {
            _amount = Mathf.Clamp(_amount + amount, int.MinValue, int.MaxValue);
        }
    }
}
