
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Gameplay
{
    public sealed class TargetingController
    {
        private readonly ITargetingService _targetingService;

        public TargetingController(ITargetingService targetingService)
        {
            _targetingService = targetingService;
        }

        public void HandleAim(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_targetingService.TryAcquireFromView())
                return;

            _targetingService.ClearTarget();
        }
    }
}
