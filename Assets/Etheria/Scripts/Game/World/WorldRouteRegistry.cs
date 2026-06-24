using System;
using System.Collections.Generic;

namespace Etheria.Game.World
{
    public sealed class WorldRouteRegistry : IWorldRouteRegistry
    {
        private readonly Dictionary<string, WorldRoute> _routesById;

        public IReadOnlyCollection<WorldRoute> Routes { get; }

        public WorldRouteRegistry(
            IReadOnlyCollection<WorldRoute> routes)
        {
            _routesById = new Dictionary<string, WorldRoute>(
                StringComparer.Ordinal);

            var routeList = new List<WorldRoute>(routes.Count);

            foreach (var route in routes)
            {
                if (route == null)
                    continue;

                if (string.IsNullOrWhiteSpace(route.Id))
                    throw new InvalidOperationException(
                        "World route has an empty ID.");

                if (!_routesById.TryAdd(route.Id, route))
                    throw new InvalidOperationException(
                        $"Duplicate world route ID: '{route.Id}'.");

                routeList.Add(route);
            }

            Routes = routeList;
        }

        public bool TryGet(
            string routeId,
            out WorldRoute route)
        {
            return _routesById.TryGetValue(
                routeId,
                out route);
        }
    }
}