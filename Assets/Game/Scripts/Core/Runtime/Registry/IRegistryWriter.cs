using System;

namespace Game.Core
{
    public interface IRegistryWriter<T>
        where T : class
    {
        void Add(
            Guid id,
            T value);

        bool Remove(
            Guid id,
            T expectedValue);
    }
}