using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GoldPickupFragment", menuName = "RPG/Gameplay/Pickup/Fragments/GoldPickupFragment")]
    public sealed class GoldPickupFragmentSO : PickupEffectFragmentSO
    {
        [Tooltip("The amount of health to add to the collector"), Min(1)]
        [field: SerializeField] public int GoldToAdd { get; private set; }

        public override bool TryApply(IPickupCollector collector, IPickupInstance instance)
        {
            if (!collector.TryGet<IInventory>(out var inventory))
                return false;

            inventory.AddGold(GoldToAdd);
            return true;
        }
    }
}

