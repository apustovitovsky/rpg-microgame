Да, я посмотрел свежий `main`: последний коммит действительно **`spawn catalogs implement`**, `0f782f9`, 58 файлов changed. ([GitHub][1])

Твое ощущение правильное: **мы решили проблему service locator, но переехали в другую крайность — registration/DI ceremony explosion**.

## Что сейчас хорошо

Сам `Game.World/Runtime` стал концептуально чище:

```text
WorldObject = только WorldId + GameObject + Root
WorldRegistry<T> = typed dictionary
WorldManager = lifetime / despawn
WorldSpawner = вызывает factory и track-ает lifetime
```

`WorldObject` больше не содержит `TryGet<T>`, то есть глобальный endpoint-locator убран. Это правильное направление. ([GitHub][2])

`WorldManager` тоже стал нормальным lifecycle-owner: он хранит `WorldId -> WorldEntry`, при `Despawn` вызывает `Lifetime.Dispose()` и уничтожает `GameObject`. ([GitHub][3])

`RegistrationToken` / `CompositeRegistration` тоже сделаны нормально: dispose удаляет записи из конкретных registry через callback, composite вызывает dispose в обратном порядке. ([GitHub][4])

## Где стало плохо

Главная проблема теперь не в `World/Runtime`, а в **composition layer**.

`ActorWorldObjectFactory` превратился в “бог-фабрику регистрации”: он получает кучу `IWorldRegistry<T>` — actors, anchors, inputBinders, targetProviders, interactions, dialogues, travels, pickupEffectHandlers — потом resolve-ит endpoints из actor scope и вручную регистрирует каждый. ([GitHub][5])

`PickupWorldObjectFactory` делает похожее, но через `GetComponent/TryGetComponent`: instantiate prefab, initialize pickup, inject interactable, register world object, pickup, interactable, targetable. ([GitHub][6])

То есть ты убрал locator из `WorldObject`, но теперь **каждая фабрика знает слишком много о глобальных индексах**.

```text
Было плохо:
  WorldObject.TryGet<T> откуда угодно

Стало плохо:
  Factory знает все registries и вручную склеивает всю систему
```

Это не ужас, но выглядит тяжело, потому что composition размазался по фабрикам.

## Самый важный вывод

Я бы **не откатывался к WorldObject.TryGet<T>**, но и **не оставлял фабрики с 8–10 registry-зависимостями**.

Правильный компромисс:

```text
Domain factory:
  instantiate + initialize object

Domain registrar/binder:
  знает, какие endpoints публикует actor/pickup

WorldManager:
  только lifetime

Typed registries:
  остаются, но спрятаны за registration facade
```

## Что я бы поменял

### 1. Убрать registries из Actor/Pickup factories

Сейчас factory делает слишком много. Я бы разделил:

```text
ActorWorldObjectFactory:
  instantiate prefab
  initialize WorldActor
  вернуть ActorSpawnedObject

ActorWorldRegistrar:
  зарегистрировать actor endpoints

PickupWorldObjectFactory:
  instantiate prefab
  initialize WorldPickup
  вернуть PickupSpawnedObject

PickupWorldRegistrar:
  зарегистрировать pickup endpoints
```

Тогда factory не знает все registry.

Примерно:

```csharp
public sealed class ActorWorldObjectFactory
{
    public ActorSpawnedObject Create(ActorSpawnRequest request)
    {
        // instantiate
        // initialize
        // resolve actor scope endpoints
        // return object with endpoints
    }
}
```

```csharp
public sealed class ActorWorldRegistrar
{
    public IRegistrationToken Register(ActorSpawnedObject actor)
    {
        var lifetime = new CompositeRegistration();

        lifetime.Add(_worldObjects.Register(actor.WorldId, actor.WorldObject));
        lifetime.Add(_actors.Register(actor.WorldId, actor.Actor));
        lifetime.Add(_anchors.Register(actor.WorldId, actor.Anchors));

        if (actor.InputBinder != null)
            lifetime.Add(_inputBinders.Register(actor.WorldId, actor.InputBinder));

        return lifetime;
    }
}
```

Да, registrar всё еще знает registries, но это **его единственная работа**. Сейчас эта работа смешана с instantiate, VContainer scope, validation, naming и initialization.

### 2. Сделать aggregate для registries, но использовать только в registration layer

Можно сделать:

```csharp
public sealed class WorldRegistries
{
    public IWorldRegistry<IWorldObject> WorldObjects { get; }
    public IWorldRegistry<IWorldActor> Actors { get; }
    public IWorldRegistry<IActorAnchors> Anchors { get; }
    public IWorldRegistry<IInteractable> Interactions { get; }
    public IWorldRegistry<ITargetable> Targets { get; }
    public IWorldRegistry<IWorldPickup> Pickups { get; }
}
```

Это **не должен быть общий сервис для всех**. Его можно inject-ить только в factories/registrars.

Правило:

```text
Gameplay services получают только нужный registry:
  InteractionService -> IWorldRegistry<IInteractable>
  PickupService -> IWorldRegistry<IWorldPickup>
  PlayerService -> IWorldRegistry<IActorInputBinder>

Factories/registrars могут получить WorldRegistries.
```

Так ты снизишь DI-шум, но не вернешь service locator в доменные сервисы.

### 3. `WorldSpawner` сейчас почти лишний

Сейчас `WorldSpawner.Spawn(request, factory)` просто вызывает `factory.Create(request)`, проверяет result и передает в `WorldManager.Track`. ([GitHub][7])

Это абстракция ради абстракции. В текущем виде можно проще:

```text
ActorSpawner:
  factory.Create
  registrar.Register
  worldManager.Track

PickupSpawner:
  factory.Create
  registrar.Register
  worldManager.Track
```

Или один `SpawnLifetimeTracker`, но не generic `WorldSpawner`.

Сейчас `GameplayManager` все равно знает `_actorFactory`, `_pickupFactory`, `_worldSpawner`, `_world`, `_player`, catalogs, navigation resolver. ([GitHub][8]) Поэтому “универсальность” `WorldSpawner` пока не дает выигрыша.

Я бы упростил:

```text
IActorSpawner.Spawn(ActorSpawnRequest)
IPickupSpawner.Spawn(PickupSpawnRequest)
```

А generic `WorldSpawner` убрать.

### 4. `GameplayManager` стал слишком orchestration-heavy

Он сейчас делает:

```text
SpawnPlayer
SpawnActors
SpawnPickups
Resolve navigation anchor
Create WorldId
Build requests
Call world spawner
Bind player
DespawnAll on Dispose
```

Это уже много. ([GitHub][8])

Я бы разделил:

```text
GameplayBootstrapper
  вызывает:
    PlayerSpawnSystem.Spawn()
    ActorSpawnSystem.SpawnAll()
    PickupSpawnSystem.SpawnAll()

SpawnPointResolver
  locationId + anchorKey -> position/rotation

WorldIdFactory
  создает WorldId
```

Хотя бы `CreateWorldId` вынести из `GameplayManager`. Сейчас ID от `DisplayName` + индекс — норм для debug, но не для save/load.

### 5. Nameplate уже показывает, почему typed registries тоже могут расползтись

`PlayerTargetNameplatePresenter` сейчас получает `targetProviders`, `actors`, `pickups`, и делает:

```text
если target actor -> actor.Definition.DisplayName
если pickup -> pickup.Definition.DisplayName
иначе WorldId
```

То есть UI снова знает про actor/pickup типы. ([GitHub][9])

Лучше добавить один registry:

```csharp
IWorldRegistry<IDisplayInfo>
```

И регистрировать туда actor/pickup display endpoint. Тогда presenter получает только:

```text
IWorldRegistry<ITargetProvider>
IWorldRegistry<IDisplayInfo>
```

А не actor + pickup отдельно.

## Что я бы оставил

Оставить:

```text
WorldId
WorldObject без TryGet
WorldRegistry<T>
RegistrationToken
CompositeRegistration
WorldManager
ActorDefinition / PickupDefinition
ActorSpawnCatalog / PickupSpawnCatalog
```

Definitions/catalogs как идея хорошие: `ActorDefinition` и `PickupDefinition` содержат `DisplayName` и `Prefab`, catalogs задают definition + location/anchor. ([GitHub][10])

## Что я бы убрал/пересобрал

Убрать или упростить:

```text
WorldSpawner<TRequest> как отдельную абстракцию
IWorldObjectFactory<TRequest> как общий интерфейс, если он только усложняет
фабрики с 8–10 registry dependencies
UI switch actor/pickup для имени
```

## Более чистая итоговая схема

```text
Definitions:
  ActorDefinition
  PickupDefinition

Catalogs:
  ActorSpawnCatalog
  PickupSpawnCatalog

Spawners:
  ActorSpawner
  PickupSpawner

Factories:
  ActorPrefabFactory
  PickupPrefabFactory

Registrars:
  ActorWorldRegistrar
  PickupWorldRegistrar

Runtime:
  WorldManager
  WorldRegistry<T>
  RegistrationToken
```

Для actor:

```text
ActorSpawner
  -> ActorPrefabFactory.Create(request)
  -> ActorWorldRegistrar.Register(spawnedActor)
  -> WorldManager.Track(worldObject, lifetime)
```

Для pickup:

```text
PickupSpawner
  -> PickupPrefabFactory.Create(request)
  -> PickupWorldRegistrar.Register(spawnedPickup)
  -> WorldManager.Track(worldObject, lifetime)
```

## Мой вердикт

Архитектура сейчас **не неправильная**, но она стала слишком “enterprise” для текущего масштаба.

Я бы оценил так:

```text
World/Runtime core: 8/10
Spawn/catalog idea: 7/10
Composition/factories/DI ergonomics: 4/10
```

Главная правка: **не возвращать service locator**, а вынести registration ceremony из factories в `ActorWorldRegistrar` / `PickupWorldRegistrar` и добавить `IDisplayInfo`, чтобы UI не знал actor/pickup.

То есть направление правильное, но следующий рефактор должен быть не “еще больше универсальности”, а наоборот: **сгруппировать composition по use-case и убрать универсальные прослойки, которые не дают реальной пользы**.

[1]: https://github.com/apustovitovsky/rpg-microgame/commits/main/ "Commits · apustovitovsky/rpg-microgame · GitHub"
[2]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/WorldObject.cs "raw.githubusercontent.com"
[3]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/World/Runtime/WorldManager.cs "raw.githubusercontent.com"
[4]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/CompositeRegistration.cs "raw.githubusercontent.com"
[5]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/Actor/Runtime/ActorWorldObjectFactory.cs "raw.githubusercontent.com"
[6]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/Pickup/Runtime/PickupWorldObjectFactory.cs "raw.githubusercontent.com"
[7]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/World/Runtime/WorldSpawner.cs "raw.githubusercontent.com"
[8]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/Gameplay/Runtime/GameplayManager.cs "raw.githubusercontent.com"
[9]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/Player/Runtime/PlayerTargetNameplatePresenter.cs "raw.githubusercontent.com"
[10]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/0f782f9cf1f37ca796872074eba3499c3d908b2c/Assets/Game/Scripts/Gameplay/SO/ActorSpawnCatalog.cs "raw.githubusercontent.com"

Да, ты правильно чувствуешь: **`WorldRegistry<IWorldObject>` почти не нужен**, если `IWorldObject` содержит только:

```csharp
WorldId
GameObject
Root
```

Сейчас это фактически “таблица уничтожаемых GameObject по id”, а не полноценная доменная сущность. `WorldObject` сейчас реально хранит только `WorldId` и `GameObject`, а `Root` просто возвращает `GameObject.transform`. ([GitHub][1])

## Что оставить

Я бы оставил **`WorldManager`**, но переосмыслил его как **lifetime registry**, а не как “реестр сущностей”.

Сейчас `WorldManager` уже делает именно это:

```text
Track(worldObject, lifetime)
Despawn(worldId)
DespawnAll()
```

При `Despawn` он вызывает `Lifetime.Dispose()` и уничтожает `GameObject`. Это нормальная роль. ([GitHub][2])

То есть лучше мыслить так:

```text
WorldManager = владелец runtime lifetime
```

а не:

```text
WorldManager = место, где лежат игровые сущности
```

## Что можно убрать

Я бы убрал отдельную регистрацию:

```csharp
IWorldRegistry<IWorldObject>
```

Если она нужна только чтобы потом найти `GameObject` по `WorldId`, то это дублирование `WorldManager`.

Вместо этого можно сделать:

```csharp
public interface IWorldManager
{
    IRegistrationToken Track(WorldId id, GameObject gameObject, IRegistrationToken lifetime);
    bool TryGetObject(WorldId id, out GameObject gameObject); // опционально
    bool Despawn(WorldId id);
    void DespawnAll();
}
```

Или даже без `TryGetObject`, если никто реально не должен искать голый `GameObject`.

## Главная проблема сейчас

Ты пытаешься сделать “сущность” из объекта, который не содержит доменной модели.

Вот это не сущность:

```text
WorldObject
  WorldId
  GameObject
  Root
```

Это скорее:

```text
WorldHandle
WorldEntry
WorldLifetimeEntry
SpawnedObjectHandle
```

А настоящие доменные сущности у тебя должны быть другие:

```text
ActorInstance / WorldActor
  WorldId
  ActorDefinition
  runtime state
  endpoints: view, abilities, dialogue, travel, etc.

PickupInstance / WorldPickup
  WorldId
  PickupDefinition
  collected state
  endpoints: pickup, targetable, interactable, display
```

## Как бы я разложил роли

### `WorldManager`

Только lifecycle:

```text
зарегистрировать spawned object
удалить object
dispose registrations
destroy GameObject
despawn all
```

Он не знает:

```text
actor это или pickup
interactable ли он
targetable ли он
display name
```

### `ActorRegistry`

Хранит акторов:

```text
WorldId -> IWorldActor / ActorInstance
```

### `PickupRegistry`

Хранит пикапы:

```text
WorldId -> IPickup / WorldPickup
```

### `TargetRegistry`

Хранит таргеты:

```text
WorldId -> ITargetable
```

### `InteractionRegistry`

Хранит interactables:

```text
WorldId -> IInteractable
```

### `DisplayRegistry`

Хранит UI-инфо:

```text
WorldId -> IDisplayInfo
```

## Тогда нужен ли `IWorldObject`?

Возможно, нет.

Или он должен быть переименован во что-то честное:

```text
IWorldLifetimeObject
ISpawnedWorldObject
IWorldHandle
IWorldEntry
```

Например:

```csharp
public readonly struct SpawnedWorldObject
{
    public WorldId WorldId { get; }
    public GameObject GameObject { get; }
    public IRegistrationToken Lifetime { get; }
}
```

И это не “entity”, а просто handle для менеджера.

## Я бы сделал так

Убрать:

```text
WorldObject как доменную сущность
IWorldRegistry<IWorldObject>
```

Оставить:

```text
WorldId
WorldManager
WorldRegistry<T> для typed registries
RegistrationToken
CompositeRegistration
```

Заменить `WorldObject` на более честный `WorldEntry` внутри `WorldManager`.

Например:

```csharp
public interface IWorldManager
{
    bool Track(
        WorldId worldId,
        GameObject gameObject,
        IRegistrationToken lifetime);

    bool Despawn(WorldId worldId);
    void DespawnAll();
}
```

Тогда factory/spawner делает:

```csharp
var lifetime = new CompositeRegistration();

lifetime.Add(_actors.Register(worldId, actor));
lifetime.Add(_targets.Register(worldId, targetable));
lifetime.Add(_interactions.Register(worldId, interactable));
lifetime.Add(_displays.Register(worldId, displayInfo));

_worldManager.Track(worldId, instance, lifetime);
```

И всё. Не нужен промежуточный `WorldObject`.

## Почему тебе сейчас кажется, что “сущности не те”

Потому что `WorldObject` звучит как доменная сущность, но по факту это инфраструктурный record для lifetime. Из-за названия кажется, что вокруг него надо строить архитектуру, registry, spawn result, factory result и т.п.

На самом деле центр модели должен быть не `WorldObject`, а:

```text
ActorDefinition / ActorInstance
PickupDefinition / PickupInstance
WorldId
Typed registries
Lifetime manager
```

## Итог

Да: **если `WorldRegistry<IWorldObject>` хранит только `GameObject + id`, он тебе почти не нужен.**

Я бы оставил один слой:

```text
WorldManager = despawn/lifetime/destroy
```

А доменные данные хранил бы в специализированных runtime-объектах:

```text
WorldActor
WorldPickup
```

Самое правильное изменение сейчас: **перестать называть `WorldObject` сущностью**. Либо удалить его, либо переименовать в `WorldLifetimeEntry` / `SpawnedObjectHandle` и не использовать как центральную модель игры.

[1]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/WorldObject.cs "raw.githubusercontent.com"
[2]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/World/Runtime/WorldManager.cs "raw.githubusercontent.com"
