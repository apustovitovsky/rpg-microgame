using System;

namespace Game.Item
{
    public sealed class ItemInstance
    {
        public ItemInstance(ItemDefinition definition)
        {
            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));

            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public ItemDefinition Definition { get; }

        public ItemInstance CreateSplitInstance()
        {
            return new ItemInstance(Definition);
        }
    }
}