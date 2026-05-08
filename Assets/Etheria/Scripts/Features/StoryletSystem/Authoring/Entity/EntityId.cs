using System;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public EntityId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(EntityId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }
    }
}
