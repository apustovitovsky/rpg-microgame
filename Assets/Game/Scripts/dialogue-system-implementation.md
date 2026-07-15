## Общая оценка

Начало правильное: ты уже отделил `DialogueSession`, результат открытия, сервис сессий и actor-scoped точку запуска. Но сейчас система моделирует в основном **реестр открытых сессий**, а не **жизненный цикл диалога**.

Текущий `DialogueSession` содержит только новый `Guid` и два `InstanceId`; `IDialogueSessionService` отдельно открывает и закрывает сессию; `IDialogueSessionStarter` отдельно запускает асинхронную операцию. В результате пока нет одного объекта, который гарантированно владеет всем процессом:

```text
проверить возможность
→ создать сессию
→ заблокировать управление
→ остановить NPC
→ запустить Yarn
→ дождаться завершения
→ восстановить участников
→ закрыть сессию
```

Это главный участок, который я бы переработал прежде, чем добавлять UI, Yarn-команды и сохранение. ([GitHub][1])

---

# Целевая архитектура

```text
DialogueInteractable
        ↓
IDialogueEndpoint              actor-scoped capability
        ↓
IDialogueCoordinator           владелец жизненного цикла
        ├── IDialogueRuntime   адаптер над Yarn
        ├── IDialogueParticipantCoordinator
        │       ├── блокировка игрока
        │       ├── пауза NPC
        │       ├── поворот участников
        │       └── камера
        └── Active DialogueSession
```

Границы ответственности:

* `DialogueInteractable` — только интеграция с системой взаимодействия.
* `IDialogueEndpoint` — конкретный NPC и его dialogue entry point.
* `IDialogueCoordinator` — транзакция разговора целиком.
* `DialogueSession` — данные активного разговора.
* `IDialogueRuntime` — исполнение сценария, сейчас Yarn Spinner.
* Yarn — последовательность реплик и вариантов, но не управление игровыми объектами напрямую.
* Gameplay-системы — источник истины для квестов, фактов, инвентаря и мира.

---

# 1. Убрать публичные `TryOpen` и `Close`

Сейчас вызывающий код может:

* открыть сессию, но не запустить Yarn;
* запустить Yarn, но забыть закрыть сессию;
* вызвать `Close`, пока Yarn и UI ещё работают;
* получить исключение и оставить систему в состоянии `AlreadyOpen`.

Поэтому `TryOpen` и `Close` должны стать деталями реализации координатора, а не публичным API.

Вместо текущего `IDialogueSessionService`:

```csharp
public interface IDialogueCoordinator
{
    bool TryGetActive(out DialogueSession session);

    DialogueAvailability Evaluate(DialogueRequest request);

    UniTask<DialogueRunResult> RunAsync(
        DialogueRequest request,
        CancellationToken cancellationToken);

    UniTask StopAsync(
        DialogueStopReason reason,
        CancellationToken cancellationToken);
}
```

`RunAsync` владеет всей транзакцией:

```csharp
public sealed class DialogueCoordinator : IDialogueCoordinator
{
    private readonly IDialogueRuntime _runtime;
    private readonly IDialogueParticipantCoordinator _participants;

    private DialogueSession _activeSession;

    public DialogueCoordinator(
        IDialogueRuntime runtime,
        IDialogueParticipantCoordinator participants)
    {
        _runtime = runtime;
        _participants = participants;
    }

    public bool TryGetActive(out DialogueSession session)
    {
        session = _activeSession;
        return session != null;
    }

    public DialogueAvailability Evaluate(DialogueRequest request)
    {
        if (!request.IsValid)
            return DialogueAvailability.InvalidRequest;

        if (_activeSession != null)
            return DialogueAvailability.Busy;

        return DialogueAvailability.Available;
    }

    public async UniTask<DialogueRunResult> RunAsync(
        DialogueRequest request,
        CancellationToken cancellationToken)
    {
        var availability = Evaluate(request);

        if (availability != DialogueAvailability.Available)
            return DialogueRunResult.Rejected(availability);

        var session = new DialogueSession(
            Guid.NewGuid(),
            request);

        _activeSession = session;

        IDialogueParticipantLease participantLease = null;

        try
        {
            participantLease = await _participants.EnterAsync(
                session,
                cancellationToken);

            await _runtime.RunAsync(
                session,
                cancellationToken);

            return DialogueRunResult.Completed(session.Id);
        }
        finally
        {
            if (participantLease != null)
            {
                await participantLease.DisposeAsync();
            }

            if (_activeSession?.Id == session.Id)
            {
                _activeSession = null;
            }
        }
    }

    public UniTask StopAsync(
        DialogueStopReason reason,
        CancellationToken cancellationToken)
    {
        if (_activeSession == null)
            return UniTask.CompletedTask;

        return _runtime.StopAsync(reason, cancellationToken);
    }
}
```

Здесь есть важное правило:

> ожидаемые отказы возвращаются как result, неожиданные ошибки не проглатываются, но очистка всё равно выполняется через `finally`.

---

# 2. `DialogueSession` должна принимать готовый ID и request

Сейчас `DialogueSession` самостоятельно делает `Guid.NewGuid()`. Это неудобно для тестирования и восстановления состояния. Кроме того, два соседних параметра типа `Guid` легко случайно переставить местами.

Лучше сначала сформировать request:

```csharp
public readonly struct DialogueRequest
{
    public DialogueRequest(
        Guid initiatorInstanceId,
        Guid speakerInstanceId,
        DialogueEntry entry)
    {
        InitiatorInstanceId = initiatorInstanceId;
        SpeakerInstanceId = speakerInstanceId;
        Entry = entry;
    }

    public Guid InitiatorInstanceId { get; }
    public Guid SpeakerInstanceId { get; }
    public DialogueEntry Entry { get; }

    public bool IsValid =>
        InitiatorInstanceId != Guid.Empty &&
        SpeakerInstanceId != Guid.Empty &&
        InitiatorInstanceId != SpeakerInstanceId &&
        Entry.IsValid;
}
```

```csharp
public readonly struct DialogueEntry
{
    public DialogueEntry(string nodeName)
    {
        NodeName = nodeName?.Trim();
    }

    public string NodeName { get; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(NodeName);
}
```

```csharp
public sealed class DialogueSession
{
    public DialogueSession(
        Guid id,
        DialogueRequest request)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Session id is empty.", nameof(id));

        if (!request.IsValid)
            throw new ArgumentException("Dialogue request is invalid.", nameof(request));

        Id = id;
        Request = request;
    }

    public Guid Id { get; }
    public DialogueRequest Request { get; }

    public Guid InitiatorInstanceId =>
        Request.InitiatorInstanceId;

    public Guid SpeakerInstanceId =>
        Request.SpeakerInstanceId;
}
```

Я бы заменил `ParticipantInstanceId` на `SpeakerInstanceId`. Слово `participant` слишком абстрактно: инициатор тоже является participant.

Когда появятся групповые разговоры, можно будет добавить:

```csharp
IReadOnlyList<DialogueParticipant>
```

Но сейчас это преждевременно.

---

# 3. Переименовать `IDialogueSessionStarter`

Этот интерфейс не управляет сессиями. Он представляет возможность конкретного NPC начать разговор.

Поэтому лучше:

```csharp
public interface IDialogueEndpoint
{
    DialogueAvailability Evaluate(Guid initiatorInstanceId);

    UniTask<DialogueRunResult> RunAsync(
        Guid initiatorInstanceId,
        CancellationToken cancellationToken);
}
```

Реализация находится в actor scope:

```csharp
public sealed class DialogueEndpoint : IDialogueEndpoint
{
    private readonly Guid _speakerInstanceId;
    private readonly DialogueEntry _entry;
    private readonly IDialogueCoordinator _coordinator;

    public DialogueEndpoint(
        Guid speakerInstanceId,
        DialogueEntry entry,
        IDialogueCoordinator coordinator)
    {
        _speakerInstanceId = speakerInstanceId;
        _entry = entry;
        _coordinator = coordinator;
    }

    public DialogueAvailability Evaluate(Guid initiatorInstanceId)
    {
        return _coordinator.Evaluate(CreateRequest(initiatorInstanceId));
    }

    public UniTask<DialogueRunResult> RunAsync(
        Guid initiatorInstanceId,
        CancellationToken cancellationToken)
    {
        return _coordinator.RunAsync(
            CreateRequest(initiatorInstanceId),
            cancellationToken);
    }

    private DialogueRequest CreateRequest(Guid initiatorInstanceId)
    {
        return new DialogueRequest(
            initiatorInstanceId,
            _speakerInstanceId,
            _entry);
    }
}
```

Таким образом endpoint знает:

* кто является собеседником;
* с какого Yarn node начать;
* какой глобальный координатор вызвать.

Но endpoint не знает:

* где находится `DialogueRunner`;
* как устроен UI;
* как отключается input;
* как ставится на паузу Behavior Graph;
* как хранятся квестовые факты.

---

# 4. Текущую Interaction-систему стоит немного рефакторить

Сейчас `IInteractable.InteractAsync` возвращает просто `UniTask`. Обработчик проверяет дистанцию, вызывает `CanInteract`, ожидает `InteractAsync`, а затем считает команду успешно завершённой. Поэтому ситуация, когда диалог стал занят между `CanInteract` и фактическим запуском, не может корректно вернуться в `InteractCommandHandler`. ([GitHub][2])

Это классический разрыв между advisory check и authoritative operation:

```text
CanInteract == true
другой процесс открыл диалог
InteractAsync получает Busy
InteractCommand всё равно считает себя Completed
```

Я бы изменил контракт:

```csharp
public interface IInteractable
{
    Vector3 InteractionPoint { get; }
    float MaxRange { get; }

    bool CanInteract(InteractionContext context);

    UniTask<InteractionResult> InteractAsync(
        InteractionContext context,
        CancellationToken cancellationToken);
}
```

Где:

```csharp
public enum InteractionStatus
{
    Completed,
    Rejected,
    Busy
}

public readonly struct InteractionResult
{
    public InteractionResult(InteractionStatus status)
    {
        Status = status;
    }

    public InteractionStatus Status { get; }

    public bool Succeeded =>
        Status == InteractionStatus.Completed;
}
```

Тогда dialogue adapter будет тонким:

```csharp
public sealed class DialogueInteractable :
    MonoBehaviour,
    IInteractable
{
    [SerializeField]
    private Transform _interactionPoint;

    [SerializeField, Min(0f)]
    private float _maxRange = 2f;

    private IDialogueEndpoint _endpoint;

    [Inject]
    public void Construct(IDialogueEndpoint endpoint)
    {
        _endpoint = endpoint;
    }

    public Vector3 InteractionPoint =>
        _interactionPoint.position;

    public float MaxRange =>
        _maxRange;

    public bool CanInteract(InteractionContext context)
    {
        return _endpoint.Evaluate(context.InteractorInstanceId) ==
               DialogueAvailability.Available;
    }

    public async UniTask<InteractionResult> InteractAsync(
        InteractionContext context,
        CancellationToken cancellationToken)
    {
        var result = await _endpoint.RunAsync(
            context.InteractorInstanceId,
            cancellationToken);

        return result.Status switch
        {
            DialogueRunStatus.Completed =>
                new InteractionResult(InteractionStatus.Completed),

            DialogueRunStatus.Busy =>
                new InteractionResult(InteractionStatus.Busy),

            _ =>
                new InteractionResult(InteractionStatus.Rejected)
        };
    }
}
```

Дистанцию здесь повторно проверять не надо: текущий `InteractCommandHandler` уже строит `InteractionContext`, проверяет `MaxRange`, `CanInteract` и cancellation token. ([GitHub][3])

При этом `DialogueCoordinator.RunAsync` всё равно обязан повторно атомарно проверить доступность. `CanInteract` нужен для UI и предварительной фильтрации, но не является гарантией запуска.

---

# 5. Для NPC использовать fragment

Твоя actor-модель уже поддерживает fragments: `ActorFragment` является сериализуемой базой, а `InventoryFragment` показывает существующий подход, при котором definition содержит данные capability и создаёт runtime-состояние. ([GitHub][4])

Добавь:

```csharp
[Serializable]
public sealed class DialogueFragment : ActorFragment
{
    [SerializeField]
    private DialogueDefinition _definition;

    public DialogueDefinition Definition =>
        _definition;
}
```

```csharp
[CreateAssetMenu(
    fileName = "DialogueDefinition",
    menuName = "Game/Dialogue/Dialogue Definition")]
public sealed class DialogueDefinition : ScriptableObject
{
    [SerializeField]
    private string _definitionId;

    [SerializeField]
    private YarnProject _project;

    [SerializeField]
    private string _entryNode;

    public string DefinitionId => _definitionId;
    public YarnProject Project => _project;
    public string EntryNode => _entryNode;

    private void OnValidate()
    {
        _definitionId = _definitionId?.Trim();
        _entryNode = _entryNode?.Trim();
    }
}
```

Но `DialogueFragment` лучше разместить не внутри `Game.Actor`, а внутри dialogue integration assembly:

```text
Game.Actor
    не зависит от
Game.Dialogue

Game.Dialogue.Yarn
    зависит от
Game.Actor
```

То есть:

```text
Dialogue/Yarn/Actor/DialogueFragment.cs
Dialogue/Yarn/Actor/DialogueEndpointInstaller.cs
```

Наследник класса может находиться в другой assembly. Так actor-модуль остаётся нейтральным и не начинает знать про Yarn, диалоги и UI.

`ActorDefinition` при этом продолжит быть обычным definition с prefab и fragments; добавление dialogue capability не требует встраивать Yarn-поля непосредственно в сам `ActorDefinition`. ([GitHub][5])

---

# 6. Yarn должен находиться за `IDialogueRuntime`

В проекте подключён Yarn Spinner 3.2.4, поэтому стоит использовать API именно третьей версии, включая `DialoguePresenter` и асинхронный lifecycle runner. ([GitHub][6])

```csharp
public interface IDialogueRuntime
{
    UniTask RunAsync(
        DialogueSession session,
        CancellationToken cancellationToken);

    UniTask StopAsync(
        DialogueStopReason reason,
        CancellationToken cancellationToken);
}
```

Реализация:

```csharp
public sealed class YarnDialogueRuntime : IDialogueRuntime
{
    private readonly DialogueRunner _runner;
    private readonly IDialogueContentResolver _contentResolver;

    public YarnDialogueRuntime(
        DialogueRunner runner,
        IDialogueContentResolver contentResolver)
    {
        _runner = runner;
        _contentResolver = contentResolver;
    }

    public async UniTask RunAsync(
        DialogueSession session,
        CancellationToken cancellationToken)
    {
        var content = _contentResolver.Resolve(
            session.Request.Entry);

        _runner.SetProject(content.Project);

        cancellationToken.ThrowIfCancellationRequested();

        using var cancellationRegistration =
            cancellationToken.RegisterWithoutCaptureExecutionContext(
                StopFromCancellation);

        await _runner.StartDialogue(content.NodeName);

        // StartDialogue подготавливает запуск и presenters;
        // DialogueTask завершается после окончания всего разговора.
        await _runner.DialogueTask;
    }

    public async UniTask StopAsync(
        DialogueStopReason reason,
        CancellationToken cancellationToken)
    {
        if (!_runner.IsDialogueRunning)
            return;

        await _runner.Stop();
    }

    private void StopFromCancellation()
    {
        if (_runner.IsDialogueRunning)
        {
            _runner.Stop().Forget();
        }
    }
}
```

Это схематичный код: точный тип `YarnTask` и конвертация в `UniTask` зависят от конфигурации Yarn package.

В Yarn Spinner 3.1+ `StartDialogue` и `Stop` стали асинхронными, но завершение `StartDialogue` означает завершение подготовки presenters, а не окончание всего разговора. Для ожидания полного диалога `DialogueRunner` предоставляет `DialogueTask`. Это важное различие для твоего `RunAsync`. ([docs.yarnspinner.dev][7])

Отмена должна не только прекращать ожидание `UniTask`, но и вызывать `DialogueRunner.Stop()`. Иначе команда завершится, а Yarn UI и presenters продолжат работать.

---

# 7. Один `DialogueRunner` на modal dialogue channel

Для текущей single-player RPG я бы сделал один `DialogueRunner` в gameplay/scene scope:

```text
Gameplay LifetimeScope
├── DialogueRunner
├── Dialogue Presenters
├── YarnDialogueRuntime
├── DialogueCoordinator
└── DialogueParticipantCoordinator
```

NPC не должны содержать собственные runner-ы. У них только:

```text
DialogueFragment
DialogueEndpoint
DialogueInteractable
```

`DialogueRunner` по своей роли является scene-level мостом между Yarn Project, variable storage, line provider и presenters. Для обычного модального диалога один runner и один комплект UI дают наиболее понятную concurrency policy. Дополнительные runner-ы нужны только для действительно независимых каналов, например фоновых реплик нескольких NPC, которые могут идти одновременно с основным разговором. ([docs.yarnspinner.dev][8])

Регистрация в VContainer примерно такая:

```csharp
builder.RegisterComponent(_dialogueRunner);

builder.Register<YarnDialogueRuntime>(Lifetime.Scoped)
    .As<IDialogueRuntime>();

builder.Register<DialogueParticipantCoordinator>(Lifetime.Scoped)
    .As<IDialogueParticipantCoordinator>();

builder.Register<DialogueCoordinator>(Lifetime.Scoped)
    .As<IDialogueCoordinator>();
```

`DialogueCoordinator` и runner должны иметь одинаковый gameplay lifetime.

---

# 8. Явно зафиксировать concurrency policy

Текущие статусы:

```csharp
Opened
AlreadyOpen
InvalidRequest
```

не отвечают на вопрос: что именно уже открыто?

* диалог этого игрока;
* диалог с этим NPC;
* тот же самый диалог;
* вообще любой глобальный диалог.

Для одного global runner я бы зафиксировал:

```csharp
public enum DialogueAvailability
{
    Available,
    Busy,
    InvalidRequest,
    ContentUnavailable
}
```

```csharp
public enum DialogueRunStatus
{
    Completed,
    Busy,
    Rejected
}
```

Повторный запуск того же NPC во время активного разговора тоже возвращает `Busy`. Отдельный `AlreadyActive` понадобится только тогда, когда реально появится потребность вернуть существующий session handle.

В реализации достаточно:

```csharp
private DialogueSession _activeSession;
```

Словарь сессий, подобный `LootSessionService`, здесь пока не нужен.

Loot-сессия является синхронным транзакционным состоянием, и проект хранит её в словарях по session ID и looter ID. Диалог же связан с одним modal UI и длительным Yarn runtime. Поэтому копировать loot architecture один в один не стоит. ([GitHub][9])

---

# 9. Блокировка управления через leases, а не через `Unbind`

Сейчас `Game.Control` предоставляет низкоуровневый `IControlInputBinder` с операциями `Bind` и `Unbind`. Этого недостаточно для диалогов, меню, кат-сцен и других пересекающихся режимов. ([GitHub][10])

Плохой вариант:

```csharp
_inputBinder.Unbind();

await _dialogue.RunAsync();

_inputBinder.Bind(input);
```

Он ломается при вложенных состояниях:

```text
открыт диалог
→ открыто системное меню
→ диалог завершился
→ input включился, хотя меню ещё открыто
```

Нужен lease/ref-counted механизм:

```csharp
[Flags]
public enum ControlRestriction
{
    None = 0,
    Movement = 1 << 0,
    Combat = 1 << 1,
    Interaction = 1 << 2,
    Targeting = 1 << 3
}
```

```csharp
public interface IActorControlRestrictions
{
    IDisposable Acquire(
        Guid actorInstanceId,
        ControlRestriction restrictions,
        ControlRestrictionReason reason);
}
```

Диалог получает lease:

```csharp
var controlLease = _controlRestrictions.Acquire(
    session.InitiatorInstanceId,
    ControlRestriction.Movement |
    ControlRestriction.Combat |
    ControlRestriction.Interaction |
    ControlRestriction.Targeting,
    ControlRestrictionReason.Dialogue);
```

После `Dispose` снимаются только ограничения, принадлежавшие этому разговору. Другие ограничения остаются активными.

Это не должен быть глобальный boolean `PlayerIsBusy`. Ограничения:

* адресуются конкретному actor ID;
* разделены по категориям;
* имеют владельца/lease;
* допускают наложение.

---

# 10. Отдельный coordinator участников

`DialogueCoordinator` не должен знать о:

* Unity Behavior;
* `NavMeshAgent`;
* input binder;
* camera;
* look controller;
* animator.

Для этого нужен:

```csharp
public interface IDialogueParticipantCoordinator
{
    UniTask<IDialogueParticipantLease> EnterAsync(
        DialogueSession session,
        CancellationToken cancellationToken);
}
```

```csharp
public interface IDialogueParticipantLease
{
    UniTask DisposeAsync();
}
```

Его реализация может:

```text
1. Заблокировать движение/бой игрока.
2. Приостановить текущую задачу NPC.
3. Остановить текущую навигацию.
4. Развернуть NPC к игроку.
5. При необходимости развернуть игрока.
6. Переключить Cinemachine camera mode.
7. При завершении восстановить всё в обратном порядке.
```

Схематично:

```csharp
public async UniTask<IDialogueParticipantLease> EnterAsync(
    DialogueSession session,
    CancellationToken cancellationToken)
{
    var composite = new DialogueParticipantLease();

    composite.Add(
        _controlRestrictions.Acquire(
            session.InitiatorInstanceId,
            DialogueControlRestrictions,
            ControlRestrictionReason.Dialogue));

    composite.Add(
        _activityController.Pause(
            session.SpeakerInstanceId,
            ActorPauseReason.Dialogue));

    await _navigation.StopAsync(
        session.SpeakerInstanceId,
        cancellationToken);

    await _facing.LookAtAsync(
        session.SpeakerInstanceId,
        session.InitiatorInstanceId,
        cancellationToken);

    return composite;
}
```

Пауза NPC должна быть lease, а не команда вида:

```csharp
behavior.Enabled = false;
```

Тогда после диалога NPC продолжит предыдущую задачу, а не потеряет её.

---

# 11. Gameplay state не должен принадлежать Yarn

Yarn предоставляет variable storage, включая in-memory реализацию и возможность написать собственный storage. In-memory storage очищается после завершения игры; собственный `VariableStorageBehaviour` можно связать с save system. ([docs.yarnspinner.dev][11])

Но для твоего проекта я бы разделил данные:

### В Yarn можно хранить

```text
$asked_about_sword
$selected_reward
$conversation_mood
```

То есть локальные переменные сценария, не имеющие самостоятельного gameplay-смысла.

### В игровых системах нужно хранить

```text
Chapter.Current
Quest.Bennet.State
Faction.Player
Npc.Bennet.Introduced
World.CityGate.Open
```

Доступ из Yarn:

```yarn
<<if has_fact("Npc.Bennet.Introduced")>>
    Bennet: Мы уже знакомы.
<<else>>
    Bennet: Кто ты такой?
    <<set_fact "Npc.Bennet.Introduced">>
<<endif>>
```

В C#:

```csharp
public sealed class YarnGameplayBindings
{
    private readonly IGameFacts _facts;

    public YarnGameplayBindings(IGameFacts facts)
    {
        _facts = facts;
    }

    public void Install(DialogueRunner runner)
    {
        runner.AddFunction<bool, string>(
            "has_fact",
            factId => _facts.Contains(factId));

        runner.AddCommandHandler<string>(
            "set_fact",
            factId => _facts.Set(factId));
    }
}
```

Yarn Spinner поддерживает регистрацию функций и command handlers через `DialogueRunner`. ([docs.yarnspinner.dev][12])

Рекомендуемое правило:

* функции делают запросы без side effects;
* команды изменяют состояние;
* handlers регистрируются централизованно через DI;
* Yarn не ищет gameplay-объекты через `GameObject.Find`;
* Yarn не обращается к registry напрямую.

Для контекстных команд вроде:

```yarn
<<give_item_to_initiator "gold" 100>>
```

handler получает текущую `DialogueSession` через read-only accessor:

```csharp
public interface IDialogueContext
{
    DialogueSession ActiveSession { get; }
}
```

Тогда Yarn не должен знать GUID игрока или NPC.

---

# 12. Assembly structure

Сейчас `Game.Dialogue` зависит от `Game.Interaction` и UniTask, а `Game.Actor` уже имеет собственный большой набор зависимостей и не ссылается на dialogue assembly. Это хорошее направление зависимости, которое стоит сохранить. ([GitHub][13])

Я бы на текущем этапе сделал две assembly:

```text
Game.Dialogue
├── DialogueRequest
├── DialogueSession
├── DialogueRunResult
├── IDialogueCoordinator
├── IDialogueEndpoint
├── IDialogueRuntime
└── IDialogueParticipantCoordinator
```

```text
Game.Dialogue.Yarn
├── DialogueDefinition
├── DialogueFragment
├── DialogueEndpoint
├── DialogueInteractable
├── YarnDialogueRuntime
├── YarnGameplayBindings
├── Presenters
└── VContainer installation
```

Зависимости:

```text
Game.Dialogue.Yarn
    ├── Game.Dialogue
    ├── Game.Actor
    ├── Game.Interaction
    ├── Game.Control
    ├── YarnSpinner.Unity
    └── VContainer

Game.Dialogue
    └── UniTask
```

При росте системы integration можно будет вынести отдельно, но сейчас три-четыре assembly будут излишними.

---

# 13. Что оставить из текущего кода

Оставить концептуально:

* отдельный `DialogueSession`;
* неизменяемые session properties;
* result вместо исключений для ожидаемых отказов;
* `CancellationToken`;
* actor-scoped интерфейс запуска;
* одну активную сессию для modal dialogue.

Изменить:

```text
IDialogueSessionStarter
    → IDialogueEndpoint

IDialogueSessionService
    → IDialogueCoordinator

DialogueSessionResult
    → DialogueRunResult

ParticipantInstanceId
    → SpeakerInstanceId

TryOpen + Close
    → один RunAsync с try/finally
```

Убрать из публичного API:

```text
TryGet(Guid sessionId)
Close(Guid sessionId)
```

Для текущего single-player варианта достаточно:

```csharp
bool TryGetActive(out DialogueSession session);
UniTask StopAsync(...);
```

---

# Порядок реализации

Я бы развивал вертикальный срез в таком порядке:

1. Ввести `DialogueRequest`, `DialogueEntry` и обновить `DialogueSession`.
2. Заменить session service на `DialogueCoordinator`.
3. Написать временный `FakeDialogueRuntime`, который просто ждёт секунду или cancellation.
4. Добавить `IDialogueEndpoint` и `DialogueInteractable`.
5. Изменить `IInteractable.InteractAsync` так, чтобы он возвращал result.
6. Добавить `DialogueFragment` и `DialogueDefinition`.
7. Зарегистрировать один scene-level `DialogueRunner`.
8. Написать `YarnDialogueRuntime`.
9. Добавить control/activity leases.
10. Добавить Yarn functions и commands для gameplay facts.
11. Только затем заниматься камерой, анимациями и несколькими собеседниками.

Первые обязательные тесты:

```text
RunAsync открывает и закрывает сессию
Busy не заменяет активную сессию
Cancellation вызывает Stop и очищает ActiveSession
Исключение runtime освобождает control leases
Ошибка EnterAsync не оставляет систему занятой
Повторный запуск после завершения работает
DialogueInteractable корректно возвращает Busy
```

Итоговая важнейшая граница:

```text
DialogueEndpoint знает, какой разговор запустить.
DialogueCoordinator знает, как провести разговор.
Yarn знает, какие реплики выполнить.
Gameplay-системы знают, что изменилось в мире.
```

Именно такая схема позволит позже подключить к диалогам квесты, Behavior Graph, камеру, анимации, NPC–NPC сцены и сохранение, не превращая `DialogueRunner` в глобальный service locator.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Dialogue/Runtime/DialogueSession.cs "rpg-microgame/Assets/Game/Scripts/Dialogue/Runtime/DialogueSession.cs at main · apustovitovsky/rpg-microgame · GitHub"
[2]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Interaction/Runtime/IInteractable.cs "rpg-microgame/Assets/Game/Scripts/Interaction/Runtime/IInteractable.cs at main · apustovitovsky/rpg-microgame · GitHub"
[3]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Interaction/Runtime/InteractCommand.cs "rpg-microgame/Assets/Game/Scripts/Interaction/Runtime/InteractCommand.cs at main · apustovitovsky/rpg-microgame · GitHub"
[4]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Actor/Fragments/ActorFragment.cs "rpg-microgame/Assets/Game/Scripts/Actor/Fragments/ActorFragment.cs at main · apustovitovsky/rpg-microgame · GitHub"
[5]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Actor/ScriptableObject/ActorDefinition.cs "rpg-microgame/Assets/Game/Scripts/Actor/ScriptableObject/ActorDefinition.cs at main · apustovitovsky/rpg-microgame · GitHub"
[6]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Packages/manifest.json "rpg-microgame/Packages/manifest.json at main · apustovitovsky/rpg-microgame · GitHub"
[7]: https://docs.yarnspinner.dev/readme/ys3.1?utm_source=chatgpt.com "Yarn Spinner 3.1"
[8]: https://docs.yarnspinner.dev/components/dialogue-runner?utm_source=chatgpt.com "Dialogue Runners and Systems"
[9]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Loot/Runtime/ILootSessionService.cs "rpg-microgame/Assets/Game/Scripts/Loot/Runtime/ILootSessionService.cs at main · apustovitovsky/rpg-microgame · GitHub"
[10]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Control/Runtime/IControlInputBinder.cs "rpg-microgame/Assets/Game/Scripts/Control/Runtime/IControlInputBinder.cs at main · apustovitovsky/rpg-microgame · GitHub"
[11]: https://docs.yarnspinner.dev/components/variable-storage?utm_source=chatgpt.com "Variable Storage"
[12]: https://docs.yarnspinner.dev/api/csharp/yarn.unity/yarn.unity.dialoguerunner?utm_source=chatgpt.com "DialogueRunner"
[13]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Dialogue/Game.Dialogue.asmdef "rpg-microgame/Assets/Game/Scripts/Dialogue/Game.Dialogue.asmdef at main · apustovitovsky/rpg-microgame · GitHub"
