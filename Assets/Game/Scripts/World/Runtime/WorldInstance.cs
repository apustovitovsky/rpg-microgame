using System;
using Game.Core;

namespace Game.World
{
    public abstract class WorldInstance :
        IInstanceIdentity
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