using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcMovementService :
        INpcMovementService
    {
        private readonly NpcMotor _motor;

        public NpcMovementService(
            NpcMotor motor)
        {
            _motor = motor;
        }

        public bool IsOnNavMesh =>
            _motor != null &&
            _motor.IsOnNavMesh;

        public async UniTask MoveToAsync(
            Vector3 position,
            float radius,
            CancellationToken cancellationToken)
        {
            if (!IsOnNavMesh)
                return;

            _motor.SetStoppingDistance(radius);
            _motor.MoveTo(position);

            await UniTask.WaitUntil(
                () => _motor.HasArrived,
                cancellationToken: cancellationToken);
        }

        public void BeginManualRotation()
        {
            _motor?.BeginManualRotation();
        }

        public void EndManualRotation()
        {
            _motor?.EndManualRotation();
        }

        public void FaceTowards(
            Vector3 worldPosition)
        {
            _motor?.FaceTowards(worldPosition);
        }

        public async UniTask MoveToCenterAsync(
            Vector3 position,
            CancellationToken cancellationToken)
        {
            if (!IsOnNavMesh)
                return;

            _motor.SetStoppingDistance(0f);
            _motor.MoveTo(position);

            await UniTask.WaitUntil(
                () => _motor.HasArrived,
                cancellationToken: cancellationToken);
        }

        public UniTask TurnToAsync(
            Quaternion rotation,
            CancellationToken cancellationToken)
        {
            return _motor != null
                ? _motor.TurnToAsync(rotation, cancellationToken)
                : UniTask.CompletedTask;
        }

        public UniTask StopAsync(
            CancellationToken cancellationToken)
        {
            _motor?.Stop();

            return UniTask.CompletedTask;
        }
    }
}