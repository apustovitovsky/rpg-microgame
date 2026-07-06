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
        private bool _hasDestination;

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

        public void Tick()
        {
            DesiredWorldDirection = Vector3.zero;

            if (!CanNavigate())
                return;

            _agent.nextPosition = _agent.transform.position;

            if (_agent.pathPending)
                return;

            if (CheckArrived())
                return;

            DesiredWorldDirection = GetDesiredWorldDirection();
        }

        public void MoveTo(Vector3 destination)
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
            DesiredWorldDirection = Vector3.zero;

            if (_agent == null ||
                !_agent.enabled ||
                !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.nextPosition = _agent.transform.position;
        }

        private bool CanNavigate()
        {
            return _hasDestination &&
                _agent != null &&
                _agent.enabled &&
                _agent.isOnNavMesh;
        }

        private bool CheckArrived()
        {
            if (_agent.pathPending)
                return false;

            if (!_agent.hasPath)
                return false;

            if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
                return false;

            float stoppingDistance = Mathf.Max(
                _agent.stoppingDistance,
                0.05f);

            return _agent.remainingDistance <= stoppingDistance;
        }

        private Vector3 GetDesiredWorldDirection()
        {
            Vector3 desiredVelocity = _agent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude > 0.0001f)
                return desiredVelocity.normalized;

            Vector3 toSteeringTarget = _agent.steeringTarget - _agent.transform.position;
            toSteeringTarget.y = 0f;

            if (toSteeringTarget.sqrMagnitude > 0.0001f)
                return toSteeringTarget.normalized;

            return Vector3.zero;
        }
    }
}