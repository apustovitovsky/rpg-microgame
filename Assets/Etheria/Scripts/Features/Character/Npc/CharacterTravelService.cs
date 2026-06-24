using System.Collections.Generic;
using Etheria.Game.Character;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class CharacterTravelService : ICharacterTravelService
    {
        private readonly ICharacterInstanceRegistry _instances;
        private readonly IWorldLocationRegistry _locations;
        private readonly ICharacterWorldStateService _worldState;

        private readonly IWorldRouteRegistry _routes;
        public CharacterTravelService(
            ICharacterInstanceRegistry instances,
            IWorldLocationRegistry locations,
            IWorldRouteRegistry routes,
            ICharacterWorldStateService worldState)
        {
            _instances = instances;
            _locations = locations;
            _routes = routes;
            _worldState = worldState;
        }

        public bool TrySend(
            string characterId,
            string locationId)
        {
            if (!_locations.TryGet(locationId, out var location))
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': location '{locationId}' was not found.");

                return false;
            }

            if (!_instances.TryGetInstance(characterId, out var instance))
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': live instance was not found.");

                return false;
            }

            var stateController =
                instance.GetComponentInChildren<NpcStateController>();

            if (stateController == null)
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': NpcStateController was not found.");

                return false;
            }

            return stateController.TravelTo(
                location.Transform.position,
                () => _worldState.TryMove(characterId, locationId));
        }

        public bool TrySendRoute(
            string characterId,
            string routeId)
        {
            if (!_routes.TryGet(routeId, out var route))
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': route '{routeId}' was not found.");

                return false;
            }

            if (route.Nodes == null || route.Nodes.Length == 0)
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': route '{routeId}' has no nodes.");

                return false;
            }

            if (!_instances.TryGetInstance(characterId, out var instance))
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': live instance was not found.");

                return false;
            }

            var stateController =
                instance.GetComponentInChildren<NpcStateController>();

            if (stateController == null)
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': NpcStateController was not found.");

                return false;
            }

            var points = new List<Vector3>();
            string finalLocationId = null;

            foreach (var node in route.Nodes)
            {
                if (node == null)
                    continue;

                points.Add(node.Transform.position);
                finalLocationId = node.Id;
            }

            if (points.Count == 0 || string.IsNullOrWhiteSpace(finalLocationId))
            {
                Debug.LogWarning(
                    $"Cannot send character '{characterId}': route '{routeId}' has no valid nodes.");

                return false;
            }

            return stateController.TravelRoute(
                points,
                () => _worldState.TryMove(characterId, finalLocationId));
        }
    }
}