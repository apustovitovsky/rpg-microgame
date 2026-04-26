using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core.VContainer
{
    [CreateAssetMenu(
        fileName = "SceneCatalog",
        menuName = "RPG/Core/Scene Loading/Scene Catalog")]
    public sealed class SceneCatalogSO : ScriptableObject
    {
        [SerializeField] private SceneCatalogEntry[] _sceneDefinitions;

        public SceneDefinitionSO[] Get(string name)
        {
            if (TryGet(name, out var definitions))
                return definitions;

            throw new InvalidOperationException(
                $"Scene definition with name '{name}' was not found in catalog.");
        }

        public bool TryGet(string name, out SceneDefinitionSO[] definitions)
        {
            definitions = null;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (_sceneDefinitions == null)
                return false;

            foreach (var entry in _sceneDefinitions)
            {
                if (entry == null)
                    continue;

                if (!string.Equals(entry.DisplayName, name, StringComparison.Ordinal))
                    continue;

                definitions = entry.SceneDefinitions;
                return definitions != null;
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
            if (_sceneDefinitions == null)
                return;

            var usedNames = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < _sceneDefinitions.Length; i++)
            {
                var entry = _sceneDefinitions[i];

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