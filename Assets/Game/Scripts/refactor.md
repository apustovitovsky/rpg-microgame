# План рефакторинга world spawning и object composition

## 1. Цель

Убрать дублирующие вертикальные фабрики:

```text
ActorFactory
PickupFactory
LootContainerFactory
```

Они сейчас одновременно отвечают за:

```text
создание runtime instance
создание prefab
DI-конфигурацию
создание command handlers
регистрацию capabilities
сбор lifetime tokens
регистрацию spawned object
```

После рефакторинга ответственность должна быть разделена:

```text
Definition
    описывает immutable данные объекта
    содержит root prefab
    создаёт соответствующий runtime instance

WorldSpawner
    выполняет единый технический spawn/despawn pipeline

Prefab LifetimeScope
    описывает capability composition объекта

Capabilities
    самостоятельно регистрируются в глобальных registry

SpawnedObjectRegistry
    хранит InstanceId → spawned GameObject
```

---

## 2. Основной принцип

Доменный тип определяется runtime instance:

```text
ActorInstance
PickupInstance
LootContainerInstance
```

Возможности объекта определяются prefab composition:

```text
ITargetable
ICollectable
IInteractable
IInventory
IDialogueParticipant
INavigationAgent
ICommandReceiver
```

Это позволяет собирать нестандартные комбинации:

```text
ActorInstance + ICollectable
LootContainerInstance + IDialogueParticipant
PickupInstance + IInventory
```

Без изменения `WorldSpawner` и без добавления новой domain factory.

## 3. Базовая система definitions и instances

Все definitions, создающие runtime instance, наследуются от общего generic-класса:
Важно: Game.Core.Definition больше нигде не нужен, его можно вообще заменить AssetDefinition<TInstance>

```csharp
public abstract class AssetDefinition<TInstance> :
    ScriptableObject
    where TInstance : class
{
    [field: SerializeField]
    public string Id { get; private set; }

    [field: SerializeField]
    public string DisplayName { get; private set; }

    public abstract TInstance CreateInstance(
        Guid instanceId);
}
```

`AssetDefinition<TInstance>` отвечает за:

```text
стабильный Id
отображаемое имя
соответствие definition → runtime instance
создание начального runtime instance
```

Он не знает о:

```text
GameObject
VContainer
world lifecycle
spawn/despawn
capabilities
registries
```

---

## 4. WorldDefinition

Definitions объектов, физически существующих в игровом мире, используют дополнительный базовый класс:

```csharp
public abstract class WorldDefinition<TInstance> :
    AssetDefinition<TInstance>
    where TInstance : class, IWorldInstance
{
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
}
```

`WorldDefinition<TInstance>` добавляет только:

```text
root prefab
ограничение TInstance : IWorldInstance
```

Иерархия:

```text
AssetDefinition<TInstance>
├─ ItemDefinition
│    → ItemInstance
│
└─ WorldDefinition<TInstance>
     ├─ ActorDefinition
     │    → ActorInstance
     ├─ PickupDefinition
     │    → PickupInstance
     └─ LootContainerDefinition
          → LootContainerInstance
```

`ItemDefinition` не является `WorldDefinition`, поскольку `ItemInstance` не обязан иметь world representation или prefab.

---

## 5. Concrete definitions

Каждая concrete definition создаёт только соответствующий runtime instance.

### Item

```csharp
public sealed class ItemDefinition :
    AssetDefinition<ItemInstance>
{
    public override ItemInstance CreateInstance(
        Guid instanceId)
    {
        return new ItemInstance(
            instanceId,
            this);
    }
}
```

### Pickup

```csharp
public sealed class PickupDefinition :
    WorldDefinition<PickupInstance>
{
    public override PickupInstance CreateInstance(
        Guid instanceId)
    {
        return new PickupInstance(
            instanceId,
            this);
    }
}
```

### Actor

```csharp
public sealed class ActorDefinition :
    WorldDefinition<ActorInstance>
{
    public override ActorInstance CreateInstance(
        Guid instanceId)
    {
        return new ActorInstance(
            instanceId,
            this);
    }
}
```

### Loot container

```csharp
public sealed class LootContainerDefinition :
    WorldDefinition<LootContainerInstance>
{
    public override LootContainerInstance CreateInstance(
        Guid instanceId)
    {
        return new LootContainerInstance(
            instanceId,
            this);
    }
}
```

`CreateInstance()` выполняет только преобразование:

```text
definition + InstanceId
    → concrete runtime instance
```

Он не создаёт prefab и не участвует в dependency injection.

---

## 6. Generic WorldSpawner

Отдельные `ActorFactory`, `PickupFactory` и `LootContainerFactory` заменяются одним generic world spawner.

```csharp
public interface IWorldSpawner
{
    ISpawnedObject Spawn<TInstance>(
        WorldDefinition<TInstance> definition,
        SpawnPlacement context)
        where TInstance : class, IWorldInstance;

    bool Despawn(Guid instanceId);
}
```

Примерный pipeline:

```csharp
public ISpawnedObject Spawn<TInstance>(
    WorldDefinition<TInstance> definition,
    SpawnPlacement context)
    where TInstance : class, IWorldInstance
{
    ArgumentNullException.ThrowIfNull(definition);

    var instance = definition.CreateInstance(
        Guid.NewGuid());

    using (LifetimeScope.EnqueueParent(_parentScope))
    using (LifetimeScope.Enqueue(builder =>
    {
        builder.RegisterInstance(instance)
            .AsSelf()
            .As<IWorldInstance>();
    }))
    {
        var gameObject = Object.Instantiate(
            definition.Prefab,
            pose.Position,
            pose.Rotation);

        var spawnedObject = new SpawnedObject(
            instance.InstanceId,
            gameObject);

        _spawnedObjects.Add(spawnedObject);

        return spawnedObject;
    }
}
```

Generic-тип сохраняется на всём пути:

```text
PickupDefinition
    → WorldDefinition<PickupInstance>
    → PickupInstance
    → RegisterInstance<PickupInstance>
```

Поэтому внутри object scope доступны обе зависимости:

```text
PickupInstance
IWorldInstance
```

Аналогично для `ActorInstance` и `LootContainerInstance`.

---

### 6.1 Более правильный и рекомендуемый вариант спавна

Правильнее: `SpawnRequest<TInstance>` отделяет **что создаём** от параметров конкретного спавна и сразу поддерживает восстановление объекта с известным `InstanceId`.

```csharp
using System;

namespace Game.World
{
    public readonly record struct SpawnRequest<TInstance>(
        WorldDefinition<TInstance> Definition,
        SpawnPlacement Placement,
        Guid? InstanceId = null)
        where TInstance : class, IWorldInstance;
}
```

Spawner:

```csharp
public interface IWorldSpawner
{
    ISpawnedObject Spawn<TInstance>(
        SpawnRequest<TInstance> request)
        where TInstance : class, IWorldInstance;

    bool Despawn(Guid instanceId);
}
```

Реализация:

```csharp
public ISpawnedObject Spawn<TInstance>(
    SpawnRequest<TInstance> request)
    where TInstance : class, IWorldInstance
{
    ArgumentNullException.ThrowIfNull(
        request.Definition);

    if (request.InstanceId == Guid.Empty)
    {
        throw new ArgumentException(
            "InstanceId cannot be Guid.Empty.",
            nameof(request));
    }

    var instanceId =
        request.InstanceId ?? Guid.NewGuid();

    if (_spawnedObjects.TryGet(
            instanceId,
            out _))
    {
        throw new InvalidOperationException(
            $"World instance '{instanceId}' is already spawned.");
    }

    var instance =
        request.Definition.CreateInstance(instanceId);

    using (LifetimeScope.EnqueueParent(_parentScope))
    using (LifetimeScope.Enqueue(builder =>
    {
        builder.RegisterInstance(instance)
            .AsSelf()
            .As<IWorldInstance>();
    }))
    {
        var gameObject = Object.Instantiate(
            request.Definition.Prefab,
            request.Placement.Position,
            request.Placement.Rotation,
            request.Placement.Parent);

        var spawnedObject = new SpawnedObject(
            instanceId,
            gameObject);

        _spawnedObjects.Add(spawnedObject);

        return spawnedObject;
    }
}
```

Обычный новый spawn:

```csharp
var request = new SpawnRequest<PickupInstance>(
    pickupDefinition,
    placement);

_worldSpawner.Spawn(request);
```

Восстановление из сохранения:

```csharp
var request = new SpawnRequest<PickupInstance>(
    pickupDefinition,
    placement,
    savedInstanceId);

_worldSpawner.Spawn(request);
```

Тип обычно выведется автоматически:

```csharp
_worldSpawner.Spawn(
    new SpawnRequest<PickupInstance>(
        pickupDefinition,
        placement));
```

`Guid?` здесь лучше, чем соглашение через `Guid.Empty`:

```text
null         → создать новый InstanceId
конкретный ID → использовать переданный
Guid.Empty    → некорректный запрос
```

Итоговые роли:

```text
WorldDefinition<TInstance>
    описывает тип объекта и создаёт instance

SpawnPlacement
    описывает размещение

SpawnRequest<TInstance>
    объединяет definition, placement и optional InstanceId

WorldSpawner
    выполняет технический spawn pipeline
```


## 7. Что передаётся в object scope

WorldSpawner передаёт только созданный runtime instance:

```csharp
builder.RegisterInstance(instance)
    .AsSelf()
    .As<IWorldInstance>();
```

Отдельно не передаются:

```text
definition
InstanceId
capabilities
command handlers
registrations
registry tokens
```

Definition доступна через concrete instance:

```csharp
public sealed class ItemPickupCollectable
{
    private readonly PickupInstance _instance;

    public ItemPickupCollectable(
        PickupInstance instance)
    {
        _instance = instance;
    }

    public PickupDefinition Definition =>
        _instance.Definition;
}
```

Общая capability использует `IWorldInstance`:

```csharp
public sealed class Targetable
{
    private readonly IWorldInstance _instance;

    public Targetable(
        IWorldInstance instance)
    {
        _instance = instance;
    }
}
```

Правило:

```text
нужна только world identity
    → IWorldInstance

нужны доменные данные
    → concrete instance
```

---

## 7. Prefab и LifetimeScope

Definition хранит root prefab как `GameObject`:

```csharp
public abstract GameObject Prefab { get; }
```

`LifetimeScope` может находиться в любом месте иерархии:

```text
PF_Actor
├─ Physics
├─ Visual
└─ Infrastructure
   └─ ActorScope
```

Spawner создаёт весь root prefab внутри `Enqueue`, поэтому object scope получает переданный instance независимо от расположения в иерархии.

Не требуется:

```text
LifetimeScope на root
WorldObjectRoot со ссылкой на scope
LifetimeScope в definition
```

---

## 8. Где конфигурируются capabilities

Выбран гибридный подход:

```text
WorldSpawner:
    регистрирует только runtime instance

Parent scope:
    предоставляет глобальные сервисы и registry writers

Prefab object scope:
    регистрирует локальные capabilities и handlers
```

Пример object scope:

```csharp
public sealed class PickupScope :
    LifetimeScope
{
    protected override void Configure(
        IContainerBuilder builder)
    {
        builder.Register<Targetable>(
            Lifetime.Scoped);

        builder.Register<ItemPickupCollectable>(
            Lifetime.Scoped);

        builder.Register<InteractCommandHandler>(
                Lifetime.Scoped)
            .As<IWorldCommandHandler>();

        builder.Register<WorldCommandReceiver>(
                Lifetime.Scoped)
            .As<ICommandReceiver>();
    }
}
```

Configuration не переносится:

```text
в WorldSpawner
в definition
в domain factory
в отдельный WorldObjectAssembler
```

Допустимы небольшие VContainer extension methods для повторяющегося boilerplate, но не собственная альтернативная DI-система.

---

## 9. Саморегистрация capabilities

Capability получает writer глобального registry из parent scope.

При построении object scope capability регистрирует себя. При уничтожении scope удаляет себя.

```csharp
public sealed class Targetable :
    ITargetable,
    IInitializable,
    IDisposable
{
    private readonly IWorldInstance _instance;
    private readonly IRegistryWriter<ITargetable> _registry;

    public Targetable(
        IWorldInstance instance,
        IRegistryWriter<ITargetable> registry)
    {
        _instance = instance;
        _registry = registry;
    }

    public void Initialize()
    {
        _registry.Add(
            _instance.InstanceId,
            this);
    }

    public void Dispose()
    {
        _registry.Remove(
            _instance.InstanceId,
            this);
    }
}
```

Это устраняет внешнюю регистрацию из factory:

```text
factory создаёт capability
→ registry.Register(...)
→ получает token
→ сохраняет token в SpawnedObject
```

Новая схема:

```text
scope создаёт capability
→ capability сама публикуется в registry
→ scope Dispose
→ capability сама удаляется
```

Для безопасного удаления writer желательно принимать ожидаемое значение:

```csharp
public interface IRegistryWriter<T>
{
    void Add(Guid instanceId, T value);

    bool Remove(
        Guid instanceId,
        T expectedValue);
}
```

Так старый объект не сможет случайно удалить новую регистрацию с тем же `InstanceId`.

---

## 10. Удаление custom lifetime tokens

После перехода на object scopes удалить:

```text
RegistrationToken
SpawnedObject.Add(token)
CompositeDisposable внутри SpawnedObject
ручной unregister в factory
ручной сбор registrations
```

VContainer scope становится единственным агрегированным lifetime объекта:

```text
Destroy root GameObject
→ уничтожается вложенный object scope
→ scoped capabilities получают Dispose
→ capabilities удаляются из registry
```

Это не означает отказ от lifecycle вообще. Удаляется собственная token-based lifecycle-система; остаётся lifecycle VContainer.

---

## 11. Command receiver

Текущую ручную сборку:

```csharp
new WorldCommandReceiver(
    instance,
    new IWorldCommandHandler[]
    {
        new InteractCommandHandler(interactable),
    });
```

заменить DI-композицией.

Object scope регистрирует handlers как коллекцию:

```csharp
builder.Register<InteractCommandHandler>(
        Lifetime.Scoped)
    .As<IWorldCommandHandler>();

builder.Register<DialogueCommandHandler>(
        Lifetime.Scoped)
    .As<IWorldCommandHandler>();
```

`WorldCommandReceiver` получает:

```text
IWorldInstance
IReadOnlyList<IWorldCommandHandler>
```

Добавление command capability требует изменения только object scope/prefab composition.

---

## 12. Spawned object

После удаления registration tokens контракт становится минимальным:

```csharp
public interface ISpawnedObject
{
    Guid InstanceId { get; }

    GameObject GameObject { get; }
}
```

Реализация:

```csharp
public sealed class SpawnedObject :
    ISpawnedObject
{
    public SpawnedObject(
        Guid instanceId,
        GameObject gameObject)
    {
        InstanceId = instanceId;
        GameObject = gameObject;
    }

    public Guid InstanceId { get; }

    public GameObject GameObject { get; }
}
```

`GameObject` — именно root созданного prefab.

---

## 13. SpawnedObjectRegistry

Registry является только индексом:

```csharp
public interface ISpawnedObjectRegistry
{
    void Add(ISpawnedObject spawnedObject);

    bool TryGet(
        Guid instanceId,
        out ISpawnedObject spawnedObject);

    bool Remove(
        Guid instanceId,
        out ISpawnedObject spawnedObject);
}
```

Registry не должен:

```text
создавать instances
создавать GameObject
конфигурировать VContainer
искать capabilities
управлять command handlers
```

---

## 14. Despawn

```csharp
public bool Despawn(Guid instanceId)
{
    if (!_spawnedObjects.Remove(
            instanceId,
            out var spawnedObject))
    {
        return false;
    }

    Object.Destroy(spawnedObject.GameObject);

    return true;
}
```

При необходимости порядок может быть защищён от исключений и повторных вызовов, но основная модель остаётся простой:

```text
удалить handle
→ уничтожить root
→ object scope cleanup
```

---

## 15. Порядок миграции

### Этап 1. Подготовить базовые контракты

Добавить:

```text
IWorldInstance
WorldDefinition<TInstance>
IWorldSpawner
ISpawnedObject
ISpawnedObjectRegistry
SpawnPose
```

Пока не удалять старые factory.

### Этап 2. Реализовать generic WorldSpawner

Проверить:

```text
создание concrete TInstance
регистрацию TInstance
регистрацию IWorldInstance
создание root prefab
получение зависимостей object scope
despawn
```

### Этап 3. Мигрировать Pickup

Pickup является самым простым тестовым объектом.

Перенести из `PickupFactory`:

```text
Targetable
Collectable
InteractCommandHandler
WorldCommandReceiver
registry registration
```

в `PickupScope`.

Проверить, что:

```text
ItemPickupCollectable получает PickupInstance
Targetable получает IWorldInstance
```

### Этап 4. Перевести capabilities на self-registration

Сначала:

```text
ICommandReceiver
ITargetable/ITargetProvider
```

Затем остальные глобально адресуемые capabilities.

### Этап 5. Удалить tokens из Pickup pipeline

После успешного scope cleanup удалить manual registration tokens для pickup.

### Этап 6. Мигрировать LootContainer

Проверить:

```text
LootContainerInstance
IInventory
loot interaction
LootSessionService integration
command receiver
```

### Этап 7. Мигрировать Actor

Последним переносится actor, поскольку у него наиболее сложный composition:

```text
navigation
dialogue
input
targeting
inventory
commands
```

### Этап 8. Удалить старые factories

После миграции всех трёх типов удалить:

```text
ActorFactory
PickupFactory
LootContainerFactory
```

Либо оставить только действительно доменные application services, если они выполняют бизнес-подготовку помимо обычного spawn.

### Этап 9. Удалить custom lifetime infrastructure

Удалить:

```text
registration token aggregation
SpawnedObject.Add
ручной capability cleanup
```

### Этап 10. Отдельно решить каталоги

Generic API требует сохранения статического типа definition:

```csharp
PickupDefinition definition;
_worldSpawner.Spawn(definition, pose);
```

Общий негeneric catalog теряет `TInstance`.

Каталоги не усложнять в рамках первого рефактора. После стабилизации pipeline отдельно рассмотреть:

```text
typed catalogs
typed spawn requests
runtime spawn descriptors
visitor/double dispatch
раздельные каталоги definitions
```

---

## 16. Критерии успешности

Рефактор завершён, когда:

1. Actor, Pickup и LootContainer создаются одним `WorldSpawner`.
2. Вызову spawn требуется только concrete definition и pose.
3. Definition предоставляет root prefab и создаёт соответствующий instance.
4. Concrete instance и `IWorldInstance` доступны внутри object scope.
5. `LifetimeScope` не обязан находиться на root.
6. Spawner не знает ни одной capability.
7. Добавление capability не требует изменения spawner.
8. Добавление capability не требует создания новой domain factory.
9. Capabilities регистрируются и удаляются через lifecycle object scope.
10. `SpawnedObject` хранит только `InstanceId` и root `GameObject`.
11. Custom registration tokens полностью удалены.
12. Говорящий сундук создаётся изменением prefab composition, а не spawn pipeline.

---

## 17. Открытые вопросы

До или во время реализации отдельно проверить:

```text
Как VContainer ведёт себя,
если внутри одного prefab находится несколько LifetimeScope?

Нужна ли editor-валидация,
что prefab содержит ровно один главный object scope?

Нужны ли разные prefab variants
для разных capability compositions?

Как общий catalog будет вызывать generic Spawn<TInstance>?

Как будущий pooling повлияет
на Initialize/Dispose и registry membership?
```

Предварительные правила:

```text
один gameplay object → один главный object scope

снаружи передаётся только runtime instance

capability composition остаётся на prefab

definitions не содержат DI-конфигурацию

каталоги решаются отдельным рефактором
```
