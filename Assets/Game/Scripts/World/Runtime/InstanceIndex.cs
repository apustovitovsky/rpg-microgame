using System;
using System.Collections.Generic;

namespace Game.World
{
    public sealed class InstanceIndex<T>
        where T : class
    {
        private readonly Dictionary<Guid, T> _items = new();

        public IDisposable Register(
            Guid instanceId,
            T value)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instanceId));
            }

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_items.TryAdd(instanceId, value))
            {
                throw new InvalidOperationException(
                    $"Instance index already contains '{instanceId}' " +
                    $"for '{typeof(T).Name}'.");
            }

            return new LifetimeToken(
                () => Remove(instanceId, value));
        }

        public bool TryGet(
            Guid instanceId,
            out T value)
        {
            value = null;

            return instanceId != Guid.Empty &&
                   _items.TryGetValue(instanceId, out value);
        }

        public bool Contains(Guid instanceId)
        {
            return instanceId != Guid.Empty &&
                   _items.ContainsKey(instanceId);
        }

        private void Remove(
            Guid instanceId,
            T value)
        {
            if (_items.TryGetValue(
                    instanceId,
                    out var existing) &&
                ReferenceEquals(existing, value))
            {
                _items.Remove(instanceId);
            }
        }
    }
}