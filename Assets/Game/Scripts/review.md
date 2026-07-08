Да, это даже хороший ход: **`WorldActor` лучше сделать plain C# runtime-сущностью**, а `MonoBehaviour` оставить только как Unity-view/adapters.

То есть не так:

```text id="nf9src"
WorldActor : MonoBehaviour
  Initialize(...)
```

а так:

```text id="7zyrll"
WorldActor = чистая runtime-сущность
ActorView / ActorTarget / ActorInteractable = MonoBehaviour adapters
ActorFactory = собирает всё вместе
```

## Как это выглядит

```csharp id="9oh6sd"
public sealed class WorldActor : IWorldActor
{
    public WorldActor(
        WorldId worldId,
        ActorDefinition definition,
        IActorView view,
        IActorInputBinder inputBinder,
        IActorTravelEndpoint travel,
        IActorDialogueEndpoint dialogue)
    {
        WorldId = worldId;
        Definition = definition;
        View = view;
        InputBinder = inputBinder;
        Travel = travel;
        Dialogue = dialogue;
    }

    public WorldId WorldId { get; }

    public ActorDefinition Definition { get; }

    public IActorView View { get; }

    public IActorInputBinder InputBinder { get; }

    public IActorTravelEndpoint Travel { get; }

    public IActorDialogueEndpoint Dialogue { get; }
}
```

Плюс: объект нельзя создать в “полуживом” состоянии. Не нужен `Initialize`, нет риска забыть вызвать его.

## Что остается MonoBehaviour

Например:

```text id="f5ld25"
ActorView : MonoBehaviour, IActorView
ActorTarget : MonoBehaviour, ITargetable
ActorInteractable : MonoBehaviour, IInteractable
ActorInputBinder : MonoBehaviour, IActorInputBinder
ActorTravelEndpoint : plain class или MonoBehaviour
```

`ActorView` может хранить Unity-ссылки:

```csharp id="fyv54f"
public sealed class ActorView : MonoBehaviour, IActorView
{
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private Transform _uiAnchor;

    public Transform Root => _root != null ? _root : transform;
    public Transform CameraPivot => _cameraPivot;
    public Transform TargetPoint => _targetPoint;
    public Transform UiAnchor => _uiAnchor;
}
```

## Кто создает `WorldActor`

Фабрика.

```csharp id="z0jq5h"
var instance = Object.Instantiate(
    request.Definition.Prefab,
    request.Position,
    request.Rotation,
    request.Parent);

var view = instance.GetComponentInChildren<IActorView>();
var inputBinder = instance.GetComponentInChildren<IActorInputBinder>();
var travel = instance.GetComponentInChildren<IActorTravelEndpoint>();
var dialogue = instance.GetComponentInChildren<IActorDialogueEndpoint>();
var interactable = instance.GetComponentInChildren<IInteractable>();

var actor = new WorldActor(
    request.WorldId,
    request.Definition,
    view,
    inputBinder,
    travel,
    dialogue);
```

Потом registrar:

```csharp id="d48sw0"
var lifetime = new WorldLifetime(
    request.WorldId,
    view.Root.gameObject);

lifetime.Add(_actors.Register(request.WorldId, actor));
lifetime.Add(_interactables.Register(request.WorldId, interactable));
lifetime.Add(_displayables.Register(request.WorldId, actor));
lifetime.Add(_possessables.Register(request.WorldId, actorPossessable));

_worldManager.Track(lifetime);
```

## Куда деть `WorldId` у MonoBehaviour компонентов

Есть два варианта.

### Вариант A — MonoBehaviour не знают `WorldId`

Лучше для чистоты.

`WorldActor` знает `WorldId`, а target/interactable получают `WorldId` через context/constructor/binder.

Но в Unity это не всегда удобно, особенно если `ITargetable` должен отдавать `WorldId`.

### Вариант B — один маленький `ActorIdentity` MonoBehaviour

Практичнее:

```csharp id="4pqjti"
public sealed class ActorIdentity : MonoBehaviour
{
    public WorldId WorldId { get; private set; }

    public void Set(WorldId worldId)
    {
        WorldId = worldId;
    }
}
```

Фабрика после instantiate:

```csharp id="a2p8uf"
identity.Set(request.WorldId);
```

А `ActorTarget`, `ActorInteractable` читают id из `ActorIdentity`.

Это не “грязный Initialize WorldActor”. Это просто установка runtime identity на Unity-side adapters.

## Главное правило

`WorldActor` — pure runtime model.

`MonoBehaviour` — Unity-порты:

```text id="mn9vim"
view
colliders
target point
camera pivot
animation
interaction trigger
navigation adapters
```

Фабрика соединяет их.

## Стоит ли так делать?

Да, если тебя раздражает, что `WorldActor : MonoBehaviour` одновременно:

```text id="nw6i90"
сущность
Unity component
display info
spatial
runtime state
registration endpoint
```

Тогда plain `WorldActor` сильно очищает модель.

## Минус

Фабрика станет немного ответственнее: она должна собрать `WorldActor` из prefab components.

Но это нормальная ответственность фабрики:

```text id="2bpjhz"
prefab = Unity representation
WorldActor = runtime entity
factory = assembly point
```

## Итог

Да, я бы сделал так:

```text id="g2up2w"
WorldActor — plain C# class, без MonoBehaviour и Initialize
ActorView/Target/Interactable/InputBinder — MonoBehaviour adapters
ActorFactory — instantiate prefab + собрать WorldActor
ActorRegistrar — зарегистрировать WorldActor и внешние роли
WorldLifetime — Dispose registrations + Destroy prefab instance
```

Это заметно чище, чем `WorldActor : MonoBehaviour`, особенно если ты хочешь нормальные runtime-сущности, а не Unity-компоненты, притворяющиеся доменной моделью.
Я бы **не клал `WorldActor` внутрь `WorldLifetime` и не клал `WorldLifetime` внутрь `WorldActor`**.

Это разные оси:

```text
WorldActor
  = игровая runtime-сущность

WorldLifetime
  = технический cleanup/despawn handle
```

Связывать их напрямую — значит опять смешать domain model и infrastructure.

## Как правильно

Связь должна быть только через `WorldId` и через composition root/factory:

```text
ActorFactory
  создает prefab
  собирает WorldActor
  создает WorldLifetime
  регистрирует WorldActor / роли
  добавляет registration tokens в WorldLifetime
  отдает WorldLifetime в WorldManager
```

То есть factory знает обоих, но они **не знают друг о друге**.

## Не так

```csharp
public sealed class WorldActor
{
    public WorldLifetime Lifetime { get; }
}
```

Плохо, потому что actor начинает знать, как он удаляется из сцены.

И не так:

```csharp
public sealed class WorldLifetime
{
    public IWorldActor Actor { get; }
}
```

Плохо, потому что lifetime начинает знать доменную сущность.

## Правильная модель

```csharp
public sealed class WorldActor : IWorldActor
{
    public WorldActor(
        WorldId worldId,
        ActorDefinition definition,
        IActorView view,
        IActorInputBinder inputBinder,
        IActorTravelEndpoint travel,
        IActorDialogueEndpoint dialogue)
    {
        WorldId = worldId;
        Definition = definition;
        View = view;
        InputBinder = inputBinder;
        Travel = travel;
        Dialogue = dialogue;
    }

    public WorldId WorldId { get; }
    public ActorDefinition Definition { get; }
    public IActorView View { get; }
    public IActorInputBinder InputBinder { get; }
    public IActorTravelEndpoint Travel { get; }
    public IActorDialogueEndpoint Dialogue { get; }
}
```

```csharp
public sealed class WorldLifetime : IDisposable
{
    private readonly CompositeRegistration _registrations = new();
    private readonly GameObject _root;
    private bool _disposed;

    public WorldLifetime(WorldId worldId, GameObject root)
    {
        WorldId = worldId;
        _root = root;
    }

    public WorldId WorldId { get; }

    public void Add(IRegistrationToken token)
    {
        if (_disposed)
        {
            token.Dispose();
            return;
        }

        _registrations.Add(token);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _registrations.Dispose();

        if (_root != null)
            Object.Destroy(_root);
    }
}
```

## Где они встречаются

Во временной сборочной структуре, не в runtime model:

```csharp
public readonly struct SpawnedActor
{
    public SpawnedActor(
        WorldActor actor,
        WorldLifetime lifetime)
    {
        Actor = actor;
        Lifetime = lifetime;
    }

    public WorldActor Actor { get; }
    public WorldLifetime Lifetime { get; }
}
```

Но `SpawnedActor` — это **только результат фабрики**, его не надо хранить в игре как сущность.

## Что еще можно оптимизировать

### 1. Убрать `ActorSpawnedObject` как огромный мешок

Если `WorldActor` содержит actor-specific endpoints:

```csharp
actor.View
actor.InputBinder
actor.Travel
actor.Dialogue
```

то `ActorSpawnedObject` больше не нужен.

Фабрика может сразу создать:

```text
WorldActor
WorldLifetime
external roles
```

### 2. Регистрировать только крупные сущности и внешние роли

Для actor достаточно примерно:

```csharp
lifetime.Add(_actors.Register(id, actor));
lifetime.Add(_interactors.Register(id, actorInteractor));
lifetime.Add(_interactables.Register(id, actorInteractable));
lifetime.Add(_displayables.Register(id, actorDisplayable));
lifetime.Add(_possessables.Register(id, actorPossessable));
```

Не надо глобально регистрировать:

```text
IActorView
IActorTravelEndpoint
IActorDialogueEndpoint
IActorInputBinder
```

Они доступны через `IWorldActor`.

### 3. Не давать actor-у `Despawn()`

Если actor умер, не надо:

```csharp
actor.Lifetime.Dispose();
```

Лучше:

```text
HealthSystem / DeathSystem
  -> WorldManager.Despawn(actor.WorldId)
```

Или actor raises event:

```csharp
actor.Died += id => _worldManager.Despawn(id);
```

Сущность сообщает факт, infrastructure удаляет объект.

### 4. `WorldManager` должен хранить только lifetime

```csharp
private readonly Dictionary<WorldId, IWorldLifetime> _lifetimes;
```

Ему не нужен `WorldActor`, `WorldPickup`, `IInteractable`.

### 5. `WorldLifetime` может сам удаляться из `WorldManager`

При `Track` добавь self-unregister token:

```csharp
public bool Track(IWorldLifetime lifetime)
{
    if (!_lifetimes.TryAdd(lifetime.WorldId, lifetime))
    {
        lifetime.Dispose();
        return false;
    }

    lifetime.Add(new RegistrationToken(() =>
    {
        _lifetimes.Remove(lifetime.WorldId);
    }));

    return true;
}
```

Тогда даже прямой `lifetime.Dispose()` не оставит stale-запись.

## Итоговая форма

```text
WorldActor
  доменная runtime-сущность

WorldPickup
  доменная runtime-сущность

WorldLifetime
  cleanup: unregister + destroy GameObject

WorldManager
  WorldId -> WorldLifetime

ActorFactory
  собирает actor + roles + lifetime

ActorRegistrar или factory-section
  добавляет registration tokens в lifetime
```

Самое важное: **не делай двустороннюю связь `WorldActor <-> WorldLifetime`**. Они должны быть собраны рядом, но не владеть друг другом.
