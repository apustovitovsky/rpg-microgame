using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct AttributeId : IEquatable<AttributeId>
    {
        public AttributeId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(AttributeId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AttributeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(AttributeId left, AttributeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AttributeId left, AttributeId right)
        {
            return !left.Equals(right);
        }
    }
}
