using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GoldPickupDefinition", menuName = "RPG/Gameplay/Pickup/Gold Pickup Definition")]
    public sealed class GoldPickupDefinitionSO : PickupDefinitionSO
    {
        [SerializeField] private int _amount = 1;

        public override bool ApplyTo(IPickupCollector collector)
        {
            if (!collector.TryGet<IInventory>(out var inventory))
                return false;

            inventory.AddGold(_amount);
            return true;
        }
    }
}
