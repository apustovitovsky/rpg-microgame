using System;
using System.Collections.Generic;

namespace Etheria.Game.World
{
    public sealed class NavigationGraph
    {
        private static readonly IReadOnlyList<NavigationEdge> EmptyEdges =
            Array.Empty<NavigationEdge>();

        private readonly IReadOnlyDictionary<string, NavigationNode> _nodesById;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<NavigationEdge>> _edgesByNodeId;

        public IEnumerable<NavigationNode> Nodes =>
            _nodesById.Values;

        public NavigationGraph(
            IReadOnlyDictionary<string, NavigationNode> nodesById,
            IReadOnlyDictionary<string, IReadOnlyList<NavigationEdge>> edgesByNodeId)
        {
            _nodesById = nodesById ?? throw new ArgumentNullException(nameof(nodesById));
            _edgesByNodeId = edgesByNodeId ?? throw new ArgumentNullException(nameof(edgesByNodeId));
        }

        public bool TryGetNode(
            string nodeId,
            out NavigationNode node)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                node = null;
                return false;
            }

            return _nodesById.TryGetValue(
                nodeId.Trim(),
                out node);
        }

        public IReadOnlyList<NavigationEdge> GetEdges(
            string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return EmptyEdges;

            return _edgesByNodeId.TryGetValue(
                nodeId.Trim(),
                out var edges)
                    ? edges
                    : EmptyEdges;
        }
    }
}