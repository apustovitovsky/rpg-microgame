using System;
using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldCapabilityProvider
    {
        bool TryGet<TCapability>(
            out TCapability capability)
            where TCapability : class;
    }

    public sealed class WorldCapabilityProvider : IWorldCapabilityProvider
    {
        private readonly Dictionary<Type, IWorldCapability> _capabilities = new();

        public WorldCapabilityProvider(
            IEnumerable<IWorldCapability> capabilities)
        {
            foreach (var capability in capabilities)
                Register(capability);
        }

        public bool TryGet<TCapability>(
            out TCapability capability)
            where TCapability : class
        {
            if (_capabilities.TryGetValue(typeof(TCapability), out var value) &&
                value is TCapability typed)
            {
                capability = typed;
                return true;
            }

            capability = null;
            return false;
        }

        private void Register(IWorldCapability capability)
        {
            if (capability == null)
                return;

            foreach (var type in capability.PublishedTypes)
                Register(type, capability);
        }

        private void Register(
            Type type,
            IWorldCapability capability)
        {
            if (type == null)
                return;

            if (!type.IsInstanceOfType(capability))
            {
                throw new InvalidOperationException(
                    $"World capability '{capability.GetType().Name}' cannot be published as '{type.Name}'.");
            }

            if (!_capabilities.TryAdd(type, capability))
            {
                throw new InvalidOperationException(
                    $"Duplicate world capability for '{type.Name}'.");
            }
        }
    }
}