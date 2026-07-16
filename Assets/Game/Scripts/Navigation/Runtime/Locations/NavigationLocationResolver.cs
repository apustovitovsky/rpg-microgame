using System;
using System.Collections.Generic;

namespace Game.Navigation
{
    public sealed class NavigationLocationResolver :
        INavigationLocationResolver
    {
        private readonly Dictionary<string, NavigationLocation>
            _locationsById =
                new(StringComparer.Ordinal);

        public NavigationLocationResolver(
            IReadOnlyList<NavigationLocation> locations)
        {
            if (locations == null)
                throw new ArgumentNullException(nameof(locations));

            foreach (var location in locations)
            {
                if (location == null)
                    continue;

                var id = location.Id?.Trim();

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!_locationsById.TryAdd(id, location))
                {
                    throw new InvalidOperationException(
                        $"Duplicate navigation location id: '{id}'.");
                }
            }
        }

        public bool TryResolveDefaultAnchorNodeId(
            string locationId,
            out string nodeId)
        {
            return TryResolveAnchorNodeId(
                locationId,
                NavigationAnchorKeys.Default,
                out nodeId);
        }

        public bool TryResolveAnchorNodeId(
            string locationId,
            string anchorKey,
            out string nodeId)
        {
            nodeId = string.Empty;

            locationId = locationId?.Trim() ?? string.Empty;
            anchorKey = anchorKey?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(locationId) ||
                string.IsNullOrWhiteSpace(anchorKey))
            {
                return false;
            }

            if (!_locationsById.TryGetValue(
                    locationId,
                    out var location))
            {
                return false;
            }

            foreach (var anchor in location.Anchors)
            {
                if (anchor == null ||
                    !string.Equals(
                        anchor.Key?.Trim(),
                        anchorKey,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                nodeId = anchor.NodeId?.Trim() ?? string.Empty;

                return !string.IsNullOrWhiteSpace(nodeId);
            }

            return false;
        }
    }
}