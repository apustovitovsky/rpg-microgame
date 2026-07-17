Да — в твоём случае **переключение поведения NPC действительно лучше полностью отдать Unity Behavior**, а мои прежние `ActorBehaviorController` и behavior-leases были избыточны.

Unity Behavior позволяет собирать общий граф из переиспользуемых статических или динамических subgraph-ов, передавать им Blackboard-переменные и прерывать низкоприоритетную ветку через observer abort/restart. При прерывании текущая ветка получает `OnEnd`, после чего дерево снова оценивается с начала. ([Документация Unity][1])

## Как должен выглядеть NPC

```text
NPC Root Graph
│
├── Combat observer
│       └── Run Combat Subgraph
│
├── Dialogue observer
│       └── Run Dialogue Subgraph
│
├── ScriptedTask observer
│       └── Run Scripted Task Subgraph
│
└── Run Routine Subgraph
        ├── Patrol
        ├── GuardPost
        ├── Work
        └── Idle
```

Приоритет задаётся структурой общего графа:

```text
Combat
→ Dialogue
→ Scripted
→ Routine
```

Когда появляется `DialogueContext`, observer прерывает патруль. Навигационная action получает `OnEnd`, отменяет свой async-вызов, после чего запускается dialogue subgraph. Unity как раз рекомендует observer nodes для прерывания низкоприоритетного поведения и сохранения ясного порядка приоритетов. ([Документация Unity][2])

## Где хранится конкретика поведения

Subgraph — это алгоритм поведения, а Blackboard — его данные.

```text
Patrol Subgraph
    PatrolRoute
    CurrentPatrolIndex
    Loop

GuardPost Subgraph
    PostAnchor
    FacingDirection
    ObservationRadius

Dialogue Subgraph
    DialogueSessionId
    DialoguePartnerId

ScriptedMove Subgraph
    Destination
    GoalId
```

Поэтому не нужен универсальный `ActorTaskKind`, содержащий все возможные поля. Каждый subgraph получает только подходящие ему Blackboard-переменные. Unity Behavior поддерживает собственные Blackboard-переменные графа и передачу переменных в subgraph. ([Документация Unity][1])

## Что делают внешние команды

Они не управляют навигацией и не вызывают subgraph напрямую. Они меняют входной контекст UB:

```text
StartDialogueCommand
    ↓
установить DialogueContext
    ↓
послать BehaviorChanged event
    ↓
UB переключается на Dialogue Subgraph
```

```text
AssignPatrolCommand
    ↓
установить RoutineGraph / PatrolRoute
    ↓
UB начинает или обновляет Routine Subgraph
```

```text
MoveActorToCommand
    ↓
установить ScriptedGoal
    ↓
UB переключается на Scripted Movement Subgraph
```

Event channel означает: «немедленно переоцени поведение». Blackboard хранит само состояние.

## Нужны ли теперь leases

**Для переключения NPC-поведения — нет.**

Не нужен такой lease:

```text
Dialogue lease
    → pause UB
    → suspend navigation
    → затем всё вручную восстановить
```

UB сам:

1. прерывает patrol/navigation branch;
2. запускает dialogue branch;
3. после очистки `DialogueContext` снова выбирает combat, scripted или routine.

Но lease всё ещё может быть полезен как технический способ гарантировать очистку временного контекста:

```csharp
var dialogueHandle = participant.EnterDialogue(context);

try
{
    await RunYarnAsync();
}
finally
{
    dialogueHandle.Dispose();
}
```

Он здесь не управляет поведением. Он всего лишь эквивалентен безопасному:

```csharp
try
{
    participant.SetDialogueContext(context);
    await RunYarnAsync();
}
finally
{
    participant.ClearDialogueContext(context.SessionId);
}
```

Можно вообще отказаться от `IDisposable` и оставить явные `EnterDialogue` / `ExitDialogue`. Главное — очищать контекст в `finally` и проверять `SessionId`, чтобы завершение старого диалога случайно не очистило более новый.

## Игрок

У игрока может быть отдельный минимальный граф:

```text
Player Graph
└── Dialogue Subgraph
```

Он отвечает только за:

* остановку или запрет gameplay-input;
* поворот к собеседнику;
* dialogue idle-анимацию;
* состояние готовности.

Обычным перемещением игрока по-прежнему управляет input-система, а не UB.

## Итоговая схема

```text
CommandSystem
    меняет Blackboard/context конкретного актора

Unity Behavior root graph
    определяет приоритет поведения

Subgraph
    исполняет конкретный режим

Action nodes
    вызывают navigation/look/combat capabilities

DialogueCoordinator
    только устанавливает DialogueContext обоим участникам,
    ждёт их readiness, запускает Yarn и очищает context
```

То есть **subgraph-архитектура действительно заменяет behavior-leases и ручную паузу навигации**. Оставлять стоит только session lifecycle и гарантированную очистку контекста — это уже не механизм управления AI, а защита целостности операции.

[1]: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/node-types.html?utm_source=chatgpt.com "Behavior graph node types | Behavior | 1.0.16"
[2]: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/behavior-graph.html?utm_source=chatgpt.com "Behavior graphs | Behavior | 1.0.16"
Да, но здесь важно разделить **authoring поведения** и **authoring пространства**.

## Где хранить патруль и пост

Не стоит складывать сами мировые точки внутрь actor fragment. Во фрагменте лучше хранить **назначение поведения и ссылки на мировые данные**:

```text
Actor fragments
├── DialogueFragment
│   └── DialogueDefinition
│
└── RoutineFragment
    └── PatrolRoutine
        ├── Route
        └── InitialStop
```

При этом сами точки остаются частью мира:

```text
World navigation authoring
├── Locations
├── Navigation anchors
├── Activity slots
└── Patrol routes
```

Текущий `NavigationPatrolRoute` уже идёт в правильную сторону: он хранит последовательность `locationId + anchorKey`. А вот `NavigationPatrol` сейчас сам сериализует `_route`, `_initialStop` и дополнительно держит `_currentLocationId/_currentAnchorKey`, из-за чего данные конкретного NPC оказались на runtime-компоненте prefab. ([GitHub][1])

Я бы заменил это примерно на:

```csharp
[Serializable]
public sealed class RoutineFragment : ActorFragment
{
    [field: SerializeField]
    public RoutineDefinition InitialRoutine { get; private set; }
}
```

```csharp
public abstract class RoutineDefinition : ScriptableObject
{
}
```

```csharp
[CreateAssetMenu]
public sealed class PatrolRoutineDefinition : RoutineDefinition
{
    [field: SerializeField]
    public NavigationPatrolRoute Route { get; private set; }

    [field: SerializeField]
    public NavigationAnchorReference InitialAnchor { get; private set; }
}
```

```csharp
[CreateAssetMenu]
public sealed class GuardPostRoutineDefinition : RoutineDefinition
{
    [field: SerializeField]
    public NavigationAnchorReference Post { get; private set; }
}
```

То есть:

```text
Fragment:
«Этот NPC по умолчанию патрулирует маршрут CityGuardNight».

Route:
«Маршрут состоит из Market/North, Gate/Inside, Barracks/Entrance».

World authoring:
«Где физически находятся эти locations и anchors».
```

`DialogueFragment` сейчас работает похожим образом: он не управляет диалогом, а лишь сообщает, какая `DialogueDefinition` принадлежит актору; `DialogueParticipant` читает её через `IFragmentProvider`. ([GitHub][2])

### Но есть оговорка

Если один `ActorDefinition` используется для десяти одинаковых стражников, нельзя помещать конкретный маршрут в общий fragment определения — иначе все получат один маршрут.

Тогда разделение такое:

```text
ActorDefinition / prefab
    Behavior profile:
    NPC умеет Patrol, GuardPost, Dialogue, Combat

Actor instance authoring / spawn placement
    Initial routine:
    этот конкретный NPC патрулирует CityNorthRoute
```

То есть конкретная routine может принадлежать не типу актора, а его размещению в мире.

---

# Убираем ли dialogue lifecycle

**Убираем lifecycle, который вручную управляет поведением.**

Сейчас цепочка такая:

```text
EnterDialogueCommand
    ↓
EnterDialogueCommandHandler
    ↓
каждый IDialogueParticipantLifecycle.EnterAsync()
    ↓
DialogueNavigationLifecycle
    ↓
IActorNavigation.AcquirePause()
```

Полученные leases складываются в `DialogueParticipantSessionStore`, а `ExitDialogueCommand` достаёт их и освобождает. ([GitHub][3])

Если UB становится владельцем поведения, этот слой действительно лишний:

```text
DialogueNavigationLifecycle                 удалить
IDialogueParticipantLifecycle               удалить
CompositeDialogueParticipantLease           удалить
DialogueParticipantSessionStore             удалить
```

Вместо паузы навигации:

```text
EnterDialogueCommand
    ↓
устанавливает DialogueContext
    ↓
посылает событие UB
    ↓
UB прерывает Patrol/GuardPost
    ↓
запускает Dialogue Subgraph
```

При выходе:

```text
ExitDialogueCommand
    ↓
очищает DialogueContext
    ↓
UB снова оценивает граф
    ↓
выбирает Combat / Scripted / Routine
```

Unity Behavior поддерживает передачу данных через Blackboard, реакцию через event channels, а также переиспользуемые статические и динамические subgraph-ы. `Abort` и `Restart` позволяют остановить текущую ветку при изменении условий. ([Документация Unity][4])

---

# Но lifecycle диалоговой сессии всё равно остаётся

Нужно различать:

```text
Behavior lifecycle
    кто сейчас управляет персонажем
    → отдаём UB

Dialogue session lifecycle
    когда установить и очистить DialogueContext
    → остаётся у DialogueCoordinator
```

Сейчас `DialogueCoordinator` уже использует `try/finally`: получает participant lease, запускает Yarn и гарантированно освобождает lease при завершении или cancellation. Это хорошая гарантия целостности, но для неё необязательно возвращать lease. ([GitHub][5])

Можно сделать проще:

```csharp
private async UniTask RunSessionAsync(
    DialogueSession session,
    CancellationToken token)
{
    try
    {
        await _participants.EnterAsync(session, token);
        await _executor.ExecuteAsync(session, token);
    }
    finally
    {
        await _participants.ExitAsync(
            session,
            CancellationToken.None);
    }
}
```

Контракт:

```csharp
public interface IDialogueParticipantCoordinator
{
    UniTask EnterAsync(
        DialogueSession session,
        CancellationToken token);

    UniTask ExitAsync(
        DialogueSession session,
        CancellationToken token);
}
```

То есть внешний `IDialogueParticipantLease` тоже можно удалить. Его роль полностью заменяет уже существующий `try/finally`.

---

# Как изменятся команды

Кстати, в текущем `main` сами команды lifecycle наружу не возвращают. `EnterDialogueCommandHandler` и `StartDialogueCommandHandler` возвращают `CommandResult`; lease создаётся внутри participant coordinator и передаётся `DialogueCoordinator`. ([GitHub][6])

После перехода на UB:

```csharp
public sealed class EnterDialogueCommandHandler
    : CommandHandler<EnterDialogueCommand>
{
    private readonly IActorDialogueContext _dialogue;
    private readonly IBehaviorSignal _behavior;

    public override async UniTask<CommandResult> HandleAsync(
        EnterDialogueCommand command,
        Guid receiverId,
        CancellationToken token)
    {
        if (!_dialogue.TryEnter(
                command.SessionId,
                command.OtherParticipantInstanceId))
        {
            return CommandResult.Busy;
        }

        _behavior.NotifyContextChanged();

        await _dialogue.WaitUntilReadyAsync(
            command.SessionId,
            token);

        return CommandResult.Completed;
    }
}
```

```csharp
public sealed class ExitDialogueCommandHandler
    : CommandHandler<ExitDialogueCommand>
{
    private readonly IActorDialogueContext _dialogue;
    private readonly IBehaviorSignal _behavior;

    public override UniTask<CommandResult> HandleAsync(
        ExitDialogueCommand command,
        Guid receiverId,
        CancellationToken token)
    {
        _dialogue.TryExit(command.SessionId);
        _behavior.NotifyContextChanged();

        return UniTask.FromResult(
            CommandResult.Completed);
    }
}
```

## Зачем `WaitUntilReadyAsync`

Необходимо различать:

```text
DialogueContext установлен
```

и:

```text
UB уже:
- прервал патруль;
- остановил персонажа;
- повернул его к собеседнику;
- заблокировал input игрока;
- вошёл в dialogue idle.
```

В конце Dialogue Subgraph должен быть action:

```text
Mark Dialogue Participant Ready
```

Он сообщает C#-контексту:

```csharp
_dialogueContext.MarkReady(sessionId);
```

Тогда `EnterDialogueCommand` завершается только после реального входа персонажа в диалоговое поведение. Yarn не начинает реплики раньше времени.

Это уже не lifecycle и не управление UB извне — это обычное подтверждение выполнения команды.

---

# Что находится в UB

Для NPC:

```text
NPC Root Graph
├── Combat Subgraph
├── Dialogue Subgraph
├── Scripted Subgraph
└── Dynamic Routine Subgraph
    ├── Patrol
    ├── GuardPost
    ├── Work
    └── Idle
```

Для игрока:

```text
Player Root Graph
└── Dialogue Subgraph
```

Unity Behavior позволяет подставлять dynamic subgraph через Blackboard-переменную либо использовать статические subgraph-ы с переопределяемыми Blackboard-параметрами. ([Документация Unity][4])

Routine-данные подаются в него через Blackboard:

```text
ActiveRoutineGraph
PatrolRoute
GuardPostAnchor
DialoguePartner
DialogueSessionId
CombatTarget
```

## Итог

**Да, UB должен сам включать и выключать поведение.**

Но данные разделяются так:

```text
Actor fragment / instance authoring
    какую routine назначить конкретному персонажу

World authoring
    где находятся маршрут, пост, вход, рабочее место

Blackboard
    текущий runtime-контекст

UB
    какое поведение сейчас активно

Commands
    установить или очистить внешний контекст

DialogueCoordinator
    гарантировать Enter → Yarn → Exit через try/finally
```

В результате lease, который вручную ставит навигацию на паузу, полностью исчезает. Сохраняется только жизненный цикл самой диалоговой сессии, но его проще выразить явными `EnterAsync/ExitAsync`, а не возвращаемым объектом lease.
Это встроенные в Unity Behavior **Event Channels** — сообщения, которые можно отправить в конкретный `BehaviorGraphAgent`.

Например:

```text
DialogueStarted(sessionId, partnerId)
CombatTargetDetected(targetId)
BehaviorContextChanged()
```

В графе на них реагируют специальные узлы:

```text
Start On Event Message
Wait for Event Message
Send Event Message
```

`Start On Event Message` может запустить или перезапустить соответствующую ветку графа после получения сообщения. ([Документация Unity][1])

Из C# это примерно:

```csharp
_stateEventChannel.Value.SendEventMessage(
    ActorEvent.DialogueStarted);
```

Важно различать:

```text
Blackboard variable
    хранит состояние:
    DialogueContext = ...

Event channel
    сообщает:
    DialogueContext изменился, отреагируй сейчас
```

Событие само по себе обычно не должно хранить данные надолго. UB после события читает актуальные значения из Blackboard. Event channel может быть привязан к конкретному экземпляру агента, поэтому сигнал получит именно нужный NPC, если переменная не `Shared`. ([Документация Unity][2])

Для твоего диалога:

```text
CommandHandler
    записал DialogueContext в Blackboard
    отправил DialogueContextChanged

UB
    получил событие
    прервал Patrol
    запустил Dialogue Subgraph
```

То есть это не глобальный event bus игры, а **встроенный механизм пробуждения и переключения Behavior Graph**.

[1]: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/event-nodes.html?utm_source=chatgpt.com "Use event nodes | Behavior | 1.0.16"
[2]: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/event-nodes-diff-graphs.html?utm_source=chatgpt.com "Integrate event nodes within and across behavior graphs"

