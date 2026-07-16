using System;

namespace Game.Core
{
    internal sealed class EventSubscription : IDisposable
    {
        private Action _unsubscribe;

        public EventSubscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe
                ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        public void Dispose()
        {
            var unsubscribe = _unsubscribe;
            _unsubscribe = null;

            unsubscribe?.Invoke();
        }
    }
}