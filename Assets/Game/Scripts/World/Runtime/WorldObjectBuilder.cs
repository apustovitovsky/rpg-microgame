using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public sealed class WorldObjectBuilder
    {
        private readonly Dictionary<Type, object> _endpoints = new();

        public WorldObjectBuilder Add<TEndpoint>(TEndpoint endpoint)
            where TEndpoint : class
        {
            if (endpoint == null)
                return this;

            var type = typeof(TEndpoint);

            if (!_endpoints.TryAdd(type, endpoint))
            {
                throw new InvalidOperationException(
                    $"Duplicate world endpoint '{type.Name}'.");
            }

            return this;
        }

        public IWorldObject Build(
            WorldId worldId,
            GameObject gameObject)
        {
            if (worldId.IsEmpty)
                throw new ArgumentException("World id is required.", nameof(worldId));

            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            return new WorldObject(
                worldId,
                gameObject,
                new Dictionary<Type, object>(_endpoints));
        }
    }
}