using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public interface IWorldObject
    {
        WorldId WorldId { get; }

        GameObject GameObject { get; }

        Transform Root { get; }

        bool TryGet<TEndpoint>(out TEndpoint endpoint)
            where TEndpoint : class;
    }
    
    public sealed class WorldObject : IWorldObject
    {
        private readonly IReadOnlyDictionary<Type, object> _endpoints;

        public WorldObject(
            WorldId worldId,
            GameObject gameObject,
            IReadOnlyDictionary<Type, object> endpoints)
        {
            WorldId = worldId;
            GameObject = gameObject;
            _endpoints = endpoints ?? new Dictionary<Type, object>();
        }

        public WorldId WorldId { get; }

        public GameObject GameObject { get; }

        public Transform Root => GameObject.transform;

        public bool TryGet<TEndpoint>(out TEndpoint endpoint)
            where TEndpoint : class
        {
            if (_endpoints.TryGetValue(typeof(TEndpoint), out var value) &&
                value is TEndpoint typed)
            {
                endpoint = typed;
                return true;
            }

            endpoint = null;
            return false;
        }
    }
}