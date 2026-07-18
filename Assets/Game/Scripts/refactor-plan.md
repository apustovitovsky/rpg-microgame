## Общая оценка

**Направление правильное.** UB теперь действительно становится владельцем поведения персонажа, а Commands только устанавливают внешний контекст и ждут подтверждения. Проблема не в самой идее, а в том, что сейчас код находится **посередине миграции**: старые названия и формы orchestration остались, а новая модель ответственности ещё не оформлена явно.

Текущая цепочка по факту уже такая:

```text
DialogueCoordinator
    ↓ commands
DialogueParticipantExecution
    ↓ записывает
DialogueParticipation
    ↓ читается
Unity Behavior
    ↓ выполняет
Face → MarkReady → WaitForEnd
    ↓ подтверждает
DialogueParticipantExecution
    ↓ завершает command
DialogueCoordinator запускает Yarn
```

Это нормальная схема handshake между application-слоем и поведением.

---

# Что в текущем коде является новой системой

### `DialogueParticipation`

Это не driver и не lifecycle. Это **actor-scoped runtime state диалога**:

* текущая `DialogueSessionContext`;
* идентификатор подтверждённой сессии;
* `TryEnter`;
* `TryMarkReady`;
* `TryExit`;
* события входа и выхода.

По сути это mailbox между внешним миром и UB. Сам класс уже защищён `SessionId`, не позволяет другой сессии перезаписать активную и делает выход идемпотентным — это хорошая часть реализации. ([GitHub][1])

Я бы переименовал его:

```text
DialogueParticipation
    ↓
ActorDialogueState
```

или:

```text
DialogueParticipationState
```

`Participation` звучит как capability или процесс, хотя фактически это состояние.

### UB condition и actions

`IsDialogueActiveCondition` читает `HasContext`. `FaceDialogueParticipantAction` выполняет поворот, `MarkDialogueReadyAction` подтверждает готовность, `WaitForDialogueEndAction` удерживает ветку активной, пока контекст не очищен. Это нормальный **UB adapter layer**, а не driver. ([GitHub][2])

В репозитории сейчас есть root-граф и три отдельных графа `NpcDialogue`, `NpcIdle`, `NpcPatrol`. Само разбиение соответствует выбранной архитектуре. ([GitHub][3])

### `DialogueSessionCoordinator`

Это тоже не старая система. Он остаётся application-level orchestrator:

```text
ввести обоих участников
→ дождаться готовности
→ провести диалог
→ вывести обоих участников
```

Commands здесь полезны, потому что координатор работает с двумя адресными actor scope и не должен напрямую получать их внутренние сервисы. ([GitHub][4])

---

# Что сейчас выглядит переходным и неудачным

## 1. `DialogueParticipantExecution` смешивает несколько ролей

Сейчас он:

1. валидирует command;
2. создаёт `DialogueSessionContext`;
3. изменяет actor state;
4. ожидает, пока UB пометит сессию готовой;
5. очищает state при cancellation.

То есть он одновременно command adapter и async lifecycle coordinator конкретного актора. Это допустимо, но из названия вообще непонятно, что он делает. ([GitHub][5])

Я бы назвал:

```text
DialogueParticipantExecution
    ↓
DialogueParticipationExecution
```

или, если перейдёшь на термин Routes:

```text
DialogueParticipationRoutes
```

Его правильная роль:

> Принимает внешнее намерение участия в сессии, записывает его в actor state и ожидает подтверждения от UB.

Это не behavior driver.

---

## 2. Ожидание UB реализовано polling-ом

Сейчас используется:

```csharp
await UniTask.WaitUntil(
    () => _participation.IsReadyFor(sessionId),
    cancellationToken: token);
```

То есть command каждую frame проверяет состояние. ([GitHub][5])

Лучше, чтобы само состояние предоставляло awaitable API:

```csharp
public interface IActorDialogueState
{
    bool TryEnter(DialogueSessionContext context);

    UniTask WaitUntilReadyAsync(
        Guid sessionId,
        CancellationToken token);

    bool TryMarkReady(Guid sessionId);
    bool TryExit(Guid sessionId);
}
```

Внутри можно использовать `UniTaskCompletionSource`.

Тогда Execution станет простым:

```csharp
public async UniTask ExecuteAsync(
    EnterDialogueSessionCommand command,
    CommandContext context)
{
    var session = new DialogueSessionContext(
        command.SessionId,
        command.OtherParticipantInstanceId);

    if (!_state.TryEnter(session))
    {
        throw new ActorBusyException();
    }

    try
    {
        await _state.WaitUntilReadyAsync(
            command.SessionId,
            context.CancellationToken);
    }
    catch
    {
        _state.TryExit(command.SessionId);
        throw;
    }
}
```

---

## 3. Для command group больше подходит `Switch`, а не `Sequential`

Сейчас `Enter` ожидает UB, а `Exit` находится в той же `Sequential`-группе. Новая команда вынуждена ждать завершения предыдущей. ([GitHub][5])

Для actor dialogue participation семантика скорее такая:

```text
Enter session A
→ приходит Exit A
→ ожидание Enter отменяется
→ состояние A очищается

Enter session A
→ приходит Enter session B
→ A отменяется
→ запускается B
```

То есть:

```csharp
public CommandExecutionPolicy ExecutionPolicy =>
    CommandExecutionPolicy.Switch;
```

Это ровно тот случай, ради которого ты хотел реализовать `Switch`.

Сам `SessionId` всё равно защищает от ситуации, когда завершение старой операции очистит новую.

---

## 4. Координатор вводит участников последовательно

Сейчас сначала полностью ожидается готовность initiator, и лишь затем command отправляется speaker. Получается:

```text
Игрок вошёл в dialogue branch и повернулся
→ только после этого NPC узнаёт о диалоге
```

([GitHub][4])

Лучше отправлять обе команды параллельно:

```csharp
await UniTask.WhenAll(
    EnterParticipantAsync(
        session.InitiatorInstanceId,
        session.SpeakerInstanceId,
        session.Id,
        token),

    EnterParticipantAsync(
        session.SpeakerInstanceId,
        session.InitiatorInstanceId,
        session.Id,
        token));
```

При ошибке или cancellation — очищать обоих.

---

## 5. Cleanup использует отменённый token

В `ExitAsync` тот же внешний cancellation token используется при очистке обоих участников. Если он уже отменён, cleanup может вообще не дойти до actor state. ([GitHub][4])

Выход из сессии должен выполняться через:

```csharp
CancellationToken.None
```

или через отдельный короткий cleanup token.

```csharp
finally
{
    await ExitBothAsync(
        session,
        CancellationToken.None);
}
```

Остановка диалога важнее отмены ожидающего caller-а.

---

## 6. В command передаётся позиция собеседника

`EnterDialogueSessionCommand` хранит и `OtherParticipantInstanceId`, и `OtherParticipantPosition`. Затем UB поворачивается к сохранённой позиции. ([GitHub][6])

Это слабое место:

* позиция может устареть;
* transport command начинает содержать world snapshot;
* при дальнейшем развитии появится потребность передавать rotation, head position и другие данные.

В command достаточно:

```csharp
SessionId
OtherParticipantActorId
```

А UB action должен получить актуальную позицию через runtime actor registry:

```csharp
if (!_actors.TryGet(
        context.OtherParticipantActorId,
        out var otherActor))
{
    return Status.Failure;
}

_navigation.FacePosition(
    otherActor.DialogueAnchor.position);
```

Это как раз хорошее применение runtime-реестра акторов.

---

## 7. `WaitForDialogueEndAction` одновременно ждёт и чистит navigation

Сейчас action после исчезновения контекста вызывает `ClearFacing()`. ([GitHub][7])

Из-за этого cleanup произойдёт только при нормальном выходе. Если dialogue branch будет прервана combat-веткой, action может завершиться через abort, не пройдя эту строку.

Лучше:

```text
Enter Dialogue
├── Stop movement
├── Face participant
├── Mark ready
└── Wait until context cleared

OnEnd / отдельный cleanup:
└── Clear facing
```

То есть очистка должна быть привязана к завершению или abort всей dialogue-ветки, а не только к успешному завершению wait-node.

---

# Что является настоящим остатком старой системы

Главный legacy сейчас находится уже не в диалоге, а в `NavigationPatrol`.

Он одновременно:

* получает placement из глобального `IActorPlacementService`;
* выбирает patrol stop;
* хранит `_nextStopIndex`;
* хранит `_currentLocationId/_currentAnchorKey`;
* строит и выполняет путь;
* обновляет собственное состояние после прибытия.

([GitHub][8])

То есть UB вызывает один крупный компонент, внутри которого спрятано почти всё поведение Patrol.

Целевая модель должна быть другой:

```text
Patrol UB subgraph
    ↓ читает
ActorRoutineState
    ├── PatrolRouteId
    ├── CurrentStopIndex
    └── CurrentNavigationAnchor
    ↓ получает definition
PatrolRouteCatalog
    ↓ action выполняет
INavigationPathFollower
    ↓ после успеха обновляет
ActorRoutineState
```

`NavigationPatrol` после этого либо исчезнет, либо превратится в маленький stateless service вроде:

```csharp
public interface IPatrolStepExecutor
{
    UniTask<NavigationPathFollowResult> MoveNextAsync(
        ActorPatrolState state,
        PatrolRoute route,
        CancellationToken token);
}
```

---

# Почему сейчас трудно отличить старое от нового

В репозитории лежит `navigation-behavior-ai-refactor.md`, который фактически содержит историю нескольких последовательных архитектурных обсуждений: сначала leases, затем controller, затем UB subgraphs, затем world authoring. Он полезен как журнал размышлений, но не как актуальная спецификация. ([GitHub][9])

Я бы заменил его коротким ADR:

```text
ADR: Actor behavior ownership

1. Unity Behavior — единственный владелец поведения актора.
2. Commands не вызывают navigation/look/combat напрямую.
3. Commands устанавливают или очищают actor-scoped contexts.
4. UB читает contexts и выбирает subgraph по приоритету.
5. UB подтверждает readiness/completion через actor state.
6. Dialogue lifecycle не использует navigation leases.
7. DialogueCoordinator гарантирует Enter → Execute → Exit.
```

После такого документа сразу станет понятно, какой код противоречит целевой системе.

---

# Про World Data

Да, **world data тебе понадобится**, но нельзя делать один универсальный реестр, куда попадут:

* живые акторы;
* patrol points;
* факты;
* save state;
* runtime-компоненты.

Это быстро превратится в новый Service Locator.

Нужно разделить три вида данных.

## 1. Authoring definitions

Неизменяемые данные проекта:

```text
WorldDefinitions
├── Actor definitions
├── Patrol route catalog
├── Locations
├── Navigation anchors
└── Routine definitions
```

Текущий `NavigationPatrolRoute` относится именно сюда: это ScriptableObject со списком `locationId + anchorKey`. Его не нужно сохранять в save-файл — достаточно сохранить его ID. ([GitHub][10])

## 2. Persistent world state

Данные конкретного прохождения:

```text
WorldState
├── ActorStateStore
├── WorldFactStore
├── QuestStateStore
└── LocationStateStore
```

Например:

```csharp
public sealed class ActorState
{
    public ActorId Id { get; init; }
    public ActorDefinitionId DefinitionId { get; init; }

    public NavigationAnchorId CurrentAnchor { get; set; }

    public RoutineId ActiveRoutine { get; set; }
    public PatrolRouteId PatrolRoute { get; set; }
    public int PatrolStopIndex { get; set; }

    public bool IsDead { get; set; }
}
```

Именно это сериализуется.

`DialogueParticipationState` сюда пока не относится: активный разговор — transient runtime operation. Его лучше не сохранять, пока специально не появится поддержка save во время диалога.

## 3. Runtime actor registry

Только загруженные объекты:

```text
ActorRuntimeRegistry
ActorId → ActorRuntimeHandle
```

```csharp
public readonly struct ActorRuntimeHandle
{
    public Guid InstanceId { get; }
    public Transform Transform { get; }
    public Transform DialogueAnchor { get; }
}
```

Это используется для:

* адресации Commands;
* получения актуальной позиции;
* поиска dialogue anchor;
* получения runtime instance ID.

Это **не сохраняется**.

---

# Нужны три вида ID

Сейчас базовый `WorldInstance` содержит только runtime `InstanceId`. Для save/load этого недостаточно. ([GitHub][11])

Разделение:

```text
ActorDefinitionId
    какой это тип актора:
    MilitiaGuard

ActorId
    кто это в мире:
    CityGuard_Heinrich

InstanceId
    конкретный загруженный runtime instance
```

Registry связывает:

```text
ActorId ↔ InstanceId
```

Commands сейчас могут продолжить адресоваться по `InstanceId`, но Yarn, квесты, расписания и save data должны использовать стабильный `ActorId`.

---

# Как организовать `TryGet`

`TryGet` здесь подходит. Не подходит универсальный:

```csharp
_worldData.TryGet<T>(id, out var value);
```

Это снова скрытый Service Locator.

Лучше типизированные stores:

```csharp
public interface IWorldState
{
    IActorStateStore Actors { get; }
    IWorldFactStore Facts { get; }
}

public interface IWorldRuntime
{
    IActorRuntimeRegistry Actors { get; }
}

public interface IWorldDefinitions
{
    IPatrolRouteCatalog PatrolRoutes { get; }
    ILocationCatalog Locations { get; }
}
```

Использование:

```csharp
_worldState.Actors.TryGet(
    actorId,
    out var actorState);

_worldRuntime.Actors.TryGet(
    actorId,
    out var runtimeActor);

_worldDefinitions.PatrolRoutes.TryGet(
    actorState.PatrolRoute,
    out var route);

_worldState.Facts.TryGet(
    WorldFacts.CityGateOpened,
    out bool opened);
```

Facade может называться `WorldData`, но внутри должны оставаться отдельные typed stores.

---

# Что переписывать дальше

Порядок я бы выбрал такой:

### 1. Закончить dialogue migration

* `DialogueParticipation` → `ActorDialogueState`;
* polling → `WaitUntilReadyAsync`;
* `Sequential` → `Switch`;
* параллельный enter участников;
* cleanup через `CancellationToken.None`;
* убрать position из command;
* вынести `ClearFacing` в abort-safe cleanup.

### 2. Удалить следы lease-терминологии

Проверить и удалить:

```text
Lifecycle
Lease
AcquirePause
SessionStore для leases
ручное navigation suspension
```

Commands и `Enter/Exit` при этом остаются — это не legacy.

### 3. Ввести IDs и три хранилища

```text
WorldDefinitions
WorldState
WorldRuntime
```

Сначала минимально:

```text
ActorId
ActorStateStore
ActorRuntimeRegistry
PatrolRouteCatalog
```

### 4. Перенести patrol progress из `NavigationPatrol`

В `ActorState`:

```text
CurrentAnchor
PatrolRouteId
PatrolStopIndex
```

После этого UB Patrol Subgraph станет настоящим владельцем патрульного поведения, а не обёрткой над старым `NavigationPatrol`.

### 5. Только потом добавлять combat/scripted routines

Сначала нужно доказать архитектуру на двух режимах:

```text
Routine Patrol
Dialogue Override
```

Если эта пара работает чисто, combat и scripted movement будут добавляться тем же паттерном.

## Итог

Сейчас это не провальная реализация, а **не завершённая миграция**. Новый диалоговый контур уже в основном правильный:

```text
Command → actor state → UB → readiness
```

Бардак возникает потому, что:

* роли плохо названы;
* command execution держит polling;
* patrol ещё построен по старой component-driven модели;
* отсутствует явное разделение definitions / persistent state / runtime registry;
* документ проекта описывает несколько противоречащих друг другу этапов.

Самый важный следующий шаг — не добавлять новые системы, а **зафиксировать эту границу и довести до неё Dialogue + Patrol**.
