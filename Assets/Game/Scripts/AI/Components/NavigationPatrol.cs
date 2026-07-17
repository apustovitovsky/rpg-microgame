using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using Game.Navigation;
using UnityEngine;
using VContainer;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavigationPatrol :
        MonoBehaviour
    {
        private INavigationPathFollower _pathFollower;
        private IActorNavigation _navigation;
        private IActorPlacementService _placements;
        private Guid _instanceId;

        private string _currentLocationId;
        private string _currentAnchorKey;
        private string _lastFailure;
        private int _nextStopIndex;

        [Inject]
        public void Construct(
            INavigationPathFollower pathFollower,
            IActorNavigation navigation,
            ActorInstance instance,
            IActorPlacementService placements)
        {
            _pathFollower = pathFollower;
            _navigation = navigation;
            _placements = placements;

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_placements == null)
            {
                throw new ArgumentNullException(nameof(placements));
            }

            _instanceId = instance.InstanceId;
        }

        public async UniTask<bool> MoveToNextAsync(
            CancellationToken cancellationToken)
        {
            if (!_placements.TryGet(
                    _instanceId,
                    out var placement))
            {
                ReportFailure(
                    $"Actor placement for instance " +
                    $"'{_instanceId}' was not found.");

                return false;
            }

            if (!placement.HasPatrol)
            {
                ReportFailure(
                    $"Actor placement for instance " +
                    $"'{_instanceId}' has no patrol locations.");

                return false;
            }

            if (_pathFollower == null)
            {
                ReportFailure(
                    "INavigationPathFollower was not injected.");

                return false;
            }

            if (_navigation == null)
            {
                ReportFailure(
                    "IActorNavigation was not injected.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    _currentLocationId))
            {
                _currentLocationId =
                    placement.SpawnLocation.LocationId;

                _currentAnchorKey =
                    placement.SpawnLocation.AnchorKey;
            }

            var target = placement.PatrolLocations[
                _nextStopIndex];

            var result = await _pathFollower.FollowAsync(
                _navigation,
                _currentLocationId,
                _currentAnchorKey,
                target.LocationId,
                target.AnchorKey,
                cancellationToken);

            if (result !=
                NavigationPathFollowResult.Completed)
            {
                ReportFailure(
                    $"Failed to travel from " +
                    $"{_currentLocationId}/" +
                    $"{_currentAnchorKey} to " +
                    $"{target.LocationId}/" +
                    $"{target.AnchorKey}: {result}.");

                return false;
            }

            _lastFailure = null;

            _currentLocationId = target.LocationId;
            _currentAnchorKey = target.AnchorKey;

            _nextStopIndex =
                (_nextStopIndex + 1) %
                placement.PatrolLocations.Count;

            return true;
        }

        private void ReportFailure(
            string message)
        {
            if (_lastFailure == message)
            {
                return;
            }

            _lastFailure = message;

            Debug.LogError(
                $"Navigation patrol on '{name}' failed: " +
                message,
                this);
        }
    }
}