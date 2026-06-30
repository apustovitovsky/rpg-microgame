using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;
using Etheria.Game.World;

namespace Etheria.Npc
{
    public sealed class NpcTravelController : INpcTravelController, IDisposable
    {
        private readonly INpcPathPlanner _pathPlanner;
        private readonly INpcRouteFollower _routeFollower;
        private readonly INpcState _state;
        private CancellationTokenSource _travelCts;

        public NpcTravelController(
            INpcPathPlanner pathPlanner,
            INpcRouteFollower routeFollower,
            INpcState state)
        {
            _pathPlanner = pathPlanner;
            _routeFollower = routeFollower;
            _state = state;
        }

        public bool TryFollowPath(
            NavigationPath path,
            Action<bool> completed = null)
        {
            if (_routeFollower == null ||
                path == null ||
                path.IsEmpty)
            {
                completed?.Invoke(false);
                return false;
            }

            CancelCurrentTravel();

            _travelCts = new CancellationTokenSource();


            var cts = _travelCts;

            FollowPathAsync(
                    path,
                    completed,
                    cts)
                .Forget();

            return true;
        }

        public bool TryMoveToLocation(
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            Action<bool> completed = null)
        {
            if (_pathPlanner == null ||
                _state == null ||
                !_state.IsAttachedToGraph)
            {
                completed?.Invoke(false);
                return false;
            }

            if (!_pathPlanner.TryBuildPathToLocation(
                    _state.CurrentNodeId,
                    locationId,
                    anchorKey,
                    filter,
                    out var path))
            {
                completed?.Invoke(false);
                return false;
            }

            return TryFollowPath(
                path,
                completed);
        }

        public bool TryMoveToNode(
            string targetNodeId,
            NavigationQueryFilter filter,
            Action<bool> completed = null)
        {
            if (_pathPlanner == null ||
                _state == null ||
                !_state.IsAttachedToGraph)
            {
                completed?.Invoke(false);
                return false;
            }

            if (!_pathPlanner.TryBuildPathToNode(
                    _state.CurrentNodeId,
                    targetNodeId,
                    filter,
                    out var path))
            {
                completed?.Invoke(false);
                return false;
            }

            return TryFollowPath(
                path,
                completed);
        }

        public void Dispose()
        {
            var cts = _travelCts;
            _travelCts = null;


            cts?.Cancel();
            cts?.Dispose();
        }

        private async UniTaskVoid FollowPathAsync(
            NavigationPath path,
            Action<bool> completed,
            CancellationTokenSource cts)
        {
            var succeeded = false;

            try
            {
                await _routeFollower.FollowPathAsync(
                    path,
                    cts.Token);

                succeeded = true;
            }
            catch (OperationCanceledException)
            {
                succeeded = false;
            }
            finally
            {
                if (ReferenceEquals(_travelCts, cts))
                {
                    _travelCts = null;

                }

                cts.Dispose();
                completed?.Invoke(succeeded);
            }
        }

        private void CancelCurrentTravel()
        {
            if (_travelCts == null)
                return;

            _travelCts.Cancel();
            _travelCts = null;
        }
    }
}