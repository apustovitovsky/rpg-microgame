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
