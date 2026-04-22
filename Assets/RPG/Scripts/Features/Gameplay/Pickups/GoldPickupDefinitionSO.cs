using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GoldPickupDefinition", menuName = "RPG/Gameplay/Pickup/Gold Pickup Definition")]
    public sealed class GoldPickupDefinitionSO : PickupDefinitionSO
    {
        [SerializeField] private int _amount = 1;

        public override IPickupInstance CreateInstance()
        {
            return new PickupInstance(new GoldPickupEffect(_amount));
        }
    }
}
