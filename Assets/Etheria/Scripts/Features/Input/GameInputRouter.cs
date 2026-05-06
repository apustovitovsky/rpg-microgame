using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etheria.Features.Input
{
    public class GameInputRouter : IGameInputRouter, InputSystem_Actions.IPlayerActions, IDisposable
    {
        private readonly InputSystem_Actions _input;

        public GameInputRouter(
            InputSystem_Actions input)
        {
            _input = input;

            _input.Enable();
            _input.Player.SetCallbacks(this);
        }

        public void Dispose()
        {
            _input.Player.SetCallbacks(null);
            _input.Disable();
        }

        public Vector2 MouseDelta { get; set; }
        public Vector2 MoveComposite { get; set; }
        public float MovementInputDuration { get; set; }
        public bool MovementInputDetected { get; set; }
        public Action OnAimActivated { get; set; }
        public Action OnAimDeactivated { get; set; }
        public Action OnCrouchActivated { get; set; }
        public Action OnCrouchDeactivated { get; set; }
        public Action OnJumpPerformed { get; set; }
        public Action OnLockOnToggled { get; set; }
        public Action OnSprintActivated { get; set; }
        public Action OnSprintDeactivated { get; set; }
        public Action OnWalkToggled { get; set; }


        /// <summary>
        ///     Defines the action to perform when the OnLook callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnLook(InputAction.CallbackContext context)
        {
            MouseDelta = context.ReadValue<Vector2>();
        }

        /// <summary>
        ///     Defines the action to perform when the OnMove callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveComposite = context.ReadValue<Vector2>();
            MovementInputDetected = MoveComposite.magnitude > 0;
        }

        /// <summary>
        ///     Defines the action to perform when the OnJump callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnJumpPerformed?.Invoke();
        }

        /// <summary>
        ///     Defines the action to perform when the OnToggleWalk callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnWalkToggled?.Invoke();
        }

        /// <summary>
        ///     Defines the action to perform when the OnSprint callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnSprintActivated?.Invoke();
            }
            else if (context.canceled)
            {
                OnSprintDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     Defines the action to perform when the OnCrouch callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnCrouchActivated?.Invoke();
            }
            else if (context.canceled)
            {
                OnCrouchDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     Defines the action to perform when the OnAim callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAimActivated?.Invoke();
            }

            if (context.canceled)
            {
                OnAimDeactivated?.Invoke();
            }
        }

        /// <summary>
        ///     Defines the action to perform when the OnLockOn callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnLockOnToggled?.Invoke();
            OnSprintDeactivated?.Invoke();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {

        }

        public void OnInteract(InputAction.CallbackContext context)
        {

        }

        public void OnPrevious(InputAction.CallbackContext context)
        {

        }

        public void OnNext(InputAction.CallbackContext context)
        {

        }

        public void OnFire(InputAction.CallbackContext context)
        {

        }

        public void OnWalkToggle(InputAction.CallbackContext context)
        {

        }

        public void OnReload(InputAction.CallbackContext context)
        {

        }

        public void OnNextWeapon(InputAction.CallbackContext context)
        {

        }

        public void OnZoom(InputAction.CallbackContext context)
        {

        }

    }

    // public sealed class PlayerInputRouter : IPlayerInputRouter, InputSystem_Actions.IPlayerActions, IDisposable
    // {
    //     private readonly InputSystem_Actions _playerInput;
    //     private readonly PlayerInputState _playerInputState;

    //     public PlayerInputRouter(
    //         InputSystem_Actions playerInput,
    //         PlayerInputState playerInputState)
    //     {
    //         _playerInput = playerInput;
    //         _playerInputState = playerInputState;

    //         _playerInput.Enable();
    //         _playerInput.Player.SetCallbacks(this);
    //     }

    //     public void OnMove(InputAction.CallbackContext context)
    //     {
    //         _playerInputState._moveComposite = context.ReadValue<Vector2>();
    //         _playerInputState._movementInputDetected = _playerInputState._moveComposite.sqrMagnitude > 0.0001f;
    //     }

    //     public void OnJump(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onJumpPerformed?.Invoke();
    //     }

    //     public void OnFire(InputAction.CallbackContext context)
    //     {

    //     }

    //     public void OnLook(InputAction.CallbackContext context)
    //     {
    //         _playerInputState._mouseDelta = context.ReadValue<Vector2>();
    //     }

    //     public void OnZoom(InputAction.CallbackContext context)
    //     {

    //     }

    //     public void OnAim(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onAimActivated?.Invoke();
    //         else if (context.canceled)
    //             _playerInputState.onAimDeactivated?.Invoke();
    //     }

    //     public void OnAttack(InputAction.CallbackContext context) { }
    //     public void OnInteract(InputAction.CallbackContext context) { }
    //     public void OnLockOn(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onLockOnToggled?.Invoke();
    //     }

    //     public void OnWalkToggle(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onWalkToggled?.Invoke();
    //     }

    //     public void OnCrouch(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onCrouchActivated?.Invoke();
    //         else if (context.canceled)
    //             _playerInputState.onCrouchDeactivated?.Invoke();
    //     }
    //     public void OnPrevious(InputAction.CallbackContext context)
    //     {

    //     }

    //     public void OnNext(InputAction.CallbackContext context)
    //     {

    //     }

    //     public void OnSprint(InputAction.CallbackContext context)
    //     {
    //         if (context.performed)
    //             _playerInputState.onSprintActivated?.Invoke();
    //         else if (context.canceled)
    //             _playerInputState.onSprintDeactivated?.Invoke();
    //     }
    //     public void OnReload(InputAction.CallbackContext context) { }
    //     public void OnNextWeapon(InputAction.CallbackContext context) { }

    //     public void Dispose()
    //     {
    //         _playerInput.Player.SetCallbacks(null);
    //         _playerInput.Disable();
    //     }
    // }
}
