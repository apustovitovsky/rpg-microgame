using UnityEngine;


namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupConfig", menuName = "RPG/Gameplay/Pickups/Pickup Config")]
    public sealed class PickupConfigSO : ScriptableObject
    {
        [field: SerializeField] public WorldPickup PickupPrefab { get; private set; }
        [field: SerializeField] public PickupDefinitionSO PickupDefinition { get; private set; }
    }
}