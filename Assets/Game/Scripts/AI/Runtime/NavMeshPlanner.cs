using System;
using Game.Core;
using UnityEngine;
using UnityEngine.AI;
using VContainer.Unity;

namespace Game.AI
{
    public sealed class NavMeshPlanner :
        INavMeshPlanner,
        ITickable
    {
        private readonly NavMeshAgent _agent;
        private readonly IGameTimeProvider _time;

        private readonly float _destinationRepathDistance;

        private Vector3 _destination;
        private float _arrivalRadius;
        private float _destinationElapsedSeconds;
        private string _lastDiagnostic;
        private bool _hasDestination;
        private int _pauseCount;

        public NavMeshPlanner(
            NavMeshAgent agent,
            IGameTimeProvider time)
        {
            _agent = agent;
            _time = time;

            _destinationRepathDistance = 0.25f;

            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        public bool HasDestination =>
            _hasDestination;

        public bool HasArrived =>
            CanNavigate() && CheckArrived();

        public bool IsNavigating =>
            CanNavigate() && !CheckArrived();

        public Vector3 DesiredWorldDirection { get; private set; }

        private bool IsPaused =>
            _pauseCount > 0;

        public void Tick()
        {
            DesiredWorldDirection = Vector3.zero;

            if (!CanNavigate())
            {
                return;
            }

            _destinationElapsedSeconds += _time.DeltaTime;

            _agent.nextPosition = _agent.transform.position;

            if (_agent.pathPending)
            {
                return;
            }

            if (CheckArrived())
            {
                ClearDiagnostic();
                return;
            }

            if (_destinationElapsedSeconds > 0.5f &&
                !_agent.hasPath)
            {
                ReportDiagnostic(
                    "NavMeshAgent has no path after " +
                    "SetDestination.");

                return;
            }

            if (_agent.pathStatus ==
                NavMeshPathStatus.PathInvalid)
            {
                ReportDiagnostic(
                    "NavMeshAgent path is invalid.");

                return;
            }

            DesiredWorldDirection = GetDesiredWorldDirection();

            if (_destinationElapsedSeconds > 0.5f &&
                DesiredWorldDirection.sqrMagnitude <=
                0.0001f)
            {
                ReportDiagnostic(
                    "NavMeshAgent has a destination but " +
                    "produces no desired movement direction.");
            }
        }

        public void MoveTo(Vector3 destination)
        {
            if (!CanUseAgent())
            {
                ReportUnavailableAgent();
                return;
            }

            MoveTo(
                destination,
                Mathf.Max(_agent.stoppingDistance, 0.05f));
        }

        public void MoveTo(
            Vector3 destination,
            float arrivalRadius)
        {
            if (!CanUseAgent())
            {
                ReportUnavailableAgent();
                return;
            }

            var normalizedRadius =
                Mathf.Max(arrivalRadius, 0.05f);

            bool shouldRepath =
                !_hasDestination ||
                Vector3.Distance(
                    _destination,
                    destination) >=
                _destinationRepathDistance ||
                !Mathf.Approximately(
                    _arrivalRadius,
                    normalizedRadius);

            _destination = destination;
            _arrivalRadius = normalizedRadius;
            _hasDestination = true;

            if (!shouldRepath || IsPaused)
            {
                return;
            }

            _agent.stoppingDistance = _arrivalRadius;
            _agent.isStopped = false;

            if (!_agent.SetDestination(destination))
            {
                ReportDiagnostic(
                    "NavMeshAgent rejected SetDestination.");

                return;
            }

            _destinationElapsedSeconds = 0f;
            ClearDiagnostic();
        }

        public void Stop()
        {
            _hasDestination = false;
            DesiredWorldDirection = Vector3.zero;
            _destinationElapsedSeconds = 0f;
            ClearDiagnostic();

            if (!CanUseAgent())
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.nextPosition = _agent.transform.position;
        }

        public IDisposable AcquirePause()
        {
            _pauseCount++;

            if (CanUseAgent())
            {
                _agent.isStopped = true;
            }

            return new PauseLease(this);
        }

        private void ReleasePause()
        {
            if (_pauseCount == 0)
            {
                return;
            }

            _pauseCount--;

            if (IsPaused ||
                !_hasDestination ||
                !CanUseAgent())
            {
                return;
            }

            _agent.stoppingDistance = _arrivalRadius;
            _agent.isStopped = false;
            _agent.SetDestination(_destination);
        }

        private bool CanNavigate()
        {
            return !IsPaused &&
                   _hasDestination &&
                   CanUseAgent();
        }

        private bool CanUseAgent()
        {
            return _agent != null &&
                   _agent.enabled &&
                   _agent.isOnNavMesh;
        }

        private bool CheckArrived()
        {
            Vector3 toDestination =
                _destination - _agent.transform.position;

            toDestination.y = 0f;

            if (toDestination.sqrMagnitude <=
                _arrivalRadius * _arrivalRadius)
            {
                return true;
            }

            if (_agent.pathPending ||
                !_agent.hasPath ||
                _agent.pathStatus ==
                NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            return _agent.remainingDistance <=
                _arrivalRadius;
        }

        private Vector3 GetDesiredWorldDirection()
        {
            Vector3 desiredVelocity = _agent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude > 0.0001f)
            {
                return desiredVelocity.normalized;
            }

            Vector3 toSteeringTarget =
                _agent.steeringTarget -
                _agent.transform.position;

            toSteeringTarget.y = 0f;

            if (toSteeringTarget.sqrMagnitude > 0.0001f)
            {
                return toSteeringTarget.normalized;
            }

            return Vector3.zero;
        }

        private void ReportUnavailableAgent()
        {
            if (_agent == null)
            {
                ReportDiagnostic("NavMeshAgent is missing.");
                return;
            }

            ReportDiagnostic(
                $"NavMeshAgent cannot navigate: " +
                $"enabled={_agent.enabled}, " +
                $"isOnNavMesh={_agent.isOnNavMesh}.");
        }

        private void ReportDiagnostic(
            string message)
        {
            if (_lastDiagnostic == message)
            {
                return;
            }

            _lastDiagnostic = message;

            Debug.LogError(
                $"NavMeshPlanner on '{_agent.name}' failed: " +
                message,
                _agent);
        }

        private void ClearDiagnostic()
        {
            _lastDiagnostic = null;
        }

        private sealed class PauseLease :
            IDisposable
        {
            private NavMeshPlanner _planner;

            public PauseLease(
                NavMeshPlanner planner)
            {
                _planner = planner;
            }

            public void Dispose()
            {
                var planner = _planner;
                _planner = null;

                planner?.ReleasePause();
            }
        }
    }
}