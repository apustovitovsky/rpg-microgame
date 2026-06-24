using System.Collections.Generic;

namespace Etheria.Game.World
{
    public interface IWorldRouteRegistry
    {
        IReadOnlyCollection<WorldRoute> Routes { get; }

        bool TryGet(
            string routeId,
            out WorldRoute route);
    }
}