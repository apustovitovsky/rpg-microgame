using System.Collections.Generic;

namespace Etheria.Game.World
{
    public interface IWorldLocationRegistry
    {
        IReadOnlyCollection<WorldLocation> Locations { get; }

        bool TryGet(
            string locationId,
            out WorldLocation location);
    }
}
