using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupAssetCatalog",
        menuName = "Game/Pickup/Pickup Asset Catalog")]
    public sealed class PickupAssetCatalog :
        ScriptableObject,
        IPickupAssetCatalog
    {
        [SerializeField]
        private PickupDefinition[] _definitions =
            Array.Empty<PickupDefinition>();

        private Dictionary<string, PickupDefinition> _index;

        public bool TryGet(
            string id,
            out PickupDefinition definition)
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

            var index = new Dictionary<string, PickupDefinition>(
                StringComparer.Ordinal);

            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                {
                    throw new InvalidOperationException(
                        $"{name} contains an empty pickup definition at index {i}.");
                }

                if (string.IsNullOrWhiteSpace(definition.Id))
                {
                    throw new InvalidOperationException(
                        $"Pickup definition '{definition.name}' has no id.");
                }

                if (!index.TryAdd(definition.Id, definition))
                {
                    throw new InvalidOperationException(
                        $"Duplicate pickup id '{definition.Id}' in '{name}'.");
                }
            }

            _index = index;
        }
    }
}