Да, ты прав. Я ошибочно принял `Pickup` за мировое состояние `ItemInstance`, хотя в твоей модели это разные сущности с разными жизненными циклами:

```text
ItemInstance
    существует только внутри inventory/equipment
    не имеет GameObject
    не участвует в world registry
    не имеет Transform
    не spawn/despawn-ится

Pickup
    самостоятельный world object
    имеет собственный InstanceId
    имеет GameObject и Transform
    участвует в lifecycle registration
    при сборе создаёт/передаёт ItemInstance либо выполняет другое collect-поведение
```

Следовательно, мой тезис про сохранение одного `ItemInstance.InstanceId` при переходе `World → Inventory → World` к твоему проекту неприменим.

## Исправленная модель

Реальное сходство находится здесь:

```text
ActorInstance                         PickupInstance
    InstanceId                           InstanceId
    ActorDefinition                      PickupDefinition
    runtime capabilities                 runtime capabilities
    spawned GameObject                   spawned GameObject
    registration lifetime                registration lifetime
```

Но содержательно это разные world instances:

```text
ActorInstance
    персонаж

PickupInstance
    мировой собираемый объект
```

А `ItemInstance` находится в другой подсистеме:

```text
Inventory domain:
    ItemDefinition
    ItemInstance
    InventoryEntry

World domain:
    ActorInstance
    PickupInstance
    spawned representations
```

Связь между pickup и item возникает только в конкретной реализации `ICollectable`:

```csharp
public sealed class ItemPickupCollectable : ICollectable
{
    private readonly ItemDefinition _itemDefinition;
    private readonly int _amount;

    public CollectResult Collect(ICollector collector)
    {
        return collector.Inventory.TryAdd(
            _itemDefinition,
            _amount);
    }
}
```

И даже эта связь необязательна:

```text
ItemPickupCollectable     → выдаёт item
HealingCollectable        → восстанавливает здоровье
GoldCollectable           → изменяет currency
QuestCollectable          → устанавливает факт
ExperienceCollectable     → выдаёт XP
```

Поэтому `PickupInstance` нельзя выводить из `ItemInstance` и нельзя считать world representation предмета.

# Что тогда действительно объединять у Actor и Pickup

Твоя исходная мысль теперь формулируется так:

> Actor и Pickup — разные доменные виды world object, но имеют одинаковую инфраструктуру появления и существования в мире.

Общая часть:

```text
Guid InstanceId
Definition с prefab
GameObject
Transform
spawn/despawn
lifetime aggregation
регистрация capabilities
targeting/interaction registration
rollback при неудачном spawn
```

Разная часть:

```text
Actor:
    ActorDefinition
    actor scope
    navigation
    dialogue
    input
    actor targeting
    возможно inventory

Pickup:
    PickupDefinition
    ICollectable
    pickup interaction
    обычно без собственного scope
```

## Не `IWorldInstance` с универсальной definition

Типизированный общий базовый контракт допустим:

```csharp
public interface IWorldInstance
{
    Guid InstanceId { get; }
}
```

Но не стоит делать:

```csharp
public interface IWorldInstance
{
    Guid InstanceId { get; }
    ScriptableObject Definition { get; }
}
```

И тем более:

```csharp
public sealed class WorldInstance
{
    public ScriptableObject Definition { get; }
    public Dictionary<Type, object> Capabilities { get; }
}
```

Это уничтожит типизацию между `ActorDefinition` и `PickupDefinition`.

Лучше:

```csharp
public sealed class ActorInstance : IWorldInstance
{
    public Guid InstanceId { get; }
    public ActorDefinition Definition { get; }
}

public sealed class PickupInstance : IWorldInstance
{
    public Guid InstanceId { get; }
    public PickupDefinition Definition { get; }
}
```

Можно выделить generic base class, но практической пользы почти нет:

```csharp
public abstract class WorldInstance<TDefinition> : IWorldInstance
{
    protected WorldInstance(
        Guid instanceId,
        TDefinition definition)
    {
        InstanceId = instanceId;
        Definition = definition;
    }

    public Guid InstanceId { get; }

    public TDefinition Definition { get; }
}
```

```csharp
public sealed class ActorInstance :
    WorldInstance<ActorDefinition>
{
    // Actor state.
}

public sealed class PickupInstance :
    WorldInstance<PickupDefinition>
{
    // Pickup state.
}
```

Для двух классов я бы пока предпочёл обычное дублирование двух properties. Generic inheritance здесь скорее косметическое.

# `ICollectable` принадлежит не `PickupInstance`, а spawned-композиции

Это важное различие.

`PickupInstance` описывает конкретный мировой объект:

```csharp
public sealed class PickupInstance
{
    public Guid InstanceId { get; }

    public PickupDefinition Definition { get; }
}
```

`ICollectable` описывает поведение его текущего представления:

```csharp
public interface ICollectable
{
    bool CanCollect(in CollectContext context);

    CollectResult Collect(in CollectContext context);
}
```

Связка собирается фабрикой:

```text
PickupInstance
    +
Pickup prefab
    +
ItemPickupCollectable
    =
spawned pickup
```

Не каждый pickup обязан создавать item, но каждый объект, который система считает pickup, вероятно, должен иметь `ICollectable`.

Тогда термин `Pickup` у тебя означает не просто capability, а конкретный вид world entity, чей обязательный контракт — `ICollectable`. Это нормальная модель:

```csharp
public interface IPickup
{
    Guid InstanceId { get; }

    PickupDefinition Definition { get; }

    ICollectable Collectable { get; }
}
```

# Actor и Pickup можно свести к одинаковому runtime aggregate

Если текущий `ActorInstance` содержит не только identity, но и resolved capabilities, логично сделать аналогичный `PickupInstance`.

Например:

```csharp
public sealed class PickupInstance
{
    public PickupInstance(
        PickupDefinition definition,
        ICollectable collectable,
        IInteractable interactable,
        ITargetable targetable)
        : this(
            Guid.NewGuid(),
            definition,
            collectable,
            interactable,
            targetable)
    {
    }

    public PickupInstance(
        Guid instanceId,
        PickupDefinition definition,
        ICollectable collectable,
        IInteractable interactable,
        ITargetable targetable)
    {
        InstanceId = instanceId;
        Definition = definition;
        Collectable = collectable;
        Interactable = interactable;
        Targetable = targetable;
    }

    public Guid InstanceId { get; }

    public PickupDefinition Definition { get; }

    public ICollectable Collectable { get; }

    public IInteractable Interactable { get; }

    public ITargetable Targetable { get; }
}
```

Тогда структуры симметричны:

```text
ActorInstance
    identity
    definition
    actor capabilities

PickupInstance
    identity
    definition
    pickup capabilities
```

Разница только в способе сборки:

```text
ActorFactory
    создаёт child LifetimeScope
    регистрирует ActorInstance в scope
    получает scoped capabilities

PickupFactory
    создаёт prefab напрямую
    получает endpoint/components
    создаёт PickupInstance
```

Это уже намного ближе к твоему текущему устройству.

# Нужно ли унифицировать фабрики

Полностью — нет. Их composition roots принципиально отличаются.

Плохая универсальная фабрика:

```csharp
Spawn(
    IWorldDefinition definition,
    Func<GameObject, object> instanceFactory);
```

Она скроет важные различия и приведёт к callback-based конструктору без ясной предметной модели.

Хорошо унифицировать только инфраструктурный хвост:

```csharp
public sealed class SpawnLifetimeBuilder
{
    private readonly CompositeLifetime _lifetime = new();

    public void Add(IDisposable registration)
    {
        _lifetime.Add(registration);
    }

    public SpawnedObject Build(
        Guid instanceId,
        GameObject gameObject)
    {
        return new SpawnedObject(
            instanceId,
            gameObject,
            _lifetime);
    }

    public void Rollback()
    {
        _lifetime.Dispose();
    }
}
```

Или helper:

```csharp
public sealed class WorldSpawnRegistrar
{
    public SpawnedObject Register(
        Guid instanceId,
        GameObject gameObject,
        params IDisposable[] registrations)
    {
        var lifetime = new CompositeLifetime();

        try
        {
            foreach (var registration in registrations)
                lifetime.Add(registration);

            var spawnedObject = new SpawnedObject(
                instanceId,
                gameObject,
                lifetime);

            lifetime.Add(
                _spawnedObjects.Register(spawnedObject));

            return spawnedObject;
        }
        catch
        {
            lifetime.Dispose();

            if (gameObject != null)
                UnityEngine.Object.Destroy(gameObject);

            throw;
        }
    }
}
```

Тогда фабрики остаются специализированными:

```text
ActorFactory
    знает VContainer actor scope
    создаёт ActorInstance
    передаёт регистрации в общий lifetime registrar

PickupFactory
    знает PickupEndpoint
    создаёт PickupInstance
    передаёт регистрации в тот же registrar
```

# Scope не является основанием разделения

В этом твоя исходная мысль была верной.

```text
Actor имеет scope
Pickup не имеет scope
```

— это разница в composition strategy, а не в lifecycle или фундаментальном типе объекта.

Оба должны проходить одинаковый внешний lifecycle:

```text
instantiate
assemble
register capabilities
track spawned object
despawn
dispose registrations
destroy GameObject
```

Actor дополнительно делает внутри:

```text
create actor LifetimeScope
register ActorInstance
resolve scoped dependencies
```

Pickup:

```text
resolve endpoint
inject/bind PickupInstance
```

При этом уничтожение actor scope тоже должно быть частью общего `CompositeLifetime`:

```csharp
spawnLifetime.Add(actorScope);
```

или частью уничтожаемого prefab, если VContainer гарантированно освобождает scope при уничтожении его `GameObject`. Я бы всё равно делал ownership явным, если scope создаётся программно.

# Исправленный архитектурный вывод

Не так:

```text
ActorInstance и ItemInstance — два domain instance
Pickup — presentation ItemInstance
```

А так:

```text
World-domain instances:
    ActorInstance
    PickupInstance

Inventory-domain instances:
    ItemInstance
```

Связи:

```text
PickupInstance
    owns/provides ICollectable

ItemPickupCollectable
    при сборе создаёт или добавляет ItemInstance

HealingCollectable
    ItemInstance вообще не затрагивает
```

Инфраструктура:

```text
ActorFactory ─────┐
                  ├─ SpawnedObject
PickupFactory ────┘  CompositeLifetime
                     InstanceId registrations
                     Despawn
```

## Моя скорректированная рекомендация

Сохрани отдельные:

```text
ActorInstance
PickupInstance
ItemInstance
```

Не объединяй их в один универсальный entity.

Унифицируй только:

```text
SpawnedObject
registration lifetime
world-object tracking
despawn
registration rollback
общие spatial spawn data
```

Оставь разные composition roots:

```text
ActorFactory
    scope-based composition

PickupFactory
    endpoint/component-based composition
```

А `ICollectable` считай обязательной capability конкретного `PickupInstance`, но не признаком `ItemInstance`.

И главное: прежде чем давать дальнейший структурный рефакторинг по конкретным классам, нужно читать именно актуальный commit/branch. В предыдущем ответе я построил вывод на неверной предметной предпосылке, поэтому его часть про `ItemInstance` и world representation следует отбросить полностью.


Тогда `ActorInstance` и `PickupInstance` должны быть **только моделями идентичности и собственного состояния**, без ссылок на scene capabilities.

А отдельные actor/pickup registry действительно не нужны, если ни одна система не делает запрос вида:

```text
найди мне любой загруженный ActorInstance
найди мне любой загруженный PickupInstance
```

## Предлагаемая модель

```text
WorldObjectRegistry
    InstanceId
        → WorldInstance
        → spawned GameObject / root
        → lifetime registrations

Capability registries
    InstanceId → IInteractable
    InstanceId → IDisplayNameProvider
    InstanceId → ITargetable
    ...

Prefab-local endpoints
    ActorDialogueEndpoint
    PickupCollectable
    ActorNavigationEndpoint
    ...
```

То есть центральный реестр отвечает на вопрос:

> Какой объект с таким `InstanceId` сейчас существует в мире и как завершить его lifetime?

А специализированные реестры отвечают:

> Предоставляет ли этот объект конкретную capability?

Это хорошая граница.

---

# 1. Инстансы без capabilities

```csharp
public interface IWorldInstance
{
    Guid InstanceId { get; }
}
```

```csharp
public sealed class ActorInstance : IWorldInstance
{
    public ActorInstance(ActorDefinition definition)
        : this(Guid.NewGuid(), definition)
    {
    }

    public ActorInstance(
        Guid instanceId,
        ActorDefinition definition)
    {
        InstanceId = instanceId;
        Definition = definition
            ?? throw new ArgumentNullException(nameof(definition));
    }

    public Guid InstanceId { get; }

    public ActorDefinition Definition { get; }
}
```

```csharp
public sealed class PickupInstance : IWorldInstance
{
    public PickupInstance(PickupDefinition definition)
        : this(Guid.NewGuid(), definition)
    {
    }

    public PickupInstance(
        Guid instanceId,
        PickupDefinition definition)
    {
        InstanceId = instanceId;
        Definition = definition
            ?? throw new ArgumentNullException(nameof(definition));
    }

    public Guid InstanceId { get; }

    public PickupDefinition Definition { get; }
}
```

Они не должны содержать:

```csharp
ICollectable Collectable
IInteractable Interactable
IActorDialogue Dialogue
Transform Transform
GameObject GameObject
```

Это всё относится к текущему spawned representation либо к внешним runtime-системам.

---

# 2. Что хранить в общем world registry

Твоя идея держать там `WorldInstance + lifetime` правильная.

Я бы только не называл сам lifetime объектом `LifetimeToken`, потому что один token обычно представляет **одну регистрацию**, а у world object их несколько. В registry лучше хранить агрегированный lifetime:

```csharp
public interface IWorldObjectLifetime : IDisposable
{
    bool IsDisposed { get; }

    void Add(IDisposable registration);
}
```

Запись реестра:

```csharp
public sealed class WorldObjectEntry
{
    public WorldObjectEntry(
        IWorldInstance instance,
        GameObject gameObject,
        IWorldObjectLifetime lifetime)
    {
        Instance = instance
            ?? throw new ArgumentNullException(nameof(instance));

        GameObject = gameObject
            ?? throw new ArgumentNullException(nameof(gameObject));

        Lifetime = lifetime
            ?? throw new ArgumentNullException(nameof(lifetime));
    }

    public Guid InstanceId =>
        Instance.InstanceId;

    public IWorldInstance Instance { get; }

    public GameObject GameObject { get; }

    public Transform Root =>
        GameObject.transform;

    public IWorldObjectLifetime Lifetime { get; }
}
```

Registry:

```csharp
public interface IWorldObjectRegistry
{
    IDisposable Register(WorldObjectEntry entry);

    bool TryGet(
        Guid instanceId,
        out WorldObjectEntry entry);

    bool TryGetInstance<TInstance>(
        Guid instanceId,
        out TInstance instance)
        where TInstance : class, IWorldInstance;

    bool Despawn(Guid instanceId);
}
```

Реализация typed lookup может быть простой:

```csharp
public bool TryGetInstance<TInstance>(
    Guid instanceId,
    out TInstance instance)
    where TInstance : class, IWorldInstance
{
    if (_entries.TryGetValue(instanceId, out var entry) &&
        entry.Instance is TInstance typed)
    {
        instance = typed;
        return true;
    }

    instance = null;
    return false;
}
```

Это не actor registry. Это typed projection общего реестра.

---

# 3. Нужен ли `GameObject` внутри entry

Скорее да, если текущий world registry управляет despawn:

```text
Dispose registrations
Destroy GameObject
remove entry
```

Но лучше определить одного владельца уничтожения.

## Вариант A: entry lifetime уничтожает всё

```csharp
public sealed class WorldObjectLifetime : IDisposable
{
    private readonly GameObject _gameObject;
    private readonly CompositeLifetime _registrations = new();

    public WorldObjectLifetime(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    public void Add(IDisposable registration)
    {
        _registrations.Add(registration);
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;

        _registrations.Dispose();

        if (_gameObject != null)
            UnityEngine.Object.Destroy(_gameObject);
    }

    public bool IsDisposed { get; private set; }
}
```

Тогда registry вызывает только:

```csharp
entry.Lifetime.Dispose();
```

## Вариант B: registry уничтожает GameObject

Тогда lifetime содержит только registrations, а registry после disposal делает `Destroy`.

Оба варианта допустимы, но нельзя распределять ownership между фабрикой, registry и самим actor scope одновременно.

С учётом твоей текущей архитектуры я бы сохранил вариант, где агрегированный world-object lifetime владеет и registrations, и `GameObject`.

---

# 4. Как жить без actor/pickup registries

Система, которой нужен конкретный instance, получает его из общего registry:

```csharp
if (!_worldObjects.TryGetInstance<ActorInstance>(
        actorId,
        out var actor))
{
    return;
}

var definition = actor.Definition;
```

Но это должно происходить только там, где действительно нужна **модель актора**, например:

```text
save/load
quest logic
actor-specific state
respawn/despawn
debugging
```

Для поведения нужно обращаться не к instance, а к capability registry:

```csharp
if (_interactables.TryGet(instanceId, out var interactable))
{
    interactable.Interact(context);
}
```

```csharp
if (_displayNames.TryGet(instanceId, out var nameProvider))
{
    label.text = nameProvider.DisplayName;
}
```

```csharp
if (_targets.TryGet(instanceId, out var target))
{
    targeting.Select(target);
}
```

Это исключает необходимость сначала получать actor/pickup, а затем извлекать capability из него.

---

# 5. Локальные endpoints не обязаны регистрироваться

Ты верно разделяешь два способа доступа.

## Capability нужна внешним системам

Тогда отдельный registry оправдан:

```text
IInteractable
ITargetable
IDisplayNameProvider
IInventoryOwner — если его ищут извне
```

## Capability используется только внутри prefab/scope

Тогда registry не нужен:

```text
IActorDialogue
IActorNavigation
ICollectable
```

Например, `PickupInteractable` может иметь прямую ссылку на `ICollectable`:

```csharp
public sealed class PickupInteractable :
    MonoBehaviour,
    IInteractable
{
    [SerializeField]
    private CollectableEndpoint _collectable;

    public bool CanInteract(
        in InteractionContext context)
    {
        return _collectable.CanCollect(context);
    }

    public UniTask InteractAsync(
        InteractionContext context,
        CancellationToken token)
    {
        return _collectable.CollectAsync(
            context,
            token);
    }
}
```

Внешняя interaction system знает только `IInteractable`.

Она не должна искать `ICollectable`:

```text
InteractionSystem
    → IInteractable
        → локальный ICollectable
```

Для actor аналогично:

```text
ActorDialogueInteractable
    → локальный ActorDialogueEndpoint

DialogueSystem
    не обязан иметь глобальный registry всех IActorDialogue
```

Глобальный registry появляется только при реальном use case:

```text
начать диалог с actorId из quest script,
даже не имея ссылки на interactable
```

---

# 6. `DisplayName` как отдельная capability

Это особенно хорошо после удаления `WorldInfo`.

Для актора:

```csharp
public sealed class ActorDisplayName :
    IDisplayNameProvider
{
    private readonly ActorInstance _instance;

    public ActorDisplayName(ActorInstance instance)
    {
        _instance = instance;
    }

    public string DisplayName =>
        _instance.Definition.DisplayName;
}
```

Для pickup:

```csharp
public sealed class PickupDisplayName :
    IDisplayNameProvider
{
    private readonly PickupInstance _instance;

    public PickupDisplayName(PickupInstance instance)
    {
        _instance = instance;
    }

    public string DisplayName =>
        _instance.Definition.DisplayName;
}
```

Либо prefab endpoint может сам делегировать definition.

Главное, что общий world registry не превращается обратно в metadata service:

```csharp
_worldObjects.TryGetInfo(...)
```

Такого больше не нужно.

---

# 7. Actor scope и pickup без scope

Это нормально сочетается с общей моделью.

## Actor factory

```text
создать ActorInstance
создать prefab
создать child scope
зарегистрировать ActorInstance в scope
resolve локальные actor dependencies
зарегистрировать внешние capabilities
создать WorldObjectEntry
зарегистрировать его в WorldObjectRegistry
```

## Pickup factory

```text
создать PickupInstance
создать prefab
передать instance в endpoint/компоненты
зарегистрировать внешние capabilities
создать WorldObjectEntry
зарегистрировать его в WorldObjectRegistry
```

Разница только в composition.

Результат одинаков:

```text
WorldObjectEntry
    Instance
    GameObject
    Aggregate lifetime
```

---

# 8. Как должен выглядеть spawn lifetime

Например:

```csharp
var lifetime = new WorldObjectLifetime(gameObject);

try
{
    lifetime.Add(
        _interactables.Register(
            instance.InstanceId,
            interactable));

    lifetime.Add(
        _displayNames.Register(
            instance.InstanceId,
            displayName));

    lifetime.Add(
        _targetables.Register(
            instance.InstanceId,
            targetable));

    var entry = new WorldObjectEntry(
        instance,
        gameObject,
        lifetime);

    lifetime.Add(
        _worldObjects.Register(entry));

    return entry;
}
catch
{
    lifetime.Dispose();
    throw;
}
```

Для actor дополнительно:

```csharp
lifetime.Add(actorScope);
```

только если scope создан вручную и не будет гарантированно корректно disposed вместе с prefab.

В actor scope сам `ActorInstance` регистрируется как external instance:

```csharp
builder.RegisterInstance(actorInstance);
```

Но регистрация `ActorInstance` в контейнере не делает его capability и не требует actor registry. Это просто scoped dependency для actor-local сервисов.

---

# 9. Важный риск общего registry с `IWorldInstance`

При такой модели можно начать злоупотреблять:

```csharp
_worldObjects.TryGet(id, out var entry);

switch (entry.Instance)
{
    case ActorInstance actor:
    case PickupInstance pickup:
}
```

Если это начинает встречаться часто, значит система нарушает capability boundary.

Нормальное использование type check:

```text
serialization
debug inspector
spawn/despawn administration
quest logic, действительно завязанная на ActorInstance
```

Ненормальное:

```text
interaction
targeting
display name
collecting
dialogue execution
```

Для них должны использоваться соответствующие registry или локальные endpoints.

---

# 10. Нужен ли вообще интерфейс `IWorldInstance`

Да, но минимальный:

```csharp
public interface IWorldInstance
{
    Guid InstanceId { get; }
}
```

Возможно, ещё общий definition-контракт:

```csharp
public interface IWorldDefinition
{
    GameObject Prefab { get; }
}
```

Но я бы не заставлял `IWorldInstance` возвращать `IWorldDefinition`, если это приводит к потере конкретного типа:

```csharp
ActorDefinition Definition
PickupDefinition Definition
```

Registry достаточно знать только `InstanceId`. Фабрики и typed instance знают конкретную definition.

---

# Итог

Твоя предлагаемая архитектура выглядит правильнее предыдущих вариантов:

```text
ActorInstance
    InstanceId
    ActorDefinition
    actor state

PickupInstance
    InstanceId
    PickupDefinition
    pickup state

WorldObjectRegistry
    InstanceId
        → IWorldInstance
        → GameObject
        → aggregate lifetime

Capability registries
    InstanceId → внешне доступная capability

Prefab-local endpoints
    capability, которая используется только внутри объекта
```

Отдельные:

```text
ActorRegistry
PickupRegistry
```

не нужны, пока нет специализированных actor/pickup queries или отдельного времени жизни их моделей.

Я бы зафиксировал правило:

> `WorldObjectRegistry` хранит identity и lifetime. Capability registry хранит внешнюю адресуемую возможность. Всё, что используется только внутри prefab или scope, остаётся локальной зависимостью.

Это даёт одну общую инфраструктуру для actor и pickup, не смешивая их модели и не превращая instances в service locator.


Да. В текущем состоянии `ActorRuntime` можно удалить **без создания нового actor-фасада**.

Сейчас он не владеет состоянием и не обеспечивает инвариантов: это контейнер из `ActorInstance`, `IActorView`, `IActorNavigation`, `IActorDialogue`, `IActorInputBinder` и `IActorTargeting`. `ActorService`, в свою очередь, существует практически только как `InstanceId → IActorRuntime`. Поэтому после перехода на общий registry world-объектов и отдельные capability-registry эти два типа теряют назначение одновременно. ([raw.githubusercontent.com][1])

## Итоговое разбиение

```text
ActorInstance
    только identity, definition и собственное сохраняемое состояние

SpawnedObjectRegistry
    InstanceId → WorldInstance + GameObject + aggregate lifetime

Capability registries
    InstanceId → IPossessable
    InstanceId → ITargetProvider
    InstanceId → IInteractable
    InstanceId → IDisplayNameProvider

Actor scope / prefab-local
    INavigationAgent
    IFacingController
    Dialogue component
    ActorInputBinder как внутренняя реализация
```

## `IActorView` и `IActorInputBinder`

На уровне реализации это разные вещи:

* `IActorView` предоставляет `Root` и `CameraPivot`;
* `ActorInputBinder` раздаёт `IActorInput` контроллерам движения, взгляда и targeting. ([GitHub][2])

Но **на уровне внешнего доступа** они действительно образуют одну цельную capability — возможность передать объект под управление игрока.

Я бы ввёл:

```csharp
public interface IPossessable
{
    Transform CameraPivot { get; }

    void BindInput(IActorInput input);

    void UnbindInput();
}
```

Реализация может оставаться тонким endpoint:

```csharp
public sealed class ActorPossessable :
    MonoBehaviour,
    IPossessable
{
    [SerializeField]
    private Transform _cameraPivot;

    private IActorInputBinder _inputBinder;

    public Transform CameraPivot => _cameraPivot;

    [Inject]
    public void Construct(IActorInputBinder inputBinder)
    {
        _inputBinder = inputBinder;
    }

    public void BindInput(IActorInput input)
    {
        _inputBinder.Bind(input);
    }

    public void UnbindInput()
    {
        _inputBinder.Unbind();
    }
}
```

То есть:

```text
IPossessable
    внешняя capability, регистрируемая по InstanceId

IActorInputBinder
    внутренняя деталь actor scope
```

`IActorView` после этого, вероятно, вообще удаляется:

* `CameraPivot` переезжает в `IPossessable`;
* `Root` уже доступен через запись spawned world object и её `GameObject.transform`.

Если `Root` нужен каким-то gameplay-системам отдельно от world registry, это должна быть общая capability вроде `ITransformProvider`, а не `IActorView`.

### Почему лучше объединить possession атомарно

При захвате управления игрок почти всегда должен одновременно:

1. передать input;
2. перевести камеру;
3. сохранить текущий possessed `InstanceId`.

Два независимых lookup:

```csharp
_inputBinders.TryGet(id, ...);
_actorViews.TryGet(id, ...);
```

допускают половинчатую конфигурацию prefab. Один `IPossessable` выражает настоящий инвариант:

> объект можно possess только тогда, когда он полностью предоставляет всё необходимое для possession.

Если в будущем player сможет управлять турелью или транспортом, название `IPossessable` уже не будет привязано к `Actor`.

---

## Что меняется в `PlayerService`

Текущий `IPlayerService` уже хранит ID текущего актора, а не ссылку на `ActorRuntime`, поэтому его легко перевести на possessable registry. ([GitHub][3])

Лучше даже сменить терминологию:

```csharp
public interface IPlayerPossessionService
{
    Guid CurrentPossessedInstanceId { get; }

    event Action CurrentPossessionChanged;

    bool TryPossess(Guid instanceId);

    void Release();
}
```

Схематично:

```csharp
public sealed class PlayerPossessionService :
    IPlayerPossessionService
{
    private readonly IInstanceRegistry<IPossessable> _possessables;
    private readonly IActorInput _playerInput;
    private readonly IPlayerCamera _camera;

    private IPossessable _current;

    public Guid CurrentPossessedInstanceId { get; private set; }

    public bool TryPossess(Guid instanceId)
    {
        if (!_possessables.TryGet(instanceId, out var next))
            return false;

        _current?.UnbindInput();

        next.BindInput(_playerInput);
        _camera.SetTarget(next.CameraPivot);

        _current = next;
        CurrentPossessedInstanceId = instanceId;

        CurrentPossessionChanged?.Invoke();
        return true;
    }

    public void Release()
    {
        _current?.UnbindInput();
        _current = null;

        CurrentPossessedInstanceId = Guid.Empty;
        CurrentPossessionChanged?.Invoke();
    }

    public event Action CurrentPossessionChanged;
}
```

Registry можно оставить на базе существующего `InstanceIndex<T>`. Не обязательно вводить отдельный класс `PossessableService`, если у тебя generic capability-registry уже является стандартным механизмом.

---

## `IActorDialogue`

Его нужно убрать из actor runtime aggregate.

Текущий контракт — это одна команда `StartDialogueAsync(...)`, то есть prefab endpoint поведения, а не состояние ActorInstance. ([GitHub][4])

Варианты:

```text
DialogueInteractable
    → прямая ссылка на ActorDialogue component
```

или:

```text
ActorDialogue component
    реализует IDialogueEndpoint
    внедряется локально через actor scope
```

Я бы переименовал интерфейс:

```csharp
public interface IDialogueEndpoint
{
    UniTask StartDialogueAsync(
        Guid interactorInstanceId,
        CancellationToken cancellationToken);
}
```

Глобальный registry для него не нужен, пока нет реального сценария:

```text
запустить диалог по InstanceId без взаимодействия с prefab
```

Если такой use case появится, можно зарегистрировать `IDialogueEndpoint`. Но не нужно делать это заранее только потому, что раньше endpoint находился в `ActorRuntime`.

---

## `IActorNavigation`

Его точно не нужно переносить в общий capability-registry автоматически.

Текущий интерфейс сочетает две разные группы операций:

```text
Navigation / locomotion:
    MoveToPositionAsync
    Stop
    IsMoving
    HasArrived

Facing:
    FaceDirectionAsync
    FaceDirection
    ClearFacing
    IsFacingComplete
```

Это видно непосредственно из текущего контракта. ([GitHub][5])

### Практический вариант сейчас

Сначала просто убрать его из `ActorRuntime`, но оставить в actor scope:

```text
Behavior graph / task handler
    → INavigationAgent

Dialogue sequence / arrival action
    → IFacingController
```

Можно временно сохранить текущий `IActorNavigation` без рефакторинга методов, но изменить его статус:

```text
раньше: globally addressable actor capability
теперь: local scoped dependency актора
```

### Более чистый вариант

```csharp
public interface INavigationAgent
{
    bool IsMoving { get; }

    bool HasArrived { get; }

    UniTask MoveToAsync(
        Vector3 destination,
        CancellationToken cancellationToken);

    void Stop();
}
```

```csharp
public interface IFacingController
{
    bool IsFacingComplete { get; }

    UniTask FaceAsync(
        Vector3 direction,
        CancellationToken cancellationToken);

    void Face(Vector3 direction);

    void Clear();
}
```

Название `IActorNavigation` несколько вводит в заблуждение, потому что facing не является navigation.

### Когда navigation-registry всё-таки нужен

Только если внешняя система должна напрямую сказать:

```text
найти объект по InstanceId и вести его в точку
```

Но даже тогда лучше регистрировать не низкоуровневый nav-agent, а более высокий command endpoint:

```csharp
public interface IActorCommandSink
{
    void Submit(ActorCommand command);
}
```

Иначе quest, dialogue и AI начнут напрямую конкурировать за `INavigationAgent`, обходя твой scheduler/Behavior Graph и правила блокировок.

Поэтому мой вывод:

```text
INavigationAgent / IFacingController
    actor-local scope

IActorCommandSink
    registry, только если действительно нужен внешний control
```

---

## `IActorTargeting`

Его можно удалить почти механически.

Текущие `IActorTargeting` и `ITargetProvider` предоставляют одну и ту же семантику:

```csharp
ITargetable CurrentTarget { get; }

event Action CurrentTargetChanged;
```

`ActorTargetController` сейчас реализует `IActorTargeting`, но фактически уже является target provider. ([GitHub][6])

Нужно сделать:

```csharp
public sealed class ActorTargetController :
    MonoBehaviour,
    ITargetProvider
{
    // Текущая реализация.
}
```

И зарегистрировать:

```csharp
targetProviderRegistry.Register(
    actorInstance.InstanceId,
    actorTargetController);
```

После этого:

```text
IActorTargeting     удалить
ActorRuntime.Targeting удалить
ITargetProvider registry оставить
```

### Текущие consumers

`PlayerTargetControlBinder` уже движется в правильном направлении: он получает `ITargetProvider` напрямую из capability-registry. А `PlayerTargetNameplatePresenter` пока делает обход через `ActorService → ActorRuntime.Targeting`. Второй consumer нужно перевести на тот же `ITargetProvider` registry. ([GitHub][7])

Схематично:

```csharp
private void BindTargetProvider(Guid possessedInstanceId)
{
    UnbindTargetProvider();

    if (!_targetProviders.TryGet(
            possessedInstanceId,
            out _targetProvider))
    {
        return;
    }

    _targetProvider.CurrentTargetChanged += OnTargetChanged;
    OnTargetChanged();
}
```

После этого nameplate presenter больше не зависит ни от `ActorService`, ни от `ActorRuntime`.

---

## Где теперь хранится `ActorInstance`

В общем registry spawned world objects, как ты и предлагал.

Сейчас `SpawnedObject` уже владеет:

* `InstanceId`;
* `GameObject`;
* агрегированными registration tokens;
* уничтожением объекта при disposal.

А `SpawnedObjectRegistry` отслеживает его по GUID и выполняет `Despawn`. ([GitHub][8])

Я бы изменил запись так:

```csharp
public interface IWorldInstance
{
    Guid InstanceId { get; }
}
```

```csharp
public sealed class SpawnedObject : IDisposable
{
    private readonly CompositeLifetime _lifetime = new();

    public SpawnedObject(
        IWorldInstance instance,
        GameObject gameObject)
    {
        Instance = instance
            ?? throw new ArgumentNullException(nameof(instance));

        GameObject = gameObject
            ?? throw new ArgumentNullException(nameof(gameObject));
    }

    public IWorldInstance Instance { get; }

    public Guid InstanceId => Instance.InstanceId;

    public GameObject GameObject { get; }

    public void Add(IDisposable token)
    {
        _lifetime.Add(token);
    }

    public void Dispose()
    {
        _lifetime.Dispose();

        if (GameObject != null)
            UnityEngine.Object.Destroy(GameObject);
    }
}
```

Тогда административный lookup модели выглядит так:

```csharp
if (_spawnedObjects.TryGet(
        instanceId,
        out var spawned) &&
    spawned.Instance is ActorInstance actor)
{
    var definition = actor.Definition;
}
```

Это допустимо для:

```text
save/load
despawn
quest state
debugging
world administration
```

Но interaction, targeting, possession и display name должны продолжать использовать свои capability-registry.

---

# Конечная схема actor prefab

```text
ActorInstance
    InstanceId
    ActorDefinition
    сохраняемое actor state

Actor scope
    ActorInstance registered as instance
    ActorInputBinder
    INavigationAgent
    IFacingController
    другие actor-local services

Actor prefab components
    ActorPossessable : IPossessable
    ActorTargetController : ITargetProvider
    ActorDialogue : IDialogueEndpoint
    ActorInteractable : IInteractable
    DisplayNameProvider : IDisplayNameProvider
```

Во внешние registry попадают только реально адресуемые capabilities:

```text
IPossessable
ITargetProvider
IInteractable
IDisplayNameProvider
```

Локально остаются:

```text
IActorInputBinder
INavigationAgent
IFacingController
IDialogueEndpoint, если используется только interactable
```

---

## Что удалить и что оставить

| Текущий тип                 | Решение                                                                            |
| --------------------------- | ---------------------------------------------------------------------------------- |
| `ActorRuntime`              | удалить                                                                            |
| `IActorRuntime`             | удалить                                                                            |
| `ActorService`              | удалить                                                                            |
| `IActorRegistrationService` | удалить                                                                            |
| `IActorView`                | удалить; `CameraPivot` перенести в `IPossessable`, `Root` брать из spawned object  |
| `IActorInputBinder`         | оставить как actor-local implementation detail                                     |
| `IActorDialogue`            | заменить на prefab-local `IDialogueEndpoint`                                       |
| `IActorNavigation`          | убрать из global aggregate; оставить локально, позднее разделить navigation/facing |
| `IActorTargeting`           | удалить                                                                            |
| `ITargetProvider`           | использовать напрямую и регистрировать по `InstanceId`                             |
| `ActorInstance`             | оставить чистым                                                                    |
| `SpawnedObjectRegistry`     | хранить сам `IWorldInstance` вместе с GameObject и lifetime                        |

Главное правило получается таким:

> `ActorInstance` хранит модель актора. `SpawnedObject` владеет его присутствием в мире. Registry capabilities предоставляет внешние возможности. Actor scope связывает внутренности prefab. Универсального `ActorRuntime` между ними больше не требуется.

Да, IDialogueParticipant лучше. Он не привязан ни к actor, ни к pickup.
public interface IDialogueParticipant
{
    UniTask StartDialogueAsync(
        Guid otherParticipantInstanceId,
        CancellationToken token);
}
Тогда один компонент DialogueParticipant можно добавить на NPC, книгу, статую, записку, терминал или pickup. Он реализует и IInteractable, и IDialogueParticipant.
Player interaction
→ IInteractionService
→ DialogueParticipant : IInteractable
→ IDialogueParticipant.StartDialogueAsync(playerId)
→ dialogue session / Yarn
А когда NPC сам начинает разговор:
npcDialogueParticipant.StartDialogueAsync(playerId)
То есть otherParticipantInstanceId всегда означает «с кем начинается диалог», независимо от того, кто вызвал метод.
В плане заменяем пункт про IDialogueEndpoint на:
IActorDialogue удалить.
В Game.Dialogue ввести IDialogueParticipant.
DialogueParticipant сделать универсальным prefab-компонентом, который при необходимости реализует IInteractable.
Не регистрировать participant глобально, пока нет сценария «найди участника диалога по InstanceId и запусти извне».


PickupDefinition добавить DefinitionId