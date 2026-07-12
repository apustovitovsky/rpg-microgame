using System;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorInstance :
        IWorldInstance
    {
        public ActorInstance(
            Guid instanceId,
            ActorDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Actor instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public string DisplayName => Definition.DisplayName;

        public ActorDefinition Definition { get; }
    }
}