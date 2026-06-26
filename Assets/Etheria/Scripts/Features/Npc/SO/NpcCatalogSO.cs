using System;
using UnityEngine;

namespace Etheria.Npc
{
    [CreateAssetMenu(
        fileName = "NpcCatalog",
        menuName = "Etheria/Npc/Npc Catalog")]
    public sealed class NpcCatalogSO : ScriptableObject
    {
        [SerializeField]
        private NpcDefinitionSO[] _definitions;

        public bool TryGet(
            string characterId,
            out NpcDefinitionSO definition)
        {
            if (!string.IsNullOrWhiteSpace(characterId) &&
                _definitions != null)
            {
                foreach (var candidate in _definitions)
                {
                    if (candidate != null &&
                        string.Equals(
                            candidate.NpcId,
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

                if (string.IsNullOrWhiteSpace(definition.NpcId))
                    throw new InvalidOperationException(
                        $"Character definition '{definition.name}' has no ID.");

                if (!ids.Add(definition.NpcId))
                    throw new InvalidOperationException(
                        $"Duplicate character ID: '{definition.NpcId}'.");

                if (definition.Prefab == null)
                    throw new InvalidOperationException(
                        $"Character '{definition.NpcId}' has no prefab.");
            }
        }
    }
}
