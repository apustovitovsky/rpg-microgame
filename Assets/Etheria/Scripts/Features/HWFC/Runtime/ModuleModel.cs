using System;
using System.Collections.Generic;

namespace Etheria.Features.HWFC
{
    public sealed class ModuleModel
    {
        private readonly float[] _weights;
        private readonly float[] _weightLogWeights;
        private readonly ModuleSet[] _allowedNeighborsByModuleDirection;

        public ModuleModel(
            DirectionModel directionModel,
            float[] weights,
            ModuleSet[] allowedNeighborsByModuleDirection)
        {
            DirectionModel = directionModel ?? throw new ArgumentNullException(nameof(directionModel));
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            if (allowedNeighborsByModuleDirection == null)
            {
                throw new ArgumentNullException(nameof(allowedNeighborsByModuleDirection));
            }

            if (weights.Length == 0)
            {
                throw new ArgumentException("Module model must contain at least one module.", nameof(weights));
            }

            if (weights.Length > BitMask256.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(weights), "Module count exceeds ModuleSet capacity.");
            }

            var expectedConstraintCount = weights.Length * DirectionModel.Count;
            if (allowedNeighborsByModuleDirection.Length != expectedConstraintCount)
            {
                throw new ArgumentException("Allowed neighbor array size does not match module and direction counts.", nameof(allowedNeighborsByModuleDirection));
            }

            _weights = new float[weights.Length];
            _weightLogWeights = new float[weights.Length];
            Array.Copy(weights, _weights, weights.Length);

            for (var moduleIndex = 0; moduleIndex < _weights.Length; moduleIndex++)
            {
                if (_weights[moduleIndex] <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(weights), "All module weights must be positive.");
                }

                _weightLogWeights[moduleIndex] = _weights[moduleIndex] * (float)Math.Log(_weights[moduleIndex]);
            }

            _allowedNeighborsByModuleDirection = new ModuleSet[allowedNeighborsByModuleDirection.Length];
            Array.Copy(allowedNeighborsByModuleDirection, _allowedNeighborsByModuleDirection, allowedNeighborsByModuleDirection.Length);

            for (var index = 0; index < _allowedNeighborsByModuleDirection.Length; index++)
            {
                if (_allowedNeighborsByModuleDirection[index].ModuleCount != _weights.Length)
                {
                    throw new ArgumentException("Every neighbor constraint must use the model module count.", nameof(allowedNeighborsByModuleDirection));
                }
            }

            Universe = ModuleSet.Full(_weights.Length);
        }

        public DirectionModel DirectionModel { get; }

        public int ModuleCount => _weights.Length;

        public IReadOnlyList<float> Weights => _weights;

        public IReadOnlyList<float> WeightLogWeights => _weightLogWeights;

        public ModuleSet Universe { get; }

        public ModuleSet GetAllowedNeighbors(int moduleIndex, int direction)
        {
            ValidateModuleIndex(moduleIndex);
            ValidateDirection(direction);
            return _allowedNeighborsByModuleDirection[GetConstraintIndex(moduleIndex, direction)];
        }

        private int GetConstraintIndex(int moduleIndex, int direction)
        {
            return (moduleIndex * DirectionModel.Count) + direction;
        }

        private void ValidateModuleIndex(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= ModuleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleIndex));
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
