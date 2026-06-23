using System;
using Etheria.Core.DI;
using Etheria.Game.Character;
using Etheria.Game.World;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterSpawner : IStartable
    {
        private readonly IWorldLocationRegistry _locations;
        private readonly ICharacterWorldStateService _worldState;
        private readonly CharacterCatalogSO _catalog;
        private readonly IObjectResolver _resolver;
        private readonly ScopeContentRoot _scopeContentRoot;

        public CharacterSpawner(
            IWorldLocationRegistry locations,
            ICharacterWorldStateService worldState,
            CharacterCatalogSO catalog,
            IObjectResolver resolver,
            ScopeContentRoot scopeContentRoot)
        {
            _locations = locations;
            _worldState = worldState;
            _catalog = catalog;
            _resolver = resolver;
            _scopeContentRoot = scopeContentRoot;
        }

        public void Start()
        {
            foreach (var state in _worldState.States)
            {
                if (!state.IsAlive ||
                    string.IsNullOrWhiteSpace(state.LocationId))
                {
                    continue;
                }

                if (!_locations.TryGet(
                        state.LocationId,
                        out var location))
                {
                    continue;
                }

                if (!_catalog.TryGet(
                        state.CharacterId,
                        out var definition))
                {
                    throw new InvalidOperationException(
                        $"Character definition '{state.CharacterId}' was not found.");
                }

                if (definition.Prefab == null)
                {
                    throw new InvalidOperationException(
                        $"Character '{state.CharacterId}' has no prefab.");
                }

                _resolver.Instantiate(
                    definition.Prefab,
                    location.Transform.position,
                    location.Transform.rotation,
                    _scopeContentRoot.Transform);
            }
        }
    }
}
