using System;

namespace Game.Core
{
    public interface IRegistry<T>
        where T : class
    {
        IDisposable Register(
            Guid id,
            T value);

        bool TryGet(
            Guid id,
            out T value);

        bool Contains(Guid id);
    }
}