using System;

namespace Game.World
{
    [Serializable]
    public readonly struct WorldId :
        IEquatable<WorldId>
    {
        private readonly string _value;

        public WorldId(string value)
        {
            _value = value?.Trim() ?? string.Empty;
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(_value);

        public override string ToString()
        {
            return _value ?? string.Empty;
        }

        public bool Equals(WorldId other)
        {
            return string.Equals(
                _value,
                other._value,
                StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is WorldId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_value ?? string.Empty);
        }

        public static bool operator ==(WorldId left, WorldId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldId left, WorldId right)
        {
            return !left.Equals(right);
        }

        public static implicit operator WorldId(string value)
        {
            return new WorldId(value);
        }
    }
}