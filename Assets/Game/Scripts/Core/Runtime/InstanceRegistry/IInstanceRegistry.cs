using System;

namespace Game.Core
{
    public interface IInstanceRegistry<T>
        where T : class
    {
        IDisposable Register(
            Guid instanceId,
            T value);

        bool TryGet(
            Guid instanceId,
            out T value);

        bool Contains(Guid instanceId);
    }
}