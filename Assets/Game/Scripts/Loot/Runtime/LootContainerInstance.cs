using System;
using Game.World;

namespace Game.Loot
{
    public sealed class LootContainerInstance :
        IWorldInstance
    {
        public LootContainerInstance(
            Guid instanceId,
            LootContainerDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot container instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public string DisplayName => Definition.DisplayName;

        public LootContainerDefinition Definition { get; }
    }
}