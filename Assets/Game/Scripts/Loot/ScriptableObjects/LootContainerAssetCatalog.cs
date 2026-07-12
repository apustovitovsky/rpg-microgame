using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loot
{
    [CreateAssetMenu(
        fileName = "LootContainerAssetCatalog",
        menuName = "Game/Loot/Loot Container Asset Catalog")]
    public sealed class LootContainerAssetCatalog :
        ScriptableObject,
        ILootContainerAssetCatalog
    {
        [SerializeField]
        private LootContainerDefinition[] _definitions =
            Array.Empty<LootContainerDefinition>();

        private Dictionary<string, LootContainerDefinition> _index;

        public bool TryGet(
            string id,
            out LootContainerDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            EnsureIndex();

            return _index.TryGetValue(
                id.Trim(),
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

            var index = new Dictionary<string, LootContainerDefinition>(
                StringComparer.Ordinal);

            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                {
                    throw new InvalidOperationException(
                        $"{name} contains an empty loot container definition " +
                        $"at index {i}.");
                }

                if (string.IsNullOrWhiteSpace(definition.Id))
                {
                    throw new InvalidOperationException(
                        $"Loot container definition '{definition.name}' has no id.");
                }

                if (!index.TryAdd(definition.Id, definition))
                {
                    throw new InvalidOperationException(
                        $"Duplicate loot container id '{definition.Id}' in '{name}'.");
                }
            }

            _index = index;
        }
    }
}