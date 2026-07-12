По умолчанию каждый instance я бы делал **обычным `sealed` C#-объектом**, содержащим только:

```text
InstanceId
Definition
```

И удобный делегирующий `TryGetFragment<T>()`.

## Минимальные instances

```csharp
public sealed class EntityInstance
{
    public EntityInstance(
        Guid instanceId,
        EntityDefinition definition)
    {
        if (instanceId == Guid.Empty)
            throw new ArgumentException(
                "Instance id cannot be empty.",
                nameof(instanceId));

        InstanceId = instanceId;
        Definition = definition
            ?? throw new ArgumentNullException(nameof(definition));
    }

    public Guid InstanceId { get; }

    public EntityDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : EntityFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

Аналогично:

```csharp
public sealed class PickupInstance
{
    public Guid InstanceId { get; }
    public PickupDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : PickupFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

```csharp
public sealed class ItemInstance
{
    public Guid InstanceId { get; }
    public ItemDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : ItemFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

## Чего в instance по умолчанию быть не должно

В `EntityInstance` и `PickupInstance` я бы не добавлял:

```text
GameObject
Transform
LifetimeScope
Inventory
Health
Navigation
CommandReceiver
список capabilities
словарь произвольного runtime-state
```

`GameObject` хранится техническим spawn registry. Inventory, здоровье, диалог и остальные состояния принадлежат соответствующим runtime-компонентам и сервисам.

Иначе instance постепенно снова станет god-object.

## Аналогия с Lyra

`ULyraInventoryItemInstance` хранит ссылку на item definition и собственное изменяемое состояние в форме `StatTags`. Поиск fragment он делегирует своей definition. При этом stack count находится не в item instance, а в отдельной inventory entry. ([GitHub][1])

При создании inventory manager:

1. создаёт item instance;
2. устанавливает ему definition;
3. вызывает `OnInstanceCreated` у fragments;
4. устанавливает количество в inventory entry. ([GitHub][2])

Например, `SetStats` fragment копирует начальные значения из authoring definition в mutable stat state item instance. ([GitHub][3])

Твой аналог:

```text
Definition
    immutable authoring fragments

Instance
    identity + definition
    собственное mutable state только при необходимости

Owner/component
    остальное runtime-state
```

## Особенность `ItemInstance`

У `ItemInstance` чаще появится собственное mutable state:

```text
durability
charges
random modifiers
quality
custom name
item stat values
```

Но не нужно добавлять эти поля заранее.

Начни с:

```csharp
public sealed class ItemInstance
{
    public Guid InstanceId { get; }
    public ItemDefinition Definition { get; }
}
```

Первое реальное изменяемое состояние добавит либо конкретное поле, либо специализированный контейнер:

```csharp
public sealed class ItemInstance
{
    private readonly Dictionary<StatId, int> _stats;
}
```

Не стоит сразу создавать универсальный `Dictionary<string, object>`.



# Что именно брать из Lyra


От Lyra стоит взять структуру:

```text
definition
    immutable configuration

instance
    ссылка на definition
    изменяемые данные конкретного экземпляра

container entry
    instance + count

manager/factory
    создаёт instance
    применяет initial fragment data
```


## Финальная рекомендация

```text
EntityInstance:
    Guid InstanceId
    EntityDefinition Definition

PickupInstance:
    Guid InstanceId
    PickupDefinition Definition

ItemInstance:
    Guid InstanceId
    ItemDefinition Definition
    только реально необходимое mutable item-state
```

То есть instance остаётся минимальной runtime-идентичностью, а не одновременно объектом мира, registry entry, контейнером компонентов.

```text
Fragment
    immutable authoring-данные capability внутри Definition

Endpoint
    prefab-компонент
    получает Instance
    достаёт нужный Fragment
    создаёт runtime capability
    реализует IRegistryBindingSource<T>

RegistryBindingSource<T>
    предоставляет готовую пару:
    InstanceId + runtime value

RegistryBinding<T>
    VContainer lifecycle-объект
    на Initialize добавляет value в Registry<T>
    на Dispose удаляет его
```

На примере inventory:

```text
InventoryFragment
    capacity + initial items

InventoryOwner
    EntityInstance
    → TryGet<InventoryFragment>()
    → создаёт Inventory
    → IRegistryBindingSource<IInventory>

RegistryBinding<IInventory>
    InventoryOwner.InstanceId + Inventory
    → Registry<IInventory>
```

Итоговое разделение:

```text
Definition/Fragment
    authoring

Endpoint
    authoring → runtime

RegistryBinding
    runtime → global lookup
```

`Registry<T>` при этом остаётся простым хранилищем `InstanceId → T` и ничего не знает ни о prefab, ни о fragments, ни о VContainer.
