namespace RPG.Gameplay
{
    public abstract class PickupEffectFragmentSO : PickupFragmentSO
    {
        public abstract bool TryApply(IPickupCollector collector, IPickupInstance instance);
    }
}

