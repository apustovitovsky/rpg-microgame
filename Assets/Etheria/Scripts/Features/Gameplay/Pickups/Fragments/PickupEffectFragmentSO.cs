namespace Etheria.Features.Gameplay
{
    public abstract class PickupEffectFragmentSO : PickupFragmentSO
    {
        public abstract bool TryApply(IPickupTarget target, PickupInstance instance);
    }

    public abstract class PickupEffectFragmentSO<TService> : PickupEffectFragmentSO
        where TService : class
    {
        public sealed override bool TryApply(IPickupTarget collector, PickupInstance instance)
        {
            if (collector == null || instance == null)
                return false;

            if (!collector.TryGet<TService>(out var service))
                return false;

            return TryApply(service, instance);
        }

        protected abstract bool TryApply(TService service, PickupInstance instance);
    }
}



