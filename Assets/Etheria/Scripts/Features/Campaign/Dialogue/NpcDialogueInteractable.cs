using Etheria.Game.Dialogue;
using Etheria.Game.Interaction;
using UnityEngine;
using VContainer;
using Etheria.Game.Character;

namespace Etheria.Features.Campaign
{
    public sealed class NpcDialogueInteractable :
        MonoBehaviour,
        IInteractable
    {
        private ICharacterIdentity _identity;
        private IDialogueService _dialogueService;
        private IDialogueParticipant _participant;

        private IPlayerCharacterProvider _playerCharacterProvider;

        public bool CanInteract =>
            _identity != null &&
            !string.IsNullOrWhiteSpace(_identity.CharacterId) &&
            isActiveAndEnabled &&
            _dialogueService != null &&
            !_dialogueService.IsActive;

        [Inject]
        public void Construct(
            IDialogueService dialogueService,
            IPlayerCharacterProvider playerCharacterProvider)
        {
            _dialogueService = dialogueService;
            _playerCharacterProvider = playerCharacterProvider;
        }

        private void Awake()
        {
            _participant = GetComponentInParent<IDialogueParticipant>();
            _identity = GetComponentInParent<ICharacterIdentity>();
        }

        public void Interact()
        {
            if (!CanInteract)
                return;

            _dialogueService.TryStart(
                _identity.CharacterId,
                _participant,
                _playerCharacterProvider.Current);
        }
    }
}