using System;
using Etheria.Game.Input;
using Etheria.Game.Interaction;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerInteractionService : IStartable, IDisposable
    {
        private readonly IPlayerInputSource _inputSource;
        private readonly IPlayerTargetProvider _targetProvider;

        public PlayerInteractionService(
            IPlayerInputSource inputSource,
            IPlayerTargetProvider targetProvider)
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
            var target = _targetProvider.CurrentTarget;
            if (target == null)
                return;

            var interactable = target.GetComponentInParent<IInteractable>();
            if (interactable == null || !interactable.CanInteract)
                return;

            interactable.Interact();
        }
    }
}