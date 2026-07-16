using System;
using System.Collections.Generic;

namespace Game.Core
{
    public sealed class EventBus :
    IEventPublisher,
    IEventSubscriber
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public IDisposable Subscribe<TEvent>(
            Action<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(eventType, handlers);
            }

            handlers.Add(handler);

            return new EventSubscription(
                () => Unsubscribe(handler));
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            if (!_handlers.TryGetValue(
                    typeof(TEvent),
                    out var handlers))
            {
                return;
            }

            // Подписчик может отписаться во время обработки.
            var snapshot = handlers.ToArray();

            foreach (var handler in snapshot)
            {
                ((Action<TEvent>)handler).Invoke(eventData);
            }
        }

        private void Unsubscribe<TEvent>(
            Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out var handlers))
                return;

            handlers.Remove(handler);

            if (handlers.Count == 0)
                _handlers.Remove(eventType);
        }
    }
}