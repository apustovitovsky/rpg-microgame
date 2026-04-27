
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etheria.Gameplay
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
            {
                // Debug.Log("TargetingController: target acquired from input.");
                return;
            }

            // Debug.Log("TargetingController: no target found, clearing current target.");
            _targetingService.ClearTarget();
        }
    }
}
