using System;
using Game.Actor;
using Game.Input;
using UnityEngine;
using UnityEngine.AI;

namespace Game.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NavMeshActorInput :
        MonoBehaviour,
        IActorInput
    {
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private ActorLookController _look;

        [SerializeField] private float _moveInputDeadZone = 0.05f;
        [SerializeField] private float _destinationRepathDistance = 0.25f;

        [SerializeField] private float _facingCompleteAngle = 3f;

        private Vector3 _facingDirection;
        private bool _hasFacingDirection;
        private bool _aimHeld;

        public bool HasDestination => _hasDestination;

        public bool HasArrived => CanNavigate() && CheckArrived();

        public bool IsNavigating => CanNavigate() && !CheckArrived();

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

        private bool CheckArrived()
        {
            if (_agent.pathPending)
                return false;

            float stoppingDistance = Mathf.Max(
                _agent.stoppingDistance,
                0.05f);

            return !_agent.hasPath ||
                _agent.remainingDistance <= stoppingDistance;
        }

        private Vector3 _destination;
        private bool _hasDestination;

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

        private void Awake()
        {
            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();

            if (_agent != null)
            {
                _agent.updatePosition = false;
                _agent.updateRotation = false;
            }
        }

        private void Update()
        {
            if (!CanNavigate())
            {
                MoveComposite = Vector2.zero;
                return;
            }

            _agent.nextPosition = transform.position;

            if (_agent.pathPending)
                return;

            if (CheckArrived())
            {
                MoveComposite = Vector2.zero;
                return;
            }

            Vector3 desiredDirection = GetDesiredWorldDirection();

            if (desiredDirection.sqrMagnitude <= 0.0001f)
                return;

            if (_look != null)
                _look.SetWorldDirection(desiredDirection);

            MoveComposite = ToLookRelativeInput(desiredDirection);
            MovementInputDuration += Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (_agent != null &&
                _agent.enabled &&
                _agent.isOnNavMesh)
            {
                _agent.nextPosition = transform.position;
            }
        }

        public void SetDestination(Vector3 destination)
        {
            if (_agent == null ||
                !_agent.enabled ||
                !_agent.isOnNavMesh)
            {
                return;
            }

            bool shouldRepath =
                !_hasDestination ||
                Vector3.Distance(_destination, destination) >= _destinationRepathDistance;

            _destination = destination;
            _hasDestination = true;

            if (!shouldRepath)
                return;

            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            _hasDestination = false;
            MoveComposite = Vector2.zero;
            MovementInputDuration = 0f;

            if (_agent == null ||
                !_agent.enabled ||
                !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.nextPosition = transform.position;
        }

        private bool CanNavigate()
        {
            return _hasDestination &&
                _agent != null &&
                _agent.enabled &&
                _agent.isOnNavMesh;
        }


        private Vector3 GetDesiredWorldDirection()
        {
            Vector3 desiredVelocity = _agent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude > 0.0001f)
                return desiredVelocity.normalized;

            Vector3 toSteeringTarget = _agent.steeringTarget - transform.position;
            toSteeringTarget.y = 0f;

            if (toSteeringTarget.sqrMagnitude > 0.0001f)
                return toSteeringTarget.normalized;

            return Vector3.zero;
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