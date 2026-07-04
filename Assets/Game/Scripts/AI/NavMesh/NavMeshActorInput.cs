using System;
using Game.Actor;
using Game.Input;
using UnityEngine;
using VContainer;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavMeshActorInput :
        MonoBehaviour,
        IActorInput
    {
        private INavMeshPlanner _planner;
        [SerializeField] private ActorLookController _look;

        [SerializeField] private float _moveInputDeadZone = 0.05f;

        [SerializeField] private float _facingCompleteAngle = 3f;

        private Vector3 _facingDirection;
        private bool _hasFacingDirection;
        private bool _aimHeld;

        public bool HasDestination =>
            _planner != null && _planner.HasDestination;

        public bool HasArrived =>
            _planner != null && _planner.HasArrived;

        public bool IsNavigating =>
            _planner != null && _planner.IsNavigating;

        [Inject]
        public void Construct(INavMeshPlanner planner)
        {
            _planner = planner;
        }

        public bool IsFacingComplete
        {
            get
            {
                if (!_hasFacingDirection)
                    return true;

                Vector3 current = transform.forward;
                current.y = 0f;

                Vector3 target = _facingDirection;
                target.y = 0f;

                if (current.sqrMagnitude <= 0.0001f ||
                    target.sqrMagnitude <= 0.0001f)
                {
                    return true;
                }

                float angle = Vector3.Angle(
                    current.normalized,
                    target.normalized);

                return angle <= _facingCompleteAngle;
            }
        }

        public void SetFacing(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
                return;

            _facingDirection = direction.normalized;
            _hasFacingDirection = true;

            if (_look != null)
                _look.SetWorldDirection(_facingDirection);

            SetAim(true);
        }

        public void ClearFacing()
        {
            _hasFacingDirection = false;
            SetAim(false);
        }

        public void SetAim(bool enabled)
        {
            if (_aimHeld == enabled)
                return;

            _aimHeld = enabled;

            if (_aimHeld)
                OnAimActivated?.Invoke();
            else
                OnAimDeactivated?.Invoke();
        }

        public Vector2 LookDelta => Vector2.zero;

        public Vector2 MoveComposite { get; private set; }

        public float MovementInputDuration { get; set; }

        public bool MovementInputDetected =>
            MoveComposite.sqrMagnitude > _moveInputDeadZone * _moveInputDeadZone;

        public event Action OnAimActivated;
        public event Action OnAimDeactivated;
        public event Action OnCrouchActivated;
        public event Action OnCrouchDeactivated;
        public event Action OnJumpPerformed;
        public event Action OnLockOnToggled;
        public event Action OnSprintActivated;
        public event Action OnSprintDeactivated;
        public event Action OnWalkToggled;

        private void Update()
        {
            MoveComposite = Vector2.zero;

            if (_planner == null || !_planner.IsNavigating)
                return;

            Vector3 desiredDirection = _planner.DesiredWorldDirection;

            if (desiredDirection.sqrMagnitude <= 0.0001f)
                return;

            MoveComposite = ToLookRelativeInput(desiredDirection);
            MovementInputDuration += Time.deltaTime;
        }

        public void Stop()
        {
            MoveComposite = Vector2.zero;
            MovementInputDuration = 0f;
        }

        private Vector2 ToLookRelativeInput(Vector3 worldDirection)
        {
            Vector3 forward = _look != null
                ? _look.ForwardFlatNormalized
                : transform.forward;

            Vector3 right = _look != null
                ? _look.RightFlatNormalized
                : transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward = forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : transform.forward;

            right = right.sqrMagnitude > 0.0001f
                ? right.normalized
                : transform.right;

            var input = new Vector2(
                Vector3.Dot(worldDirection, right),
                Vector3.Dot(worldDirection, forward));

            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}