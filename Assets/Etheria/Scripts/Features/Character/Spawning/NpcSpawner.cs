using System;
using System.Collections.Generic;
using Etheria.Core.DI;
using Etheria.Game.Character;
using Etheria.Game.World;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class NpcSpawner : IStartable
    {
        private readonly IWorldLocationRegistry _locations;
        private readonly ICharacterWorldStateService _worldState;
        private readonly WorldCharacterSetupSO _setup;
        private readonly IObjectResolver _resolver;
        private readonly ScopeHierarchy _scopeHierarchy;

        public NpcSpawner(
            IWorldLocationRegistry locations,
            ICharacterWorldStateService worldState,
            WorldCharacterSetupSO setup,
            IObjectResolver resolver,
            ScopeHierarchy scopeHierarchy)
        {
            _locations = locations;
            _worldState = worldState;
            _setup = setup;
            _resolver = resolver;
            _scopeHierarchy = scopeHierarchy;
        }

        public void Start()
        {
            var definitionsById = CreateDefinitionMap();

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

                if (!definitionsById.TryGetValue(
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
                    _scopeHierarchy.ContentRoot);
            }
        }

        private Dictionary<string, CharacterDefinitionSO>
            CreateDefinitionMap()
        {
            var result =
                new Dictionary<string, CharacterDefinitionSO>(
                    StringComparer.Ordinal);

            foreach (var entry in _setup.Characters)
            {
                var definition = entry.Character;

                if (definition == null)
                    continue;

                if (!result.TryAdd(definition.Id, definition))
                {
                    throw new InvalidOperationException(
                        $"Duplicate character definition: '{definition.Id}'.");
                }
            }

            return result;
        }
    }
}
