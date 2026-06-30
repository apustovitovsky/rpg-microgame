using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Game.World
{
    public sealed class NavigationPath
    {
        public static NavigationPath Empty { get; } =
            new(Array.Empty<string>(), 0f);

        private readonly IReadOnlyList<string> _nodeIds;

        public IReadOnlyList<string> NodeIds =>
            _nodeIds;

        public float TotalCost { get; }

        public bool IsEmpty =>
            _nodeIds.Count == 0;

        public string StartNodeId =>
            IsEmpty ? string.Empty : _nodeIds[0];

        public string EndNodeId =>
            IsEmpty ? string.Empty : _nodeIds[^1];

        public NavigationPath(
            IEnumerable<string> nodeIds,
            float totalCost)
        {
            if (nodeIds == null)
                throw new ArgumentNullException(nameof(nodeIds));

            _nodeIds = nodeIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToArray();

            TotalCost = totalCost;
        }
    }
}