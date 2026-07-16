using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Navigation
{
    public static class NavigationGraphBuilder
    {
        public static NavigationGraph Build(
            IEnumerable<NavigationWaypoint> waypoints)
        {
            if (waypoints == null)
                throw new ArgumentNullException(nameof(waypoints));

            var waypointsById =
                new Dictionary<string, NavigationWaypoint>();

            var nodesById =
                new Dictionary<string, NavigationNode>();

            foreach (var waypoint in waypoints)
            {
                if (waypoint == null)
                    continue;

                var id = waypoint.Id?.Trim();

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!waypointsById.TryAdd(id, waypoint))
                {
                    throw new InvalidOperationException(
                        $"Duplicate navigation waypoint id: '{id}'.");
                }

                nodesById.Add(
                    id,
                    new NavigationNode(
                        id,
                        waypoint.Position,
                        waypoint.Rotation,
                        waypoint.Radius,
                        waypoint.Flags));
            }

            var edgesByNodeId =
                new Dictionary<
                    string,
                    IReadOnlyList<NavigationEdge>>();

            foreach (var pair in waypointsById)
            {
                var fromNodeId = pair.Key;
                var fromWaypoint = pair.Value;

                var edges = new List<NavigationEdge>();

                foreach (var neighbor in fromWaypoint.Neighbors)
                {
                    if (neighbor == null ||
                        neighbor.Waypoint == null)
                    {
                        continue;
                    }

                    var toNodeId =
                        neighbor.Waypoint.Id?.Trim();

                    if (string.IsNullOrWhiteSpace(toNodeId) ||
                        !waypointsById.ContainsKey(toNodeId))
                    {
                        continue;
                    }

                    var cost = Vector3.Distance(
                        fromWaypoint.Position,
                        neighbor.Waypoint.Position);

                    edges.Add(
                        new NavigationEdge(
                            fromNodeId,
                            toNodeId,
                            cost,
                            neighbor.Flags));
                }

                edgesByNodeId.Add(
                    fromNodeId,
                    edges);
            }

            return new NavigationGraph(
                nodesById,
                edgesByNodeId);
        }
    }
}