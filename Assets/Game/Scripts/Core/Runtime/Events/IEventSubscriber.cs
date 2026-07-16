using System;

namespace Game.Core
{
    public interface IEventSubscriber
    {
        IDisposable Subscribe<TEvent>(
            Action<TEvent> handler);
    }
}