using System;

namespace Game.Actor
{
    public interface IActorPlacementService
    {
        void Register(
            Guid instanceId,
            ActorPlacement placement);

        bool TryGet(
            Guid instanceId,
            out ActorPlacement placement);

        bool Unregister(Guid instanceId);
    }
}