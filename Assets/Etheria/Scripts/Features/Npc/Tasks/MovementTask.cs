using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class MovementTask : INpcTask
    {
        private readonly NpcMotor _motor;
        private readonly Transform _destination;

        public int Priority { get; }
        public NpcTaskStatus Status { get; private set; } =
            NpcTaskStatus.Pending;
        public NpcTaskType Type =>
            NpcTaskType.Movement;
        public bool CanSuspend => true;
        public bool IsBlocking => false;

        public MovementTask(
            NpcMotor motor,
            Transform destination,
            int priority)
        {
            _motor = motor;
            _destination = destination;
            Priority = priority;
        }


        public async UniTask RunAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                if (!_motor.IsOnNavMesh)
                {
                    Status = NpcTaskStatus.Failed;

                    Debug.LogWarning(
                        $"Cannot move NPC: {nameof(NpcMotor)} is not on NavMesh.");

                    return;
                }

                Status = NpcTaskStatus.Running;

                if (_destination == null)
                {
                    Status = NpcTaskStatus.Failed;
                    return;
                }

                _motor.MoveTo(_destination.position);

                await UniTask.WaitUntil(
                    () => _motor.HasArrived,
                    cancellationToken: cancellationToken);

                await _motor.TurnToAsync(
                    _destination.rotation,
                    cancellationToken);

                Status = NpcTaskStatus.Completed;
                return;
            }
            catch (OperationCanceledException)
            {
                Status = CanSuspend
                    ? NpcTaskStatus.Suspended
                    : NpcTaskStatus.Cancelled;

                return;
            }
            finally
            {
                _motor.Stop();
            }
        }
    }
}