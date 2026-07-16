По аналогии с твоим `Registry<T>` я бы сделал **один generic `EventBus`**, разделив API на публикацию и подписку. Твой registry уже использует похожий принцип: отдельные read/write-интерфейсы, одна реализация и автоматическое снятие binding через `IDisposable`. ([GitHub][1])

## Структура

```text
Core/Runtime/Events/
├── IEventPublisher.cs
├── IEventSubscriber.cs
├── EventBus.cs
└── EventSubscription.cs
```

```csharp
public interface IEventPublisher
{
    void Publish<TEvent>(TEvent eventData);
}
```

```csharp
using System;

public interface IEventSubscriber
{
    IDisposable Subscribe<TEvent>(
        Action<TEvent> handler);
}
```

```csharp
using System;

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
```

```csharp
using System;
using System.Collections.Generic;

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
```

## Регистрация VContainer

```csharp
builder.Register<EventBus>(Lifetime.Scoped)
    .AsImplementedInterfaces();
```

Я бы держал bus в gameplay/scene scope, а не глобальным static singleton.

## События диалога

```csharp
public readonly record struct DialogueSessionOpened(
    DialogueSession Session);

public readonly record struct DialogueSessionClosed(
    DialogueSession Session);
```

В текущем `DialogueCoordinator`:

```csharp
private readonly IDialogueExecutor _executor;
private readonly IEventPublisher _events;

public DialogueCoordinator(
    IDialogueExecutor executor,
    IEventPublisher events)
{
    _executor = executor;
    _events = events;
}
```

После установки активной сессии:

```csharp
_activeSession = session;

_events.Publish(
    new DialogueSessionOpened(session));

RunSessionAsync(
    session,
    cancellationToken).Forget();
```

И в `finally`:

```csharp
if (_activeSession == session)
{
    _activeSession = null;

    _events.Publish(
        new DialogueSessionClosed(session));
}
```

Это точно соответствует текущему lifecycle: координатор устанавливает `_activeSession`, запускает `ExecuteAsync` и очищает сессию в `finally`. ([GitHub][2])

## Подписчик

```csharp
using System;
using VContainer.Unity;

public sealed class DialogueUiEvents :
    IInitializable,
    IDisposable
{
    private readonly IEventSubscriber _events;
    private IDisposable _openedSubscription;
    private IDisposable _closedSubscription;

    public DialogueUiEvents(
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
```

Для начала этого достаточно. Не добавлял бы пока `IGameEvent`, приоритеты, асинхронных handlers или адресацию по `InstanceId`: обычные события должны быть широковещательными и синхронными.
