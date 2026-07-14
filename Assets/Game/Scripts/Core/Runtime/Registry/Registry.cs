using System;
using System.Collections.Generic;

namespace Game.Core
{
    public sealed class Registry<T> :
        IRegistry<T>,
        IRegistryWriter<T>
        where T : class
    {
        private readonly Dictionary<Guid, T> _items = new();

        public void Add(
            Guid id,
            T value)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id is required.",
                    nameof(id));
            }

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_items.TryAdd(id, value))
            {
                throw new InvalidOperationException(
                    $"Registry already contains '{id}' " +
                    $"for '{typeof(T).Name}'.");
            }
        }

        public bool Remove(
            Guid id,
            T expectedValue)
        {
            if (id == Guid.Empty ||
                expectedValue == null)
            {
                return false;
            }

            if (!_items.TryGetValue(
                    id,
                    out var existing) ||
                !ReferenceEquals(existing, expectedValue))
            {
                return false;
            }

            return _items.Remove(id);
        }

        public bool TryGet(
            Guid id,
            out T value)
        {
            value = null;

            return id != Guid.Empty &&
                   _items.TryGetValue(
                       id,
                       out value);
        }

        public bool Contains(Guid id)
        {
            return id != Guid.Empty &&
                   _items.ContainsKey(id);
        }
    }
}