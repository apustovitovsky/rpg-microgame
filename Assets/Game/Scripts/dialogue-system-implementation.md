Главная причина разрастания диалога — не сам Yarn и не coordinator. Твой `CommandSystem` сейчас имеет два ограничения:

1. Любая команда может вернуть только плоский `CommandResult`.
2. `CommandReceiver` блокирует **все** команды одним `_isExecuting`.

Из-за первого ограничения `EnterDialogueCommand` не может вернуть созданный lease, поэтому пришлось добавить `DialogueParticipantSessionStore`, `ExitDialogueCommand`, `ExitDialogueCommandHandler` и отдельный `DialogueParticipantLease`, который повторно отправляет команды участникам. Из-за второго `DialogueCoordinator` запускает работу через `.Forget()` и ждёт `UniTask.Yield`, что выглядит как обход receiver-wide `Busy` перед повторной отправкой команды тому же NPC. ([GitHub][1])

## 1. Добавить типизированные команды

```csharp
public interface ICommand
{
}

public interface ICommand<TResult>
{
}
```

Handlers:

```csharp
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    UniTask HandleAsync(
        TCommand command,
        CommandContext context);
}

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    UniTask<TResult> HandleAsync(
        TCommand command,
        CommandContext context);
}
```

`CommandContext` убирает постоянно повторяющиеся аргументы:

```csharp
public readonly struct CommandContext
{
    public CommandContext(
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        ReceiverId = receiverId;
        CancellationToken = cancellationToken;
    }

    public Guid ReceiverId { get; }
    public CancellationToken CancellationToken { get; }
}
```

Тогда handler читается проще:

```csharp
public sealed class StartDialogueCommandHandler :
    ICommandHandler<StartDialogueCommand, DialogueStartResult>
{
    private readonly IDialogueCoordinator _coordinator;

    public UniTask<DialogueStartResult> HandleAsync(
        StartDialogueCommand command,
        CommandContext context)
    {
        return _coordinator.StartAsync(
            new DialogueRequest(
                command.InitiatorId,
                command.SpeakerId,
                command.Entry),
            context.CancellationToken);
    }
}
```

Сейчас `StartDialogueCommandHandler` превращает `DialogueStartResult` обратно в общий `CommandResult`, теряя, в частности, `SessionId`. Типизированный результат устраняет этот бессмысленный mapping. ([GitHub][2])

---

## 2. Разделить transport-result и domain-result

Сейчас в одном `CommandResult` смешаны разные уровни:

```text
TargetNotFound / Unsupported / Failed  — доставка
Busy / Rejected                       — gameplay
```

Лучше:

```csharp
public enum CommandDispatchStatus
{
    Delivered,
    TargetNotFound,
    Unsupported,
    Cancelled,
    Failed
}

public readonly struct CommandDispatchResult<TResult>
{
    public CommandDispatchResult(
        CommandDispatchStatus status,
        TResult value = default)
    {
        Status = status;
        Value = value;
    }

    public CommandDispatchStatus Status { get; }
    public TResult Value { get; }
}
```

А `DialogueStartResult` продолжает содержать:

```text
Started(sessionId)
Busy
InvalidRequest
```

Для orchestration-кода стоит добавить удобную обязательную отправку:

```csharp
public interface ICommandDispatch
{
    UniTask<CommandDispatchResult> SendAsync(
        Guid receiverId,
        ICommand command,
        CancellationToken token);

    UniTask<CommandDispatchResult<TResult>> SendAsync<TResult>(
        Guid receiverId,
        ICommand<TResult> command,
        CancellationToken token);

    UniTask<TResult> SendRequiredAsync<TResult>(
        Guid receiverId,
        ICommand<TResult> command,
        CancellationToken token);
}
```

`SendRequiredAsync` возвращает значение или бросает исключение при `TargetNotFound`, `Unsupported` и `Failed`. Тогда координаторы не будут повсюду вручную писать `EnsureEntered`.

---

## 3. Возвращать lease из команды

Это даст самое большое сокращение кода.

```csharp
public readonly struct EnterDialogueCommand :
    ICommand<IDialogueParticipantLease>
{
    public EnterDialogueCommand(
        Guid sessionId,
        Guid otherParticipantId)
    {
        SessionId = sessionId;
        OtherParticipantId = otherParticipantId;
    }

    public Guid SessionId { get; }
    public Guid OtherParticipantId { get; }
}
```

Handler:

```csharp
public sealed class EnterDialogueCommandHandler :
    ICommandHandler<
        EnterDialogueCommand,
        IDialogueParticipantLease>
{
    private readonly IEnumerable<IDialogueParticipantLifecycle> _lifecycles;

    public async UniTask<IDialogueParticipantLease> HandleAsync(
        EnterDialogueCommand command,
        CommandContext context)
    {
        var leases = new AsyncLeaseGroup();

        try
        {
            var participantContext =
                new DialogueParticipantContext(
                    command.SessionId,
                    command.OtherParticipantId);

            foreach (var lifecycle in _lifecycles)
            {
                leases.Add(
                    await lifecycle.EnterAsync(
                        participantContext,
                        context.CancellationToken));
            }

            return leases;
        }
        catch
        {
            await leases.DisposeAsync();
            throw;
        }
    }
}
```

Coordinator:

```csharp
public async UniTask<IDialogueParticipantLease> EnterAsync(
    DialogueSession session,
    CancellationToken token)
{
    var initiatorLease =
        await _commands.SendRequiredAsync(
            session.InitiatorInstanceId,
            new EnterDialogueCommand(
                session.Id,
                session.SpeakerInstanceId),
            token);

    try
    {
        var speakerLease =
            await _commands.SendRequiredAsync(
                session.SpeakerInstanceId,
                new EnterDialogueCommand(
                    session.Id,
                    session.InitiatorInstanceId),
                token);

        return AsyncLeaseGroup.Combine(
            initiatorLease,
            speakerLease);
    }
    catch
    {
        await initiatorLease.DisposeAsync();
        throw;
    }
}
```

После этого можно удалить:

```text
DialogueParticipantSessionStore
ExitDialogueCommand
ExitDialogueCommandHandler
DialogueParticipantLease,
который отправляет Exit-команды
```

Сейчас эти классы существуют только для того, чтобы сохранить lease внутри receiver scope, а затем отдельной командой найти и освободить его. ([GitHub][3])

`CompositeDialogueParticipantLease` при этом нужно переименовать в общий:

```csharp
AsyncLeaseGroup
```

и вынести в `Game.Core`, потому что он пригодится для:

* control restrictions;
* Behavior pause;
* камеры;
* interaction locks;
* targeting locks;
* dialogue participants.

---

## 4. Убрать единый `_isExecuting`

Сейчас любая выполняющаяся команда делает receiver полностью `Busy`, независимо от типа команды. Это слишком грубая политика: длительный `MoveCommand` способен заблокировать `ExitDialogueCommand`, а `StartDialogueCommand` — повторный вход в receiver. ([GitHub][1])

Вдохновившись VitalRouter, добавь политики выполнения:

```csharp
public enum CommandOrdering
{
    Parallel,
    Drop,
    Sequential,
    Switch
}
```

VitalRouter использует такие политики для декларативного управления конкурирующими async-командами; `Sequential` ставит их в очередь, а filters/interceptors позволяют вынести общее поведение из handlers. ([VitalRouter][4])

Но применять политику нужно не ко всему receiver, а к **каналу**:

```csharp
public readonly struct CommandPolicy
{
    public CommandPolicy(
        string channel,
        CommandOrdering ordering)
    {
        Channel = channel;
        Ordering = ordering;
    }

    public string Channel { get; }
    public CommandOrdering Ordering { get; }
}
```

Примеры:

```text
InteractCommand
    channel: Interaction
    ordering: Drop

EnterDialogueCommand
    channel: Dialogue
    ordering: Sequential

MoveCommand
    channel: Movement
    ordering: Switch

LookAtCommand
    channel: Facing
    ordering: Switch

PlayReactionCommand
    channel: Animation
    ordering: Sequential
```

Регистрация:

```csharp
builder.RegisterCommandHandler<
    EnterDialogueCommand,
    IDialogueParticipantLease,
    EnterDialogueCommandHandler>(
        CommandPolicy.Sequential("Dialogue"));
```

Тогда `CommandReceiver` хранит scheduler на каждый channel вместо одного `_isExecuting`.

На первом этапе достаточно трёх режимов:

```text
Parallel
Drop
Sequential
```

`Switch` стоит добавить при рефакторинге movement/navigation.

После этого из `DialogueCoordinator` должен исчезнуть подозрительный:

```csharp
await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
```

Система сама будет знать, какие команды конфликтуют, а какие могут выполняться одновременно.

---

## 5. Добавить pipeline/middleware

Сейчас `CommandReceiver` одновременно:

* ищет handler;
* проверяет cancellation;
* управляет `Busy`;
* ловит исключения;
* пишет лог;
* преобразует результат. ([GitHub][1])

Оставь receiver только маршрутизатором:

```text
Receive
→ найти route
→ построить CommandContext
→ выполнить pipeline
→ вызвать typed handler
```

Общее поведение вынеси:

```csharp
public interface ICommandMiddleware
{
    UniTask<object> InvokeAsync(
        object command,
        CommandContext context,
        CommandContinuation next);
}
```

Минимальный набор:

```text
CommandConcurrencyMiddleware
CommandExceptionMiddleware
CommandTraceMiddleware
```

Позже:

```text
CommandValidationMiddleware
CommandMetricsMiddleware
```

Это тот же полезный принцип, что и VitalRouter interceptors: обработка до и после handler без дублирования внутри каждого route. ([VitalRouter][5])

---

## 6. Убрать наследование от `CommandHandler<T>`

Текущий `CommandHandler<T>` существует только для runtime cast из `ICommand` и хранения `CommandType`. ([GitHub][6])

Пусть feature-код реализует только чистый интерфейс:

```csharp
public sealed class EnterDialogueCommandHandler :
    ICommandHandler<
        EnterDialogueCommand,
        IDialogueParticipantLease>
{
}
```

А type erasure скрывается внутри инфраструктуры:

```csharp
internal sealed class CommandRoute<TCommand, TResult> :
    ICommandRoute
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _handler;
}
```

VContainer extension:

```csharp
builder.RegisterCommandHandler<
    EnterDialogueCommand,
    IDialogueParticipantLease,
    EnterDialogueCommandHandler>(
        CommandPolicy.Sequential("Dialogue"));
```

Feature-код больше не содержит:

```text
CommandType
override
ручной cast
общий CommandResult
```

---

## 7. Разделить target-команды и use-case команды

Диалог и передача предмета действительно не имеют одного естественного адресата.

Поэтому добавь второй путь:

```csharp
// Команда конкретному world instance.
_commands.SendAsync(
    actorId,
    new EnterDialogueCommand(...),
    token);

// Application/use-case команда.
_commands.ExecuteAsync(
    new StartDialogueCommand(
        initiatorId,
        speakerId,
        entry),
    token);
```

`StartDialogueCommand` должен обрабатываться глобальным handler-ом:

```text
StartDialogueCommand
    ↓
StartDialogueCommandHandler
    ↓
DialogueCoordinator
    ├── EnterDialogueCommand → initiator
    └── EnterDialogueCommand → speaker
```

Передача предмета:

```text
TransferItemCommand(sourceId, destinationId, itemId)
    ↓
TransferItemCommandHandler
    ↓
InventoryTransferService
```

Не нужно публиковать одну команду обоим участникам. **Один handler владеет use case**, а участники являются данными команды.

Это даст понятное правило:

```text
Send(target, command)
    — действие одного world object

Execute(command)
    — игровая операция с любым числом участников
```

---

## Как станет выглядеть диалог

```text
InteractCommand
    ↓
InteractionService
    ↓
Execute StartDialogueCommand(playerId, npcId)
    ↓
DialogueCoordinator
    ├── Send EnterDialogueCommand → player
    │       возвращает player lease
    ├── Send EnterDialogueCommand → NPC
    │       возвращает NPC lease
    ├── Yarn executor
    └── Dispose обоих leases
```

Без:

```text
DialogueParticipantSessionStore
ExitDialogueCommand
ExitDialogueCommandHandler
ручного CommandResult mapping
receiver-wide Busy
UniTask.Yield workaround
```

## Приоритет внедрения

Я бы делал строго так:

1. `ICommand<TResult>` и typed handlers.
2. Возврат participant lease из `EnterDialogueCommand`.
3. Удаление `ExitDialogueCommand` и session store.
4. `CommandContext`.
5. Политики `Parallel/Drop/Sequential` по channel.
6. Middleware для trace и exception.
7. `ExecuteAsync` для multi-participant use cases.
8. Только потом упрощение регистрации handlers.

Именно первые три пункта сильнее всего уменьшат текущую диалоговую систему: вместо пяти инфраструктурных классов останутся команда входа, её handler и coordinator.

Да, твой вариант с `Routes` лучше моего `channel + ordering`. **Routes-класс должен быть единицей маршрутизации и конкурентного выполнения.** Именно так связанные команды получают одну очередь; VitalRouter также применяет ordering на уровне routes-класса, причём `Sequential` упорядочивает даже разные типы команд внутри него. ([VitalRouter][1])

## Ренейминг

Я бы зафиксировал такие имена:

```text
CommandReceiver  → CommandRouter
ICommandReceiver → ICommandRouter

CommandDispatch  → CommandBus
ICommandDispatch → ICommandBus

Middleware       → Interceptor
```

Получается понятное разделение:

```text
CommandBus
    находит нужный instance-router

CommandRouter
    находит routes для типа команды

CommandRoutes
    содержит handlers и владеет scheduler

Interceptor
    выполняется до/после handler
```

Сейчас `CommandDispatch` уже фактически является bus/director: он хранит `Guid → ICommandReceiver`, а `CommandReceiver` маршрутизирует команду по её типу. При этом receiver блокирует вообще все handlers одним `_isExecuting`. ([GitHub][2])

---

## Реестры capabilities можно удалить

Да, можно полностью убрать:

```text
InventoryRegistry
InteractableRegistry
TargetableRegistry
DialogueParticipantRegistry
NavigationRegistry
```

Вместо них у каждого world instance остаётся один локальный `CommandRouter`:

```text
Actor CommandRouter
├── InteractionRoutes
├── InventoryRoutes
├── NavigationRoutes
├── DialogueParticipantRoutes
└── TargetRoutes
```

Capability внедряется напрямую в локальный routes-класс:

```csharp
public sealed class InventoryRoutes :
    ICommandRoutes,
    ICommandHandler<GetInventoryCommand, InventorySnapshot>,
    ICommandHandler<AddItemCommand, AddItemResult>,
    ICommandHandler<RemoveItemCommand, RemoveItemResult>
{
    private readonly IInventory _inventory;

    public InventoryRoutes(IInventory inventory)
    {
        _inventory = inventory;
    }

    public CommandOrdering Ordering =>
        CommandOrdering.Sequential;

    // handlers...
}
```

Снаружи никто не ищет `IInventory`:

```csharp
var result = await _commands.RequestAsync(
    actorId,
    new AddItemCommand(itemId, amount),
    token);
```

### Но один индекс всё равно остаётся

При адресации по `Guid` технически неизбежна таблица:

```text
InstanceId → CommandRouter
```

Но это больше не публичный generic registry и не feature registry. Это **внутренняя routing table самого `CommandBus`**:

```csharp
public sealed class CommandBus : ICommandBus
{
    private readonly Dictionary<Guid, ICommandRouter> _routers = new();
}
```

То есть в проекте остаётся один механизм адресации вместо пяти-десяти реестров. Текущий `CommandDispatch` уже содержит именно такую таблицу; нужно лишь перестать реализовывать через него `IRegistryWriter<ICommandReceiver>` и скрыть регистрацию внутри command infrastructure. ([GitHub][2])

---

## Commands не заменяют discovery

Это единственное существенное ограничение.

Например, targeting всё равно должен **обнаружить** объект:

```text
Raycast / overlap / collider
    ↓
WorldEndpoint.InstanceId
    ↓
CommandBus.SendAsync(instanceId, command)
```

Тебе не нужен `TargetableRegistry`, но collider должен дать хотя бы `InstanceId`. Команда может проверить, доступен ли объект как цель:

```csharp
var evaluation = await _commands.RequestAsync(
    candidateId,
    new EvaluateTargetCommand(actorId),
    token);
```

То же для interaction:

```text
Raycast получил InstanceId
→ InteractCommand отправлен объекту
→ Unsupported, если объект не интерактивный
```

Команды заменяют **поиск runtime capability**, но не физический поиск объекта в мире.

---

# Твой `ICommandRoutes` — правильная модель

```csharp
public interface ICommandRoutes
{
    CommandOrdering Ordering { get; }
}
```

```csharp
public sealed class DialogueParticipantRoutes :
    ICommandRoutes,
    ICommandHandler<
        EnterDialogueCommand,
        IDialogueParticipantLease>,
    ICommandHandler<InterruptDialogueCommand>
{
    public CommandOrdering Ordering =>
        CommandOrdering.Sequential;

    // ...
}
```

Router строит такую структуру:

```text
CommandRouter
├── EnterDialogueCommand
│       → DialogueParticipantRoutes
├── InterruptDialogueCommand
│       → DialogueParticipantRoutes
└── MoveCommand
        → NavigationRoutes

Schedulers
├── DialogueParticipantRoutes → Sequential
└── NavigationRoutes          → Switch
```

Один экземпляр routes — один scheduler:

```csharp
internal sealed class CommandRouter : ICommandRouter
{
    private readonly Dictionary<Type, ICommandRoute> _routes;
    private readonly Dictionary<ICommandRoutes, ICommandScheduler> _schedulers;
    private readonly IReadOnlyList<ICommandInterceptor> _interceptors;
}
```

Регистрация:

```csharp
builder.RegisterCommandRoutes<DialogueParticipantRoutes>();
builder.RegisterCommandRoutes<NavigationRoutes>();
builder.RegisterCommandRoutes<InventoryRoutes>();
```

`RegisterCommandRoutes<T>()` должен:

1. Зарегистрировать `T` один раз.
2. Найти реализованные `ICommandHandler<TCommand>` и `ICommandHandler<TCommand, TResult>`.
3. Создать type-erased route entries.
4. Создать один scheduler для экземпляра `T`.
5. Передать route entries в `CommandRouter`.

Reflection здесь допустим: она выполняется один раз при построении scope, source generator пока не нужен.

---

## Важное следствие lifetime

Политика принадлежит **экземпляру routes**:

```text
Actor A NavigationRoutes → своя Switch-операция
Actor B NavigationRoutes → своя Switch-операция
```

Поэтому actor routes должны быть `Lifetime.Scoped`.

А глобальные use-case routes:

```csharp
public sealed class DialogueRoutes :
    ICommandRoutes,
    ICommandHandler<StartDialogueCommand, DialogueStartResult>,
    ICommandHandler<StopDialogueCommand>
{
    public CommandOrdering Ordering =>
        CommandOrdering.Drop;
}
```

живут в gameplay scope. Один экземпляр означает одну глобальную модальную dialogue policy.

---

# Два типа отправки

Чтобы убрать реестры и поддерживать операции с несколькими участниками, нужны два входа.

### Команда конкретному instance

```csharp
await _commands.SendAsync(
    actorId,
    new StopMovementCommand(),
    token);
```

```csharp
var lease = await _commands.RequestAsync(
    actorId,
    new EnterDialogueCommand(sessionId, otherActorId),
    token);
```

### Глобальная use-case-команда

```csharp
var result = await _commands.ExecuteAsync(
    new StartDialogueCommand(playerId, npcId, entry),
    token);
```

Она попадает в root/application router:

```text
StartDialogueCommand
    ↓
DialogueRoutes
    ↓
DialogueCoordinator
    ├── EnterDialogueCommand → player router
    └── EnterDialogueCommand → NPC router
```

Аналогично предметы:

```text
TransferItemCommand(sourceId, destinationId, item)
    ↓
InventoryTransferRoutes
    ├── Remove/Reserve → source router
    └── Add           → destination router
```

У `TransferItemCommand` нет одного адресата — это глобальный use case с двумя участниками.

---

## Предлагаемый API

```csharp
public interface ICommandBus
{
    UniTask<CommandResult> SendAsync(
        Guid targetId,
        ICommand command,
        CancellationToken token);

    UniTask<CommandResult<TResult>> RequestAsync<TResult>(
        Guid targetId,
        ICommand<TResult> command,
        CancellationToken token);

    UniTask<CommandResult> ExecuteAsync(
        ICommand command,
        CancellationToken token);

    UniTask<CommandResult<TResult>> ExecuteAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken token);
}
```

`Send/Request` идут в instance router, `Execute` — в root router.

---

## Caveat для общей ordering policy

Class-level policy действительно удобна, но границу routes нужно выбирать по **конкурентному поведению**, а не только по тематике.

Например:

```csharp
NavigationRoutes : Switch
```

хорошо:

```text
MoveCommand
→ новый MoveCommand отменяет старый

StopMovementCommand
→ отменяет MoveCommand и выполняет stop
```

Но если `DialogueRoutes : Sequential` содержит handler, который ждёт весь пяти­минутный диалог, `InterruptDialogueCommand` встанет позади него и не сможет прервать диалог.

Поэтому:

```text
EnterDialogueCommand
    должен быстро вернуть lease

RunDialogueCommand
    принадлежит глобальному coordinator

InterruptDialogueCommand
    либо отдельные routes,
    либо позже route-level override
```

Для первой версии я бы не добавлял override. Разделяй routes-классы, когда команды требуют разных concurrency semantics.

---

## Итоговая схема

```text
CommandBus
├── root CommandRouter
│   ├── DialogueRoutes
│   └── InventoryTransferRoutes
│
└── InstanceId → CommandRouter
    ├── InteractionRoutes
    ├── InventoryRoutes
    ├── NavigationRoutes
    ├── DialogueParticipantRoutes
    └── TargetRoutes
```

Таким образом:

* `Receiver → Router` — да.
* `Middleware → Interceptor` — да.
* `Routes` как владелец handlers и ordering — да, это лучше channel.
* специализированные capability registries — удалить.
* оставить только скрытую таблицу `InstanceId → CommandRouter`.
* targeting/interaction обнаруживают ID через Unity physics, а поведение вызывается командами.
* multi-participant операции обрабатываются root routes и координируют несколько instance routers.
