using UnityEngine;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "GoldPickupFragment", menuName = "Etheria/Gameplay/Pickup/Fragments/GoldPickupFragment")]
    public sealed class GoldPickupFragmentSO : PickupEffectFragmentSO<IInventory>
    {
        [Tooltip("The amount of gold to add to the collector"), Min(1)]
        [field: SerializeField] public int GoldToAdd { get; private set; }

        protected override bool TryApply(IInventory inventory, PickupInstance instance)
        {
            inventory.AddGold(GoldToAdd);
            return true;
        }
    }
}

