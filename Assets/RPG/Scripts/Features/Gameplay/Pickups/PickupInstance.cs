namespace RPG.Gameplay
{
    public class PickupInstance : IPickupInstance
    {
        private readonly IPickupEffect _effect;

        public PickupInstance(IPickupEffect effect)
        {
            _effect = effect;
        }

        public bool IsCollected { get; private set; }

        public bool TryCollect(IPickupCollector collector)
        {
            if (IsCollected || _effect == null)
                return false;

            if (!_effect.TryApply(collector))
                return false;

            IsCollected = true;
            OnCollected();
            return true;
        }

        protected virtual void OnCollected()
        {
        }
    }
}
