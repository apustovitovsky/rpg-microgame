using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "HealthPickupFragment", menuName = "RPG/Gameplay/Pickup/Fragments/HealthPickupFragment")]
    public sealed class HealthPickupFragmentSO : PickupEffectFragmentSO<IHealth>
    {
        [Tooltip("The amount of health to add to the collector"), Min(1)]
        [field: SerializeField] public float HealAmount { get; private set; }

        protected override bool TryApply(IHealth health, PickupInstance instance)
        {
            if (health.IsFull)
                return false;

            health.Heal(HealAmount);
            return true;
        }
    }
}

