using System;
using System.Collections.Generic;

namespace Etheria.Gameplay
{
    public sealed class PickupPoolRegistry
    {
        private readonly Dictionary<Type, PickupPool> _pools = new();

        public PickupPool GetOrCreate<T>() where T : Pickup
        {
            var type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = CreatePool(type);
                _pools.Add(type, pool);
            }

            return pool;
        }

        private PickupPool CreatePool(Type type)
        {
            throw new NotImplementedException();
        }
    }
}