using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Etheria.Npc.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Start NPC Dialogue Command",
        story: "[Self] starts dialogue with [TargetActorId]",
        category: "Etheria/Npc",
        id: "8e08e6b0b01f4d61b3f5d2e5a1b7a901")]
    public partial class StartDialogueCommandAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<string> TargetActorId;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private bool _isCompleted;
        private bool _isSuccessful;

        protected override Status OnStart()
        {
            if (Self?.Value == null)
                return Status.Failure;

            var endpoint =
                Self.Value.GetComponentInParent<IActorCommandEndpoint>();

            if (endpoint == null)
                return Status.Failure;

            var sensor =
                Self.Value.GetComponentInParent<NpcAwarenessSensor>();

            sensor?.MarkDialogueRequested();

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            _isRunning = true;
            _isCompleted = false;
            _isSuccessful = false;

            ExecuteAsync(
                    endpoint,
                    _cancellationTokenSource.Token)
                .Forget();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!_isRunning)
                return Status.Failure;

            if (!_isCompleted)
                return Status.Running;

            return _isSuccessful
                ? Status.Success
                : Status.Failure;
        }

        protected override void OnEnd()
        {
            if (!_isCompleted)
                _cancellationTokenSource?.Cancel();

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _isRunning = false;
        }

        private async UniTaskVoid ExecuteAsync(
            IActorCommandEndpoint endpoint,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await endpoint.StartDialogueAsync(
                    TargetActorId?.Value ?? string.Empty,
                    cancellationToken);

                _isSuccessful = result.Succeeded;
            }
            catch (OperationCanceledException)
            {
                _isSuccessful = false;
            }
            finally
            {
                _isCompleted = true;
            }
        }
    }
}