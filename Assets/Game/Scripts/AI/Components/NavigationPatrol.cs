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
        [SerializeField]
        private NavigationPatrolRoute _route;

        [SerializeField]
        private NavigationPatrolRoute.Stop _initialStop =
            new();

        private INavigationPathFollower _pathFollower;
        private IActorNavigation _navigation;

        private string _currentLocationId;
        private string _currentAnchorKey;
        private string _lastFailure;
        private int _nextStopIndex;

        [Inject]
        public void Construct(
            INavigationPathFollower pathFollower,
            IActorNavigation navigation)
        {
            _pathFollower = pathFollower;
            _navigation = navigation;
        }

        public async UniTask<bool> MoveToNextAsync(
            CancellationToken cancellationToken)
        {
            if (_route == null)
            {
                ReportFailure("Patrol route is not assigned.");
                return false;
            }

            if (!_initialStop.IsValid)
            {
                ReportFailure("Initial patrol stop is invalid.");
                return false;
            }

            if (_route.Stops.Count == 0)
            {
                ReportFailure("Patrol route has no stops.");
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
                    _initialStop.LocationId;

                _currentAnchorKey =
                    _initialStop.AnchorKey;
            }

            var target = _route.Stops[
                _nextStopIndex];

            if (target == null ||
                !target.IsValid)
            {
                ReportFailure(
                    $"Patrol stop {_nextStopIndex} is invalid.");

                return false;
            }

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
                _route.Stops.Count;

            return true;
        }

        private void OnValidate()
        {
            _initialStop?.Normalize();
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