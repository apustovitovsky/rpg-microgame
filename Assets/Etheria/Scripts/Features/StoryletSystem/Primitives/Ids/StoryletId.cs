using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct StoryletId : IEquatable<StoryletId>
    {
        public StoryletId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(StoryletId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is StoryletId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(StoryletId left, StoryletId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StoryletId left, StoryletId right)
        {
            return !left.Equals(right);
        }
    }
}
