using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Etheria.Npc
{
    public sealed class NpcMotor
    {
        private const float ArrivedVelocityThreshold = 0.01f;
        private const float RotationSpeed = 360f;

        private readonly NavMeshAgent _agent;

        public NpcMotor(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public bool IsOnNavMesh =>
            _agent != null &&
            _agent.isOnNavMesh;

        public bool HasArrived =>
            IsOnNavMesh &&
            !_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance &&
            _agent.velocity.sqrMagnitude <= ArrivedVelocityThreshold;

        public Quaternion Rotation =>
            _agent != null
                ? _agent.transform.rotation
                : Quaternion.identity;

        public void MoveTo(Vector3 destination)
        {
            if (!IsOnNavMesh)
                return;

            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!IsOnNavMesh)
                return;

            _agent.ResetPath();
            _agent.isStopped = true;
        }

        public void TeleportTo(
            Vector3 position,
            Quaternion rotation)
        {
            if (IsOnNavMesh)
                _agent.Warp(position);
            else if (_agent != null)
                _agent.transform.position = position;

            if (_agent != null)
                _agent.transform.rotation = rotation;
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_agent == null)
                return;

            _agent.transform.rotation = rotation;
        }

        public async UniTask TurnToAsync(
            Quaternion targetRotation,
            CancellationToken cancellationToken)
        {
            while (Quaternion.Angle(Rotation, targetRotation) > 1f)
            {
                SetRotation(
                    Quaternion.RotateTowards(
                        Rotation,
                        targetRotation,
                        RotationSpeed * Time.deltaTime));

                await UniTask.Yield(cancellationToken);
            }

            SetRotation(targetRotation);
        }

        public void BeginManualRotation()
        {
            if (_agent == null)
                return;

            _agent.updateRotation = false;
        }

        public void EndManualRotation()
        {
            if (_agent == null)
                return;

            _agent.updateRotation = true;
        }

        public void FaceTowards(Vector3 worldPosition)
        {
            if (_agent == null)
                return;

            Vector3 direction =
                worldPosition - _agent.transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up);

            _agent.transform.rotation = Quaternion.RotateTowards(
                _agent.transform.rotation,
                targetRotation,
                RotationSpeed * Time.deltaTime);
        }
    }
}