using System;
using System.Collections.Generic;
using Etheria.Core.DI;
using Etheria.Game.Character;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class NpcSpawner : IStartable
    {
        private readonly IReadOnlyList<NpcSpawnPoint> _spawnPoints;
        private readonly ICharacterWorldStateService _worldState;
        private readonly WorldCharacterSetupSO _setup;
        private readonly IObjectResolver _resolver;
        private readonly ScopeHierarchy _scopeHierarchy;

        public NpcSpawner(
            IReadOnlyList<NpcSpawnPoint> spawnPoints,
            ICharacterWorldStateService worldState,
            WorldCharacterSetupSO setup,
            IObjectResolver resolver,
            ScopeHierarchy scopeHierarchy)
        {
            _spawnPoints = spawnPoints;
            _worldState = worldState;
            _setup = setup;
            _resolver = resolver;
            _scopeHierarchy = scopeHierarchy;
        }

        public void Start()
        {
            var pointsById = CreateSpawnPointMap();
            var definitionsById = CreateDefinitionMap();

            foreach (var state in _worldState.States)
            {
                if (!state.IsAlive ||
                    string.IsNullOrWhiteSpace(state.SpawnPointId))
                {
                    continue;
                }

                if (!pointsById.TryGetValue(
                        state.SpawnPointId,
                        out var spawnPoint))
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
                    spawnPoint.Transform.position,
                    spawnPoint.Transform.rotation,
                    _scopeHierarchy.ContentRoot);
            }
        }

        private Dictionary<string, NpcSpawnPoint> CreateSpawnPointMap()
        {
            var result = new Dictionary<string, NpcSpawnPoint>(
                StringComparer.Ordinal);

            foreach (var point in _spawnPoints)
            {
                if (string.IsNullOrWhiteSpace(point.Id))
                {
                    throw new InvalidOperationException(
                        $"NPC spawn point '{point.name}' has no ID.");
                }

                if (!result.TryAdd(point.Id, point))
                {
                    throw new InvalidOperationException(
                        $"Duplicate NPC spawn point ID: '{point.Id}'.");
                }
            }

            return result;
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