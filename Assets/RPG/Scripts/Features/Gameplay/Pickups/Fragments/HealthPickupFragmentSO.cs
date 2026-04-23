using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "HealthPickupFragment", menuName = "RPG/Gameplay/Pickup/Fragments/HealthPickupFragment")]
    public sealed class HealthPickupFragmentSO : PickupEffectFragmentSO
    {
        [Tooltip("The amount of health to add to the collector"), Min(1)]
        [field: SerializeField] public float HealAmount { get; private set; }

        public override bool TryApply(IPickupCollector collector, IPickupInstance instance)
        {
            if (!collector.TryGet<IHealth>(out var health))
                return false;

            if (health.IsFull)
                return false;

            health.Heal(HealAmount);
            return true;
        }
    }
}

