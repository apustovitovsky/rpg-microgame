using System;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupInstance :
        WorldInstance
    {
        public PickupInstance(
            Guid instanceId,
            PickupDefinition definition)
            : base(instanceId)
        {
            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public PickupDefinition Definition { get; }

        public override string DisplayName =>
            Definition.DisplayName;

        public bool TryGetFragment<TFragment>(
            out TFragment fragment)
            where TFragment : PickupFragment
        {
            return Definition.TryGetFragment(out fragment);
        }
    }
}