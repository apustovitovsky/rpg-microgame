Да, это хорошая идея. Я бы даже сказал: **это лучше, чем глобально выставлять `IWorldRegistry<T>` наружу**.

Тогда архитектура становится такой:

```text id="2fwc4y"
InteractionService
  внутри имеет registry/interactors/interactables

DisplayService
  внутри имеет registry/displayables

PossessionService
  внутри имеет registry/possessables

WorldLifetime
  хранит просто IDisposable cleanup actions
```

То есть registry становится **implementation detail конкретного сервиса**, а не глобальной инфраструктурой.

## Как это выглядит

Не так:

```csharp id="11dxwn"
public sealed class InteractionService
{
    public InteractionService(
        IWorldRegistry<IInteractor> interactors,
        IWorldRegistry<IInteractable> interactables)
    {
    }
}
```

А так:

```csharp id="tv5ut9"
public sealed class InteractionService :
    IInteractionService,
    IInteractionRegistration
{
    private readonly WorldIndex<IInteractor> _interactors = new();
    private readonly WorldIndex<IInteractable> _interactables = new();

    public IDisposable RegisterInteractor(WorldId id, IInteractor interactor)
    {
        return _interactors.Register(id, interactor);
    }

    public IDisposable RegisterInteractable(WorldId id, IInteractable interactable)
    {
        return _interactables.Register(id, interactable);
    }

    public bool CanInteract(WorldId interactorId, WorldId targetId)
    {
        return _interactors.TryGet(interactorId, out var interactor)
            && _interactables.TryGet(targetId, out var interactable)
            && interactable.CanInteract(interactor);
    }

    public UniTask InteractAsync(
        WorldId interactorId,
        WorldId targetId,
        CancellationToken cancellationToken)
    {
        if (!_interactors.TryGet(interactorId, out var interactor))
            return UniTask.CompletedTask;

        if (!_interactables.TryGet(targetId, out var interactable))
            return UniTask.CompletedTask;

        return interactable.InteractAsync(interactor, cancellationToken);
    }
}
```

## Главное — разделить service API и registration API

Наружу для gameplay-кода:

```csharp id="wkefcu"
public interface IInteractionService
{
    bool CanInteract(WorldId interactorId, WorldId targetId);

    UniTask InteractAsync(
        WorldId interactorId,
        WorldId targetId,
        CancellationToken cancellationToken);
}
```

А для factory/registrar/composition layer:

```csharp id="50lptr"
public interface IInteractionRegistration
{
    IDisposable RegisterInteractor(WorldId id, IInteractor interactor);

    IDisposable RegisterInteractable(WorldId id, IInteractable interactable);
}
```

То есть `InteractionService` физически один, но виден под разными интерфейсами.

Gameplay-системы получают:

```csharp id="sf059y"
IInteractionService
```

А `ActorFactory` / `ActorRegistrar` получает:

```csharp id="h5lqey"
IInteractionRegistration
```

Это гораздо лучше, чем раздавать всем:

```csharp id="2v27u2"
IWorldRegistry<IInteractor>
IWorldRegistry<IInteractable>
```

## Внутренний абстрактный registry

Можно сделать маленький internal/helper класс:

```csharp id="ec7tlq"
internal sealed class WorldIndex<T> where T : class
{
    private readonly Dictionary<WorldId, T> _items = new();

    public IDisposable Register(WorldId id, T item)
    {
        if (id.IsEmpty)
            throw new ArgumentException("WorldId is empty.", nameof(id));

        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (!_items.TryAdd(id, item))
            throw new InvalidOperationException(
                $"Item of type {typeof(T).Name} with id {id} is already registered.");

        return new DisposableAction(() =>
        {
            if (_items.TryGetValue(id, out var current) &&
                ReferenceEquals(current, item))
            {
                _items.Remove(id);
            }
        });
    }

    public bool TryGet(WorldId id, out T item)
    {
        return _items.TryGetValue(id, out item);
    }
}
```

И disposable token:

```csharp id="hov5ry"
public sealed class DisposableAction : IDisposable
{
    private Action _onDispose;
    private bool _disposed;

    public DisposableAction(Action onDispose)
    {
        _onDispose = onDispose;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        var onDispose = _onDispose;
        _onDispose = null;

        onDispose?.Invoke();
    }
}
```

## Да, `IRegistrationToken` можно заменить на `IDisposable`

В твоем случае можно спокойно сделать так:

```csharp id="cj98aw"
public sealed class WorldLifetime : IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly GameObject _root;
    private bool _disposed;

    public WorldLifetime(WorldId worldId, GameObject root)
    {
        WorldId = worldId;
        _root = root;
    }

    public WorldId WorldId { get; }

    public bool IsDisposed => _disposed;

    public void Add(IDisposable disposable)
    {
        if (disposable == null)
            return;

        if (_disposed)
        {
            disposable.Dispose();
            return;
        }

        _disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _disposables.Dispose();

        if (_root != null)
            UnityEngine.Object.Destroy(_root);
    }
}
```

Где `CompositeDisposable`:

```csharp id="h0fycq"
public sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _items = new();
    private bool _disposed;

    public void Add(IDisposable item)
    {
        if (item == null)
            return;

        if (_disposed)
        {
            item.Dispose();
            return;
        }

        _items.Add(item);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        for (var i = _items.Count - 1; i >= 0; i--)
            _items[i].Dispose();

        _items.Clear();
    }
}
```

`IRegistrationToken` можно оставить только если хочешь семантически показать:

```text id="5hkl1m"
это не любой IDisposable, а именно token регистрации
```

Но технически `IDisposable` достаточно.

## Тогда actor registration выглядит так

```csharp id="yqftmu"
var lifetime = new WorldLifetime(id, root);

lifetime.Add(_actorRegistration.Register(id, actor));
lifetime.Add(_interactionRegistration.RegisterInteractor(id, actorInteractor));
lifetime.Add(_interactionRegistration.RegisterInteractable(id, actorInteractable));
lifetime.Add(_displayRegistration.Register(id, actorDisplayable));
lifetime.Add(_possessionRegistration.Register(id, actorPossessable));

_worldManager.Track(lifetime);
```

А pickup:

```csharp id="pl7p2f"
var lifetime = new WorldLifetime(id, root);

lifetime.Add(_pickupRegistration.Register(id, pickup));
lifetime.Add(_interactionRegistration.RegisterInteractable(id, pickupInteractable));
lifetime.Add(_displayRegistration.Register(id, pickupDisplayable));

_worldManager.Track(lifetime);
```

## Что получается в итоге

Вместо глобальных registry:

```text id="9v2l1o"
IWorldRegistry<IInteractable>
IWorldRegistry<IInteractor>
IWorldRegistry<IDisplayable>
IWorldRegistry<IPossessable>
```

у тебя feature-owned registries:

```text id="qmdzhq"
InteractionService
  owns interactors/interactables

DisplayService
  owns displayables

PossessionService
  owns possessables

ActorManager
  owns actors

PickupManager
  owns pickups
```

И наружу они дают только нужные интерфейсы:

```text id="mze59d"
IInteractionService
IInteractionRegistration

IDisplayService
IDisplayRegistration

IPossessionService
IPossessionRegistration

IActorProvider / IActorRegistration
IPickupProvider / IPickupRegistration
```

## Это лучше по смыслу

Потому что `IInteractable` нужен не “миру вообще”, а конкретно `InteractionService`.

`IDisplayable` нужен не “миру вообще”, а UI/display layer.

`IPossessable` нужен не “миру вообще”, а possession/player-control layer.

Значит пусть эти системы сами владеют своими индексами.

## Самое важное правило

```text id="w9n6kx"
WorldLifetime не знает о реестрах.
WorldManager не знает о реестрах.
Feature service владеет своим registry.
Factory/Registrar получает registration ports и кладет IDisposable tokens в WorldLifetime.
```

Это, на мой взгляд, чище, чем глобальный `IWorldRegistry<T>` как отдельная инфраструктура.


Да, так можно. Это даже логичнее, чем держать наружу `WorldRegistry<IWorldActor>`.

Но я бы формулировал не так:

```text id="m02cbj"
InteractionManager обращается к WorldActorManager
```

а так:

```text id="h9ndv0"
InteractionManager зависит от IActorProvider
```

То есть конкретный `WorldActorManager` скрывает registry, но наружу дает узкий интерфейс.

## Нормальная схема

```text id="6h2zcx"
WorldActorManager
  внутри:
    WorldIndex<IWorldActor>

  наружу:
    IActorProvider
    IActorRegistration
```

```csharp id="q8oxd9"
public interface IActorProvider
{
    bool TryGet(WorldId id, out IWorldActor actor);
}

public interface IActorRegistration
{
    IDisposable Register(IWorldActor actor);
}
```

```csharp id="hnk2rr"
public sealed class WorldActorManager :
    IActorProvider,
    IActorRegistration
{
    private readonly WorldIndex<IWorldActor> _actors = new();

    public IDisposable Register(IWorldActor actor)
    {
        return _actors.Register(actor.WorldId, actor);
    }

    public bool TryGet(WorldId id, out IWorldActor actor)
    {
        return _actors.TryGet(id, out actor);
    }
}
```

Теперь у тебя нет публичного:

```csharp id="qo33q9"
IWorldRegistry<IWorldActor>
```

Есть только actor-specific API.

## InteractionManager может использовать actor provider

Если `interactor` у тебя теперь всегда actor, то `IInteractor` реально можно убрать.

Тогда:

```csharp id="z0l51x"
public sealed class InteractionManager :
    IInteractionService,
    IInteractionRegistration
{
    private readonly IActorProvider _actors;
    private readonly WorldIndex<IInteractable> _interactables = new();

    public InteractionManager(IActorProvider actors)
    {
        _actors = actors;
    }

    public IDisposable RegisterInteractable(WorldId id, IInteractable interactable)
    {
        return _interactables.Register(id, interactable);
    }

    public bool CanInteract(WorldId actorId, WorldId targetId)
    {
        if (!_actors.TryGet(actorId, out var actor))
            return false;

        if (!_interactables.TryGet(targetId, out var interactable))
            return false;

        var context = new InteractionContext(actor);

        return interactable.CanInteract(context);
    }

    public UniTask InteractAsync(
        WorldId actorId,
        WorldId targetId,
        CancellationToken cancellationToken)
    {
        if (!_actors.TryGet(actorId, out var actor))
            return UniTask.CompletedTask;

        if (!_interactables.TryGet(targetId, out var interactable))
            return UniTask.CompletedTask;

        var context = new InteractionContext(actor);

        return interactable.InteractAsync(context, cancellationToken);
    }
}
```

То есть раньше было:

```text id="bf9n53"
InteractionManager
  -> IInteractor registry
  -> IInteractable registry
```

Теперь:

```text id="90vb75"
InteractionManager
  -> IActorProvider
  -> internal IInteractable index
```

Это нормально.

## Тогда `InteractionContext`

```csharp id="fytxuk"
public readonly struct InteractionContext
{
    public InteractionContext(IWorldActor actor)
    {
        Actor = actor;
    }

    public IWorldActor Actor { get; }

    public WorldId ActorId => Actor.WorldId;
}
```

Если потом нужно добавить больше данных:

```csharp id="3h1310"
public readonly struct InteractionContext
{
    public IWorldActor Actor { get; }
    public Vector3 Origin => Actor.View.Root.position;
    public ActorDefinition Definition => Actor.Definition;
}
```

## Регистрация через `WorldLifetime`

Actor spawn:

```csharp id="wxclwu"
var lifetime = new WorldLifetime(id, root);

lifetime.Add(_actorRegistration.Register(actor));
lifetime.Add(_interactionRegistration.RegisterInteractable(id, actorInteractable));
lifetime.Add(_displayRegistration.Register(id, actorDisplayable));
lifetime.Add(_possessionRegistration.Register(id, actorPossessable));

_worldManager.Track(lifetime);
```

Pickup spawn:

```csharp id="vja4p6"
var lifetime = new WorldLifetime(id, root);

lifetime.Add(_pickupRegistration.Register(pickup));
lifetime.Add(_interactionRegistration.RegisterInteractable(id, pickupInteractable));
lifetime.Add(_displayRegistration.Register(id, pickupDisplayable));

_worldManager.Track(lifetime);
```

`WorldLifetime` все еще один и тот же:

```text id="dgvj7c"
Dispose()
  -> dispose all registration tokens
  -> destroy GameObject
```

## Но осторожно с “взять обязанности других сервисов”

`WorldActorManager` может делать больше, чем registry, но только **actor-domain обязанности**.

Нормально:

```text id="mefqxh"
WorldActorManager:
  register/unregister actors
  TryGet actor
  enumerate actors
  actor spawned/despawned events
  find actors by definition/faction/team
  maybe find nearest actor
```

Опасно:

```text id="7945ws"
WorldActorManager:
  start dialogue
  perform interaction
  pickup items
  bind camera
  control player input
  update UI
  despawn world objects
```

Это уже превращение в god manager.

То есть он может быть **actor directory / actor domain service**, но не “главный сервис всего, что связано с актерами”.

## Как я бы разложил

```text id="9u9t11"
WorldManager
  WorldId -> WorldLifetime
  Despawn(id)

WorldActorManager
  WorldId -> IWorldActor
  actor-specific queries/events

PickupManager
  WorldId -> IWorldPickup
  pickup-specific queries/events

InteractionManager
  internal WorldId -> IInteractable
  uses IActorProvider instead of IInteractor registry

DisplayManager
  internal WorldId -> IDisplayable

PossessionManager
  internal WorldId -> IPossessable
  maybe uses IActorProvider too
```

## Это лучше, чем голые реестры

Вместо такого:

```text id="ho03ie"
IWorldRegistry<IWorldActor>
IWorldRegistry<IWorldPickup>
IWorldRegistry<IInteractable>
IWorldRegistry<IDisplayable>
```

у тебя:

```text id="e9cymd"
IActorProvider / IActorRegistration
IPickupProvider / IPickupRegistration
IInteractionService / IInteractionRegistration
IDisplayService / IDisplayRegistration
```

Реестры остаются внутри как низкоуровневая структура данных.

## Итог

Да:

```text id="p2kbli"
WorldRegistry<IWorldActor> скрыть внутри WorldActorManager — хорошо.
IInteractor убрать, если interactor всегда actor — хорошо.
InteractionManager может использовать IActorProvider — хорошо.
```

Но не делай `InteractionManager` зависимым от конкретного `WorldActorManager`. Дай ему узкий интерфейс:

```csharp id="j42es3"
IActorProvider
```

Тогда у тебя будет чистая связь:

```text id="ugwtt6"
InteractionManager не знает, где и как хранятся actors.
Он просто умеет получить actor по WorldId.
```
