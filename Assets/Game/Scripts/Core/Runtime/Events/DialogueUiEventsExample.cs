using System;
using VContainer.Unity;

namespace Game.Core
{
    public readonly struct DialogueSessionOpened
    {
    }

    public readonly struct DialogueSessionClosed
    {
    }

    public sealed class DialogueUiEventsExample :
    IInitializable,
    IDisposable
    {
        private readonly IEventSubscriber _events;
        private IDisposable _openedSubscription;
        private IDisposable _closedSubscription;

        public DialogueUiEventsExample(
            IEventSubscriber events)
        {
            _events = events;
        }

        public void Initialize()
        {
            _openedSubscription =
                _events.Subscribe<DialogueSessionOpened>(OnOpened);

            _closedSubscription =
                _events.Subscribe<DialogueSessionClosed>(OnClosed);
        }

        public void Dispose()
        {
            _openedSubscription?.Dispose();
            _closedSubscription?.Dispose();
        }

        private void OnOpened(DialogueSessionOpened eventData)
        {
            // Открыть UI.
        }

        private void OnClosed(DialogueSessionClosed eventData)
        {
            // Закрыть UI.
        }
    }
}