using System;

namespace Etheria.Features.HWFC
{
    public sealed class DirectionModel
    {
        public static DirectionModel Cartesian3D { get; } = new(new[] { 3, 4, 5, 0, 1, 2 });

        private readonly int[] _opposite;

        public DirectionModel(int[] opposite)
        {
            if (opposite == null)
            {
                throw new ArgumentNullException(nameof(opposite));
            }

            if (opposite.Length == 0)
            {
                throw new ArgumentException("Direction model must contain at least one direction.", nameof(opposite));
            }

            _opposite = new int[opposite.Length];
            Array.Copy(opposite, _opposite, opposite.Length);

            for (var direction = 0; direction < _opposite.Length; direction++)
            {
                var oppositeDirection = _opposite[direction];
                if (oppositeDirection < 0 || oppositeDirection >= _opposite.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(opposite), "Opposite direction index is out of range.");
                }
            }

            for (var direction = 0; direction < _opposite.Length; direction++)
            {
                if (_opposite[_opposite[direction]] != direction)
                {
                    throw new ArgumentException("Direction opposites must be symmetrical.", nameof(opposite));
                }
            }
        }

        public int Count => _opposite.Length;

        public int GetOpposite(int direction)
        {
            ValidateDirection(direction);
            return _opposite[direction];
        }

        public bool IsEquivalentTo(DirectionModel other)
        {
            if (other == null || other.Count != Count)
            {
                return false;
            }

            for (var direction = 0; direction < Count; direction++)
            {
                if (_opposite[direction] != other._opposite[direction])
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateDirection(int direction)
        {
            if (direction < 0 || direction >= _opposite.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }
    }
}
