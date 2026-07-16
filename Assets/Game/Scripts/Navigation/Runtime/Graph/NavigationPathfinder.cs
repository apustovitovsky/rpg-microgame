using System;
using System.Collections.Generic;

namespace Game.Navigation
{
    public sealed class NavigationPathfinder :
        INavigationPathfinder
    {
        public NavigationPath FindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId)
        {
            return FindPath(
                graph,
                fromNodeId,
                toNodeId,
                NavigationQueryFilter.Any);
        }

        public NavigationPath FindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter)
        {
            return TryFindPath(
                    graph,
                    fromNodeId,
                    toNodeId,
                    filter,
                    out var path)
                ? path
                : NavigationPath.Empty;
        }

        public bool TryFindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter,
            out NavigationPath path)
        {
            path = NavigationPath.Empty;

            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            fromNodeId = fromNodeId?.Trim() ?? string.Empty;
            toNodeId = toNodeId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fromNodeId) ||
                string.IsNullOrWhiteSpace(toNodeId))
            {
                return false;
            }

            if (!IsTraversableNode(
                    graph,
                    fromNodeId,
                    filter) ||
                !IsTraversableNode(
                    graph,
                    toNodeId,
                    filter))
            {
                return false;
            }

            if (fromNodeId == toNodeId)
            {
                path = new NavigationPath(
                    new[] { fromNodeId },
                    0f);

                return true;
            }

            var distances = new Dictionary<string, float>
            {
                [fromNodeId] = 0f
            };

            var previous =
                new Dictionary<string, string>();

            var openSet = new HashSet<string>
            {
                fromNodeId
            };

            var closedSet = new HashSet<string>();

            while (openSet.Count > 0)
            {
                if (!TryGetLowestCostOpenNodeId(
                        openSet,
                        distances,
                        out var currentNodeId))
                {
                    break;
                }

                openSet.Remove(currentNodeId);

                if (!closedSet.Add(currentNodeId))
                    continue;

                if (currentNodeId == toNodeId)
                {
                    path = BuildPath(
                        previous,
                        fromNodeId,
                        toNodeId,
                        distances[toNodeId]);

                    return !path.IsEmpty;
                }

                var currentDistance =
                    distances[currentNodeId];

                foreach (var edge in graph.GetEdges(
                             currentNodeId))
                {
                    if (!IsTraversableEdge(
                            graph,
                            edge,
                            filter,
                            closedSet))
                    {
                        continue;
                    }

                    var nextDistance =
                        currentDistance + edge.Cost;

                    if (distances.TryGetValue(
                            edge.ToNodeId,
                            out var knownDistance) &&
                        nextDistance >= knownDistance)
                    {
                        continue;
                    }

                    distances[edge.ToNodeId] =
                        nextDistance;

                    previous[edge.ToNodeId] =
                        currentNodeId;

                    openSet.Add(edge.ToNodeId);
                }
            }

            return false;
        }

        private static bool IsTraversableNode(
            NavigationGraph graph,
            string nodeId,
            NavigationQueryFilter filter)
        {
            return graph.TryGetNode(
                       nodeId,
                       out var node) &&
                   filter.IsNodeAllowed(node);
        }

        private static bool IsTraversableEdge(
            NavigationGraph graph,
            NavigationEdge edge,
            NavigationQueryFilter filter,
            HashSet<string> closedSet)
        {
            if (edge.Cost < 0f ||
                string.IsNullOrWhiteSpace(
                    edge.ToNodeId) ||
                !filter.IsEdgeAllowed(edge) ||
                closedSet.Contains(edge.ToNodeId))
            {
                return false;
            }

            return IsTraversableNode(
                graph,
                edge.ToNodeId,
                filter);
        }

        private static bool TryGetLowestCostOpenNodeId(
            IEnumerable<string> nodeIds,
            IReadOnlyDictionary<string, float> distances,
            out string result)
        {
            result = null;
            var lowestCost = float.PositiveInfinity;

            foreach (var nodeId in nodeIds)
            {
                if (!distances.TryGetValue(
                        nodeId,
                        out var cost) ||
                    cost >= lowestCost)
                {
                    continue;
                }

                result = nodeId;
                lowestCost = cost;
            }

            return result != null;
        }

        private static NavigationPath BuildPath(
            IReadOnlyDictionary<string, string> previous,
            string fromNodeId,
            string toNodeId,
            float totalCost)
        {
            var nodeIds = new List<string>();
            var currentNodeId = toNodeId;

            nodeIds.Add(currentNodeId);

            while (currentNodeId != fromNodeId)
            {
                if (!previous.TryGetValue(
                        currentNodeId,
                        out var previousNodeId))
                {
                    return NavigationPath.Empty;
                }

                currentNodeId = previousNodeId;
                nodeIds.Add(currentNodeId);
            }

            nodeIds.Reverse();

            return new NavigationPath(
                nodeIds,
                totalCost);
        }
    }
}