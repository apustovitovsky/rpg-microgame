using System;
using Etheria.Features.Camera;
using Etheria.Features.Targeting;
using Etheria.Game.Targeting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etheria.Features.Input
{
    public sealed class GameplayInputRouter : IGameplayInputRouter, InputSystem_Actions.IPlayerActions, IDisposable
    {
        private readonly InputSystem_Actions _input;
        private readonly ITargetingService _targetingService;
        private IActorInputHandler _playerHandler;
        private IPlayerLookInputHandler _lookHandler;
        private ICameraInputHandler _cameraHandler;

        public GameplayInputRouter(
            InputSystem_Actions input,
            ITargetingService targetingService)
        {
            _input = input;
            _targetingService = targetingService;

            _input.Enable();
            _input.Player.SetCallbacks(this);
        }

        public void SetHandler(IActorInputHandler handler)
        {
            _playerHandler = handler;
        }

        public void SetHandler(IPlayerLookInputHandler handler)
        {
            _lookHandler = handler;
        }

        public void SetHandler(ICameraInputHandler handler)
        {
            _cameraHandler = handler;
        }

        public void RemoveHandler(IActorInputHandler handler)
        {
            if (_playerHandler == handler) _playerHandler = null;
        }

        public void RemoveHandler(IPlayerLookInputHandler handler)
        {
            if (_lookHandler == handler) _lookHandler = null;
        }

        public void RemoveHandler(ICameraInputHandler handler)
        {
            if (_cameraHandler == handler) _cameraHandler = null;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _playerHandler?.HandleMove(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                _playerHandler?.HandleJump(context.ReadValueAsButton());
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                _playerHandler?.HandleFire(context.ReadValueAsButton());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _lookHandler?.HandleLook(context.ReadValue<Vector2>());
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            _cameraHandler?.HandleZoom(context.ReadValue<float>());
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_targetingService.TryAcquireFromView())
                return;

            _targetingService.ClearTarget();
        }

        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _targetingService.TryCycleTarget(-1);
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _targetingService.TryCycleTarget(1);
        }

        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnReload(InputAction.CallbackContext context) { }
        public void OnNextWeapon(InputAction.CallbackContext context) { }

        public void Dispose()
        {
            _input.Player.SetCallbacks(null);
            _input.Disable();
            _playerHandler = null;
            _lookHandler = null;
            _cameraHandler = null;
        }
    }
}
