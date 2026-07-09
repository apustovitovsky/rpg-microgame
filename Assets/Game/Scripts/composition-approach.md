Да, в целом ты правильно понял: **не нужен специальный тип prefab-а “pickup/chest/corpse” как наследование/класс объекта**. Нужен prefab/GameObject, на котором собран нужный набор **ролей**:

```text
Targetable + Interactable + InventoryOwner/LootSource
```

И да: **от набора реализованных ролей зависит, в какие сервисы/реестры объект будет опубликован**. Но лучше, чтобы это происходило не “магически само”, а через явный registrar/factory/bootstrap.

## Что сейчас в репо

Сейчас обычный pickup у тебя уже устроен как набор ролей:

```text
WorldPickup       = runtime-сущность pickup-а
PickupTarget      = ITargetable
PickupInteractable = IInteractable
WorldPickupService = collect/use-case сервис
```

`WorldPickup` сейчас чистая runtime-сущность: хранит `WorldInfo`, `WorldId`, `PickupDefinition`, `IsCollectable`, и умеет `SetCollectedAsync`. Это хорошее направление. ([GitHub][1])

`PickupTarget` реализует `ITargetable` и отдает `WorldId`, `UiAnchor`, `TargetPoint`, `IsTargetable`. То есть targetability уже отдельная роль, а не “тип pickup-а”. ([GitHub][2])

`PickupInteractable` реализует `IInteractable`, имеет `InteractionPosition`, `MaxRange`, а при взаимодействии вызывает:

```csharp
_pickupService.CollectAsync(context.InteractorWorldId, _pickup.WorldId, token)
```

То есть interaction уже является адаптером: “если на этот объект нажали interact — выполнить pickup collect”. ([GitHub][3])

`IInteractable` сейчас тоже уже в правильной форме: он содержит interaction-позицию, range, `CanInteract` и `InteractAsync`. ([GitHub][4])

## Для сундука и трупа не нужен `IWorldPickup`

Обычный pickup:

```text
лежит в мире
поднял
применил эффекты
despawn
```

Сундук/труп:

```text
стоит/лежит в мире
по interact открывает loot/inventory
может остаться после лута
может быть заперт
может иметь UI
может не исчезать
```

Поэтому сундук и труп лучше не делать `IWorldPickup`.

Для них я бы сделал роль:

```csharp
public interface ILootSource
{
    WorldId WorldId { get; }
    IInventory Inventory { get; }

    bool CanLoot(WorldId looterId);
    UniTask OnLootedAsync(WorldId looterId, CancellationToken token);
}
```

Или для MVP можно проще:

```csharp
public interface IInventoryOwner
{
    WorldId WorldId { get; }
    IInventory Inventory { get; }
}
```

Тогда сундук:

```text
ChestTarget : ITargetable
ChestInteractable : IInteractable
ChestInventoryOwner : IInventoryOwner / ILootSource
```

Труп:

```text
CorpseTarget : ITargetable
CorpseInteractable : IInteractable
CorpseLootSource : ILootSource
```

А если труп “находится на акторе”, это нормально. Actor может после смерти начать публиковать наружу `ILootSource`, либо иметь компонент `CorpseLootInteractable`, который активен только когда actor dead.

## Как это должно попадать в менеджеры

Не так:

```text
Unity видит компонент IInventoryOwner и сам магически регистрирует его
```

Лучше так:

```text
Spawner/Registrar создал объект
нашел нужные роли
зарегистрировал их в соответствующие сервисы
добавил tokens в WorldLifetime
```

Например для chest:

```csharp
lifetime.Add(_interaction.RegisterInteractable(chestId, chestInteractable));
lifetime.Add(_lootSources.Register(chestId, lootSource));
lifetime.Add(_displayables.Register(chestId, displayable));
```

Для pickup:

```csharp
lifetime.Add(_pickups.Register(pickup));
lifetime.Add(_interaction.RegisterInteractable(pickupId, pickupInteractable));
```

Для actor corpse:

```csharp
lifetime.Add(_actors.Register(actorId, actor));
lifetime.Add(_interaction.RegisterInteractable(actorId, actorCorpseInteractable));
lifetime.Add(_lootSources.Register(actorId, actorLootSource));
```

`InteractionService` у тебя уже примерно так устроен: он имеет внутренний индекс interactables и регистрирует `IInteractable` по `WorldId`, а потом по `TargetWorldId` находит нужный interactable и вызывает его. ([GitHub][5])

## Что важно: `IInventoryOwner` не должен означать “это pickup”

`IInventoryOwner` — это не “предмет для подбора”.

Это просто:

```text
у объекта есть inventory
```

Его могут иметь:

```text
player
npc
chest
corpse
merchant
party stash
container
```

А вот “можно ли это лутать” — это уже лучше отдельная роль:

```text
ILootSource
```

Поэтому я бы не делал так:

```text
любой IInventoryOwner автоматически lootable
```

Лучше:

```text
IInventoryOwner = владеет inventory
ILootSource = его можно лутать
IInteractable = с ним можно взаимодействовать
```

Один объект может иметь все три, но это разные смыслы.

## Что с pickup effects

Сейчас `WorldPickupService` всё еще берет `IPickupEffectHandlerProvider` по `collectorId`, потом для каждого `PickupEffect` ищет handler внутри этого provider-а. То есть collector/actor всё еще выступает носителем handler provider-а. ([GitHub][6])

Вот это я бы убрал.

Правильнее:

```text
PickupEffectHandlerProvider / PickupEffectApplier
  = глобальный application service

Actor
  = публикует IInventoryOwner / IAttributeOwner / ICurrencyWallet

Handler
  = сам находит нужную роль по collectorId
```

То есть не actor говорит:

```text
у меня есть обработчики pickup effects
```

а система говорит:

```text
у collectorId есть inventory?
есть attributes?
есть wallet?
тогда применяю соответствующий effect
```

Пример:

```csharp
public sealed class AddItemPickupEffectHandler
    : PickupEffectHandler<AddItemPickupEffect>
{
    private readonly IWorldRegistry<IInventoryOwner> _inventories;

    protected override bool CanApply(
        AddItemPickupEffect effect,
        WorldId collectorId,
        IWorldPickup pickup)
    {
        return _inventories.TryGet(collectorId, out var owner)
            && owner.Inventory.CanAdd(effect.Item, effect.Amount);
    }

    protected override UniTask ApplyAsync(
        AddItemPickupEffect effect,
        WorldId collectorId,
        IWorldPickup pickup,
        CancellationToken token)
    {
        var owner = _inventories.Get(collectorId);
        owner.Inventory.Add(effect.Item, effect.Amount);
        return UniTask.CompletedTask;
    }
}
```

Для restore hp/mp handler будет искать не inventory, а:

```text
IAttributeOwner
```

## Как я бы развел типы объектов

### 1. Обычный pickup

```text
IWorldPickup
ITargetable
IInteractable
IDisplayable
```

Interact:

```text
PickupInteractable -> IPickupService.CollectAsync(...)
```

После collect:

```text
apply effects
SetCollectedAsync
Despawn
```

### 2. Сундук

```text
ITargetable
IInteractable
ILootSource
IDisplayable
```

Interact:

```text
ChestInteractable -> LootService.OpenLootAsync(looterId, chestId)
```

Сундук не despawn-ится автоматически.

### 3. Труп

Если труп — это отдельный объект:

```text
CorpseTarget
CorpseInteractable
CorpseLootSource
```

Если труп — это состояние actor-а:

```text
WorldActor
ActorTarget
ActorCorpseInteractable
ActorLootSource
```

Но `ActorLootSource` активен только когда actor dead.

## Мой совет по именам

Я бы не называл всё `Pickup`, если оно не подбирается мгновенно.

Раздели:

```text
Pickup = мгновенно собрать объект из мира
Loot = забрать содержимое из контейнера/трупа
Inventory = место хранения предметов
Effect = что происходит при pickup/use
```

Тогда архитектура станет заметно чище:

```text
IPickupService
  collect world pickup

ILootService
  open/transfer loot from source to receiver

IInventoryService
  add/remove/move items

IPickupEffectApplier
  apply pickup definition effects
```

## Ответ на твой вопрос коротко

Да, prefab должен быть просто набором правильных компонентов:

```text
обычный pickup:
  ITargetable + IInteractable + IWorldPickup

сундук:
  ITargetable + IInteractable + ILootSource/IInventoryOwner

труп:
  ITargetable + IInteractable + ILootSource
```

И да, **то, что объект реализует/публикует, определяет, в какие registry/service-index он попадает**. Но лучше делать это явно через registrar/spawner, а не через скрытую магию. Самое важное: `IWorldPickup` оставить только для “поднял и исчез”, а сундуки/трупы вести через `ILootSource`/`IInventoryOwner`.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Pickup/Components/WorldPickup.cs "rpg-microgame/Assets/Game/Scripts/Pickup/Components/WorldPickup.cs at main · apustovitovsky/rpg-microgame · GitHub"
[2]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Pickup/Components/PickupTarget.cs "rpg-microgame/Assets/Game/Scripts/Pickup/Components/PickupTarget.cs at main · apustovitovsky/rpg-microgame · GitHub"
[3]: https://github.com/apustovitovsky/rpg-microgame/raw/refs/heads/main/Assets/Game/Scripts/Pickup/Components/PickupInteractable.cs "raw.githubusercontent.com"
[4]: https://github.com/apustovitovsky/rpg-microgame/raw/refs/heads/main/Assets/Game/Scripts/Interaction/Runtime/IInteractable.cs "raw.githubusercontent.com"
[5]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Interaction/Runtime/InteractionService.cs "rpg-microgame/Assets/Game/Scripts/Interaction/Runtime/InteractionService.cs at main · apustovitovsky/rpg-microgame · GitHub"
[6]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Pickup/Runtime/WorldPickupService.cs "rpg-microgame/Assets/Game/Scripts/Pickup/Runtime/WorldPickupService.cs at main · apustovitovsky/rpg-microgame · GitHub"
