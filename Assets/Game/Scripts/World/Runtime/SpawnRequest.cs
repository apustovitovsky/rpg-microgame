using System;

namespace Game.World
{
    public readonly struct SpawnRequest<TInstance>
        where TInstance : class, IWorldInstance
    {
        public SpawnRequest(
            WorldDefinition<TInstance> definition,
            SpawnPlacement placement,
            Guid? instanceId = null)
        {
            Definition = definition;
            Placement = placement;
            InstanceId = instanceId;
        }

        public WorldDefinition<TInstance> Definition { get; }

        public SpawnPlacement Placement { get; }

        public Guid? InstanceId { get; }
    }
}