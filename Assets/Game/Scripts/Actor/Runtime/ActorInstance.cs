using System;

namespace Game.Actor
{
    public sealed class ActorInstance
    {
        public ActorInstance(ActorDefinition definition)
            : this(Guid.NewGuid(), definition)
        {
        }

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
            Definition = definition != null ?
                definition :
                throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public ActorDefinition Definition { get; }
    }
}