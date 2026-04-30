using System;

namespace Etheria.Game.Actor
{
    public readonly struct ActorId : IEquatable<ActorId>
    {
        public Guid Value { get; }

        public ActorId(Guid value)
        {
            Value = value;
        }

        public bool Equals(ActorId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ActorId other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(ActorId left, ActorId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorId left, ActorId right)
        {
            return !left.Equals(right);
        }
    }
}
