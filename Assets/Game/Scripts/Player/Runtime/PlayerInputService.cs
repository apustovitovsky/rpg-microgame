using System;
using Game.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public class PlayerInputService :
        InputActions_Generated.IPlayerActions,
        InputActions_Generated.IUIActions,
        IActorInput,
        IPlayerInteractionInput,
        IDisposable
    {
        private readonly InputActions_Generated _input;

        public event Action InteractPerformed;
        public event Action ToggleJournalPerformed;
        public event Action UiSubmitPerformed;
        public event Action UiCancelPerformed;
        public event Action PossessPerformed;

        public PlayerInputService(
            InputActions_Generated input)
        {
            _input = input;

            _input.Enable();
            _input.Player.SetCallbacks(this);
            _input.UI.SetCallbacks(this);

            EnterGameplayInput();
        }

        public void EnterGameplayInput()
        {
            _input.UI.Disable();
            _input.Player.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            LookDelta = Vector2.zero;
            MoveComposite = Vector2.zero;
            MovementInputDetected = false;
        }

        public void EnterUiInput()
        {
            _input.Player.Disable();
            _input.UI.Enable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            LookDelta = Vector2.zero;
            MoveComposite = Vector2.zero;
            MovementInputDetected = false;
        }

        public void Dispose()
        {
            _input.Player.SetCallbacks(null);
            _input.UI.SetCallbacks(null);
            _input.Disable();
        }

        public Vector2 LookDelta { get; set; }
        public Vector2 MoveComposite { get; set; }
        public float MovementInputDuration { get; set; }
        public bool MovementInputDetected { get; set; }
        public event Action OnAimActivated;
        public event Action OnAimDeactivated;
        public event Action OnCrouchActivated;
        public event Action OnCrouchDeactivated;
        public event Action OnJumpPerformed;
        public event Action OnLockOnToggled;
        public event Action OnSprintActivated;
        public event Action OnSprintDeactivated;
        public event Action OnWalkToggled;


        public void OnLook(InputAction.CallbackContext context)
        {
            LookDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveComposite = context.ReadValue<Vector2>();
            MovementInputDetected = MoveComposite.magnitude > 0;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnJumpPerformed?.Invoke();
        }

        public void OnWalkToggle(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            OnWalkToggled?.Invoke();
        }

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
            if (!context.performed)
                return;

            InteractPerformed?.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            PossessPerformed?.Invoke();
        }

        public void OnFire(InputAction.CallbackContext context)
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

        public void OnToggleJournal(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            ToggleJournalPerformed?.Invoke();
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            UiSubmitPerformed?.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            UiCancelPerformed?.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
        }

        public void OnMenu(InputAction.CallbackContext context)
        {
        }
    }

}
