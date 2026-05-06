using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Core.DI
{
    [CreateAssetMenu(
        fileName = "SceneCatalog",
        menuName = "Etheria/Core/Scene Loading/Scene Catalog")]
    public sealed class SceneCatalogSO : ScriptableObject
    {
        [field: SerializeField] public SceneCatalogEntry[] Entries { get; private set; }

        public SceneCatalogEntry Get(string name)
        {
            if (TryGet(name, out var entry))
                return entry;

            throw new InvalidOperationException(
                $"Scene stack with name '{name}' was not found in catalog.");
        }

        public bool TryGet(string name, out SceneCatalogEntry entry)
        {
            entry = null;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (Entries == null)
                return false;

            foreach (var candidate in Entries)
            {
                if (candidate == null)
                    continue;

                if (!string.Equals(candidate.DisplayName, name, StringComparison.Ordinal))
                    continue;

                entry = candidate;
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateDuplicateDisplayNames();
        }

        private void ValidateDuplicateDisplayNames()
        {
            if (Entries == null)
                return;

            var usedNames = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];

                if (entry == null)
                    continue;

                var displayName = entry.DisplayName;

                if (string.IsNullOrWhiteSpace(displayName))
                    continue;

                if (usedNames.Add(displayName))
                    continue;

                Debug.LogWarning(
                    $"Scene catalog '{name}' contains duplicate scene definition name '{displayName}' at index {i}. " +
                    "Only the first matching entry will be used.",
                    this);
            }
        }
#endif
    }
}
