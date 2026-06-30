using System;
using Etheria.Game.World;

namespace Etheria.Game.Npc
{
    public interface INpcTravelController
    {
        bool TryFollowPath(
            NavigationPath path,
            Action<bool> completed = null);

        bool TryMoveToNode(
            string targetNodeId,
            NavigationQueryFilter filter,
            Action<bool> completed = null);

        bool TryMoveToLocation(
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            Action<bool> completed = null);
    }
}