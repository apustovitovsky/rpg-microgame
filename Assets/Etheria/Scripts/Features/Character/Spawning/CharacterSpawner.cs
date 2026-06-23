using System;
using Etheria.Core.DI;
using Etheria.Game.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterSpawner
    {
        private readonly CharacterCatalogSO _catalog;
        private readonly IObjectResolver _resolver;
        private readonly ScopeContentRoot _scopeContentRoot;

        public CharacterSpawner(
            CharacterCatalogSO catalog,
            IObjectResolver resolver,
            ScopeContentRoot scopeContentRoot)
        {
            _catalog = catalog;
            _resolver = resolver;
            _scopeContentRoot = scopeContentRoot;
        }

        public GameObject Spawn(
            string characterId,
            WorldLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!_catalog.TryGet(characterId, out var definition))
            {
                throw new InvalidOperationException(
                    $"Character definition '{characterId}' was not found.");
            }

            if (definition.Prefab == null)
            {
                throw new InvalidOperationException(
                    $"Character '{characterId}' has no prefab.");
            }

            return _resolver.Instantiate(
                definition.Prefab,
                location.Transform.position,
                location.Transform.rotation,
                _scopeContentRoot.Transform);
        }
    }
}
