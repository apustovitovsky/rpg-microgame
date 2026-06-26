using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class DialogueTask : INpcTask
    {

        public NpcTaskStatus Status { get; private set; } =
            NpcTaskStatus.Pending;

        private readonly NpcMotor _motor;
        private readonly Transform _target;
        public int Priority { get; }

        public NpcTaskType Type => NpcTaskType.Dialogue;

        public bool CanSuspend => false;

        public bool IsBlocking => false;


        public DialogueTask(
            NpcMotor motor,
            Transform target,
            int priority)
        {
            _motor = motor;
            _target = target;
            Priority = priority;
        }

        public async UniTask RunAsync(
            CancellationToken cancellationToken)
        {
            Status = NpcTaskStatus.Running;

            _motor.BeginManualRotation();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _motor.FaceTowards(_target.position);

                    await UniTask.Yield(cancellationToken);
                }

                Status = NpcTaskStatus.Completed;
                return;
            }
            catch (OperationCanceledException)
            {
                Status = NpcTaskStatus.Cancelled;
                return;
            }
            finally
            {
                _motor.EndManualRotation();
            }
        }
    }
}