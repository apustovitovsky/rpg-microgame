using System;

namespace Etheria.Features.HWFC
{
    public sealed class Topology
    {
        public const int MissingNeighbor = -1;

        private readonly int[] _neighbors;

        public Topology(DirectionModel directionModel, int nodeCount, int[] neighbors)
        {
            DirectionModel = directionModel ?? throw new ArgumentNullException(nameof(directionModel));
            if (nodeCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeCount));
            }

            if (neighbors == null)
            {
                throw new ArgumentNullException(nameof(neighbors));
            }

            var expectedNeighborCount = nodeCount * DirectionModel.Count;
            if (neighbors.Length != expectedNeighborCount)
            {
                throw new ArgumentException("Neighbor array size does not match node and direction counts.", nameof(neighbors));
            }

            _neighbors = new int[neighbors.Length];
            Array.Copy(neighbors, _neighbors, neighbors.Length);

            for (var index = 0; index < _neighbors.Length; index++)
            {
                var neighborIndex = _neighbors[index];
                if (neighborIndex < MissingNeighbor || neighborIndex >= nodeCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(neighbors), "Neighbor index is out of range.");
                }
            }

            NodeCount = nodeCount;
        }

        public DirectionModel DirectionModel { get; }

        public int NodeCount { get; }

        public bool HasNeighbor(int nodeIndex, int direction)
        {
            return GetNeighbor(nodeIndex, direction) != MissingNeighbor;
        }

        public int GetNeighbor(int nodeIndex, int direction)
        {
            ValidateNodeIndex(nodeIndex);
            ValidateDirection(direction);
            return _neighbors[GetNeighborOffset(nodeIndex, direction)];
        }

        private int GetNeighborOffset(int nodeIndex, int direction)
        {
            return (nodeIndex * DirectionModel.Count) + direction;
        }

        private void ValidateNodeIndex(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= NodeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));
            }
        }

        private void ValidateDirection(int direction)
        {
            if (direction < 0 || direction >= DirectionModel.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }
    }
}
