using System;
using Game.World;

namespace Game.Pickup
{
    public readonly struct PickupSpawnRequest
    {
        public PickupSpawnRequest(
            PickupDefinition definition,
            SpawnPlacement placement,
            Guid? instanceId = null)
        {
            Definition = definition;
            Placement = placement;
            InstanceId = instanceId;
        }

        public PickupDefinition Definition { get; }

        public SpawnPlacement Placement { get; }

        public Guid? InstanceId { get; }
    }
}