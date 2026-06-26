using System.Collections.Generic;
using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcTravelService : ICharacterTravelService
    {
        private readonly INpcAgentRegistry _agents;
        private readonly IWorldLocationRegistry _locations;
        private readonly IWorldRouteRegistry _routes;
        private readonly ICharacterWorldStateService _worldState;

        public NpcTravelService(
            INpcAgentRegistry agents,
            IWorldLocationRegistry locations,
            IWorldRouteRegistry routes,
            ICharacterWorldStateService worldState)
        {

            _agents = agents;
            _locations = locations;
            _routes = routes;
            _worldState = worldState;
        }

        public bool TrySend(
            string npcId,
            string locationId)
        {
            if (!_locations.TryGet(locationId, out var location))
            {
                Debug.LogWarning(
                    $"Cannot send NPC '{npcId}': location '{locationId}' was not found.");

                return false;
            }

            if (_agents.TryGet(npcId, out var agent))
            {
                return agent.TryMoveTo(
                    location.Transform,
                    arrived =>
                    {
                        if (arrived)
                            _worldState.TryMove(npcId, locationId);
                    });
            }

            Debug.LogWarning(
                $"Cannot send NPC '{npcId}': live NPC agent was not found.");

            return false;
        }

        public bool TrySendRoute(
            string npcId,
            string routeId)
        {
            if (!_routes.TryGet(routeId, out var route))
            {
                Debug.LogWarning(
                    $"Cannot send NPC '{npcId}': route '{routeId}' was not found.");

                return false;
            }

            if (route.Nodes == null || route.Nodes.Length == 0)
            {
                Debug.LogWarning(
                    $"Cannot send NPC '{npcId}': route '{routeId}' has no nodes.");

                return false;
            }

            var points = new List<Transform>();
            string finalLocationId = null;

            foreach (var node in route.Nodes)
            {
                if (node == null)
                    continue;

                points.Add(node.Transform);
                finalLocationId = node.Id;
            }

            if (points.Count == 0 || string.IsNullOrWhiteSpace(finalLocationId))
            {
                Debug.LogWarning(
                    $"Cannot send NPC '{npcId}': route '{routeId}' has no valid nodes.");

                return false;
            }

            if (_agents.TryGet(npcId, out var agent))
            {
                return agent.TryFollowRoute(
                    points,
                    arrived =>
                    {
                        if (arrived)
                            _worldState.TryMove(npcId, finalLocationId);
                    });
            }

            Debug.LogWarning(
                $"Cannot send NPC '{npcId}' through route '{routeId}': live NPC agent was not found.");

            return false;
        }
    }
}