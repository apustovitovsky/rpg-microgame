using System;
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

        public CharacterInteractionService(
            IPlayerInputSource inputSource,
            ITargetProvider targetProvider)
        {
            _inputSource = inputSource;
            _targetProvider = targetProvider;
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