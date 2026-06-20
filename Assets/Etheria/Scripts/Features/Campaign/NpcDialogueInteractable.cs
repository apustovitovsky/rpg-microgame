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
        [SerializeField] private string _startNode = "GuardGreeting";

        private IDialogueService _dialogueService;
        private IDialogueParticipant _participant;

        private IPlayerCharacterProvider _playerCharacterProvider;

        public bool CanInteract =>
            isActiveAndEnabled &&
            _dialogueService != null &&
            !_dialogueService.IsRunning;

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
        }

        public void Interact()
        {
            if (!CanInteract)
                return;

            _dialogueService.TryStart(
            _startNode,
            _participant,
            _playerCharacterProvider.Current);
        }
    }
}