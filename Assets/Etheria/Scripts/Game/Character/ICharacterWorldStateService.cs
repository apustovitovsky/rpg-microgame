using System;
using System.Collections.Generic;

namespace Etheria.Game.Character
{
    public interface ICharacterWorldStateService
    {
        IReadOnlyCollection<WorldCharacterState> States { get; }

        bool TryGetState(
            string characterId,
            out WorldCharacterState state);

        bool TryMove(
            string characterId,
            string locationId);

        bool TrySetAlive(
            string characterId,
            bool isAlive);

        bool TrySetPresent(
            string characterId,
            bool isPresent);

        event Action<string> CharacterChanged;
    }
}
