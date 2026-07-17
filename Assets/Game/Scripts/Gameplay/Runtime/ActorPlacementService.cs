using System;
using System.Collections.Generic;
using Game.Actor;

namespace Game.Gameplay
{
    public sealed class ActorPlacementService :
        IActorPlacementService
    {
        private readonly Dictionary<Guid, ActorPlacement>
            _placements = new();

        public void Register(
            Guid instanceId,
            ActorPlacement placement)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Actor instance id cannot be empty.",
                    nameof(instanceId));
            }

            if (placement == null)
            {
                throw new ArgumentNullException(
                    nameof(placement));
            }

            if (!_placements.TryAdd(
                    instanceId,
                    placement))
            {
                throw new InvalidOperationException(
                    $"Actor placement for instance " +
                    $"'{instanceId}' is already registered.");
            }
        }

        public bool TryGet(
            Guid instanceId,
            out ActorPlacement placement)
        {
            placement = null;

            return instanceId != Guid.Empty &&
                   _placements.TryGetValue(
                       instanceId,
                       out placement);
        }

        public bool Unregister(Guid instanceId)
        {
            return instanceId != Guid.Empty &&
                   _placements.Remove(instanceId);
        }
    }
}