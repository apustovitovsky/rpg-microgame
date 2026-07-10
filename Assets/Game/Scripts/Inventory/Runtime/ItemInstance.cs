using System;

namespace Game.Inventory
{
    public sealed class ItemInstance
    {
        public ItemInstance(ItemDefinition definition)
        {
            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));

            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public ItemDefinition Definition { get; }
    }
}