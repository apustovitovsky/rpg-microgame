using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.AI.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Move To Next Patrol Node",
        story: "[Patrol] moves to its next route node",
        category: "Game/AI/Navigation",
        id: "4e4c445925954d1c8c3485de935e4d8a")]
    public partial class MoveToNextPatrolNodeAction :
        Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<NavigationPatrol> Patrol;

        private CancellationTokenSource _cancellation;
        private bool _completed;
        private bool _succeeded;
        private int _runVersion;

        protected override Status OnStart()
        {
            if (Patrol?.Value == null)
            {
                return Status.Failure;
            }

            _completed = false;
            _succeeded = false;

            _cancellation = new CancellationTokenSource();

            var runVersion = ++_runVersion;

            MoveAsync(
                Patrol.Value,
                runVersion,
                _cancellation.Token).Forget();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!_completed)
            {
                return Status.Running;
            }

            return _succeeded
                ? Status.Success
                : Status.Failure;
        }

        protected override void OnEnd()
        {
            _runVersion++;

            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }

        private async UniTask MoveAsync(
            NavigationPatrol patrol,
            int runVersion,
            CancellationToken cancellationToken)
        {
            try
            {
                var succeeded = await patrol.MoveToNextAsync(
                    cancellationToken);

                if (runVersion != _runVersion)
                {
                    return;
                }

                _succeeded = succeeded;
                _completed = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                if (runVersion != _runVersion)
                {
                    return;
                }

                _succeeded = false;
                _completed = true;
            }
        }
    }
}