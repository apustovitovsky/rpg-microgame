Да, я бы именно так и сделал: **capabilities собирать в фабриках/биндерах**, а не заставлять каждый компонент реализовывать `IWorldCapability`.

Твое ощущение правильное.

## Проблема текущего подхода

`IWorldCapability` выглядит универсально, но на практике:

```text id="yy3z2f"
Actor:
  много компонентов
  можно авто-собрать endpoints

Pickup:
  endpoints часто известны заранее
  их всё равно собирает binder/factory
```

То есть для actor это удобно, а для pickup получается церемония:

```text id="3ks2m7"
PickupComponent implements IPickup, IWorldCapability
PickupTarget implements ITargetable, IWorldCapability
PickupInteractable implements IInteractable, IWorldCapability
WorldCapabilityProvider собирает то, что ты и так знаешь руками
```

Это уже не магия, а лишний слой.

## Что лучше

Оставить `WorldObject` как универсальную runtime-сущность:

```text id="lb2rhu"
WorldObject
  WorldId
  Root
  endpoints dictionary
```

Но endpoints добавлять явно в composition root:

```text id="na0a34"
ActorWorldBinder
PickupWorldBinder
ChestWorldBinder
DoorWorldBinder
Spawner/Factory
```

Примерно так:

```csharp id="hf3yem"
public sealed class WorldObjectBuilder
{
    private readonly Dictionary<Type, object> _endpoints = new();

    public WorldObjectBuilder Add<T>(T endpoint) where T : class
    {
        if (!_endpoints.TryAdd(typeof(T), endpoint))
            throw new InvalidOperationException($"Duplicate endpoint: {typeof(T).Name}");

        return this;
    }

    public IWorldObject Build(WorldId id, Transform root)
    {
        return new WorldObject(id, root, _endpoints);
    }
}
```

И pickup binder:

```csharp id="943fcj"
public sealed class PickupWorldBinder : MonoBehaviour
{
    [SerializeField] private PickupComponent _pickup;
    [SerializeField] private PickupTarget _target;
    [SerializeField] private PickupInteractable _interactable;
    [SerializeField] private Transform _root;

    public IWorldObject Build()
    {
        return new WorldObjectBuilder()
            .Add<IPickup>(_pickup)
            .Add<ITargetable>(_target)
            .Add<IInteractable>(_interactable)
            .Build(_pickup.WorldId, _root);
    }
}
```

Вот это проще и честнее.

## Что делать с `IWorldCapability`

Я бы сделал один из двух вариантов.

### Вариант A — удалить

Самый чистый вариант:

```text id="e3jppc"
нет IWorldCapability
нет PublishedTypes
нет WorldCapabilityProvider
есть только explicit binders/builders
```

Это проще всего контролировать.

### Вариант B — оставить как optional shortcut

Например для actor prefab:

```text id="2sbv0u"
ActorWorldBinder может собрать все IWorldCapability в детях
```

Но это не основной контракт системы, а удобный helper.

То есть:

```text id="280ayb"
WorldObjectBuilder = основной способ
IWorldCapability = опциональный auto-publish helper
```

## Моя рекомендация для твоего проекта

Я бы выбрал **explicit builder/binder**.

```text id="4hrbgv"
WorldObjectRegistry
  хранит готовые WorldObject

ActorWorldBinder
  явно добавляет actor endpoints

PickupWorldBinder
  явно добавляет pickup endpoints

WorldObjectBuilder
  проверяет duplicate endpoints
```

А `IWorldCapability` либо убрать, либо оставить только если реально помогает на actor prefab.

## Почему это лучше

Потому что сборка capabilities — это не ответственность gameplay-компонента.

`PickupComponent` должен знать:

```text id="nespaz"
я pickup
я collectable
у меня definition
я могу mark collected
```

Но не обязан знать:

```text id="cn9vdg"
как меня публиковать в WorldObjectRegistry
под какими интерфейсами меня регистрировать
```

Это задача binder/factory.

## Итог

Да, **не усложняй маркерными интерфейсами там, где composition root и так знает состав объекта**.

Я бы сделал так:

```text id="gnketw"
оставить:
  IWorldObject
  IWorldObjectRegistry
  WorldObjectBuilder

убрать/сильно ограничить:
  IWorldCapability
  WorldCapabilityProvider

перенести сборку:
  в ActorWorldBinder
  в PickupWorldBinder
  в factories/spawners
```

Это будет более явно, проще дебажить и лучше масштабируется для pickup/chest/door/scene object.

При таком подходе можно сделать универсальный спавнер, но разные фабрики для пикапа и актора?