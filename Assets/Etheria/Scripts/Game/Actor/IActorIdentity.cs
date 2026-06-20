using System;

namespace Etheria.Game.Actor
{
    public interface IActorIdentity
    {
        Guid Id { get; }
        string DisplayName { get; }
    }
}