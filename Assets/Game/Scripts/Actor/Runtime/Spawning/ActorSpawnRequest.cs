using System;
using Game.World;

namespace Game.Actor
{
    public readonly struct ActorSpawnRequest
    {
        public ActorSpawnRequest(
            ActorDefinition definition,
            SpawnPlacement placement,
            Guid? instanceId = null)
        {
            Definition = definition;
            Placement = placement;
            InstanceId = instanceId;
        }

        public ActorDefinition Definition { get; }

        public SpawnPlacement Placement { get; }

        public Guid? InstanceId { get; }
    }
}