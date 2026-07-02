using System;

namespace Game.Actor
{
    public interface IActionGate
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