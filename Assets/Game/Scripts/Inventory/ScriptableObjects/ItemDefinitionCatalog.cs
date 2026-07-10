using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Inventory
{
    [CreateAssetMenu(
        fileName = "ItemDefinitionCatalog",
        menuName = "Game/Inventory/Item Definition Catalog")]
    public sealed class ItemDefinitionCatalog :
        ScriptableObject,
        IItemDefinitionCatalog
    {
        [SerializeField]
        private ItemDefinition[] _definitions =
            Array.Empty<ItemDefinition>();

        private Dictionary<string, ItemDefinition> _index;

        public bool TryGet(
            string definitionId,
            out ItemDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(definitionId))
                return false;

            EnsureIndex();

            return _index.TryGetValue(
                definitionId.Trim(),
                out definition);
        }

        private void OnEnable()
        {
            _index = null;
        }

        private void OnValidate()
        {
            _index = null;
        }

        private void EnsureIndex()
        {
            if (_index != null)
                return;

            var index = new Dictionary<string, ItemDefinition>(
                StringComparer.Ordinal);

            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                {
                    throw new InvalidOperationException(
                        $"{name} contains an empty item definition at index {i}.");
                }

                var definitionId = definition.DefinitionId;

                if (string.IsNullOrWhiteSpace(definitionId))
                {
                    throw new InvalidOperationException(
                        $"Item definition '{definition.name}' has no definition id.");
                }

                if (!index.TryAdd(definitionId, definition))
                {
                    throw new InvalidOperationException(
                        $"Duplicate item definition id '{definitionId}' in '{name}'.");
                }
            }

            _index = index;
        }
    }
}