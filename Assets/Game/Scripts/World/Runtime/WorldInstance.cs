using System;

namespace Game.World
{
    public abstract class WorldInstance
    {
        protected WorldInstance(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "World instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;
        }

        public Guid InstanceId { get; }

        public abstract string DisplayName { get; }
    }
}