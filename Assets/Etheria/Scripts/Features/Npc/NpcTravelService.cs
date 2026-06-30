using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcTravelService : INpcTravelService
    {
        private readonly INpcRuntimeRegistry _runtimes;
        private readonly ICharacterWorldStateService _worldState;

        public NpcTravelService(
            INpcRuntimeRegistry runtimes,
            ICharacterWorldStateService worldState)
        {
            _runtimes = runtimes;
            _worldState = worldState;
        }

        public bool TrySendToLocation(
            string npcId,
            string locationId)
        {
            return TrySendToAnchor(
                npcId,
                locationId,
                NavigationAnchorKeys.Default,
                NavigationQueryFilter.Any);
        }

        public bool TrySendToAnchor(
            string npcId,
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter)
        {
            if (_runtimes.TryGet(npcId, out var runtime))
            {
                return runtime.Travel.TryMoveToLocation(
                    locationId,
                    anchorKey,
                    filter,
                    arrived =>
                    {
                        if (arrived)
                            _worldState.TryMove(npcId, locationId);
                    });
            }

            Debug.LogWarning(
                $"Cannot send NPC '{npcId}': live NPC runtime was not found.");

            return false;
        }
    }
}