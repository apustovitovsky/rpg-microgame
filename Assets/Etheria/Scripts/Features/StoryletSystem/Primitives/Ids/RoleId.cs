using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct RoleId : IEquatable<RoleId>
    {
        public RoleId(int value)
        {
            Value = StoryletIdRange.EnsureValid(nameof(RoleId), value);
        }

        public int Value { get; }

        public bool Equals(RoleId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is RoleId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(RoleId left, RoleId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RoleId left, RoleId right)
        {
            return !left.Equals(right);
        }
    }
}
