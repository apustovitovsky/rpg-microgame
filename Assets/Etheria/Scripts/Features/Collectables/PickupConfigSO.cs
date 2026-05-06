using UnityEngine;


namespace Etheria.Features.Collectables
{
    [CreateAssetMenu(fileName = "PickupConfig", menuName = "Etheria/Gameplay/Pickups/Pickup Config")]
    public sealed class PickupConfigSO : ScriptableObject
    {
        [field: SerializeField] public WorldPickup PickupPrefab { get; private set; }
        [field: SerializeField] public PickupDefinitionSO PickupDefinition { get; private set; }
    }
}
