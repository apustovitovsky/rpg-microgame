Да, ты правильно видишь проблему. **WorldManager не должен быть event-bus’ом для всех доменов**, и `WorldObjectRegistration` “на всё сразу” тоже плохой компромисс.

Я бы сделал так:

```text id="jeqtd2"
WorldManager / WorldRegistry
  только lifetime + identity

Specialized registries
  ActorRegistry
  PickupRegistry
  InteractionRegistry
  TargetRegistry
  DisplayRegistry

Factory/Binder
  знает composition конкретного типа
  регистрирует объект сразу в нужные registry
```

## То есть не так

```text id="hpfwi7"
WorldManager.Register(worldObject)
  -> event WorldObjectRegistered
  -> все сервисы пытаются вытащить из него что-то свое
```

Это просто locator/event-bus hybrid.

## А так

```text id="uxzv94"
PickupFactory:
  create GameObject
  initialize pickup
  worldRegistry.Register(worldRef)
  pickupRegistry.Register(worldId, pickup)
  targetRegistry.Register(worldId, targetable)
  interactionRegistry.Register(worldId, interactable)
  displayRegistry.Register(worldId, displayInfo)

ActorFactory:
  create GameObject
  initialize actor
  worldRegistry.Register(worldRef)
  actorRegistry.Register(worldId, actor)
  targetRegistry.Register(worldId, targetable)
  interactionRegistry.Register(worldId, interactable)
  displayRegistry.Register(worldId, displayInfo)
```

То есть **factory/binder является composition root для конкретного объекта**.

## Роль WorldRegistry

`WorldRegistry` должен знать только это:

```csharp id="j4lm25"
public interface IWorldObject
{
    WorldId WorldId { get; }
    GameObject GameObject { get; }
    Transform Root { get; }
}
```

Без:

```csharp id="r1i9w6"
TryGet<T>()
TryGetEndpoint<T>()
capabilities
```

Сейчас у тебя `WorldObject` хранит dictionary endpoints и делает `TryGet<TEndpoint>()`, а `WorldObjectRegistry` еще имеет `TryGetEndpoint<TEndpoint>()`. Вот именно это я бы убрал. ([raw.githubusercontent.com](https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/WorldObject.cs)) ([raw.githubusercontent.com](https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/WorldObjectRegistry.cs))

## Как сервисы тогда работают

`InteractionService` не должен получать общий `IWorldObjectRegistry` и вытаскивать `IInteractable` из `WorldObject`, как сейчас. Сейчас он делает `target.TryGet(out var interactable)`. ([raw.githubusercontent.com](https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/Interaction/Runtime/InteractionService.cs))

Лучше:

```csharp id="7e7daf"
public sealed class InteractionService : IInteractionService
{
    private readonly IWorldRegistry _world;
    private readonly IInteractionRegistry _interactions;

    public async UniTask<bool> TryInteractAsync(
        IWorldObject interactor,
        WorldId targetId,
        CancellationToken token)
    {
        if (!_world.TryGet(targetId, out var target))
            return false;

        if (!_interactions.TryGet(targetId, out var interactable))
            return false;

        // range/context/can/interact
    }
}
```

`PickupService` тоже не должен делать generic lookup через `_worldObjects.TryGetEndpoint(...)`, как сейчас. ([raw.githubusercontent.com](https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/Pickup/Runtime/WorldPickupService.cs))

Лучше:

```csharp id="qbpf50"
public sealed class PickupService : IPickupService
{
    private readonly IPickupRegistry _pickups;
    private readonly IPickupCollectorRegistry _collectors;
    private readonly IWorldManager _world;

    public async UniTask<PickupResult> CollectAsync(
        WorldId collectorId,
        WorldId pickupId,
        CancellationToken token)
    {
        if (!_pickups.TryGet(pickupId, out var pickup))
            return PickupResult.PickupNotFound;

        if (!_collectors.TryGet(collectorId, out var collector))
            return PickupResult.CollectorNotFound;

        // pickup rules + collector rules + apply + despawn
    }
}
```

## Как не получить хаос unregister

Вот тут нужен **composite registration handle**.

Каждый registry при регистрации возвращает token:

```csharp id="28c759"
public interface IRegistrationToken : IDisposable
{
}
```

```csharp id="ermqdj"
public interface IPickupRegistry
{
    IRegistrationToken Register(WorldId id, IPickup pickup);
    bool TryGet(WorldId id, out IPickup pickup);
}
```

Фабрика собирает все регистрации:

```csharp id="e4m4c7"
var lifetime = new CompositeRegistration();

lifetime.Add(_worldRegistry.Register(worldObject));
lifetime.Add(_pickupRegistry.Register(worldId, pickup));
lifetime.Add(_targetRegistry.Register(worldId, targetable));
lifetime.Add(_interactionRegistry.Register(worldId, interactable));
lifetime.Add(_displayRegistry.Register(worldId, displayInfo));

_worldManager.Track(worldObject, lifetime);
```

А `WorldManager` знает только:

```text id="d5rwu9"
у объекта есть WorldId
у объекта есть GameObject
у объекта есть lifetime token, который надо Dispose при despawn
```

Он не знает, что внутри token лежали pickup/interaction/target/display регистрации.

## Это снимает обе твои проблемы

### 1. Разные объекты — разные регистрации

Да. `PickupFactory` делает pickup registrations, `ActorFactory` делает actor registrations.

Не нужен один огромный `WorldObjectRegistration`.

### 2. WorldManager не знает детали

Да. `WorldManager` только хранит объект и вызывает `Dispose()` на lifetime.

```csharp id="s72zp8"
public sealed class WorldManager : IWorldManager
{
    private readonly Dictionary<WorldId, WorldEntry> _entries = new();

    public bool Track(IWorldObject worldObject, IDisposable lifetime)
    {
        if (worldObject == null || worldObject.WorldId.IsEmpty)
            return false;

        if (_entries.ContainsKey(worldObject.WorldId))
            return false;

        _entries.Add(
            worldObject.WorldId,
            new WorldEntry(worldObject, lifetime));

        return true;
    }

    public bool Despawn(WorldId id)
    {
        if (!_entries.Remove(id, out var entry))
            return false;

        entry.Lifetime.Dispose();

        if (entry.WorldObject.GameObject != null)
            Object.Destroy(entry.WorldObject.GameObject);

        return true;
    }
}
```

## Где тут WorldRegistry

Можно сделать так:

```text id="71t2at"
WorldRegistry = один из specialized registries,
но базовый:
  WorldId -> IWorldObject
```

То есть фабрика регистрирует объект туда так же, как и в остальные:

```csharp id="xas9je"
lifetime.Add(_worldRegistry.Register(worldObject));
lifetime.Add(_pickupRegistry.Register(worldId, pickup));
```

`WorldManager` не обязан сам регистрировать в `WorldRegistry`. Он может только владеть lifetime. Это даже чище.

## Итоговая архитектура

```text id="6myxaj"
WorldManager
  owns lifetime/despawn/destroy
  no endpoint knowledge

WorldRegistry
  WorldId -> IWorldObject

TargetRegistry
  WorldId -> ITargetable

InteractionRegistry
  WorldId -> IInteractable

PickupRegistry
  WorldId -> IPickup

ActorRegistry
  WorldId -> IActor

DisplayRegistry
  WorldId -> IDisplayInfo

PickupFactory / PickupBinder
  registers pickup into needed registries

ActorFactory / ActorBinder
  registers actor into needed registries
```

Это лучше, чем глобальный locator.

Главная мысль: **не WorldManager должен уведомлять системы, а factory/binder должен явно зарегистрировать объект во все нужные typed registries и отдать WorldManager’у общий lifetime handle.**

Да, **удалится**, если `RegistrationToken` сделан как handle на unregister.

Идея такая:

```text
Register(...) добавляет запись в registry
Register(...) возвращает token
token.Dispose() удаляет эту же запись из registry
CompositeRegistration.Dispose() вызывает Dispose у всех token
```

## Минимальный `RegistrationToken`

```csharp
public sealed class RegistrationToken : IDisposable
{
    private Action? _onDispose;
    private bool _disposed;

    public RegistrationToken(Action onDispose)
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

## Registry возвращает token

```csharp
public sealed class PickupRegistry : IPickupRegistry
{
    private readonly Dictionary<WorldId, IPickup> _items = new();

    public IDisposable Register(WorldId worldId, IPickup pickup)
    {
        if (!_items.TryAdd(worldId, pickup))
            throw new InvalidOperationException($"Pickup already registered: {worldId}");

        return new RegistrationToken(() =>
        {
            _items.Remove(worldId);
        });
    }

    public bool TryGet(WorldId worldId, out IPickup pickup)
    {
        return _items.TryGetValue(worldId, out pickup);
    }
}
```

То есть token **помнит, как удалить именно эту регистрацию**.

## Composite lifetime

```csharp
public sealed class CompositeRegistration : IDisposable
{
    private readonly List<IDisposable> _tokens = new();
    private bool _disposed;

    public void Add(IDisposable token)
    {
        if (_disposed)
        {
            token.Dispose();
            return;
        }

        _tokens.Add(token);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        for (var i = _tokens.Count - 1; i >= 0; i--)
            _tokens[i].Dispose();

        _tokens.Clear();
    }
}
```

## Использование в factory/binder

```csharp
var lifetime = new CompositeRegistration();

lifetime.Add(_worldRegistry.Register(worldObject));
lifetime.Add(_pickupRegistry.Register(worldId, pickup));
lifetime.Add(_targetRegistry.Register(worldId, targetable));
lifetime.Add(_interactionRegistry.Register(worldId, interactable));
lifetime.Add(_displayRegistry.Register(worldId, displayInfo));

_worldManager.Track(worldObject, lifetime);
```

## WorldManager

```csharp
public bool Despawn(WorldId worldId)
{
    if (!_entries.Remove(worldId, out var entry))
        return false;

    entry.Lifetime.Dispose(); // удалит из всех registry

    if (entry.WorldObject.GameObject != null)
        UnityEngine.Object.Destroy(entry.WorldObject.GameObject);

    return true;
}
```

То есть `WorldManager` не знает про `PickupRegistry`, `InteractionRegistry`, `TargetRegistry`. Он знает только:

```text
у объекта есть lifetime
при despawn надо вызвать lifetime.Dispose()
```

А уже `lifetime.Dispose()` удаляет объект из всех сервисов.

## Главное правило

Нельзя просто сделать:

```csharp
_worldManager.Track(worldObject);
```

Нужно track-ать именно:

```csharp
_worldManager.Track(worldObject, lifetime);
```

Иначе да — объект уничтожится, но записи в specialized registries останутся.

Лучший вариант: **каждая регистрация возвращает `IDisposable`, а factory/binder собирает их в `CompositeRegistration`**. Это и есть безопасный unregister без знания деталей внутри `WorldManager`.

Да, **стоит сделать generic registry**, но не как один общий сервис-локатор, а как **generic implementation для typed registries**.

То есть не так:

```csharp id="4jvsww"
IGenericRegistry registry;
registry.TryGet<IInteractable>(id, out var interactable);
registry.TryGet<IPickup>(id, out var pickup);
```

А так:

```csharp id="n3r1ku"
IWorldRegistry        : IRegistry<IWorldObject>
IInteractionRegistry  : IRegistry<IInteractable>
IPickupRegistry       : IRegistry<IPickup>
ITargetRegistry       : IRegistry<ITargetable>
IDisplayRegistry      : IRegistry<IDisplayInfo>
```

При этом implementation один:

```csharp id="a4f83n"
public interface IRegistry<T> where T : class
{
    IDisposable Register(WorldId id, T item);
    bool TryGet(WorldId id, out T item);
    bool Contains(WorldId id);
}
```

```csharp id="y1l9qf"
public sealed class WorldRegistry<T> : IRegistry<T> where T : class
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
                $"Duplicate registration for {typeof(T).Name}: {id}");

        return new RegistrationToken(() =>
        {
            _items.Remove(id);
        });
    }

    public bool TryGet(WorldId id, out T item)
    {
        return _items.TryGetValue(id, out item);
    }

    public bool Contains(WorldId id)
    {
        return _items.ContainsKey(id);
    }
}
```

## Почему так лучше

Ты не плодишь одинаковые классы:

```text id="6uff83"
PickupRegistry
InteractionRegistry
TargetRegistry
DisplayRegistry
```

но при этом зависимости остаются честными:

```csharp id="z3jonz"
public sealed class InteractionService
{
    private readonly IRegistry<IInteractable> _interactions;

    public InteractionService(IRegistry<IInteractable> interactions)
    {
        _interactions = interactions;
    }
}
```

`InteractionService` физически не может достать `IPickup` или `IActorTravel`, потому что ему дали только `IRegistry<IInteractable>`.

## С VContainer

VContainer действительно поддерживает open generic registration через `builder.Register(typeof(GenericType<>), Lifetime.Singleton)`, и закрытый generic type создается при resolve; в документации также отмечено, что это подтверждено для IL2CPP начиная с Unity 2022.1. ([VContainer][1])

Примерно:

```csharp id="xr5n4d"
builder.Register(typeof(WorldRegistry<>), Lifetime.Singleton)
    .As(typeof(IRegistry<>));
```

Если API в твоей версии VContainer не примет `.As(typeof(IRegistry<>))`, можно регистрировать закрытые версии явно:

```csharp id="frcgkv"
builder.Register<WorldRegistry<IWorldObject>>(Lifetime.Singleton)
    .As<IRegistry<IWorldObject>>();

builder.Register<WorldRegistry<IInteractable>>(Lifetime.Singleton)
    .As<IRegistry<IInteractable>>();

builder.Register<WorldRegistry<IPickup>>(Lifetime.Singleton)
    .As<IRegistry<IPickup>>();

builder.Register<WorldRegistry<ITargetable>>(Lifetime.Singleton)
    .As<IRegistry<ITargetable>>();

builder.Register<WorldRegistry<IDisplayInfo>>(Lifetime.Singleton)
    .As<IRegistry<IDisplayInfo>>();
```

Для 5–10 registries я бы даже предпочел **явные closed registrations**: меньше магии, проще читать composition root.

## Как будет выглядеть factory

```csharp id="5g9c67"
var lifetime = new CompositeRegistration();

lifetime.Add(_worlds.Register(worldId, worldObject));
lifetime.Add(_pickups.Register(worldId, pickup));
lifetime.Add(_targets.Register(worldId, targetable));
lifetime.Add(_interactions.Register(worldId, interactable));
lifetime.Add(_displays.Register(worldId, displayInfo));

_worldManager.Track(worldObject, lifetime);
```

Где зависимости фабрики:

```csharp id="z1zuxm"
private readonly IRegistry<IWorldObject> _worlds;
private readonly IRegistry<IPickup> _pickups;
private readonly IRegistry<ITargetable> _targets;
private readonly IRegistry<IInteractable> _interactions;
private readonly IRegistry<IDisplayInfo> _displays;
```

## Когда делать отдельный негeneric интерфейс

Если registry получает особые методы, тогда делай специализированный интерфейс:

```csharp id="ml97k4"
public interface IActorRegistry : IRegistry<IActorRuntime>
{
    bool TryGetPlayer(out IActorRuntime player);
    IEnumerable<IActorRuntime> GetAliveActors();
}
```

```csharp id="7q7d9x"
public interface IPickupRegistry : IRegistry<IPickup>
{
    IEnumerable<IPickup> GetCollectableInRadius(Vector3 center, float radius);
}
```

Но пока методов нет — generic достаточно.

## Итог

Я бы сделал так:

```text id="52zcff"
Да:
  IRegistry<T>
  WorldRegistry<T>
  RegistrationToken
  CompositeRegistration

Нет:
  один IGenericRegistry с TryGet<TEndpoint>()
  WorldObject.TryGet<T>()
  WorldRegistry.TryGetEndpoint<T>()
```

Главная мысль: **generic implementation — да; generic locator — нет.**

[1]: https://vcontainer.hadashikick.jp/registering/register-type "Register Plain C# Type | VContainer"
