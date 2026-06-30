using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Etheria.Game.World;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Etheria.Npc.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Move NPC To Location Command",
        story: "[Self] moves to location [LocationId] anchor [AnchorKey]",
        category: "Etheria/Npc",
        id: "f6d9d98b10fd4fd1a3dd4e12a62f80e1")]
    public partial class MoveNpcToLocationCommandAction :
        Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<string> LocationId;
        [SerializeReference]
        public BlackboardVariable<string> AnchorKey =
            new BlackboardVariable<string>(NavigationAnchorKeys.Default);

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private bool _isCompleted;
        private bool _isSuccessful;

        protected override Status OnStart()
        {
            if (Self?.Value == null ||
                string.IsNullOrWhiteSpace(LocationId?.Value))
            {
                return Status.Failure;
            }

            var endpoint =
                Self.Value.GetComponentInParent<IActorCommandEndpoint>();

            if (endpoint == null)
                return Status.Failure;

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
                var result = await endpoint.MoveToLocationAsync(
                    LocationId.Value,
                    string.IsNullOrWhiteSpace(AnchorKey?.Value)
                        ? NavigationAnchorKeys.Default
                        : AnchorKey.Value,
                    NavigationQueryFilter.Any,
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