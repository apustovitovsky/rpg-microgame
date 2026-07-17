using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavMeshNavigationModule :
        MonoBehaviour,
        IActorNavigation,
        IModuleInstaller
    {
        private const float CenterArrivalRadius = 0.05f;

        private INavMeshPlanner _planner;
        private NavMeshActorInput _input;

        public bool HasArrived =>
            _planner != null && _planner.HasArrived;

        public bool IsMoving =>
            _planner != null && _planner.IsNavigating;

        public bool IsFacingComplete =>
            _input == null || _input.IsFacingComplete;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<IActorNavigation>();
        }

        [Inject]
        public void Construct(
            INavMeshPlanner planner,
            NavMeshActorInput input)
        {
            _planner = planner;
            _input = input;
        }

        public UniTask MoveToPositionAsync(
            Vector3 destination,
            CancellationToken cancellationToken)
        {
            if (_planner == null)
            {
                return UniTask.CompletedTask;
            }

            _planner.MoveTo(destination);

            return WaitForArrivalAsync(
                cancellationToken);
        }

        public UniTask MoveToAsync(
            Vector3 destination,
            float arrivalRadius,
            CancellationToken cancellationToken)
        {
            if (_planner == null)
            {
                return UniTask.CompletedTask;
            }

            _planner.MoveTo(
                destination,
                arrivalRadius);

            return WaitForArrivalAsync(
                cancellationToken);
        }

        public UniTask MoveToCenterAsync(
            Vector3 destination,
            CancellationToken cancellationToken)
        {
            return MoveToAsync(
                destination,
                CenterArrivalRadius,
                cancellationToken);
        }

        public async UniTask FaceDirectionAsync(
            Vector3 direction,
            CancellationToken cancellationToken)
        {
            if (_input == null)
            {
                return;
            }

            _input.SetFacing(direction);

            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(1f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _input.ClearFacing();
                throw;
            }
        }

        public void FaceDirection(Vector3 direction)
        {
            _input?.SetFacing(direction);
        }

        public void Stop()
        {
            _input?.Stop();
            _planner?.Stop();
        }

        public void ClearFacing()
        {
            _input?.ClearFacing();
        }

        private async UniTask WaitForArrivalAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(
                    () => _planner.HasArrived,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                Stop();
                throw;
            }
        }
    }
}