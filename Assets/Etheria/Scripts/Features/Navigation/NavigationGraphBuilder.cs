using System;
using System.Collections.Generic;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Navigation
{
    public static class NavigationGraphBuilder
    {
        public static NavigationGraph Build(
            IEnumerable<NavigationWaypoint> waypoints)
        {
            if (waypoints == null)
                throw new ArgumentNullException(nameof(waypoints));

            var waypointById = new Dictionary<string, NavigationWaypoint>();
            var nodesById = new Dictionary<string, NavigationNode>();

            foreach (var waypoint in waypoints)
            {
                if (waypoint == null)
                    continue;

                var id = waypoint.Id?.Trim();

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (waypointById.ContainsKey(id))
                    throw new InvalidOperationException($"Duplicate navigation waypoint id: {id}");

                waypointById.Add(id, waypoint);

                nodesById.Add(
                    id,
                    new NavigationNode(
                        id,
                        waypoint.Position,
                        waypoint.Rotation,
                        waypoint.Radius,
                        waypoint.Flags));
            }

            var edgesByNodeId = new Dictionary<string, IReadOnlyList<NavigationEdge>>();

            foreach (var pair in waypointById)
            {
                var fromId = pair.Key;
                var fromWaypoint = pair.Value;
                var edges = new List<NavigationEdge>();

                foreach (var neighbor in fromWaypoint.Neighbors)
                {
                    if (neighbor == null || neighbor.Waypoint == null)
                        continue;

                    var toId = neighbor.Waypoint.Id?.Trim();

                    if (string.IsNullOrWhiteSpace(toId))
                        continue;

                    if (!waypointById.ContainsKey(toId))
                        continue;

                    var cost = Vector3.Distance(
                                fromWaypoint.Position,
                                neighbor.Waypoint.Position);

                    edges.Add(
                        new NavigationEdge(
                            fromId,
                            toId,
                            cost,
                            neighbor.Flags));
                }

                edgesByNodeId.Add(
                    fromId,
                    edges);
            }

            return new NavigationGraph(
                nodesById,
                edgesByNodeId);
        }
    }
}