using System;

namespace Game.Actor
{
    public interface IActorRuntimeRegistry
    {
        void Register(ActorRuntime runtime);

        bool TryGet(
            Guid instanceId,
            out ActorRuntime runtime);

        bool Unregister(Guid instanceId);
    }
}