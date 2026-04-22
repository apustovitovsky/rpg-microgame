namespace RPG.Gameplay
{
    public class HealthPickup : IPickup
    {
        private readonly float _amount;
        public bool IsCollected { get; private set; }

        public HealthPickup(int amount)
        {
            _amount = amount;
        }

        public bool TryCollect(IPickupCollector collector)
        {
            if (!collector.TryGet<IHealth>(out var health))
                return false;

            if (health.IsFull)
                return false;

            health.Heal(_amount);

            IsCollected = true;
            OnCollected();

            return true;
        }

        protected virtual void OnCollected()
        {
        }
    }
}
