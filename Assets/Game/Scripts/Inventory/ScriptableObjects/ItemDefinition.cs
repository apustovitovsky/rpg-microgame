using Game.Core;
using UnityEngine;

namespace Game.Inventory
{
    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Game/Inventory/Item Definition")]
    public sealed class ItemDefinition : Definition
    {
        [SerializeField] private string _definitionId;

        [field: SerializeField, Min(1)]
        public int MaxStackSize { get; private set; } = 1;

        public string DefinitionId => _definitionId;

        protected override void OnValidate()
        {
            base.OnValidate();

            _definitionId = _definitionId?.Trim();

            if (MaxStackSize < 1)
                MaxStackSize = 1;
        }
    }
}