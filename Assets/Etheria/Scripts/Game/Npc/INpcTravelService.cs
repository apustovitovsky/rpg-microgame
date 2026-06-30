using Etheria.Game.World;

namespace Etheria.Game.Npc
{
    public interface INpcTravelService
    {
        bool TrySendToLocation(
            string npcId,
            string locationId);

        bool TrySendToAnchor(
            string npcId,
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter);
    }
}