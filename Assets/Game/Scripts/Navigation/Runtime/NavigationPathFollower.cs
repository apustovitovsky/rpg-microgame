using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using UnityEngine;

namespace Game.Navigation
{
    public sealed class NavigationPathFollower :
        INavigationPathFollower
    {
        private readonly INavigationGraphProvider _graphProvider;
        private readonly INavigationPathfinder _pathfinder;
        private readonly INavigationLocationResolver _locations;

        public NavigationPathFollower(
            INavigationGraphProvider graphProvider,
            INavigationPathfinder pathfinder,
            INavigationLocationResolver locations)
        {
            if (graphProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(graphProvider));
            }

            if (pathfinder == null)
            {
                throw new ArgumentNullException(
                    nameof(pathfinder));
            }

            if (locations == null)
            {
                throw new ArgumentNullException(
                    nameof(locations));
            }

            _graphProvider = graphProvider;
            _pathfinder = pathfinder;
            _locations = locations;
        }

        public UniTask<NavigationPathFollowResult> FollowAsync(
            IActorNavigation navigation,
            string startLocationId,
            string startAnchorKey,
            string targetLocationId,
            string targetAnchorKey,
            CancellationToken cancellationToken)
        {
            if (!_locations.TryResolveAnchorNodeId(
                    startLocationId,
                    startAnchorKey,
                    out var startNodeId))
            {
                return UniTask.FromResult(
                    NavigationPathFollowResult.StartAnchorNotFound);
            }

            if (!_locations.TryResolveAnchorNodeId(
                    targetLocationId,
                    targetAnchorKey,
                    out var targetNodeId))
            {
                return UniTask.FromResult(
                    NavigationPathFollowResult.TargetAnchorNotFound);
            }

            return FollowAsync(
                navigation,
                startNodeId,
                targetNodeId,
                cancellationToken);
        }

        public async UniTask<NavigationPathFollowResult> FollowAsync(
            IActorNavigation navigation,
            string startNodeId,
            string targetNodeId,
            CancellationToken cancellationToken)
        {
            if (navigation == null ||
                string.IsNullOrWhiteSpace(startNodeId) ||
                string.IsNullOrWhiteSpace(targetNodeId))
            {
                return NavigationPathFollowResult.InvalidRequest;
            }

            var graph = _graphProvider.Graph;

            if (graph == null ||
                !graph.TryGetNode(
                    startNodeId,
                    out _))
            {
                return NavigationPathFollowResult.StartNodeNotFound;
            }

            if (!graph.TryGetNode(
                    targetNodeId,
                    out var targetNode))
            {
                return NavigationPathFollowResult.TargetNodeNotFound;
            }

            if (!_pathfinder.TryFindPath(
                    graph,
                    startNodeId,
                    targetNodeId,
                    NavigationQueryFilter.Any,
                    out var path) ||
                path.IsEmpty)
            {
                return NavigationPathFollowResult.PathNotFound;
            }

            for (var index = 1;
                 index < path.NodeIds.Count - 1;
                 index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!graph.TryGetNode(
                        path.NodeIds[index],
                        out var node))
                {
                    return NavigationPathFollowResult.PathNotFound;
                }

                await navigation.MoveToAsync(
                    node.Position,
                    node.Radius,
                    cancellationToken);
            }

            await navigation.MoveToCenterAsync(
                targetNode.Position,
                cancellationToken);

            var facingDirection =
                targetNode.Rotation * Vector3.forward;

            await navigation.FaceDirectionAsync(
                facingDirection,
                cancellationToken);

            return NavigationPathFollowResult.Completed;
        }
    }
}