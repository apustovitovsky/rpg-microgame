using System;
using Etheria.Game.Dialogue;
using Etheria.Game.Input;
using Etheria.Game.Interaction;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterInteractionService : IStartable, IDisposable
    {
        private readonly IPlayerInputSource _inputSource;
        private readonly ITargetProvider _targetProvider;

        private readonly IDialogueService _dialogueService;

        public CharacterInteractionService(
            IPlayerInputSource inputSource,
            ITargetProvider targetProvider,
            IDialogueService dialogueService)
        {
            _inputSource = inputSource;
            _targetProvider = targetProvider;
            _dialogueService = dialogueService;
        }

        public void Start()
        {
            _inputSource.InteractPerformed += OnInteractPerformed;
        }

        public void Dispose()
        {
            _inputSource.InteractPerformed -= OnInteractPerformed;
        }

        private void OnInteractPerformed()
        {
            if (_dialogueService.IsActive)
                return;
                
            ITargetCandidate target = _targetProvider.CurrentTarget;

            if (target?.Root == null)
                return;

            var interactable =
                target.Root.GetComponentInParent<IInteractable>();

            if (interactable == null || !interactable.CanInteract)
                return;

            interactable.Interact();
        }
    }
}