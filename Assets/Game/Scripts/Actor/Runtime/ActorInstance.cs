using System;
using Game.Core;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorInstance :
        WorldInstance,
        IFragmentProvider
    {
        public ActorInstance(
            Guid instanceId,
            ActorDefinition definition)
            : base(instanceId)
        {
            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
        }

        public ActorDefinition Definition { get; }

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