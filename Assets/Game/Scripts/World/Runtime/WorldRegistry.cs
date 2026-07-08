using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public interface IWorldRegistry<T>
        where T : class
    {
        IRegistrationToken Register(
            WorldId worldId,
            T value);

        bool TryGet(
            WorldId worldId,
            out T value);

        bool Contains(WorldId worldId);
    }

    public sealed class WorldRegistry<T> : IWorldRegistry<T>
        where T : class
    {
        private readonly Dictionary<WorldId, T> _items = new();

        public IRegistrationToken Register(
            WorldId worldId,
            T value)
        {
            if (worldId.IsEmpty)
                return new RegistrationToken(null);

            if (value == null)
                return new RegistrationToken(null);

            if (!_items.TryAdd(worldId, value))
            {
                throw new InvalidOperationException(
                    $"World registry already contains '{worldId}' for '{typeof(T).Name}'.");
            }

            return new RegistrationToken(
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

                Debug.Log(
                    $"[WorldRegistry] Removed {typeof(T).Name}: {worldId}");
            }
        }
    }
}