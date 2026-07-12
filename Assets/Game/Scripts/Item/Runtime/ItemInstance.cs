using System;

namespace Game.Item
{
    public sealed class ItemInstance
    {
        public ItemInstance(
            Guid instanceId,
            ItemDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Item instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public ItemDefinition Definition { get; }
    }
}