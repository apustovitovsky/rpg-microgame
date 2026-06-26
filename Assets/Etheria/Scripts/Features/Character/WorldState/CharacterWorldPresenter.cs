using System;
using System.Collections.Generic;
using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterWorldPresenter :
        IStartable,
        IDisposable,
        ICharacterInstanceRegistry
    {
        private readonly ICharacterWorldStateService _worldState;
        private readonly IWorldLocationRegistry _locations;
        private readonly INpcSpawner _spawner;

        private readonly Dictionary<string, GameObject> _instances =
            new(StringComparer.Ordinal);

        public CharacterWorldPresenter(
            ICharacterWorldStateService worldState,
            IWorldLocationRegistry locations,
            INpcSpawner spawner)
        {
            _worldState = worldState;
            _locations = locations;
            _spawner = spawner;
        }

        public void Start()
        {
            _worldState.CharacterChanged += OnCharacterChanged;

            foreach (var state in _worldState.States)
                Synchronize(state);
        }

        public bool TryGetInstance(
            string characterId,
            out GameObject instance)
        {
            return _instances.TryGetValue(characterId, out instance) &&
                   instance != null;
        }

        public void Dispose()
        {
            _worldState.CharacterChanged -= OnCharacterChanged;
            _instances.Clear();
        }

        private void OnCharacterChanged(string characterId)
        {
            if (_worldState.TryGetState(characterId, out var state))
                Synchronize(state);
            else
                Despawn(characterId);
        }

        private void Synchronize(WorldCharacterState state)
        {
            if (!state.IsAlive ||
                !state.IsPresent ||
                string.IsNullOrWhiteSpace(state.LocationId) ||
                !_locations.TryGet(state.LocationId, out var location))
            {
                Despawn(state.CharacterId);
                return;
            }

            if (_instances.TryGetValue(
                    state.CharacterId,
                    out var instance) &&
                instance != null)
            {
                return;
            }

            _instances[state.CharacterId] =
                _spawner.Spawn(state.CharacterId, location.transform);
        }

        private void Despawn(string characterId)
        {
            if (!_instances.Remove(characterId, out var instance) ||
                instance == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(instance);
        }
    }
}
