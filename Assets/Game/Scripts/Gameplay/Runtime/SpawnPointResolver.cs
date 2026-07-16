using Game.Navigation;

namespace Game.Gameplay
{
    public interface ISpawnPointResolver
    {
        bool TryResolve(
            string locationId,
            string anchorKey,
            out NavigationNode node);
    }

    public sealed class SpawnPointResolver : ISpawnPointResolver
    {
        private readonly INavigationLocationResolver _locations;
        private readonly INavigationGraphProvider _graphProvider;

        public SpawnPointResolver(
            INavigationLocationResolver locations,
            INavigationGraphProvider graphProvider)
        {
            _locations = locations;
            _graphProvider = graphProvider;
        }

        public bool TryResolve(
            string locationId,
            string anchorKey,
            out NavigationNode node)
        {
            node = null;

            if (string.IsNullOrWhiteSpace(locationId) ||
                string.IsNullOrWhiteSpace(anchorKey))
            {
                return false;
            }

            if (_graphProvider.Graph == null)
                return false;

            if (!_locations.TryResolveAnchorNodeId(
                    locationId,
                    anchorKey,
                    out var nodeId))
            {
                return false;
            }

            return _graphProvider.Graph.TryGetNode(
                nodeId,
                out node);
        }
    }
}