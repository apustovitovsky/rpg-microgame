using System;
using Etheria.Game.Character;
using VContainer.Unity;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class CharacterWorldCommandHandler :
        IStartable,
        IDisposable
    {
        private readonly DialogueRunner _runner;
        private readonly ICharacterWorldStateService _worldState;

        public CharacterWorldCommandHandler(
            DialogueRunner runner,
            ICharacterWorldStateService worldState)
        {
            _runner = runner;
            _worldState = worldState;
        }

        public void Start()
        {
            _runner.AddCommandHandler<string, string>(
                "move_character",
                MoveCharacter);

            _runner.AddCommandHandler<string, bool>(
                "set_character_alive",
                SetCharacterAlive);

            _runner.AddCommandHandler<string, bool>(
                "set_character_present",
                SetCharacterPresent);

            _runner.AddFunction<string, bool>(
                "character_is_alive",
                IsCharacterAlive);

            _runner.AddFunction<string, bool>(
                "character_is_present",
                IsCharacterPresent);

            _runner.AddFunction<string, string>(
                "character_location",
                GetCharacterLocation);
        }

        public void Dispose()
        {
            _runner.RemoveCommandHandler("move_character");
            _runner.RemoveCommandHandler("set_character_alive");

            _runner.RemoveFunction("character_is_alive");
            _runner.RemoveFunction("character_location");

            _runner.RemoveCommandHandler("set_character_present");
            _runner.RemoveFunction("character_is_present");
        }

        private void MoveCharacter(
            string characterId,
            string locationId)
        {
            _worldState.TryMove(characterId, locationId);
        }

        private void SetCharacterAlive(
            string characterId,
            bool isAlive)
        {
            _worldState.TrySetAlive(characterId, isAlive);
        }

        private bool IsCharacterAlive(string characterId)
        {
            return _worldState.TryGetState(
                       characterId,
                       out var state) &&
                   state.IsAlive;
        }

        private void SetCharacterPresent(
            string characterId,
            bool isPresent)
        {
            _worldState.TrySetPresent(characterId, isPresent);
        }

        private bool IsCharacterPresent(string characterId)
        {
            return _worldState.TryGetState(
                       characterId,
                       out var state) &&
                   state.IsPresent;
        }

        private string GetCharacterLocation(string characterId)
        {
            return _worldState.TryGetState(
                characterId,
                out var state)
                    ? state.LocationId
                    : string.Empty;
        }
    }
}