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

        public bool Equals(ActorId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is ActorId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
