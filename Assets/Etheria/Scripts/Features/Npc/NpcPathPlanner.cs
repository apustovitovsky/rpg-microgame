using Etheria.Game.Npc;
using Etheria.Game.World;

namespace Etheria.Npc
{
    public sealed class NpcPathPlanner : INpcPathPlanner
    {
        private readonly INavigationGraphProvider _graphProvider;
        private readonly INavigationPathfinder _pathfinder;
        private readonly INavigationLocationResolver _locationResolver;

        public NpcPathPlanner(
            INavigationGraphProvider graphProvider,
            INavigationPathfinder pathfinder,
            INavigationLocationResolver locationResolver)
        {
            _graphProvider = graphProvider;
            _pathfinder = pathfinder;
            _locationResolver = locationResolver;
        }

        public bool TryBuildPathToNode(
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter,
            out NavigationPath path)
        {
            path = NavigationPath.Empty;

            if (_graphProvider?.Graph == null ||
                _pathfinder == null)
                return false;

            return _pathfinder.TryFindPath(
                _graphProvider.Graph,
                fromNodeId,
                toNodeId,
                filter,
                out path);
        }

        public bool TryBuildPathToLocation(
            string fromNodeId,
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            out NavigationPath path)
        {
            path = NavigationPath.Empty;

            if (_locationResolver == null)
                return false;

            if (!_locationResolver.TryResolveAnchorNodeId(
                    locationId,
                    anchorKey,
                    out var targetNodeId))
                return false;

            return TryBuildPathToNode(
                fromNodeId,
                targetNodeId,
                filter,
                out path);
        }
    }
}