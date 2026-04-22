namespace RPG.Gameplay
{
    public class HealthPickupEffect : IPickupEffect
    {
        private readonly float _amount;

        public HealthPickupEffect(int amount)
        {
            _amount = amount;
        }

        public bool TryApply(IPickupCollector collector)
        {
            if (!collector.TryGet<IHealth>(out var health))
                return false;

            if (health.IsFull)
                return false;

            health.Heal(_amount);

            return true;
        }
    }
}
