using System;

namespace Etheria.Game.Commands
{
    public interface IActorActionGate
    {
        bool TryEnter(
            string actorId,
            ActorActionChannel channel,
            ActorActionChannel blocks,
            out IDisposable scope);

        bool IsBlocked(
            string actorId,
            ActorActionChannel channel);
    }
}