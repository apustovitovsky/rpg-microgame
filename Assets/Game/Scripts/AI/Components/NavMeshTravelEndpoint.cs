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
    public sealed class NavMeshTravelEndpoint :
        MonoBehaviour,
        IActorNavigation,
        IModuleInstaller
    {
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

        public async UniTask MoveToPositionAsync(
            Vector3 destination,
            CancellationToken cancellationToken)
        {
            if (_planner == null)
                return;

            _planner.MoveTo(destination);

            await UniTask.WaitUntil(
                () => _planner.HasArrived,
                cancellationToken: cancellationToken);
        }

        public async UniTask FaceDirectionAsync(
            Vector3 direction,
            CancellationToken cancellationToken)
        {
            if (_input == null)
                return;

            _input.SetFacing(direction);

            await UniTask.WaitUntil(
                () => _input.IsFacingComplete,
                cancellationToken: cancellationToken);
        }

        public void FaceDirection(Vector3 direction)
        {
            _input?.SetFacing(direction);
        }

        public void Stop()
        {
            _planner?.Stop();
        }

        public void ClearFacing()
        {
            _input?.ClearFacing();
        }
    }
}