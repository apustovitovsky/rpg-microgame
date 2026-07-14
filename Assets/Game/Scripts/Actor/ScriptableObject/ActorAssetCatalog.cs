using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "EntityAssetCatalog",
        menuName = "Game/Actor/Actor Asset Catalog")]
    public sealed class ActorAssetCatalog :
        ScriptableObject,
        IActorAssetCatalog
    {
        [SerializeField]
        private ActorDefinition[] _definitions =
            Array.Empty<ActorDefinition>();

        [NonSerialized]
        private Dictionary<string, ActorDefinition> _index;

        public bool TryGet(
            string definitionId,
            out ActorDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(definitionId))
                return false;

            RebuildIndex();

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

        private void RebuildIndex()
        {
            var index = new Dictionary<string, ActorDefinition>(
                StringComparer.Ordinal);

            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                    continue;

                var definitionId = definition.Id;

                if (string.IsNullOrWhiteSpace(definitionId))
                {
                    Debug.LogError(
                        $"Actor definition '{definition.name}' has no definition id.",
                        definition);

                    continue;
                }

                definitionId = definitionId.Trim();

                if (!index.TryAdd(definitionId, definition))
                {
                    Debug.LogError(
                        $"Duplicate actor definition id '{definitionId}' in '{name}'.",
                        this);
                }
            }

            _index = index;
        }
    }
}