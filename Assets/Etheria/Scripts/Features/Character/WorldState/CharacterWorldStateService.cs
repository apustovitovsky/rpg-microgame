using System;
using System.Collections.Generic;
using Etheria.Game.Character;

namespace Etheria.Features.Character
{
    public sealed class CharacterWorldStateService :
        ICharacterWorldStateService
    {
        private readonly Dictionary<string, RuntimeState> _states = new(
            StringComparer.Ordinal);

        public event Action<string> CharacterChanged;

        private sealed class RuntimeState
        {
            public string CharacterId;
            public string LocationId;
            public bool IsAlive;
            public bool IsPresent;
            public string AnchorKey;
        }

        public CharacterWorldStateService(
            WorldCharacterSetupSO setup)
        {
            foreach (var entry in setup.Characters)
            {
                if (entry?.Character == null)
                    throw new InvalidOperationException(
                        "World character setup contains an empty character.");

                var characterId = entry.Character.Id;

                if (!_states.TryAdd(
                    characterId,
                    new RuntimeState
                    {
                        CharacterId = characterId,
                        LocationId = entry.LocationId,
                        IsAlive = entry.IsAlive,
                        IsPresent = entry.IsPresent,
                        AnchorKey = entry.AnchorKey
                    }))
                {
                    throw new InvalidOperationException(
                        $"Duplicate character ID: '{characterId}'.");
                }
            }
        }

        public IReadOnlyCollection<WorldCharacterState> States
        {
            get
            {
                var result = new List<WorldCharacterState>();

                foreach (var state in _states.Values)
                    result.Add(CreateSnapshot(state));

                return result;
            }
        }

        public bool TryGetState(
            string characterId,
            out WorldCharacterState state)
        {
            if (_states.TryGetValue(characterId, out var runtime))
            {
                state = CreateSnapshot(runtime);
                return true;
            }

            state = null;
            return false;
        }

        public bool TryMove(
            string characterId,
            string locationId)
        {
            if (!_states.TryGetValue(characterId, out var state) ||
                string.IsNullOrWhiteSpace(locationId) ||
                state.LocationId == locationId)
            {
                return false;
            }

            state.LocationId = locationId;
            CharacterChanged?.Invoke(characterId);
            return true;
        }

        public bool TrySetAlive(
            string characterId,
            bool isAlive)
        {
            if (!_states.TryGetValue(characterId, out var state) ||
                state.IsAlive == isAlive)
            {
                return false;
            }

            state.IsAlive = isAlive;
            CharacterChanged?.Invoke(characterId);
            return true;
        }

        public bool TrySetPresent(
            string characterId,
            bool isPresent)
        {
            if (!_states.TryGetValue(characterId, out var state) ||
                state.IsPresent == isPresent)
            {
                return false;
            }

            state.IsPresent = isPresent;
            CharacterChanged?.Invoke(characterId);
            return true;
        }

        private static WorldCharacterState CreateSnapshot(
            RuntimeState state)
        {
            return new WorldCharacterState(
                state.CharacterId,
                state.LocationId,
                state.AnchorKey,
                state.IsAlive,
                state.IsPresent);
        }
    }
}
