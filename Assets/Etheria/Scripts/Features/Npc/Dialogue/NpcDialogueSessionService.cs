using System;
using Etheria.Game.Character;
using Etheria.Game.Dialogue;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcDialogueSessionService
    {
        private readonly IDialogueService _dialogueService;
        private readonly IPlayerCharacterProvider _playerCharacterProvider;
        private Action _completedCallback;

        public Transform Interlocutor =>
            _playerCharacterProvider.Current;

        public NpcDialogueSessionService(
            IDialogueService dialogueService,
            IPlayerCharacterProvider playerCharacterProvider)
        {
            _dialogueService = dialogueService;
            _playerCharacterProvider = playerCharacterProvider;
        }

        public bool CanStartDialogue(string npcId)
        {
            return !string.IsNullOrWhiteSpace(npcId) &&
                   _dialogueService != null &&
                   !_dialogueService.IsActive;
        }

        public bool TryStartDialogue(string npcId)
        {
            if (!CanStartDialogue(npcId))
                return false;

            return _dialogueService.TryStart(
                npcId,
                null,
                _playerCharacterProvider.Current);
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
    }
}