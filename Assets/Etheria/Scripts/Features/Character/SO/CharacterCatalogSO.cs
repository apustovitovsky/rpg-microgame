using System;
using UnityEngine;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterCatalog",
        menuName = "Etheria/Character/Catalog")]
    public sealed class CharacterCatalogSO : ScriptableObject
    {
        [SerializeField]
        private CharacterDefinitionSO[] _definitions;

        public bool TryGet(
            string characterId,
            out CharacterDefinitionSO definition)
        {
            if (!string.IsNullOrWhiteSpace(characterId) &&
                _definitions != null)
            {
                foreach (var candidate in _definitions)
                {
                    if (candidate != null &&
                        string.Equals(
                            candidate.Id,
                            characterId,
                            StringComparison.Ordinal))
                    {
                        definition = candidate;
                        return true;
                    }
                }
            }

            definition = null;
            return false;
        }

        public void Validate()
        {
            if (_definitions == null)
                throw new InvalidOperationException(
                    $"Character catalog '{name}' has no definitions.");

            var ids = new System.Collections.Generic.HashSet<string>(
                StringComparer.Ordinal);

            foreach (var definition in _definitions)
            {
                if (definition == null)
                    throw new InvalidOperationException(
                        $"Character catalog '{name}' contains an empty definition.");

                if (string.IsNullOrWhiteSpace(definition.Id))
                    throw new InvalidOperationException(
                        $"Character definition '{definition.name}' has no ID.");

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException(
                        $"Duplicate character ID: '{definition.Id}'.");

                if (definition.Prefab == null)
                    throw new InvalidOperationException(
                        $"Character '{definition.Id}' has no prefab.");
            }
        }
    }
}
