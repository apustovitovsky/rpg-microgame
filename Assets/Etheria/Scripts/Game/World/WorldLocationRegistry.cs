using System;
using System.Collections.Generic;

namespace Etheria.Game.World
{
    public sealed class WorldLocationRegistry :
        IWorldLocationRegistry
    {
        private readonly Dictionary<string, WorldLocation> _locationsById;

        public IReadOnlyCollection<WorldLocation> Locations { get; }

        public WorldLocationRegistry(
            IReadOnlyCollection<WorldLocation> locations)
        {
            _locationsById = new Dictionary<string, WorldLocation>(
                StringComparer.Ordinal);

            var locationList = new List<WorldLocation>(locations.Count);

            foreach (var location in locations)
            {
                if (location == null)
                    throw new InvalidOperationException(
                        "World location collection contains a null entry.");

                if (string.IsNullOrWhiteSpace(location.Id))
                {
                    throw new InvalidOperationException(
                        $"World location '{location.name}' has no ID.");
                }

                if (!_locationsById.TryAdd(location.Id, location))
                {
                    throw new InvalidOperationException(
                        $"Duplicate world location ID: '{location.Id}'.");
                }

                locationList.Add(location);
            }

            Locations = locationList.AsReadOnly();
        }

        public bool TryGet(
            string locationId,
            out WorldLocation location)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                location = null;
                return false;
            }

            return _locationsById.TryGetValue(
                locationId,
                out location);
        }
    }
}
