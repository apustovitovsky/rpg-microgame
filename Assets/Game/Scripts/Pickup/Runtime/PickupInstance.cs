using System;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupInstance :
        IWorldInstance
    {
        public PickupInstance(
            Guid instanceId,
            PickupDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Pickup instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public string DisplayName => Definition.DisplayName;

        public PickupDefinition Definition { get; }
    }
}