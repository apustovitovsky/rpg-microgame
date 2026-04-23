using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class PickupInstance : IPickupInstance
    {
        public PickupDefinitionSO Definition { get; }

        public PickupInstance(PickupDefinitionSO definition)
        {
            Definition = definition;
        }

        public bool TryCollect(IPickupCollector collector)
        {
            if (Definition == null)
                return false;

            bool anySucceeded = false;

            foreach (var fragment in Definition.GetFragments<PickupEffectFragmentSO>())
            {
                if (fragment.TryApply(collector, this))
                    anySucceeded = true;
            }

            return anySucceeded;
        }
    }
}

