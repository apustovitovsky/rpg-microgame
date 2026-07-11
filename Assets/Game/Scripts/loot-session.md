Здесь лучше разделить **команду открытия сессии** и **операции внутри уже открытой сессии**.

```text
InteractCommand → сундук → открыть LootSession
TakeLootCommand → LootSession → перенести конкретный stack
```

Но `TakeLootCommand` уже не обязательно отправлять сундуку через `InstanceId → ICommandReceiver`.

## Рекомендуемая цепочка

```text
Player
  → Send(chestId, InteractCommand)
  → ChestCommandReceiver
  → ChestInteractable
  → LootSessionService.Open(chestId, playerId)
  → создаётся LootSessionId
  → UI получает snapshot содержимого
```

После этого UI работает с **сессией**, а не напрямую с сундуком:

```text
UI click
  → TakeLootCommand(sessionId, entryId, amount)
  → LootSessionCommandHandler
  → найти активную LootSession
  → проверить участника и состояние source
  → InventoryTransferService
  → обновить session snapshot/event
```

## Почему не слать каждое взятие снова сундуку

Потому что после открытия важен уже контекст сессии:

```text
кто лутает
какой источник открыт
валидна ли сессия
не отошёл ли игрок
не уничтожен ли сундук
не закрыт ли UI
какие inventory участвуют
```

Если каждый раз делать:

```text
Send(chestId, TakeLootCommand)
```

сундук начнёт отвечать и за UI-session, и за destination inventory, и за transfer. Это лишняя ответственность.

## Минимальные типы

```csharp
public readonly record struct LootSessionId(Guid Value);

public readonly record struct TakeLootCommand(
    LootSessionId SessionId,
    InventoryEntryId EntryId,
    int Amount) : ICommand;
```

```csharp
public sealed class TakeLootCommandHandler
    : ICommandHandler<TakeLootCommand>
{
    private readonly ILootSessionService _sessions;

    public UniTask<CommandResult> HandleAsync(
        TakeLootCommand command,
        CancellationToken token)
    {
        return _sessions.TakeAsync(
            command.SessionId,
            command.EntryId,
            command.Amount,
            token);
    }
}
```

`LootSession` хранит только ссылки/ID участников:

```csharp
public sealed class LootSession
{
    public LootSessionId Id { get; }
    public Guid LooterId { get; }
    public Guid SourceId { get; }
    public IInventory Source { get; }
    public IInventory Destination { get; }
}
```

## Где здесь command system

Есть два вида маршрутизации:

```text
World command:
target InstanceId → ICommandReceiver

Application command:
command type → global handler
```

Для твоего кейса:

```text
InteractCommand
    world command, адресован сундуку

TakeLootCommand
    application/session command, адресован LootSessionId
```

Не нужно насильно прогонять всё через один тип адресации.

## MVP без лишней сложности

Можно вообще не делать `TakeLootCommand` через общий bus:

```csharp
await _lootSessionService.TakeAsync(
    sessionId,
    entryId,
    amount,
    token);
```

А command добавить позже, если понадобится:

```text
логирование
replay
networking
единый action pipeline
GAS/cost/cooldown
```

Для MVP я бы сделал так:

```text
InteractCommand
  → открывает LootSession

Loot UI
  → напрямую вызывает ILootSessionService.TakeAsync(...)
```

Best practice: command адресует **use case**, а session object/service владеет последовательностью операций после начала use case. Не стоит превращать каждый метод сессии в world command только ради единообразия.
**Отдельный универсальный `SessionSystem` сейчас делать не стоит.** Общая часть у `DialogueSession` и `LootSession` пока слишком мала, а правила сильно различаются.

Твоя `CommandSystem` оправданно универсальна: `CommandDispatcher` всегда делает одно и то же — `InstanceId → ICommandReceiver`, а receiver выбирает handler по типу команды и контролирует одно активное выполнение. ([GitHub][1])

У сессий общего значительно меньше:

| Dialogue session        | Loot session                               |
| ----------------------- | ------------------------------------------ |
| обычно одна глобальная  | возможно по одной на игрока                |
| участники и Yarn runner | source/destination inventory               |
| выбор реплик            | многократные transfer-операции             |
| блокировка управления   | UI может не блокировать весь actor         |
| заканчивается диалогом  | заканчивается закрытием/дистанцией/despawn |

## Рекомендуемая цепочка

```text
InteractCommand → Chest receiver
    → LootSessionService.Open(...)
    → вернуть LootSessionId
    → команда завершена

UI → LootSessionService.Take(sessionId, entryId, amount)
UI → LootSessionService.Close(sessionId)
```

Для диалога:

```text
InteractCommand → NPC receiver
    → DialogueSessionService.Start(...)
    → DialogueSession

UI/Yarn → Choose / Continue
    → DialogueSessionService

Dialogue finished
    → Close session
```

Важно: **не держать `WorldCommandReceiver._isExecuting = true` на всё время открытого UI**. Command запускает сессию и заканчивается. Конкурентность и эксклюзивность дальше контролирует сама domain-session. Иначе `Busy` receiver станет случайной политикой для dialogue, loot, trade и других совершенно разных процессов.

## Что можно переиспользовать

Не `SessionSystem`, а маленький технический lifecycle:

```csharp
public sealed class SessionLifetime : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public Guid Id { get; } = Guid.NewGuid();

    public CancellationToken Token => _cts.Token;

    public bool IsClosed { get; private set; }

    public void Close()
    {
        if (IsClosed)
            return;

        IsClosed = true;
        _cts.Cancel();
    }

    public void Dispose()
    {
        Close();
        _cts.Dispose();
    }
}
```

Композиция:

```csharp
public sealed class LootSession
{
    public SessionLifetime Lifetime { get; }

    public Guid LooterId { get; }
    public Guid SourceId { get; }

    public IInventory Source { get; }
    public IInventory Destination { get; }
}
```

```csharp
public sealed class DialogueSession
{
    public SessionLifetime Lifetime { get; }

    public Guid InitiatorId { get; }
    public Guid SpeakerId { get; }

    // Dialogue-specific state.
}
```

Общие только:

```text
Id
CancellationToken
Close/Dispose
IsClosed
```

Связывать session cancellation можно с закрытием UI, despawn источника и внешней отменой через linked cancellation token. Это штатный механизм .NET для объединения нескольких причин отмены. ([Microsoft Learn][2])

## Сервисы оставить отдельными

```csharp
ILootSessionService
    Open
    Take
    TakeAll
    Close

IDialogueSessionService
    Start
    Choose
    Continue
    Close
```

Каждая сессия является своей consistency boundary и должна сама защищать собственные инварианты; объединять такие модели только из-за общего жизненного цикла обычно слишком рано. ([Microsoft Learn][3])

В текущем репозитории dialogue endpoint пока фактически заглушка, а `InventoryService` содержит базовые операции inventory, но ещё нет двух сформировавшихся session-реализаций, из которых можно достоверно извлечь общую систему. ([GitHub][4])

### Итог

```text
Сейчас:
    LootSessionService
    DialogueSessionService
    общий SessionLifetime

Позже, после 3–4 реальных сессий:
    возможно SessionRegistry<TSession>
    возможно общий modal-session coordinator

Не делать сейчас:
    ISession с object Payload
    единый SessionCommandDispatcher
    один Dictionary<Guid, ISession>
    switch по типам сессий
```

Наиболее вероятная полезная общая система в будущем — не `SessionSystem`, а **PlayerModalActivityCoordinator**, который гарантирует, что игрок одновременно находится только в одном модальном процессе: dialogue, loot, trade или crafting. Сами сессии при этом остаются типизированными.

Да, взаимоисключение нужно. Но **`CommandReceiver.Busy` — не то место, которое надёжно запрещает открыть лут во время диалога**.

Причина: команды адресуются разным объектам.

```text
Dialogue:
Send(npcId, InteractCommand)
    → busy становится receiver NPC

Loot:
Send(chestId, InteractCommand)
    → receiver сундука свободен
```

То есть занятый NPC никак не блокирует сундук.

## Правильное разделение

Есть два разных ограничения:

```text
Receiver Busy
    объект сейчас обрабатывает команду

Player Modal Activity
    игрок сейчас находится в Dialogue / Loot / Trade
```

Взаимоисключение диалога и лута должно принадлежать игроку или его session coordinator:

```csharp
public enum PlayerActivity
{
    Dialogue,
    Loot,
    Trade
}

public interface IPlayerActivityGate
{
    bool TryEnter(
        Guid playerId,
        PlayerActivity activity,
        out IDisposable lease);
}
```

Цепочка диалога:

```text
NPC получает InteractCommand
    → TryEnter(playerId, Dialogue)
    → DialogueSession получает lease
    → command handler завершается
    → диалог продолжается как session
    → session.Close()
    → lease.Dispose()
```

Лут:

```text
Chest получает InteractCommand
    → TryEnter(playerId, Loot)
    → отказ, потому что Dialogue уже активен
```

## Почему handler лучше завершить после открытия

Потому что команда означает:

```text
«попробовать начать диалог»
```

а не:

```text
«существовать столько же, сколько открыт весь диалог»
```

Долгим жизненным циклом владеет `DialogueSession`:

```csharp
public sealed class DialogueSession : IDisposable
{
    private readonly IDisposable _activityLease;

    public DialogueSession(IDisposable activityLease)
    {
        _activityLease = activityLease;
    }

    public void Dispose()
    {
        _activityLease.Dispose();
    }
}
```

При этом NPC может отдельно заблокировать свои локальные действия на время диалога — например движение и бой. Но это уже actor-local gate или scheduler.

Итого:

```text
CommandReceiver.Busy
    короткая защита выполнения команды конкретным target

PlayerActivityGate
    запрещает Dialogue + Loot одновременно

DialogueSession / LootSession
    удерживает блокировку до закрытия
```

Для GAS роль `PlayerActivityGate` естественно выполняют теги вроде `State.Dialogue` и `State.Looting`, взаимно блокирующие активацию способностей.

Да, эта версия плана выглядит последовательно и подходит для MVP.

Lyra действительно подтверждает два ключевых решения: inventory entry хранит конкретный item instance вместе с количеством, а interaction запускает специализированный gameplay-сценарий. При этом Lyra широко использует GAS и Gameplay Tags для блокировки и отмены конфликтующих действий. ([Epic Games Developers][1])

## Зафиксировал бы именно так

```text
InteractCommand
    → LootInteractable
    → LootSessionService.Open(looterId, sourceId)

Loot UI
    → Take(sessionId, entryId, amount)
    → InventoryTransferService
    → LootSessionSnapshot

Loot UI
    → Close(sessionId)
```

### Границы ответственности

* `LootInteractable` только открывает сценарий.
* `LootSessionService` проверяет валидность сессии и её участников.
* `InventoryTransferService` обеспечивает атомарный перенос.
* `IInventory` отвечает за локальные операции `extract/insert`.
* UI получает snapshot и не меняет inventory напрямую.

## Одна важная оговорка

Двух индексов достаточно для правила:

> Один looter — одна активная loot-сессия.

```text
sessionId → LootSession
looterId  → sessionId
```

Если позднее один сундук нельзя будет одновременно лутать нескольким персонажам, понадобится третий индекс:

```text
sourceId → sessionId
```

Для однопользовательского MVP он не нужен.

## Ссылки по ID

Хранить в сессии только:

```csharp
public readonly struct LootSession
{
    public LootSessionId Id { get; }
    public Guid LooterInstanceId { get; }
    public Guid SourceInstanceId { get; }
}
```

— разумно. При каждом `Take` сервис повторно получает оба inventory. Если источник исчез:

```text
lookup source failed
    → Close(sessionId)
    → вернуть SourceUnavailable
```

Так сессия не продолжает жить после despawn.

## Перенос instance

Правильная модель:

```text
InventoryEntryId
    идентифицирует стек

ItemInstance
    является переносимым runtime-объектом

InventoryTransferPayload
    ItemInstance + Amount
```

То есть:

```text
source.TryExtract(entryId, amount)
    → payload

destination.TryInsert(payload)
```

Не:

```text
source.Remove(...)
destination.Add(itemDefinition, amount)
```

Иначе runtime-состояние `ItemInstance` потеряется.

## GAS и состояния

`PlayerActivityGate` сейчас действительно преждевременен. Когда реальные dialogue/loot/trade начнут конкурировать, открытие сессий можно оформить как способности:

```text
LootAbility
    requires absence: State.Dialogue, State.Trade
    grants: State.Loot

DialogueAbility
    requires absence: State.Loot, State.Trade
    grants: State.Dialogue
```

Lyra использует ability/gameplay tags для классификации, блокировки и отмены действий, поэтому такой путь соответствует её общей архитектуре. Но сами `LootSessionService` и `DialogueSessionService` всё равно останутся отдельными владельцами своих доменных процессов. ([Epic Games Developers][2])

## Итог

План можно брать в реализацию. Я бы лишь явно добавил два правила:

```text
потеря source/looter → автоматическое закрытие сессии
TransferPayload переносит существующий ItemInstance
```

А `SessionLifetime`, общий session registry и универсальные activity-gates пока не вводить.

[1]: https://dev.epicgames.com/documentation/unreal-engine/lyra-inventory-and-equipment-in-unreal-engine?utm_source=chatgpt.com "Lyra Inventory and Equipment in Unreal Engine"
[2]: https://dev.epicgames.com/documentation/unreal-engine/abilities-in-lyra-in-unreal-engine?utm_source=chatgpt.com "Abilities in Lyra in Unreal Engine"

Это **не обязательно плохое дублирование**, потому что роли разные:

```text
InventoryEntry
    запись, принадлежащая конкретному inventory

TransferPayload
    временно извлечённое содержимое, не принадлежащее ни одному inventory
```

Но именно твоя реализация для MVP немного переусложнена.

## Что в ней лишнее

`IsConsumed/Consume()` пытаются имитировать линейное владение, которого C# всё равно строго не гарантирует. Если payload создаётся и используется только внутри `InventoryTransferService`, сервис и так контролирует его жизненный цикл:

```csharp
if (!source.TryExtract(entryId, amount, out var extracted))
    return TransferResult.SourceFailed;

if (destination.TryInsert(extracted))
    return TransferResult.Success;

source.TryInsert(extracted); // rollback
```

Поэтому достаточно immutable value object:

```csharp
public readonly struct InventoryStack
{
    internal InventoryStack(
        ItemInstance instance,
        int count)
    {
        Instance = instance
            ?? throw new ArgumentNullException(nameof(instance));

        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        Instance = instance;
        Count = count;
    }

    public ItemInstance Instance { get; }

    public int Count { get; }
}
```

Я бы назвал его скорее:

```text
ExtractedInventoryStack
InventoryStack
InventoryTransferStack
```

`Payload` слишком инфраструктурное название.

## Главная проблема не в дублировании

Нужно определить поведение при **частичном переносе**.

Допустим:

```text
InventoryEntry:
    ItemInstance A
    Count = 10

переносим 3
```

Нельзя получить:

```text
source:
    ItemInstance A × 7

destination:
    ItemInstance A × 3
```

если один и тот же mutable `ItemInstance A` теперь находится сразу в двух inventory.

Правила должны быть такими:

* перенос всего stack — можно передать исходный `ItemInstance`;
* разделение stack — нужно создать новый `ItemInstance` через явную операцию `Split/Clone`, сохранив необходимое runtime-состояние;
* объединение stack — destination решает, можно ли объединить instances.

Например:

```csharp
public bool TryExtract(
    InventoryEntryId entryId,
    int count,
    out InventoryStack extracted)
{
    var entry = Find(entryId);

    if (count == entry.Count)
    {
        RemoveEntry(entry);
        extracted = new InventoryStack(
            entry.Instance,
            count);

        return true;
    }

    var splitInstance =
        entry.Instance.CreateSplitInstance();

    entry.Decrease(count);

    extracted = new InventoryStack(
        splitInstance,
        count);

    return true;
}
```

Если у `ItemInstance` пока вообще нет изменяемого состояния, можно временно создавать:

```csharp
new ItemInstance(entry.Instance.Definition)
```

Но лучше зафиксировать это как сознательное правило split, а не случайно размножать один instance.

## Рекомендация

Оставь отдельный извлечённый объект, потому что он хорошо выражает переход владения:

```text
InventoryEntry
    → TryExtract
    → InventoryStack
    → TryInsert
```

Но:

* убери `IsConsumed`;
* сделай его `readonly struct`;
* не проверяй `MaxStackSize` внутри него — это правило inventory/stacking, а не транспортного контейнера;
* явно реализуй split полного и частичного stack.

Итог: **отдельный transfer object полезен, но он должен быть простым значением, а не stateful mini-transaction.**
**Как частный случай переноса — частично да. Как `LootSession` — нет.**

Сейчас pickup у тебя не имеет source inventory: `PickupDefinition` хранит `ItemDefinition + Amount`, а `ItemPickupCollectable` напрямую создаёт/добавляет этот предмет в инвентарь коллектора. `ItemPickupService` лишь защищает от повторного сбора и despawn’ит pickup после успеха. ([GitHub][1])

## Почему сессия не нужна

Сундук:

```text
открыть
посмотреть несколько записей
несколько раз Take
закрыть
```

Pickup:

```text
попытаться забрать всё
при успехе despawn
```

У pickup нет продолжительного состояния взаимодействия, UI и серии операций. Искусственная `LootSession` на один предмет только усложнит цепочку.

## Что можно переиспользовать

Общий низкоуровневый шаг **вставки payload в inventory**:

```text
Loot:
source.TryExtract(entryId, count)
    → InventoryStack
    → destination.TryInsert(stack)

Pickup:
создать InventoryStack
    → destination.TryInsert(stack)
    → despawn pickup
```

Но `InventoryTransferService` с сигнатурой:

```csharp
TryTransfer(
    IInventory source,
    IInventory destination,
    Guid sourceEntryId,
    int count);
```

pickup использовать не сможет — у него нет `source` и `sourceEntryId`.

Поэтому лучше выделить примитив:

```csharp
public interface IInventory
{
    bool TryInsert(
        InventoryStack stack,
        out int insertedCount);
}
```

А transfer service использует его после extraction.

## Два возможных значения pickup

### 1. Pickup — награда-шаблон

Это твоя текущая модель:

```text
PickupDefinition:
    ItemDefinition
    Amount
```

При сборе создаётся новый `ItemInstance`.

```text
ItemPickupCollectable
    → создать InventoryStack
    → destination.TryInsert(...)
    → despawn
```

Это **grant**, а не transfer.

### 2. Pickup несёт конкретный runtime item

Нужно для выброшенного меча с прочностью или уникальными параметрами:

```text
PickupInstance
    ItemInstance
    Count
```

Тогда сбор действительно переносит существующий runtime payload:

```text
PickupInstance.TakePayload()
    → destination.TryInsert(payload)
    → despawn
```

Но даже здесь `LootSession` не нужна. Это **одношаговый ownership transfer**, а не сессия.

## Рекомендуемая структура

```text
InteractCommand
    → ItemPickupInteractable
    → ItemPickupService.CollectAsync
        → ICollectable.Create/ExtractPayload
        → destination.TryInsert(payload)
        → SpawnedObjectRegistry.Despawn(pickupId)
```

Для сундука:

```text
InteractCommand
    → LootSessionService.Open

Take
    → InventoryTransferService
        → source.TryExtract
        → destination.TryInsert
```

Итог: **pickup можно считать одноразовым loot source концептуально, но не стоит превращать его в inventory и `LootSession`.** Лучше переиспользовать `InventoryStack`/payload и операцию `TryInsert`, оставив отдельный `ItemPickupService`.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Pickup/Components/ItemPickupCollectable.cs "rpg-microgame/Assets/Game/Scripts/Pickup/Components/ItemPickupCollectable.cs at main · apustovitovsky/rpg-microgame · GitHub"
