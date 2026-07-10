using Game.Inventory;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupDefinition",
        menuName = "Game/Pickup/Pickup Definition")]
    public sealed class PickupDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        [field: SerializeField]
        public ItemDefinition Item { get; private set; }

        [field: SerializeField, Min(1)]
        public int Amount { get; private set; } = 1;

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_displayName))
                    return _displayName.Trim();

                return Item != null
                    ? Item.DisplayName
                    : name;
            }
        }
    }
}