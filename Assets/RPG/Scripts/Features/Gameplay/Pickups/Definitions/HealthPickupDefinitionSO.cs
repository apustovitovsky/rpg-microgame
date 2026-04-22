using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "HealthPickupDefinition", menuName = "RPG/Gameplay/Pickup/Health Pickup Definition")]
    public sealed class HealthPickupDefinitionSO : PickupDefinitionSO
    {
        [SerializeField] private int _amountPerStack = 10;

        public override bool ApplyTo(IPickupCollector collector)
        {
            if (!collector.TryGet<IHealth>(out var health))
                return false;

            if (health.IsFull)
                return false;

            health.Heal(_amountPerStack * InitialStackCount);
            return true;
        }
    }
}
