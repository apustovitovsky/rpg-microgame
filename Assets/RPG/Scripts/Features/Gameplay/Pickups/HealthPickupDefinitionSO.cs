using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "HealthPickupDefinition", menuName = "RPG/Gameplay/Pickup/Health Pickup Definition")]
    public sealed class HealthPickupDefinitionSO : PickupDefinitionSO
    {
        [SerializeField] private int _amount = 10;

        public override IPickupInstance CreateInstance()
        {
            return new PickupInstance(new HealthPickupEffect(_amount));
        }
    }
}
