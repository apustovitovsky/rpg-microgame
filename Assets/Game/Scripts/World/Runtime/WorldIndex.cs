using System;
using System.Collections.Generic;

namespace Game.World
{
    public sealed class WorldIndex<T>
        where T : class
    {
        private readonly Dictionary<WorldId, T> _items = new();

        public IDisposable Register(
            WorldId worldId,
            T value)
        {
            if (worldId.IsEmpty)
                throw new ArgumentException("World id is required.", nameof(worldId));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_items.TryAdd(worldId, value))
            {
                throw new InvalidOperationException(
                    $"World index already contains '{worldId}' for '{typeof(T).Name}'.");
            }

            return new LifetimeToken(
                () => Remove(worldId, value));
        }

        public bool TryGet(
            WorldId worldId,
            out T value)
        {
            value = null;

            if (worldId.IsEmpty)
                return false;

            return _items.TryGetValue(worldId, out value);
        }

        public bool Contains(WorldId worldId)
        {
            return !worldId.IsEmpty &&
                _items.ContainsKey(worldId);
        }

        private void Remove(
            WorldId worldId,
            T value)
        {
            if (worldId.IsEmpty)
                return;

            if (_items.TryGetValue(worldId, out var existing) &&
                ReferenceEquals(existing, value))
            {
                _items.Remove(worldId);
            }
        }
    }
}