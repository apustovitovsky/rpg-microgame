namespace Etheria.Features.Gameplay
{
    public sealed class PickupInstance : IPickup
    {
        public PickupDefinitionSO Definition { get; }

        public PickupInstance(PickupDefinitionSO definition)
        {
            Definition = definition;
        }

        public bool TryApplyTo(IPickupTarget target)
        {
            if (Definition == null)
                return false;

            bool anySucceeded = false;

            foreach (var fragment in Definition.GetFragments<PickupEffectFragmentSO>())
            {
                if (fragment.TryApply(target, this))
                    anySucceeded = true;
            }

            return anySucceeded;
        }
    }
}


