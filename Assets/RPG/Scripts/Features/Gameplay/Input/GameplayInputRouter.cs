using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Gameplay
{
    public sealed class GameplayInputRouter : IGameplayInputRouter, InputSystem_Actions.IPlayerActions, IDisposable
    {
        private readonly InputSystem_Actions _input;
        private readonly TargetingController _targetingController;
        private IActorInputHandler _playerHandler;
        private ICameraInputHandler _cameraHandler;

        public GameplayInputRouter(
            InputSystem_Actions input,
            TargetingController targetingController)
        {
            _input = input;
            _targetingController = targetingController;

            _input.Enable();
            _input.Player.SetCallbacks(this);
        }

        public void SetHandler(IActorInputHandler handler)
        {
            _playerHandler = handler;
        }

        public void SetHandler(ICameraInputHandler handler)
        {
            _cameraHandler = handler;
        }

        public void RemoveHandler(IActorInputHandler handler)
        {
            if (_playerHandler == handler) _playerHandler = null;
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
            _cameraHandler?.HandleLook(context.ReadValue<Vector2>());
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            _cameraHandler?.HandleZoom(context.ReadValue<float>());
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            _targetingController.HandleAim(context);
        }

        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnReload(InputAction.CallbackContext context) { }
        public void OnNextWeapon(InputAction.CallbackContext context) { }

        public void Dispose()
        {
            _input.Player.SetCallbacks(null);
            _input.Disable();
            _playerHandler = null;
            _cameraHandler = null;
        }
    }
}
