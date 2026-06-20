using UnityEngine;
using UnityEngine.AI;

namespace Etheria.Features.Character
{
    public sealed class NpcMotor : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _agent;

        public bool IsMoving =>
            _agent.isOnNavMesh &&
            !_agent.pathPending &&
            _agent.remainingDistance > _agent.stoppingDistance;

        public bool HasArrived =>
            _agent.isOnNavMesh &&
            !_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance &&
            _agent.velocity.sqrMagnitude < 0.01f;

        public void MoveTo(Vector3 destination)
        {
            if (!_agent.isOnNavMesh)
                return;

            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void BeginManualRotation()
        {
            _agent.updateRotation = false;
        }

        public void EndManualRotation()
        {
            _agent.updateRotation = true;
        }

        public void Stop()
        {
            if (!_agent.isOnNavMesh)
                return;

            _agent.ResetPath();
            _agent.isStopped = true;
        }

        public void Resume()
        {
            if (_agent.isOnNavMesh)
                _agent.isStopped = false;
        }
    }
}