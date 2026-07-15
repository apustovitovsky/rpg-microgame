using System;
using Game.Core;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupInstance :
        WorldInstance,
        IFragmentProvider
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
            where TFragment : class
        {
            return Definition.TryGetFragment(
                out fragment);
        }
    }
}