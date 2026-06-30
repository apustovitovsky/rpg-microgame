using Etheria.Game.World;

namespace Etheria.Game.Npc
{
    public interface INpcPathPlanner
    {
        bool TryBuildPathToNode(
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter,
            out NavigationPath path);

        bool TryBuildPathToLocation(
            string fromNodeId,
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            out NavigationPath path);
    }
}