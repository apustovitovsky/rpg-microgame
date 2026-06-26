using Etheria.Game.Dialogue;
using Etheria.Game.Character;
using UnityEngine;
using System;

namespace Etheria.Npc
{
    public sealed class NpcInteractionService
    {
        private readonly IDialogueService _dialogueService;
        private readonly IPlayerCharacterProvider _playerCharacterProvider;
        private Action _completedCallback;

        public Transform Interlocutor =>
            _playerCharacterProvider.Current;

        public NpcInteractionService(
            IDialogueService dialogueService,
            IPlayerCharacterProvider playerCharacterProvider)
        {

            _dialogueService = dialogueService;
            _playerCharacterProvider = playerCharacterProvider;
        }

        public void SetCompletedCallback(Action callback)
        {
            if (_completedCallback != null)
                _dialogueService.Completed -= _completedCallback;

            _completedCallback = callback;

            if (_completedCallback != null)
                _dialogueService.Completed += _completedCallback;
        }

        public void ClearCompletedCallback()
        {
            if (_completedCallback == null)
                return;

            _dialogueService.Completed -= _completedCallback;
            _completedCallback = null;
        }

        public bool CanInteract(string npcId)
        {
            return !string.IsNullOrWhiteSpace(npcId) &&
                   _dialogueService != null &&
                   !_dialogueService.IsActive;
        }

        public bool Interact(string npcId)
        {
            if (!CanInteract(npcId))
                return false;

            return _dialogueService.TryStart(
                npcId,
                null,
                _playerCharacterProvider.Current);
        }
    }
}