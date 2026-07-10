using UnityEngine;

namespace Game.Inventory
{
    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Game/Inventory/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [field: SerializeField, Min(1)]
        public int MaxStackSize { get; private set; } = 1;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName)
                ? name
                : _displayName.Trim();
    }
}